using DataLayer.Models.Hospital;
using DataLayer.Models.SystemCore.NonPersistent;
using static Dapper.SqlMapper;

namespace DataLayer.Repos.Hospital;

public interface IHealthcareFacilityRepos : IBaseRepos<HealthcareFacility>
{
	Task<HealthcareFacility?> GetFullAsync(int id);
	Task<int> InsertFullAsync(HealthcareFacility obj);
	Task<bool> UpdateFullAsync(HealthcareFacility obj);

	Task<List<HealthcareFacility>> SearchAsync(
		int pgSize = 0,
		int pgNo = 0,
		string? objectCode = null,
		string? objectName = null,
		List<int>? facilityTypeDdlIdList = null,
		List<int>? cambodiaProvinceIdList = null,
		List<int>? cambodiaDistrictIdList = null);

	Task<DataPagination> GetSearchPaginationAsync(
		int pgSize = 0,
		string? objectCode = null,
		string? objectName = null,
		List<int>? facilityTypeDdlIdList = null,
		List<int>? cambodiaProvinceIdList = null,
		List<int>? cambodiaDistrictIdList = null);
}

public class HealthcareFacilityRepos(IConnectionFactory connectionFactory) : BaseRepos<HealthcareFacility>(connectionFactory, HealthcareFacility.DatabaseObject), IHealthcareFacilityRepos
{
	public async Task<HealthcareFacility?> GetFullAsync(int id)
	{
        SqlBuilder sbSql = new();
        DynamicParameters param = new();

        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.Id=@Id");
		param.Add("@Id", id);

        sbSql.LeftJoin($"{CambodiaAddress.MsSqlTable} regAddr ON regAddr.IsDeleted=0 AND regAddr.Id=t.RegisteredAddressId");
        sbSql.LeftJoin($"{CambodiaAddress.MsSqlTable} mbAddr ON mbAddr.IsDeleted=0 AND mbAddr.Id=t.MainBranchAddressId");
        sbSql.LeftJoin($"{DropdownDataList.MsSqlTable} ft ON ft.IsDeleted=0 AND ft.Id=t.FacilityTypeDdlId");

        using var cn = ConnectionFactory.GetDbConnection()!;

		string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;

        List<HealthcareFacility> dataList = (await cn.QueryAsync<HealthcareFacility, CambodiaAddress, CambodiaAddress, DropdownDataList, HealthcareFacility>(
                sql, (obj, regAddr, mainBranchAddr, facilityType) =>
                {
                    obj.RegisteredAddress = regAddr;
                    obj.MainBranchAddress = mainBranchAddr;
                    obj.FacilityType = facilityType;

                    return obj;
                }, param, splitOn: "Id")).AsList();


		if (dataList.Any())
		{
			DynamicParameters paramContact = new();
			SqlBuilder sbSqlContact = new();
			sbSqlContact.Where("t.IsDeleted=0");
			sbSqlContact.Where("t.LinkedObjectId=@LinkedObjectId");
			paramContact.Add("@LinkedObjectId", id);
			sbSqlContact.Where("t.LinkedObjectType=@LinkedObjectType");
			paramContact.Add("@LinkedObjectType", typeof(HealthcareFacility).Name, DbType.AnsiString);
			string sqlContact = sbSqlContact.AddTemplate($"SELECT * FROM {ContactPhone.MsSqlTable} t /**where**/").RawSql;
			List<ContactPhone> contactPhones = (await cn.QueryAsync<ContactPhone>(sqlContact, paramContact)).AsList();

			if (contactPhones.Any())
				dataList[0].Contacts = contactPhones;

			return dataList[0];
		}
		else return null;
    }

    public async Task<int> InsertFullAsync(HealthcareFacility obj)
	{
		DateTime timestamp = DateTime.Now;

        using var cn = ConnectionFactory.GetDbConnection()!;

        // <!IMPORTANT> Connection required to be open before calling BeginTransaction() function
        if (cn.State != ConnectionState.Open) cn.Open();

        using var tran = cn.BeginTransaction();

		try
		{
			obj.CreatedDateTime = timestamp;
			obj.ModifiedDateTime = timestamp;

			int objId = await cn.InsertAsync(obj, tran);

			if (objId <= 0)
				throw new Exception("Failed to insert object into database.");

			obj.Id = objId;

            DynamicParameters addressUpdateParam = new();
            SqlBuilder sbAddressUpdateSql = new();

            if (obj.RegisteredAddress != null && (
				obj.RegisteredAddress.CambodiaProvinceId is not null ||
                !string.IsNullOrEmpty(obj.RegisteredAddress.UnitFloor) ||
                !string.IsNullOrEmpty(obj.RegisteredAddress.StreetNo)))
			{
				obj.RegisteredAddress.CreatedUser = obj.CreatedUser;
				obj.RegisteredAddress.CreatedDateTime = timestamp;
                obj.RegisteredAddress.ModifiedUser = obj.ModifiedUser;
                obj.RegisteredAddress.ModifiedDateTime = timestamp;
				obj.RegisteredAddress.LinkedObjectId = objId;
				obj.RegisteredAddress.LinkedObjectType = obj.GetType().Name;

				int registeredAddressId = await cn.InsertAsync(obj.RegisteredAddress, tran);

				obj.RegisteredAddressId = registeredAddressId;
                addressUpdateParam.Add("@RegisteredAddressId", registeredAddressId);
            }

			if (obj.MainBranchAddress != null && (
                obj.MainBranchAddress.CambodiaProvinceId is not null ||
                !string.IsNullOrEmpty(obj.MainBranchAddress.UnitFloor) ||
                !string.IsNullOrEmpty(obj.MainBranchAddress.StreetNo)))
			{
                obj.MainBranchAddress.CreatedUser = obj.CreatedUser;
                obj.MainBranchAddress.CreatedDateTime = timestamp;
                obj.MainBranchAddress.ModifiedUser = obj.ModifiedUser;
                obj.MainBranchAddress.ModifiedDateTime = timestamp;
                obj.MainBranchAddress.LinkedObjectId = objId;
                obj.MainBranchAddress.LinkedObjectType = obj.GetType().Name;

                int mainBranchAddressId = await cn.InsertAsync(obj.MainBranchAddress, tran);

                obj.MainBranchAddressId = mainBranchAddressId;
                addressUpdateParam.Add("@MainBranchAddressId", mainBranchAddressId);
            }

			if (addressUpdateParam.ParameterNames.Any())
			{
				addressUpdateParam.Add("@Id", objId);
				string addressUpdateSql = sbAddressUpdateSql.AddTemplate($"UPDATE {DbObject.MsSqlTable} SET RegisteredAddressId=@RegisteredAddressId, MainBranchAddressId=@MainBranchAddressId WHERE Id=@Id").RawSql;
				int addressUpdCount = await cn.ExecuteAsync(addressUpdateSql, addressUpdateParam);
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

    public async Task<bool> UpdateFullAsync(HealthcareFacility obj)
	{
        DateTime timestamp = DateTime.Now;

        using var cn = ConnectionFactory.GetDbConnection()!;

        // <!IMPORTANT> Connection required to be open before calling BeginTransaction() function
        if (cn.State != ConnectionState.Open) cn.Open();

        using var tran = cn.BeginTransaction();

        try
        {
            obj.ModifiedDateTime = timestamp;

            if (obj.RegisteredAddress != null)
            {
                if (obj.RegisteredAddress.Id == 0 && (
					obj.RegisteredAddress.CambodiaProvinceId is not null ||
					!string.IsNullOrEmpty(obj.RegisteredAddress.UnitFloor) ||
					!string.IsNullOrEmpty(obj.RegisteredAddress.StreetNo)))
                {
                    obj.RegisteredAddress.CreatedUser = obj.CreatedUser;
                    obj.RegisteredAddress.CreatedDateTime = timestamp;
                    obj.RegisteredAddress.ModifiedUser = obj.ModifiedUser;
                    obj.RegisteredAddress.ModifiedDateTime = timestamp;
                    obj.RegisteredAddress.LinkedObjectId = obj.Id;
                    obj.RegisteredAddress.LinkedObjectType = obj.GetType().Name;

                    int registeredAddressId = await cn.InsertAsync(obj.RegisteredAddress, tran);

					obj.RegisteredAddressId = registeredAddressId;
                }
                else if (obj.RegisteredAddress.Id > 0)
                {
                    obj.RegisteredAddress.ModifiedUser = obj.ModifiedUser;
                    obj.RegisteredAddress.ModifiedDateTime = timestamp;
                    obj.RegisteredAddress.LinkedObjectId = obj.Id;
                    obj.RegisteredAddress.LinkedObjectType = obj.GetType().Name;

                    bool isRegisteredAddressUpdated = await cn.UpdateAsync(obj.RegisteredAddress, tran);
                }
            }

            if (obj.MainBranchAddress != null)
            {
                if (obj.MainBranchAddress.Id == 0 && (
					obj.MainBranchAddress.CambodiaProvinceId is not null ||
					!string.IsNullOrEmpty(obj.MainBranchAddress.UnitFloor) ||
					!string.IsNullOrEmpty(obj.MainBranchAddress.StreetNo)))
                {
                    obj.MainBranchAddress.CreatedUser = obj.CreatedUser;
                    obj.MainBranchAddress.CreatedDateTime = timestamp;
                    obj.MainBranchAddress.ModifiedUser = obj.ModifiedUser;
                    obj.MainBranchAddress.ModifiedDateTime = timestamp;
                    obj.MainBranchAddress.LinkedObjectId = obj.Id;
                    obj.MainBranchAddress.LinkedObjectType = obj.GetType().Name;

                    int mainBranchAddressId = await cn.InsertAsync(obj.MainBranchAddress, tran);

                    obj.MainBranchAddressId = mainBranchAddressId;
                }
                else if (obj.MainBranchAddress.Id > 0)
                {
                    obj.MainBranchAddress.ModifiedUser = obj.ModifiedUser;
                    obj.MainBranchAddress.ModifiedDateTime = timestamp;
                    obj.MainBranchAddress.LinkedObjectId = obj.Id;
                    obj.MainBranchAddress.LinkedObjectType = obj.GetType().Name;

                    bool isMainBranchAddressUpdated = await cn.UpdateAsync(obj.MainBranchAddress, tran);
                }
            }

            bool isUpdated = await cn.UpdateAsync(obj, tran);

            if (!isUpdated)
                throw new Exception("Failed to insert object into database.");

            tran.Commit();
            return isUpdated;
        }
        catch
        {
            tran.Rollback();
            throw;
        }
    }

    public override async Task<List<HealthcareFacility>> QuickSearchAsync(int pgSize = 0, int pgNo = 0, string? searchText = null, List<int>? excludeIdList = null)
	{
		if (pgNo < 0 && pgSize < 0)
			throw new ArgumentOutOfRangeException(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

		SqlBuilder sbSql = new();
		DynamicParameters param = new();

		sbSql.Where("t.IsDeleted=0");

		#region Form Search Conditions
		if (!string.IsNullOrEmpty(searchText))
		{
			if (searchText.StartsWith("id:", StringComparison.OrdinalIgnoreCase))
			{
				sbSql.Where("UPPER(t.ObjectCode) LIKE '%'+UPPER(@SearchText)+'%'");
				param.Add("@SearchText", searchText.Replace("id:", "", StringComparison.OrdinalIgnoreCase), DbType.AnsiString);
			}
			else if (searchText.StartsWith("code:", StringComparison.OrdinalIgnoreCase))
			{
				sbSql.Where("UPPER(t.ObjectCode) LIKE '%'+UPPER(@SearchText)+'%'");
				param.Add("@SearchText", searchText.Replace("code:", "", StringComparison.OrdinalIgnoreCase), DbType.AnsiString);
			}
			else
			{
				sbSql.Where("UPPER(t.ObjectName) LIKE '%'+UPPER(@SearchText)+'%'");
				param.Add("@SearchText", searchText, DbType.AnsiString);
			}
		}

        if (excludeIdList != null && excludeIdList.Count > 0)
        {
            sbSql.Where("t.Id NOT IN @ExcludeIdList");
            param.Add("@ExcludeIdList", excludeIdList);
        }
        #endregion

		sbSql.LeftJoin($"{CambodiaAddress.MsSqlTable} regAddr ON regAddr.IsDeleted=0 AND regAddr.Id=t.RegisteredAddressId");
		sbSql.LeftJoin($"{CambodiaAddress.MsSqlTable} mbAddr ON mbAddr.IsDeleted=0 AND mbAddr.Id=t.MainBranchAddressId");
		sbSql.LeftJoin($"{DropdownDataList.MsSqlTable} ft ON ft.IsDeleted=0 AND ft.Id=t.FacilityTypeDdlId");

        sbSql.OrderBy("t.ObjectName ASC");

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
				$";WITH pg AS (SELECT t.Id FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
				$"SELECT t.*, regAddr.*, mbAddr.*, ft.* FROM {DbObject.MsSqlTable} t INNER JOIN pg p ON p.Id=t.Id /**leftjoin**/ /**orderby**/").RawSql;
		}

		using var cn = ConnectionFactory.GetDbConnection()!;

        var dataList = (await cn.QueryAsync<HealthcareFacility, CambodiaAddress, CambodiaAddress, DropdownDataList, HealthcareFacility>(
                sql, (obj, regAddr, mainBranchAddr, facilityType) =>
                {
                    obj.RegisteredAddress = regAddr;
                    obj.MainBranchAddress = mainBranchAddr;
					obj.FacilityType = facilityType;

                    return obj;
                }, param, splitOn: "Id")).AsList();

        return dataList;
	}

	public async Task<DataPagination> GetSearchPaginationAsync(int pgSize = 0, 
		string? objectCode = null, 
		string? objectName = null,
		List<int>? facilityTypeDdlIdList = null,
		List<int>? cambodiaProvinceIdList = null, 
		List<int>? cambodiaDistrictIdList = null)
	{
		if (pgSize < 0)
			throw new ArgumentOutOfRangeException(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

		DynamicParameters param = new();
		SqlBuilder sbSql = new();

		sbSql.Where("t.IsDeleted=0");

		#region Form Search Conditions
		if (!string.IsNullOrEmpty(objectCode))
		{
			sbSql.Where("UPPER(t.ObjectCode) LIKE '%'+UPPER(@ObjectCode)+'%'");
			param.Add("@ObjectCode", objectCode, DbType.AnsiString);
		}

		if (!string.IsNullOrEmpty(objectName))
		{
			sbSql.Where("LOWER(t.ObjectName) LIKE '%'+LOWER(@ObjectName)+'%'");
			param.Add("@ObjectName", objectName, DbType.AnsiString);
		}

		if (facilityTypeDdlIdList is not null && facilityTypeDdlIdList.Any())
		{
			if (facilityTypeDdlIdList.Count == 1)
			{
				sbSql.Where("t.FacilityTypeDdlId=@FacilityTypeDdlId");
				param.Add("@FacilityTypeDdlId", facilityTypeDdlIdList[0]);
			}
			else
			{
				sbSql.Where("t.FacilityTypeDdlId IN @FacilityTypeDdlIdList");
				param.Add("@FacilityTypeDdlIdList", facilityTypeDdlIdList);
			}
		}

		if (cambodiaProvinceIdList != null && cambodiaProvinceIdList.Any())
		{
			if (cambodiaProvinceIdList.Count == 1)
			{
				sbSql.Where("mbAddr.CambodiaProvinceId=@CambodiaProvinceId");
				param.Add("@CambodiaProvinceId", cambodiaProvinceIdList[0]);
			}
			else
			{
				sbSql.Where("mbAddr.CambodiaProvinceId IN @CambodiaProvinceIdList");
				param.Add("@CambodiaProvinceIdList", cambodiaProvinceIdList);
			}
		}

		if (cambodiaDistrictIdList != null && cambodiaDistrictIdList.Any())
		{
			if (cambodiaDistrictIdList.Count == 1)
			{
				sbSql.Where("mbAddr.CambodiaDistrictId=@CambodiaDistrictId");
				param.Add("@CambodiaDistrictId", cambodiaDistrictIdList[0]);
			}
			else
			{
				sbSql.Where("mbAddr.CambodiaProvinceId IN @CambodiaDistrictIdList");
				param.Add("@CambodiaDistrictIdList", cambodiaDistrictIdList);
			}
		}
		#endregion

		sbSql.LeftJoin($"{CambodiaAddress.MsSqlTable} regAddr ON regAddr.IsDeleted=0 AND regAddr.Id=t.RegisteredAddressId");
		sbSql.LeftJoin($"{CambodiaAddress.MsSqlTable} mbAddr ON mbAddr.IsDeleted=0 AND mbAddr.Id=t.MainBranchAddressId");
		sbSql.LeftJoin($"{DropdownDataList.MsSqlTable} ft ON ft.IsDeleted=0 AND ft.Id=t.FacilityTypeDdlId");

		string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;

		using var cn = ConnectionFactory.GetDbConnection()!;

		decimal recordCount = await cn.ExecuteScalarAsync<int>(sql, param);
		int pageCount = (int)Math.Ceiling(recordCount / (pgSize == 0 ? 1 : pgSize));

		DataPagination pagination = new()
		{
			ObjectType = typeof(HealthcareFacility).Name,
			PageSize = pgSize,
			PageCount = pageCount,
			RecordCount = (int)recordCount
		};

		return pagination;
	}

	public async Task<List<HealthcareFacility>> SearchAsync(
		int pgSize = 0, int pgNo = 0, 
		string? objectCode = null, 
		string? objectName = null,
		List<int>? facilityTypeDdlIdList = null,
		List<int>? cambodiaProvinceIdList = null, 
		List<int>? cambodiaDistrictIdList = null)
	{
		if (pgNo < 0 && pgSize < 0)
			throw new Exception(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

		SqlBuilder sbSql = new();
		DynamicParameters param = new();

		sbSql.Where("t.IsDeleted=0");

		#region Form Search Conditions
		if (!string.IsNullOrEmpty(objectCode))
		{
			sbSql.Where("UPPER(t.ObjectCode) LIKE '%'+UPPER(@ObjectCode)+'%'");
			param.Add("@ObjectCode", objectCode, DbType.AnsiString);
		}

		if (!string.IsNullOrEmpty(objectName))
		{
			sbSql.Where("LOWER(t.ObjectName) LIKE '%'+LOWER(@ObjectName)+'%'");
			param.Add("@ObjectName", objectName, DbType.AnsiString);
		}

		if (facilityTypeDdlIdList is not null && facilityTypeDdlIdList.Any())
		{
			if (facilityTypeDdlIdList.Count == 1)
			{
				sbSql.Where("t.FacilityTypeDdlId=@FacilityTypeDdlId");
				param.Add("@FacilityTypeDdlId", facilityTypeDdlIdList[0]);
			}
			else
			{
				sbSql.Where("t.FacilityTypeDdlId IN @FacilityTypeDdlIdList");
				param.Add("@FacilityTypeDdlIdList", facilityTypeDdlIdList);
			}
		}

		if (cambodiaProvinceIdList != null && cambodiaProvinceIdList.Any())
		{
			if (cambodiaProvinceIdList.Count == 1)
			{
				sbSql.Where("mbAddr.CambodiaProvinceId=@CambodiaProvinceId");
				param.Add("@CambodiaProvinceId", cambodiaProvinceIdList[0]);
			}
			else
			{
				sbSql.Where("mbAddr.CambodiaProvinceId IN @CambodiaProvinceIdList");
				param.Add("@CambodiaProvinceIdList", cambodiaProvinceIdList);
			}
		}

		if (cambodiaDistrictIdList != null && cambodiaDistrictIdList.Any())
		{
			if (cambodiaDistrictIdList.Count == 1)
			{
				sbSql.Where("mbAddr.CambodiaDistrictId=@CambodiaDistrictId");
				param.Add("@CambodiaDistrictId", cambodiaDistrictIdList[0]);
			}
			else
			{
				sbSql.Where("mbAddr.CambodiaProvinceId IN @CambodiaDistrictIdList");
				param.Add("@CambodiaDistrictIdList", cambodiaDistrictIdList);
			}
		}
		#endregion

		sbSql.LeftJoin($"{CambodiaAddress.MsSqlTable} regAddr ON regAddr.IsDeleted=0 AND regAddr.Id=t.RegisteredAddressId");
		sbSql.LeftJoin($"{CambodiaAddress.MsSqlTable} mbAddr ON mbAddr.IsDeleted=0 AND mbAddr.Id=t.MainBranchAddressId");
		sbSql.LeftJoin($"{DropdownDataList.MsSqlTable} ft ON ft.IsDeleted=0 AND ft.Id=t.FacilityTypeDdlId");

		sbSql.OrderBy("t.ObjectName ASC");

		string sql;

		if (pgNo == 0 && pgSize == 0)
		{
			sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/ /**orderby**/").RawSql;
		}
		else
		{
			param.Add("@PageSize", pgSize);
			param.Add("@PageNo", pgNo);

			sql = sbSql.AddTemplate($";WITH pg AS (SELECT t.Id FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
				  $"SELECT t.*, regAddr.*, mbAddr.*, ft.* FROM {DbObject.MsSqlTable} t INNER JOIN pg p ON p.Id=t.Id /**leftjoin**/ /**orderby**/").RawSql;
		}

		using var cn = ConnectionFactory.GetDbConnection()!;

        var dataList = (await cn.QueryAsync<HealthcareFacility, CambodiaAddress, CambodiaAddress, DropdownDataList, HealthcareFacility>(
                sql, (obj, regAddr, mainBranchAddr, facilityType) =>
                {
                    obj.RegisteredAddress = regAddr;
                    obj.MainBranchAddress = mainBranchAddr;
                    obj.FacilityType = facilityType;

                    return obj;
                }, param, splitOn: "Id")).AsList();

        return dataList;
	}
}