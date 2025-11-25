using DataLayer.Models.HMS;
using static Dapper.SqlMapper;

namespace DataLayer.Repos.HMS;

public interface IPatientRepos : IBaseRepos<Patient>
{
    Task<string?> CheckExistingAsync(string givenName, string surName, string gender, DateTime birthDate, int id);

	Task<List<Patient>> SearchAsync(
		int pgSize = 0,
		int pgNo = 0,
		string? objectCode = null,
		string? objectName = null,
		string? name = null,
		string? nameKh = null,
		DateTime? birthDateFrom = null,
		DateTime? birthDateTo = null,
		string? gender = null,
		List<string>? maritalStatusList = null,
		string? nationalIdNum = null,
		List<string>? nationalityCountryCodeList = null,
		string? phoneNum = null,
		int? healthcareFacilityId = null,
		DateTime? registrationDateTimeFrom = null,
		DateTime? registrationDateTimeTo = null);

	Task<DataPagination> GetSearchPaginationAsync(
		int pgSize = 0,
		string? objectCode = null,
		string? objectName = null,
		string? name = null,
		string? nameKh = null,
		DateTime? birthDateFrom = null,
		DateTime? birthDateTo = null,
		string? gender = null,
		List<string>? maritalStatusList = null,
		string? nationalIdNum = null,
		List<string>? nationalityCountryCodeList = null,
		string? phoneNum = null,
		int? healthcareFacilityId = null,
		DateTime? registrationDateTimeFrom = null,
		DateTime? registrationDateTimeTo = null);
}

public class PatientRepos(IDbContext dbContext) : BaseRepos<Patient>(dbContext, Patient.DatabaseObject), IPatientRepos
{
	public async Task<string?> CheckExistingAsync(string givenName, string surName, string gender, DateTime birthDate, int id)
    {
		DynamicParameters param = new();
		SqlBuilder sbSql = new();

        sbSql.Where("t.IsDeleted=0");
		sbSql.Where("LOWER(TRIM(ISNULL(t.Surname,'')))=@Surname");
		sbSql.Where("LOWER(TRIM(ISNULL(t.GivenName, '')))=@GivenName");
        sbSql.Where("t.Gender=@Gender");
        sbSql.Where("t.BirthDate=@BirthDate");
        sbSql.Where("t.Id<>@Id");

        param.Add("@Surname", surName.Trim().ToLower(), DbType.AnsiString);
        param.Add("@GivenName", givenName.Trim().ToLower(), DbType.AnsiString);
        param.Add("@Gender", gender, DbType.AnsiString);
        param.Add("@BirthDate", birthDate);
        param.Add("@Id", id);

		using var cn = DbContext.DbCxn;
        string sql = sbSql.AddTemplate($"SELECT TOP 1 t.ObjectCode FROM {DbObject.MsSqlTable} t /**where**/").RawSql;

        string? existingCode = await cn.ExecuteScalarAsync<string>(sql, param);

        return existingCode;
	}

	public override async Task<KeyValuePair<int, IEnumerable<Patient>>> SearchNewAsync(
        int pgSize = 0, int pgNo = 0, string? searchText = null, 
        IEnumerable<SqlSortCond>? sortConds = null, 
        IEnumerable<SqlFilterCond>? filterConds = null, List<int>? excludeIdList = null)
	{
		DynamicParameters param = new();
		SqlBuilder sbSql = new();

		sbSql.Where("t.IsDeleted=0");

		#region Form Search Conditions
		if (!string.IsNullOrEmpty(searchText))
		{
			if (searchText.StartsWith("id:"))
			{
				sbSql.Where("UPPER(t.ObjectCode) LIKE '%'+@SearchText+'%'");
				param.Add("@SearchText", searchText.Replace("id:", "", StringComparison.CurrentCultureIgnoreCase), DbType.AnsiString);
			}
			else
			{
				sbSql.Where("(UPPER(t.ObjectName) LIKE '%'+UPPER(@SearchText)+'%' OR UPPER(t.ObjectCode) LIKE '%'+UPPER(@SearchText)+'%')");
				param.Add("@SearchText", searchText, DbType.AnsiString);
			}
		}

		if (excludeIdList != null && excludeIdList.Count != 0)
		{
			sbSql.Where("t.Id NOT IN @ExcludeIdList");
			param.Add("@ExcludeIdList", excludeIdList);
		}
		#endregion

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
				$";WITH pg AS (SELECT Id FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
				$"SELECT * FROM {DbObject.MsSqlTable} t WHERE t.Id IN (SELECT Id FROM pg) /**orderby**/").RawSql;
		}

		using var cn = DbContext.DbCxn;

		var dataList = await cn.QueryAsync<Patient>(sql, param);

		string sqlCount = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
		int dataCount = await cn.ExecuteScalarAsync<int>(sqlCount, param);
		return new(dataCount, dataList);
	}

    public override async Task<List<Patient>> QuickSearchAsync(int pgSize = 0, int pgNo = 0, string? searchText = null, List<int>? excludeIdList = null)
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
            else if (searchText.StartsWith("tel:"))
            {
                sbSql.Where("(t.PhoneLine1 LIKE '%'+@SearchText+'%' OR t.PhoneLine2 LIKE '%'+@SearchText+'%')");
                param.Add("@SearchText", searchText.Replace("tel:", "", StringComparison.OrdinalIgnoreCase), DbType.AnsiString);
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

        sbSql.LeftJoin($"{Customer.MsSqlTable} cust ON cust.Id=t.CustomerId");
        sbSql.LeftJoin($"{HealthcareFacility.MsSqlTable} hcf ON hcf.Id=t.HealthcareFacilityId");

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
                $"SELECT t.*, cust.*, hcf.* FROM {DbObject.MsSqlTable} t INNER JOIN pg p ON p.Id=t.Id /**leftjoin**/ /**orderby**/").RawSql;
        }

        using var cn = DbContext.DbCxn;
        var dataList = (await cn.QueryAsync<Patient, Customer, HealthcareFacility, Patient>(sql, (obj, customer, hcf) =>
            {
                obj.Customer = customer;
                obj.HealthcareFacility = hcf;

                return obj;
            }, param, splitOn: "Id")).AsList();

        return dataList;
    }

    public async Task<DataPagination> GetSearchPaginationAsync(
        int pgSize = 0, 
        string? objectCode = null, 
        string? objectName = null,
        string? name = null,
        string? nameKh = null,
        DateTime? birthDateFrom = null,
        DateTime? birthDateTo = null,
        string? gender = null,
        List<string>? maritalStatusList = null,
        string? nationalIdNum = null,
        List<string>? nationalityCountryCodeList = null,
        string? phoneNum = null,
        int? healthcareFacilityId = null,
        DateTime? registrationDateTimeFrom = null,
        DateTime? registrationDateTimeTo = null)
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

        if (!string.IsNullOrEmpty(name))
        {
            sbSql.Where("(LOWER(t.Surname) LIKE '%'+LOWER(@Name)+'%' OR LOWER(t.GivenName) LIKE '%'+LOWER(@Name)+'%')");
            param.Add("@Name", name, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(nameKh))
        {
            sbSql.Where("(LOWER(t.SurnameKh) LIKE '%'+LOWER(@NameKh)+'%' OR LOWER(t.GivenNameKh) LIKE '%'+LOWER(@NameKh)+'%')");
            param.Add("@NameKh", nameKh, DbType.AnsiString);
        }

        if (birthDateFrom != null)
        {
            sbSql.Where("t.BirthDate IS NOT NULL");
            sbSql.Where("t.BirthDate>=@BirthDateFrom");

            param.Add("@BirthDateFrom", birthDateFrom!.Value);

            if (birthDateTo != null)
            {
                sbSql.Where("t.BirthDate<=@BirthDateTo");

                param.Add("@BirthDateTo", birthDateTo!.Value);
            }
        }
        else if (birthDateTo != null)
        {
            sbSql.Where("t.BirthDate IS NOT NULL");
            sbSql.Where("t.BirthDate<=@BirthDateTo");

            param.Add("@BirthDateTo", birthDateTo!.Value);
        }

        if (!string.IsNullOrEmpty(gender))
        {
            sbSql.Where("t.Gender=@Gender");
            param.Add("@Gender", gender, DbType.AnsiString);
        }

        if (maritalStatusList != null && maritalStatusList.Any())
        {
            if (maritalStatusList.Count == 1)
            {
                sbSql.Where("t.MaritalStatus=@MaritalStatus");
                param.Add("@MaritalStatus", maritalStatusList[0], DbType.AnsiString);
            }
            else
            {
                sbSql.Where("t.MaritalStatus IN @MaritalStatusList");
                param.Add("@MaritalStatusList", maritalStatusList);
            }
        }

        if (!string.IsNullOrEmpty(nationalIdNum))
        {
            sbSql.Where("t.NationalIdNum LIKE '%'+@NationalIdNum+'%'");
            param.Add("@NationalIdNum", nationalIdNum, DbType.AnsiString);
        }

        if (healthcareFacilityId != null)
        {
            sbSql.Where("t.HealthcareFacilityId=@HealthcareFacilityId");
            param.Add("@HealthcareFacilityId", healthcareFacilityId!.Value);
        }

        if (nationalityCountryCodeList != null && nationalityCountryCodeList.Any())
        {
            if (nationalityCountryCodeList.Count == 1)
            {
                sbSql.Where("t.NationalityCountryCode=@NationalityCountryCode");
                param.Add("@NationalityCountryCode", nationalityCountryCodeList[0], DbType.AnsiString);
            }
            else
            {
                sbSql.Where("t.NationalityCountryCode IN @NationalityCountryCodeList");
                param.Add("@NationalityCountryCodeList", nationalityCountryCodeList);
            }
        }

        if (!string.IsNullOrEmpty(phoneNum))
        {
            sbSql.Where("(t.PhoneLine1 LIKE '%'+@PhoneNum+'%' OR t.PhoneLine2 LIKE '%'+@PhoneNum+'%'");
            param.Add("@PhoneNum", phoneNum, DbType.AnsiString);
        }

        if (registrationDateTimeFrom != null)
        {
            sbSql.Where("t.RegistrationDateTime IS NOT NULL");
            sbSql.Where("t.RegistrationDateTime>=@RegistrationDateTimeFrom");

            param.Add("@RegistrationDateTimeFrom", registrationDateTimeFrom!.Value);

            if (registrationDateTimeTo != null)
            {
                sbSql.Where("t.RegistrationDateTime<=@RegistrationDateTimeTo");

                param.Add("@RegistrationDateTimeTo", registrationDateTimeTo!.Value);
            }
        }
        else if (registrationDateTimeTo != null)
        {
            sbSql.Where("t.RegistrationDateTime IS NOT NULL");
            sbSql.Where("t.RegistrationDateTime<=@RegistrationDateTimeTo");

            param.Add("@RegistrationDateTimeTo", registrationDateTimeTo!.Value);
        }
        #endregion

        string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;

        using var cn = DbContext.DbCxn;

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

    public async Task<List<Patient>> SearchAsync(
        int pgSize = 0, 
        int pgNo = 0,
        string? objectCode = null,
        string? objectName = null,
        string? name = null,
        string? nameKh = null,
        DateTime? birthDateFrom = null,
        DateTime? birthDateTo = null,
        string? gender = null,
        List<string>? maritalStatusList = null,
        string? nationalIdNum = null,
        List<string>? nationalityCountryCodeList = null,
        string? phoneNum = null,
        int? healthcareFacilityId = null,
        DateTime? registrationDateTimeFrom = null,
        DateTime? registrationDateTimeTo = null)
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

        if (!string.IsNullOrEmpty(name))
        {
            sbSql.Where("(LOWER(t.Surname) LIKE '%'+LOWER(@Name)+'%' OR LOWER(t.GivenName) LIKE '%'+LOWER(@Name)+'%')");
            param.Add("@Name", name, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(nameKh))
        {
            sbSql.Where("(LOWER(t.SurnameKh) LIKE '%'+LOWER(@NameKh)+'%' OR LOWER(t.GivenNameKh) LIKE '%'+LOWER(@NameKh)+'%')");
            param.Add("@NameKh", nameKh, DbType.AnsiString);
        }

        if (birthDateFrom != null)
        {
            sbSql.Where("t.BirthDate IS NOT NULL");
            sbSql.Where("t.BirthDate>=@BirthDateFrom");

            param.Add("@BirthDateFrom", birthDateFrom!.Value);

            if (birthDateTo != null)
            {
                sbSql.Where("t.BirthDate<=@BirthDateTo");

                param.Add("@BirthDateTo", birthDateTo!.Value);
            }
        }
        else if (birthDateTo != null)
        {
            sbSql.Where("t.BirthDate IS NOT NULL");
            sbSql.Where("t.BirthDate<=@BirthDateTo");

            param.Add("@BirthDateTo", birthDateTo!.Value);
        }

        if (!string.IsNullOrEmpty(gender))
        {
            sbSql.Where("t.Gender=@Gender");
            param.Add("@Gender", gender, DbType.AnsiString);
        }

        if (maritalStatusList != null && maritalStatusList.Any())
        {
            if (maritalStatusList.Count == 1)
            {
                sbSql.Where("t.MaritalStatus=@MaritalStatus");
                param.Add("@MaritalStatus", maritalStatusList[0], DbType.AnsiString);
            }
            else
            {
                sbSql.Where("t.MaritalStatus IN @MaritalStatusList");
                param.Add("@MaritalStatusList", maritalStatusList);
            }
        }

        if (!string.IsNullOrEmpty(nationalIdNum))
        {
            sbSql.Where("t.NationalIdNum LIKE '%'+@NationalIdNum+'%'");
            param.Add("@NationalIdNum", nationalIdNum, DbType.AnsiString);
        }

        if (healthcareFacilityId != null)
        {
            sbSql.Where("t.HealthcareFacilityId=@HealthcareFacilityId");
            param.Add("@HealthcareFacilityId", healthcareFacilityId!.Value);
        }

        if (nationalityCountryCodeList != null && nationalityCountryCodeList.Any())
        {
            if (nationalityCountryCodeList.Count == 1)
            {
                sbSql.Where("t.NationalityCountryCode=@NationalityCountryCode");
                param.Add("@NationalityCountryCode", nationalityCountryCodeList[0], DbType.AnsiString);
            }
            else
            {
                sbSql.Where("t.NationalityCountryCode IN @NationalityCountryCodeList");
                param.Add("@NationalityCountryCodeList", nationalityCountryCodeList);
            }
        }

        if (!string.IsNullOrEmpty(phoneNum))
        {
            sbSql.Where("(t.PhoneLine1 LIKE '%'+@PhoneNum+'%' OR t.PhoneLine2 LIKE '%'+@PhoneNum+'%'");
            param.Add("@PhoneNum", phoneNum, DbType.AnsiString);
        }

        if (registrationDateTimeFrom != null)
        {
            sbSql.Where("t.RegistrationDateTime IS NOT NULL");
            sbSql.Where("t.RegistrationDateTime>=@RegistrationDateTimeFrom");

            param.Add("@RegistrationDateTimeFrom", registrationDateTimeFrom!.Value);

            if (registrationDateTimeTo != null)
            {
                sbSql.Where("t.RegistrationDateTime<=@RegistrationDateTimeTo");

                param.Add("@RegistrationDateTimeTo", registrationDateTimeTo!.Value);
            }
        }
        else if (registrationDateTimeTo != null)
        {
            sbSql.Where("t.RegistrationDateTime IS NOT NULL");
            sbSql.Where("t.RegistrationDateTime<=@RegistrationDateTimeTo");

            param.Add("@RegistrationDateTimeTo", registrationDateTimeTo!.Value);
        }
        #endregion

        sbSql.LeftJoin($"{Customer.MsSqlTable} cust ON cust.Id=t.CustomerId");
        sbSql.LeftJoin($"{HealthcareFacility.MsSqlTable} hcf ON hcf.Id=t.HealthcareFacilityId");

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
                  $"SELECT t.*, cust.*, hcf.* FROM {DbObject.MsSqlTable} t INNER JOIN pg p ON p.Id=t.Id /**leftjoin**/ /**orderby**/").RawSql;
        }

        using var cn = DbContext.DbCxn;

        var dataList = (await cn.QueryAsync<Patient, Customer, HealthcareFacility, Patient>(
                sql, (obj, customer, hcf) =>
                {
                    obj.Customer = customer;
                    obj.HealthcareFacility = hcf;

                    return obj;
                }, param, splitOn: "Id")).AsList();

        return dataList;
    }
}