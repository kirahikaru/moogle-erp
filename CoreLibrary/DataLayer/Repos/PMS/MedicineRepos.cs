using DataLayer.Models.PMS;
using DataLayer.Models.PMS.NonPersistent;
using DataLayer.Models.RMS;
using System.Text.RegularExpressions;
using static Dapper.SqlMapper;

namespace DataLayer.Repos.PMS;

public interface IMedicineRepos : IBaseRepos<Medicine>
{
	Task<Medicine?> GetFullAsync(int id);
    Task<int> CreateOrUpdateFullAsync(Medicine obj);
	Task<int> InsertFullAsync(Medicine obj);
	Task<bool> UpdateFullAsync(Medicine obj);

	Task<List<Medicine>> GetByMedicalCompositionAsync(string compositionName);
	Task<List<Medicine>> GetByMedicalCompositionAsync(int medicalCompositionId);
	Task<List<MedicineQuickInfo>> GetQuickInfoByMedicalCompositionAsync(int medicalCompositionId);

	Task<List<Medicine>> SearchAsync(
		int pgSize = 0, int pgNo = 0,
		string? objectCode = null,
		string? objectName = null,
		string? localCallName = null,
		string? barcode = null,
		List<string>? countryCodeList = null,
		string? compositionSummary = null);

	Task<DataPagination> GetSearchPaginationAsync(
		int pgSize = 0,
		string? objectCode = null,
		string? objectName = null,
		string? localCallName = null,
		string? barcode = null,
		List<string>? countryCodeList = null,
		string? compositionSummary = null);

	Task<bool> IsDuplicateBarcodeAsync(int objId, string barcode);
}

public class MedicineRepos(IDbContext dbContext) : BaseRepos<Medicine>(dbContext, Medicine.DatabaseObject), IMedicineRepos
{
	public async Task<Medicine?> GetFullAsync(int id)
    {
        SqlBuilder sbSql = new();

        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.Id=@Id");

        sbSql.LeftJoin($"{UnitOfMeasure.MsSqlTable} pu ON pu.IsDeleted=0 AND pu.ObjectCode=t.PackageUnitCode");
        sbSql.LeftJoin($"{UnitOfMeasure.MsSqlTable} cu ON cu.IsDeleted=0 AND cu.ObjectCode=t.ConsumableUnitCode");
        sbSql.LeftJoin($"{Country.MsSqlTable} cty ON cty.IsDeleted=0 AND cty.ObjectCode=t.MfgCountryCode");
        sbSql.LeftJoin($"{Item.MsSqlTable} i ON i.Id=t.ItemId");

        var sql = sbSql.AddTemplate($"SELECT * FROM {Medicine.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;

        using var cn = DbContext.DbCxn;

        Medicine? obj = (await cn.QueryAsync<Medicine, UnitOfMeasure, UnitOfMeasure, Country, Item, Medicine>(sql, (t, uomPkg, uomCon, cty, item) =>
        {
            t.PackageUnit = uomPkg;
            t.ConsumableUnit = uomCon;
            t.Item = item;
            t.ManufacturingCountry = cty;
            return t;
        }, new { Id = id }, splitOn: "Id")).FirstOrDefault();
        
        if (obj != null)
        {
            var medCompositionQry = $"SELECT * FROM {MedicineComposition.MsSqlTable} mc " +
                                $"LEFT JOIN {MedicalComposition.MsSqlTable} comp ON comp.Id=mc.MedicalCompositionId " +
                                $"LEFT JOIN {UnitOfMeasure.MsSqlTable} uom ON uom.IsDeleted=0 AND uom.ObjectCode=mc.UnitCode " +
                                $"WHERE mc.IsDeleted=0 AND mc.MedicineId=@Id;";

            obj.Compositions = (await cn.QueryAsync<MedicineComposition, MedicalComposition, UnitOfMeasure, MedicineComposition>(medCompositionQry,
                    (o, mc, uom) =>
                    {
                        o.MedicalComposition = mc;
                        o.Unit = uom;
                        return o;
                    }, new { Id = id }, splitOn: "Id")).AsList();
        }

        return obj;
    }

    public async Task<int> CreateOrUpdateFullAsync(Medicine obj)
    {
		using var cn = DbContext.DbCxn;

		// <!IMPORTANT> Connection required to be open before calling BeginTransaction() function
		if (cn.State != ConnectionState.Open) cn.Open();

		using var tran = cn.BeginTransaction();

        try
        {
            int objId = -1;
            bool isUpd = false;


			if (obj.Id == 0)
            {
				objId = await cn.InsertAsync(obj, tran);

                if (objId > 0)
                {
					obj.Id = objId;

					if (obj.Compositions != null && obj.Compositions.Count != 0)
					{
						foreach (MedicineComposition item in obj.Compositions)
						{
                            if (item.IsDeleted) continue;

							item.MedicineId = objId;
							item.MedicineCode = obj.ObjectCode;
							item.CreatedUser = obj.CreatedUser;
							item.CreatedDateTime = obj.CreatedDateTime;
							item.ModifiedUser = obj.ModifiedUser;
							item.ModifiedDateTime = obj.ModifiedDateTime;
							int medCompId = await cn.InsertAsync(item, tran);
						}
					}
				}
			}
            else
            {
                isUpd = await cn.UpdateAsync(obj, tran);

                if (isUpd)
                {
					objId = obj.Id;

					if (obj.Compositions != null && obj.Compositions.Count != 0)
					{
						foreach (MedicineComposition item in obj.Compositions)
						{
							item.MedicineId = objId;
							item.MedicineCode = obj.ObjectCode;

							if (item.Id > 0)
                            {
								item.ModifiedUser = obj.ModifiedUser;
								item.ModifiedDateTime = obj.ModifiedDateTime;
								bool isMedCompUpd = await cn.UpdateAsync(item, tran);
							}
                            else if (!item.IsDeleted)
                            {
								item.CreatedUser = obj.ModifiedUser;
								item.CreatedDateTime = obj.ModifiedDateTime;
								item.ModifiedUser = obj.ModifiedUser;
								item.ModifiedDateTime = obj.ModifiedDateTime;
                                int medCompId = await cn.InsertAsync(item, tran);
							}
						}
					}
				}
            }

            if (objId > 0)
            {
				if (!string.IsNullOrEmpty(obj.Barcode) && obj.ItemId == null)
				{
					string itemQry = $"SELECT * FROM {Item.MsSqlTable} WHERE IsDeleted=0 AND ObjectCode=@Barcode";
					Item? item = await cn.QuerySingleOrDefaultAsync<Item?>(itemQry, new { Barcode = new DbString { Value = obj.Barcode, IsAnsi = true } }, tran);

					if (item is null)
					{
						item = new Item()
						{
							ObjectCode = obj.Barcode,
							ObjectName = obj.ObjectName,
							Barcode = obj.Barcode,
							UPC = obj.Barcode.Length == 12 ? obj.Barcode : null,
							EAN = obj.Barcode.Length == 13 ? obj.Barcode : null,
							Brand = obj.MfgCompanyName,
							Description = obj.Description,
							MfgCountryCode = obj.MfgCountryCode,
							CreatedUser = obj.CreatedUser,
							CreatedDateTime = obj.CreatedDateTime,
							ModifiedUser = obj.ModifiedUser,
							ModifiedDateTime = obj.ModifiedDateTime
						};

						int itemId = await cn.InsertAsync(item, tran);

						if (itemId > 0)
							obj.ItemId = itemId;
					}
					else
					{
						obj.ItemId = item!.Id;
					}
				}
			}

            tran.Commit();
			return objId;
		}
		catch
		{
			tran.Rollback();
			throw;
		}
	}

    public async Task<int> InsertFullAsync(Medicine obj)
    {
        ArgumentNullException.ThrowIfNull(obj, nameof(obj));

        using var cn = DbContext.DbCxn;

        // <!IMPORTANT> Connection required to be open before calling BeginTransaction() function
        if (cn.State != ConnectionState.Open) cn.Open();

        using var tran = cn.BeginTransaction();

        try
        {
            DateTime khTimestamp = DateTime.UtcNow.AddHours(7);
            obj.CreatedDateTime = khTimestamp;
            obj.ModifiedDateTime = khTimestamp;

            if (!string.IsNullOrEmpty(obj.Barcode) && obj.ItemId == null)
            {
                string itemQry = $"SELECT * FROM {Item.MsSqlTable} WHERE IsDeleted=0 AND ObjectCode=@Barcode";
                Item? item = await cn.QuerySingleOrDefaultAsync<Item?>(itemQry, new { Barcode = new DbString { Value = obj.Barcode, IsAnsi = true } }, tran);

                if (item is null)
                {
                    item = new Item()
                    {
                        ObjectCode = obj.Barcode,
                        ObjectName = obj.ObjectName,
                        Barcode = obj.Barcode,
                        UPC = obj.Barcode.Length == 12 ? obj.Barcode : null,
                        EAN = obj.Barcode.Length == 13 ? obj.Barcode : null,
                        Brand = obj.MfgCompanyName,
                        Description = obj.Description,
                        MfgCountryCode = obj.MfgCountryCode,
                        CreatedUser = obj.CreatedUser,
                        CreatedDateTime = obj.CreatedDateTime,
                        ModifiedUser = obj.ModifiedUser,
                        ModifiedDateTime = obj.ModifiedDateTime
                    };

                    int itemId = await cn.InsertAsync(item, tran);

                    if (itemId > 0)
                        obj.ItemId = itemId;
                }
                else
                {
                    obj.ItemId = item!.Id;
                }
            }

            int mainObjId = await cn.InsertAsync(obj, tran);

            if (mainObjId <= 0)
                throw new Exception("Failed to insert object into database.");

            if (obj.Compositions != null && obj.Compositions.Count != 0)
            {
                foreach (MedicineComposition item in obj.Compositions)
                {
                    if (item.IsDeleted) continue;

                    if (item.Id > 0)
                        throw new Exception("Item must be new. i.e. Id=0;");

                    item.MedicineId = mainObjId;
                    item.CreatedUser = obj.CreatedUser;
                    item.CreatedDateTime = obj.CreatedDateTime;
                    item.ModifiedUser = obj.ModifiedUser;
                    item.ModifiedDateTime = obj.ModifiedDateTime;

                    int itemId = await cn.InsertAsync(item, tran);

                    if (itemId <= 0)
                        throw new Exception("Failed to insert item");
                }
            }

            tran.Commit();
            return mainObjId;
        }
        catch
        {
            tran.Rollback();
            throw;
        }
    }

    public async Task<bool> UpdateFullAsync(Medicine obj)
    {
        ArgumentNullException.ThrowIfNull(obj, nameof(obj));

        DateTime khTimestamp = DateTime.UtcNow.AddHours(7);
        obj.ModifiedDateTime = khTimestamp;

        using var cn = DbContext.DbCxn;

        // <!IMPORTANT> Connection required to be open before calling BeginTransaction() function
        if (cn.State != ConnectionState.Open) cn.Open();

        using var tran = cn.BeginTransaction();

        try
        {
            bool isUpdated = await cn.UpdateAsync(obj, tran);

            if (isUpdated && obj.Compositions != null && obj.Compositions.Any())
            {
                foreach (MedicineComposition item in obj.Compositions)
                {
                    if (item.Id > 0)
                    {
                        item.MedicineId = obj.Id;
                        item.ModifiedUser = obj.ModifiedUser;
                        item.ModifiedDateTime = obj.ModifiedDateTime;

                        bool isItemUpdate = await cn.UpdateAsync(item, tran);

                        if (!isItemUpdate)
                            throw new Exception($"Failed to update Medicine Composition item with Id={item.Id}");
                    }
                    else if (!item.IsDeleted)
                    {
                        item.MedicineId = obj.Id;
                        item.CreatedUser = obj.ModifiedUser;
                        item.CreatedDateTime = obj.ModifiedDateTime;
                        item.ModifiedUser = obj.ModifiedUser;
                        item.ModifiedDateTime = obj.ModifiedDateTime;

                        int itemId = await cn.InsertAsync(item, tran);

                        if (itemId < 0)
                            throw new Exception($"Failed to insert medicine composition item.");
                    }
                }
            }

            tran.Commit();
            return isUpdated;
        }
        catch
        {
            tran.Rollback();
            throw;
        }
    }

    public async Task<List<Medicine>> GetByMedicalCompositionAsync(string compositionName)
    {
        SqlBuilder sbSql = new();

        sbSql.LeftJoin($"{MedicalComposition.MsSqlTable} mc1 ON mc1.Id=mc.MedicalCompositionId");
        sbSql.LeftJoin($"{Medicine.MsSqlTable} m ON m.Id=mc.MedicineId");
        sbSql.LeftJoin($"{Country.MsSqlTable} c ON c.IsDeleted=0 AND c.ObjectCode=m.MfgCountryCode");
        sbSql.LeftJoin($"{UnitOfMeasure.MsSqlTable} pu ON pu.IsDeleted=0 AND pu.ObjectCode=m.PackageUnitCode");
        sbSql.LeftJoin($"{UnitOfMeasure.MsSqlTable} cu ON cu.IsDeleted=0 AND cu.ObjectCode=m.ConsumableUnitCode");

        sbSql.Where("mc.IsDeleted=0");
        sbSql.Where("UPPER(mc1.ObjectName) LIKE '%'+UPPER(@MedicalCompositionName)+'%'");


        string sql = sbSql.AddTemplate($"SELECT m.*, c*, pu.*, cu.* FROM {MedicineComposition.MsSqlTable} mc /**leftjoin**/ /**where**/").RawSql;

        DynamicParameters param = new();
        param.Add("@MedicalCompositionName", compositionName, DbType.AnsiString);

        using var cn = DbContext.DbCxn;

        var dataList = (await cn.QueryAsync<Medicine, Country, UnitOfMeasure, UnitOfMeasure, Medicine>(sql,
                                    (obj, country, pkgUnit, consumableUnit) =>
                                    {
                                        obj.ManufacturingCountry = country;
                                        obj.PackageUnit = pkgUnit;
                                        obj.ConsumableUnit = consumableUnit;
                                        return obj;

                                    }, param, splitOn: "Id")).AsList();

        return dataList;
    }

    public async Task<List<Medicine>> GetByMedicalCompositionAsync(int medicalCompositionId)
    {
        SqlBuilder sbSql = new();

        sbSql.LeftJoin($"{MedicalComposition.MsSqlTable} mc1 ON mc1.Id=mc.MedicalCompositionId");
        sbSql.LeftJoin($"{Medicine.MsSqlTable} m ON m.Id=mc.MedicineId");
        sbSql.LeftJoin($"{Country.MsSqlTable} c ON c.IsDeleted=0 AND c.ObjectCode=m.MfgCountryCode");
        sbSql.LeftJoin($"{UnitOfMeasure.MsSqlTable} pu ON pu.IsDeleted=0 AND pu.ObjectCode=m.PackageUnitCode");
        sbSql.LeftJoin($"{UnitOfMeasure.MsSqlTable} cu ON cu.IsDeleted=0 AND cu.ObjectCode=m.ConsumableUnitCode");

        sbSql.Where("mc.IsDeleted=0");
        sbSql.Where("mc.MedicalCompositionId=@MedicalCompositionId");
        
        string sql = sbSql.AddTemplate($"SELECT m.*, c*, pu.*, cu.* FROM {MedicineComposition.MsSqlTable} mc /**leftjoin**/ /**where**/").RawSql;

        DynamicParameters param = new();
        param.Add("@MedicalCompositionId", medicalCompositionId);

        using var cn = DbContext.DbCxn;

        var dataList = (await cn.QueryAsync<Medicine, Country, UnitOfMeasure, UnitOfMeasure, Medicine>(sql,
                                    (obj, country, pkgUnit, consumableUnit) =>
                                    {
                                        obj.ManufacturingCountry = country;
                                        obj.PackageUnit = pkgUnit;
                                        obj.ConsumableUnit = consumableUnit;
                                        return obj;

                                    }, param, splitOn: "Id")).AsList();

        return dataList;
    }

    public async Task<List<MedicineQuickInfo>> GetQuickInfoByMedicalCompositionAsync(int medicalCompositionId)
    {
        SqlBuilder sbSql = new();

        sbSql.Select("m.Id")
            .Select("m.ObjectCode")
            .Select("m.ObjectName")
            .Select("'MfgCountryName'=c.ObjectName")
            .Select("m.CompositionSummary")
            .Select("m.ItemId")
            .Select("m.Barcode")
            .Select("i.CurrencyCode")
            .Select("i.RetailUnitPrice")
            .Select("i.WholeSaleUnitPrice")
            .Select("i.RetailUnitPriceKhr")
            .Select("i.WholeSaleUnitPriceKhr")
            .Select("i.InfoLink")
            .Select("i.PurchaseLink");


        sbSql.LeftJoin($"{MedicalComposition.MsSqlTable} mc1 ON mc1.Id=t.MedicalCompositionId");
        sbSql.LeftJoin($"{Medicine.MsSqlTable} m ON m.Id=t.MedicineId");
        sbSql.LeftJoin($"{Country.MsSqlTable} c ON c.IsDeleted=0 AND c.ObjectCode=m.MfgCountryCode");
        sbSql.LeftJoin($"{UnitOfMeasure.MsSqlTable} pu ON pu.IsDeleted=0 AND pu.ObjectCode=m.PackageUnitCode");
        sbSql.LeftJoin($"{UnitOfMeasure.MsSqlTable} cu ON cu.IsDeleted=0 AND cu.ObjectCode=m.ConsumableUnitCode");
        sbSql.LeftJoin($"{Item.MsSqlTable} i ON i.Id=m.ItemId");

        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.MedicalCompositionId=@MedicalCompositionId");

        string sql = sbSql.AddTemplate($"SELECT /**select**/ FROM {MedicineComposition.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;

        DynamicParameters param = new();
        param.Add("@MedicalCompositionId", medicalCompositionId);

        using var cn = DbContext.DbCxn;

        var dataList = (await cn.QueryAsync<MedicineQuickInfo>(sql, param)).AsList();

        return dataList;
    }

	public override async Task<KeyValuePair<int, IEnumerable<Medicine>>> SearchNewAsync(
	    int pgSize = 0, int pgNo = 0,
	    string? searchText = null,
	    IEnumerable<SqlSortCond>? sortConds = null,
	    IEnumerable<SqlFilterCond>? filterConds = null,
	    List<int>? excludeIdList = null
    )
	{
		DynamicParameters param = new();
		SqlBuilder sbSql = new();

		sbSql.Where("t.IsDeleted=0");

		#region Form Search Conditions
		if (!string.IsNullOrEmpty(searchText))
		{
            if (Regex.IsMatch(searchText, @"^[0-9]{5,}$"))
            {
				sbSql.Where("t.Barcode=@SearchText");
				param.Add("@SearchText", searchText);
			}
            else
            {
				sbSql.Where("(UPPER(t.ObjectName) LIKE '%'+UPPER(@SearchText)+'%' OR UPPER(t.ObjectCode) LIKE '%'+UPPER(@SearchText)+'%' OR UPPER(t.CompositionSummary) LIKE '%'+UPPER(@SearchText)+'%' OR UPPER(t.IngredientSummary) LIKE '%'+UPPER(@SearchText)+'%')");
				param.Add("@SearchText", searchText);
			}
		}

		if (excludeIdList != null && excludeIdList.Count != 0)
		{
			sbSql.Where("t.Id NOT IN @ExcludeIdList");
			param.Add("@ExcludeIdList", excludeIdList);
		}

		if (filterConds != null && filterConds.Any())
		{
			foreach (SqlFilterCond filterCond in filterConds)
			{

			}
		}

		#endregion

		sbSql.LeftJoin($"{Country.MsSqlTable} c ON c.IsDeleted=0 AND c.ObjectCode=t.MfgCountryCode");
		sbSql.LeftJoin($"{UnitOfMeasure.MsSqlTable} pu ON pu.IsDeleted=0 AND pu.ObjectCode=t.PackageUnitCode");
		sbSql.LeftJoin($"{UnitOfMeasure.MsSqlTable} cu ON cu.IsDeleted=0 AND cu.ObjectCode=t.ConsumableUnitCode");
		sbSql.LeftJoin($"{DropdownDataList.MsSqlTable} mt ON mt.Id=t.MedicineTypeId");

		if (sortConds is null || !sortConds.Any())
		{
			foreach (string order in GetSearchOrderbBy())
			{
				sbSql.OrderBy(order);
			}
		}
		else
		{
			foreach (SqlSortCond sortCond in sortConds)
			{
				sbSql.OrderBy(sortCond.GetSortCommand("t"));
			}
		}

		string sql;

		if (pgNo == 0 && pgSize == 0)
		{
			sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/ /**orderby**/").RawSql;
		}
		else
		{
			param.Add("@PageSize", pgSize);
			param.Add("@PageNo", pgNo);
			sql = sbSql.AddTemplate(
				$";WITH pg AS (SELECT Id FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
				$"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ WHERE t.Id IN (SELECT Id FROM pg) /**orderby**/").RawSql;
		}

		using var cn = DbContext.DbCxn;

		var dataList = (await cn.QueryAsync<Medicine, Country, UnitOfMeasure, UnitOfMeasure, DropdownDataList, Medicine>(
										sql, (medicine, mfgCty, pkgUom, cuUnom, medType) =>
										{
											medicine.ManufacturingCountry = mfgCty;
											medicine.PackageUnit = pkgUom;
											medicine.ConsumableUnit = cuUnom;
											medicine.MedicineType = medType;

											return medicine;
										}, param, splitOn: "Id")).ToList();

		string sqlCount = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
		int dataCount = await cn.ExecuteScalarAsync<int>(sqlCount, param);
		return new(dataCount, dataList);
	}

	public override async Task<List<Medicine>> QuickSearchAsync(int pgSize = 0, int pgNo = 0, string? searchText = null, List<int>? excludeIdList = null)
    {
        if (pgNo < 0 && pgSize < 0)
            throw new ArgumentOutOfRangeException(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        DynamicParameters param = new();
        SqlBuilder sbSql = new();

        sbSql.Where("t.IsDeleted=0");

        #region Form Search Conditions
        Regex alphabets = new(@"^[a-zA-Z ]{1,}$");
		Regex numbers = new(@"^[0-9\-]{1,}$");

		if (!string.IsNullOrEmpty(searchText))
        {
            if (searchText.StartsWith("id:", StringComparison.OrdinalIgnoreCase))
            {
                sbSql.Where("t.ObjectCode LIKE '%'+@SearchText+'%'");
                param.Add("@SearchText", searchText.Replace("id:","", StringComparison.OrdinalIgnoreCase), DbType.AnsiString);
            }
            else if (searchText.StartsWith("compo:", StringComparison.OrdinalIgnoreCase))
            {
                sbSql.Where("LOWER(t.CompositionSummary) LIKE '%'+LOWER(@SearchText)+'%'");
                param.Add("@SearchText", searchText.Replace("compo:", "", StringComparison.OrdinalIgnoreCase), DbType.AnsiString);
            }
            else if (numbers.IsMatch(searchText))
			{
				sbSql.Where("t.Barcode LIKE '%'+@SearchText+'%'");
				param.Add("@SearchText", searchText, DbType.AnsiString);
			}
			else
            {
                sbSql.Where("UPPER(t.ObjectName) LIKE '%'+UPPER(@SearchText)+'%' OR LOWER(t.CompositionSummary) LIKE '%'+LOWER(@SearchText)+'%'");
                param.Add("@SearchText", searchText, DbType.AnsiString);
            }
        }

        if (excludeIdList != null && excludeIdList.Count > 0)
        {
            sbSql.Where("t.Id NOT IN @ExcludeIdList");
            param.Add("@ExcludeIdList", excludeIdList);
        }
        #endregion

        sbSql.LeftJoin($"{Country.MsSqlTable} c ON c.IsDeleted=0 AND c.ObjectCode=t.MfgCountryCode");
        sbSql.LeftJoin($"{UnitOfMeasure.MsSqlTable} pu ON pu.IsDeleted=0 AND pu.ObjectCode=t.PackageUnitCode");
        sbSql.LeftJoin($"{UnitOfMeasure.MsSqlTable} cu ON cu.IsDeleted=0 AND cu.ObjectCode=t.ConsumableUnitCode");
		sbSql.LeftJoin($"{DropdownDataList.MsSqlTable} mt ON mt.Id=t.MedicineTypeId");

		sbSql.OrderBy("t.ObjectName ASC");

        string sql;

        if (pgNo == 0 && pgSize == 0)
        {
            sql = sbSql.AddTemplate(
                $"SELECT * FROM {Medicine.MsSqlTable} t /**leftjoin**/ /**where**/ /**orderby**/").RawSql;
        }
        else
        {
            param.Add("@PageSize", pgSize);
            param.Add("@PageNo", pgNo);
            sql = sbSql.AddTemplate(
                  $"WITH pg AS (SELECT Id FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
                  $"SELECT t.*, c.*, pu.*, cu.*, mt.* FROM {DbObject.MsSqlTable} t INNER JOIN pg p ON p.Id=t.Id /**leftjoin**/ /**orderby**/").RawSql;
        }

        using var cn = DbContext.DbCxn;

        var dataList = (await cn.QueryAsync<Medicine, Country, UnitOfMeasure, UnitOfMeasure, DropdownDataList, Medicine>(
                                        sql, (medicine, mfgCty, pkgUom, cuUnom, medType) =>
                                        {
                                            medicine.ManufacturingCountry = mfgCty;
                                            medicine.PackageUnit = pkgUom;
                                            medicine.ConsumableUnit = cuUnom;
                                            medicine.MedicineType = medType;

                                            return medicine;
                                        }, param, splitOn: "Id")).ToList();

        return dataList;
    }

	public override async Task<DataPagination> GetQuickSearchPaginationAsync(int pgSize = 0, string? searchText = null, List<int>? excludeIdList = null)
	{
		if (pgSize < 0)
			throw new ArgumentOutOfRangeException(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

		DynamicParameters param = new();
		SqlBuilder sbSql = new();
		sbSql.Where("t.IsDeleted=0");

        #region Form Search Conditions
        Regex alphabets = new(@"^[a-zA-Z ]{1,}$");
        Regex numbers = new(@"^[0-9\-]{1,}$");

        if (!string.IsNullOrEmpty(searchText))
        {
            if (searchText.StartsWith("id:", StringComparison.OrdinalIgnoreCase))
            {
                sbSql.Where("t.ObjectCode LIKE '%'+@SearchText+'%'");
                param.Add("@SearchText", searchText, DbType.AnsiString);
            }
            else if (numbers.IsMatch(searchText))
            {
                sbSql.Where("t.Barcode LIKE '%'+@SearchText+'%'");
                param.Add("@SearchText", searchText, DbType.AnsiString);
            }
            else
            {
                sbSql.Where("UPPER(t.ObjectName) LIKE '%'+UPPER(@SearchText)+'%' OR LOWER(t.CompositionSummary) LIKE '%'+LOWER(@SearchText)+'%'");
                param.Add("@SearchText", searchText, DbType.AnsiString);
            }
        }

        if (excludeIdList != null && excludeIdList.Count > 0)
        {
            sbSql.Where("t.Id NOT IN @ExcludeIdList");
            param.Add("@ExcludeIdList", excludeIdList);
        }
        #endregion

        string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;

		using var cn = DbContext.DbCxn;

		decimal recordCount = await cn.ExecuteScalarAsync<int>(sql, param);
		int pageCount = (int)Math.Ceiling(recordCount / (pgSize == 0 ? 1 : pgSize));

		DataPagination pagination = new()
		{
			ObjectType = typeof(Medicine).Name,
			PageSize = pgSize,
			PageCount = pageCount,
			RecordCount = (int)recordCount
		};

		return pagination;
	}

	public async Task<List<Medicine>> SearchAsync(
        int pgSize = 0, int pgNo = 0,
        string? objectCode = null,
        string? objectName = null,
        string? localCallName = null,
        string? barcode = null,
        List<string>? countryCodeList = null,
        string? compositionSummary = null)

    {
        if (pgNo < 0 && pgSize < 0)
            throw new ArgumentOutOfRangeException(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        DynamicParameters param = new();
        SqlBuilder sbSql = new();

		#region Form Search Conditions
		sbSql.Where("t.IsDeleted=0");

        if (!string.IsNullOrEmpty(objectCode))
        {
            sbSql.Where("LOWER(t.[ObjectCode]) LIKE '%'+LOWER(@ObjectCode)+'%'");
            param.Add("@ObjectCode", objectCode, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(objectName))
        {
            sbSql.Where("LOWER(t.[ObjectName]) LIKE '%'+LOWER(@ObjectName)+'%'");
            param.Add("@ObjectName", objectName, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(localCallName))
        {
            sbSql.Where("t.LocalCallName LIKE '%'+@LocalCallName+'%'");
            param.Add("@LocalCallName", localCallName, DbType.String);
        }

        if (!string.IsNullOrEmpty(barcode))
        {
            sbSql.Where("t.Barcode LIKE '%'+@Barcode+'%'");
            param.Add("@Barcode", barcode, DbType.AnsiString);
        }

        if (countryCodeList != null && countryCodeList.Any())
        {
            if (countryCodeList.Count == 1)
            {
                sbSql.Where("t.MfgCountryCode=@MfgCountryCode");
                param.Add("@MfgCountryCode", countryCodeList[0], DbType.AnsiString);
            }
            else
            {
                sbSql.Where("t.MfgCountryCode IN @MfgCountryCodeList");
                param.Add("@MfgCountryCodeList", countryCodeList);
            }
        }

        if (!string.IsNullOrEmpty(compositionSummary))
        {
            sbSql.Where("LOWER(t.CompositionSummary) LIKE '%'+LOWER(@CompositionSummary)+'%'");
            param.Add("@CompositionSummary", compositionSummary, DbType.AnsiString);
        }

        sbSql.LeftJoin($"{Country.MsSqlTable} c ON c.IsDeleted=0 AND c.ObjectCode=t.MfgCountryCode");
        sbSql.LeftJoin($"{UnitOfMeasure.MsSqlTable} pu ON pu.IsDeleted=0 AND pu.ObjectCode=t.PackageUnitCode");
        sbSql.LeftJoin($"{UnitOfMeasure.MsSqlTable} cu ON cu.IsDeleted=0 AND cu.ObjectCode=t.ConsumableUnitCode");
		sbSql.LeftJoin($"{DropdownDataList.MsSqlTable} mt ON mt.Id=t.MedicineTypeId");

		sbSql.OrderBy("t.ObjectName ASC");

        string sql;

        if (pgNo == 0 && pgSize == 0)
        {
            sql = sbSql.AddTemplate(
                $"SELECT * FROM {Medicine.MsSqlTable} t /**leftjoin**/ /**where**/ /**orderby**/").RawSql;
        }
        else
        {
            param.Add("@PageSize", pgSize);
            param.Add("@PageNo", pgNo);
            sql = sbSql.AddTemplate(
                  $"WITH pg AS (SELECT Id FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
                  $"SELECT t.*, c.*, pu.*, cu.*, mt.* FROM {DbObject.MsSqlTable} t INNER JOIN pg p ON p.Id=t.Id /**leftjoin**/ /**orderby**/").RawSql;
        }
		#endregion

		using var cn = DbContext.DbCxn;

        var dataList = (await cn.QueryAsync<Medicine, Country, UnitOfMeasure, UnitOfMeasure, DropdownDataList, Medicine>(
                                        sql, (medicine, mfgCty, pkgUom, cuUnom, medType) =>
                                        {
                                            medicine.ManufacturingCountry = mfgCty;
                                            medicine.PackageUnit = pkgUom;
                                            medicine.ConsumableUnit = cuUnom;
                                            medicine.MedicineType = medType;

											return medicine;
                                        }, param, splitOn: "Id")).ToList();

        return dataList;
    }

    public async Task<DataPagination> GetSearchPaginationAsync(
        int pgSize = 0,
        string? objectCode = null,
        string? objectName = null,
        string? localCallName = null,
        string? barcode = null,
        List<string>? countryCodeList = null,
        string? compositionSummary = null)
    {
        if (pgSize < 0)
            throw new Exception(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        DynamicParameters param = new();
        SqlBuilder sbSql = new();

        sbSql.Where("t.IsDeleted=0");

		#region Form Search Conditions
		if (!string.IsNullOrEmpty(objectCode))
        {
            sbSql.Where("LOWER(t.[ObjectCode]) LIKE @ObjectCode+'%'");
            param.Add("@ObjectCode", objectCode.ToLower(), DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(objectName))
        {
            sbSql.Where("LOWER(t.[ObjectName]) LIKE '%'+@ObjectName+'%'");
            param.Add("@ObjectName", objectName.ToLower(), DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(localCallName))
        {
            sbSql.Where("t.LocalCallName LIKE '%'+@LocalCallName+'%'");
            param.Add("@LocalCallName", localCallName, DbType.String);
        }

        if (!string.IsNullOrEmpty(barcode))
        {
            sbSql.Where("t.Barcode LIKE '%'+@Barcode+'%'");
            param.Add("@Barcode", barcode, DbType.AnsiString);
        }

        if (countryCodeList != null && countryCodeList.Any())
        {
            if (countryCodeList.Count == 1)
            {
                sbSql.Where("t.MfgCountryCode=@MfgCountryCode");
                param.Add("@MfgCountryCode", countryCodeList[0], DbType.AnsiString);
            }
            else
            {
                sbSql.Where("t.MfgCountryCode IN @MfgCountryCodeList");
                param.Add("@MfgCountryCodeList", countryCodeList);
            }
        }

        if (!string.IsNullOrEmpty(compositionSummary))
        {
            sbSql.Where("t.CompositionSummary LIKE '%'+@CompositionSummary+'%'");
            param.Add("@CompositionSummary", compositionSummary);
        }
		#endregion

		string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;

        using var cn = DbContext.DbCxn;

		decimal recordCount = await cn.ExecuteScalarAsync<int>(sql, param);
		int pageCount = (int)Math.Ceiling(recordCount / pgSize);

        DataPagination pagination = new()
        {
            ObjectType = typeof(Medicine).Name,
            PageSize = pgSize,
            RecordCount = (int)recordCount,
            PageCount = pageCount
        };

        return pagination;
    }

    #region Validation Functions
    public async Task<bool> IsDuplicateBarcodeAsync(int objId, string barcode)
    {
        string sql = $"SELECT COUNT(*) FROM {DbObject.MsSqlTable} WHERE IsDeleted=0 AND Id<>@Id AND Barcode IS NOT NULL AND Barcode=@Barcode";
        DynamicParameters param = new();
        param.Add("@Id", objId);
        param.Add("@Barcode", barcode, DbType.AnsiString);

        using var cn = DbContext.DbCxn;

        int count = await cn.ExecuteScalarAsync<int>(sql, param);

        return count > 0;
    }
    #endregion
}