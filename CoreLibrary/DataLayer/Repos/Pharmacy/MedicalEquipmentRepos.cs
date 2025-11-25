using DataLayer.Models.Pharmacy;
using DataLayer.Models.Retail;
using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Repos.Pharmacy;

public interface IMedicalEquipmentRepos : IBaseRepos<MedicalEquipment>
{
	Task<MedicalEquipment?> GetFullAsync(int id);

	//Task<int> InsertFullAsync(Medicine obj);
	//Task<bool> UpdateFullAsync(Medicine obj);

	Task<List<MedicalEquipment>> SearchAsync(
		int pgSize = 0, int pgNo = 0,
		string? objectCode = null,
		string? objectName = null,
		string? objectNameKh = null,
		string? barcode = null,
		string? brand = null,
		string? modelNo = null,
		List<string>? mfgCountryCodeList = null);

	Task<DataPagination> GetSearchPaginationAsync(
		int pgSize = 0,
		string? objectCode = null,
		string? objectName = null,
		string? objectNameKh = null,
		string? barcode = null,
		string? brand = null,
		string? modelNo = null,
		List<string>? mfgCountryCodeList = null);
}

public class MedicalEquipmentRepos(IConnectionFactory connectionFactory) : BaseRepos<MedicalEquipment>(connectionFactory, MedicalEquipment.DatabaseObject), IMedicalEquipmentRepos
{
	public async Task<MedicalEquipment?> GetFullAsync(int id)
    {
        SqlBuilder sbSql = new();
        DynamicParameters param = new();

        sbSql.Where("t.IsDeleted=0");

        sbSql.LeftJoin($"{Item.MsSqlTable} i ON i.Id=t.ItemId");
        sbSql.LeftJoin($"{Country.MsSqlTable} cty ON cty.IsDeleted=0 AND cty.ObjectCode=t.MfgCountryCode");

        param.Add("@Id", id);

        var sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;

        using var cn = ConnectionFactory.GetDbConnection()!;

        var data = (await cn.QueryAsync<MedicalEquipment, Item, Country, MedicalEquipment>(sql, (obj, item, mfgCty) =>
        {
            obj.Item = item;
            obj.MfgCountry = mfgCty;

            return obj;
        }, param, splitOn: "Id")).FirstOrDefault();

        return data;
    }

	public async Task<List<MedicalEquipment>> SearchAsync(
		int pgSize = 0, int pgNo = 0,
		string? objectCode = null,
		string? objectName = null,
		string? objectNameKh = null,
		string? barcode = null,
		string? brand = null,
		string? modelNo = null,
		List<string>? mfgCountryCodeList = null)
    {
		if (pgNo < 0 && pgSize < 0)
			throw new ArgumentOutOfRangeException(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

		DynamicParameters param = new();
		SqlBuilder sbSql = new();

		sbSql.Where("t.IsDeleted=0");

		#region Form Search Conditions
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

		if (!string.IsNullOrEmpty(objectNameKh))
		{
			sbSql.Where("t.ObjectNameKh LIKE '%'+@ObjectNameKh+'%'");
			param.Add("@ObjectNameKh", objectNameKh);
		}

		if (!string.IsNullOrEmpty(barcode))
		{
			sbSql.Where("t.Barcode LIKE '%'+@Barcode+'%'");
			param.Add("@Barcode", barcode, DbType.AnsiString);
		}

		if (!string.IsNullOrEmpty(brand))
		{
			sbSql.Where("t.Brand LIKE '%'+@Brand+'%'");
			param.Add("@Brand", brand, DbType.AnsiString);
		}

		if (!string.IsNullOrEmpty(modelNo))
		{
			sbSql.Where("t.ModelNo LIKE '%'+@Brand+'%'");
			param.Add("@ModelNo", modelNo, DbType.AnsiString);
		}

		if (mfgCountryCodeList != null && mfgCountryCodeList.Count != 0)
		{
			if (mfgCountryCodeList.Count == 1)
			{
				sbSql.Where("t.MfgCountryCode=@MfgCountryCode");
				param.Add("@MfgCountryCode", mfgCountryCodeList[0], DbType.AnsiString);
			}
			else
			{
				sbSql.Where("t.MfgCountryCode IN @MfgCountryCodeList");
				param.Add("@MfgCountryCodeList", mfgCountryCodeList);
			}
		}
		#endregion

		sbSql.LeftJoin($"{Country.MsSqlTable} mfgCty ON mfgCty.IsDeleted=0 AND mfgCty.ObjectCode=t.MfgCountryCode");
		sbSql.LeftJoin($"{Item.MsSqlTable} i ON i.Id=t.ItemId");

		sbSql.OrderBy("t.ObjectName ASC");

		string sql;

		if (pgNo == 0 && pgSize == 0)
		{
			sql = sbSql.AddTemplate(
				$"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/ /**orderby**/").RawSql;
		}
		else
		{
			param.Add("@PageSize", pgSize);
			param.Add("@PageNo", pgNo);
			sql = sbSql.AddTemplate(
				  $"WITH pg AS (SELECT Id FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
				  $"SELECT t.*, c.*, pu.*, cu.*, mt.* FROM {DbObject.MsSqlTable} t INNER JOIN pg p ON p.Id=t.Id /**leftjoin**/ /**orderby**/").RawSql;
		}

		using var cn = ConnectionFactory.GetDbConnection()!;

		var dataList = (await cn.QueryAsync<MedicalEquipment, Item, Country, MedicalEquipment>(
										sql, (obj, item, mfgCty) =>
										{
											obj.Item = item;
											obj.MfgCountry = mfgCty;

											return obj;
										}, param, splitOn: "Id")).AsList();

		return dataList;
	}

	public async Task<DataPagination> GetSearchPaginationAsync(
		int pgSize = 0,
		string? objectCode = null,
		string? objectName = null,
		string? objectNameKh = null,
		string? barcode = null,
		string? brand = null,
		string? modelNo = null,
		List<string>? mfgCountryCodeList = null)
	{
		if (pgSize < 0)
			throw new Exception(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

		DynamicParameters param = new();
		SqlBuilder sbSql = new();

		sbSql.Where("t.IsDeleted=0");

		#region Form Search Conditions
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

		if (!string.IsNullOrEmpty(objectNameKh))
		{
			sbSql.Where("t.ObjectNameKh LIKE '%'+@ObjectNameKh+'%'");
			param.Add("@ObjectNameKh", objectNameKh);
		}

		if (!string.IsNullOrEmpty(barcode))
		{
			sbSql.Where("t.Barcode LIKE '%'+@Barcode+'%'");
			param.Add("@Barcode", barcode, DbType.AnsiString);
		}

		if (!string.IsNullOrEmpty(brand))
		{
			sbSql.Where("t.Brand LIKE '%'+@Brand+'%'");
			param.Add("@Brand", brand, DbType.AnsiString);
		}

		if (!string.IsNullOrEmpty(modelNo))
		{
			sbSql.Where("t.ModelNo LIKE '%'+@Brand+'%'");
			param.Add("@ModelNo", modelNo, DbType.AnsiString);
		}

		if (mfgCountryCodeList != null && mfgCountryCodeList.Count != 0)
		{
			if (mfgCountryCodeList.Count == 1)
			{
				sbSql.Where("t.MfgCountryCode=@MfgCountryCode");
				param.Add("@MfgCountryCode", mfgCountryCodeList[0], DbType.AnsiString);
			}
			else
			{
				sbSql.Where("t.MfgCountryCode IN @MfgCountryCodeList");
				param.Add("@MfgCountryCodeList", mfgCountryCodeList);
			}
		}
		#endregion

		string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;

		using var cn = ConnectionFactory.GetDbConnection()!;

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
}