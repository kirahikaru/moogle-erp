using DataLayer.Models.SysCore.NonPersistent;
using System.Text.RegularExpressions;

namespace DataLayer.Repos.SysCore;

public interface IPersonRepos : IBaseRepos<Person>
{
	Task<Person?> GetFullAsync(int id);
	Task<int> InsertFullAsync(Person p);
	Task<bool> UpdateFullAsync(Person p);
	Task<bool> IsExistSamePersonAsync(int objId, string surname, string givenName, string gender, DateTime birthDate);
	Task<bool> IsExistSamePersonKhAsync(int objId, string surnameKh, string givenNameKh, string gender, DateTime birthDate);
	Task<int> CountExistingNationalIDAsync(int objId, string idNum, string countryCode);

	Task<List<Person>> FindAsync(
		int? objId,
		string? surname,
		string? givenName,
		string? surnameKh,
		string? givenNameKh,
		string? gender,
		DateTime? birthDate,
		string? nationalIdNum,
		string? passportIdNum,
		bool? showCustomerOrNonCustomer = null);


	Task<List<Person>> QuickSearchAsync(
		int pgSize = 0,
		int pgNo = 0,
		string? searchText = null,
		bool? showCustomerOrNonCustomer = null,
		Dictionary<string, bool>? fieldRequiredToHaveValues = null,
		List<int>? excludeIdList = null);

	Task<DataPagination> GetQuickSearchPaginationAsync(
		int pgSize = 0,
		string? searchText = null,
		bool? showCustomerOrNonCustomer = null,
		Dictionary<string, bool>? fieldRequiredToHaveValues = null,
		List<int>? excludeIdList = null);


	Task<List<Person>> SearchAsync(
			int pgSize = 0,
			int pgNo = 0,
			string? objectCode = null,
			string? objectName = null,
			string? nameEn = null,
			string? nameKh = null,
			List<string>? genderList = null,
			List<string>? maritalStatusList = null,
			DateTime? birthDateFrom = null,
			DateTime? birthDateTo = null,
			string? nationalIdNum = null,
			string? passportNo = null,
			string? email = null,
			string? phoneNum = null,
			bool? showCustomerOrNonCustomer = null,
			string? customerId = null,
			Dictionary<string, bool>? fieldRequiredToHaveValues = null,
			List<int>? excludeIdList = null);

	Task<List<DropdownSelectItem>> GetForDropdownAsync(string? searchText = null);

	Task<DataPagination> GetSearchPaginationAsync(
			int pgSize = 0,
			string? objectCode = null,
			string? objectName = null,
			string? nameEn = null,
			string? nameKh = null,
			List<string>? genderList = null,
			List<string>? maritalStatusList = null,
			DateTime? birthDateFrom = null,
			DateTime? birthDateTo = null,
			string? nationalIdNum = null,
			string? passportNo = null,
			string? email = null,
			string? phoneNum = null,
			bool? showCustomerOrNonCustomer = null,
			string? customerId = null,
			Dictionary<string, bool>? fieldRequiredToHaveValues = null,
			List<int>? excludeIdList = null
		);

	Task<List<DropdownSelectItem>> GetForDropdownEnAsync(string? searchText = null);
	Task<List<DropdownSelectItem>> GetForDropdownKhAsync(string? searchText = null);
}

public class PersonRepos(IDbContext dbContext) : BaseRepos<Person>(dbContext, Person.DatabaseObject), IPersonRepos
{
	public async Task<Person?> GetFullAsync(int id)
    {
        SqlBuilder sbSql = new();

        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.Id=@Id");

        sbSql.LeftJoin($"{CambodiaAddress.MsSqlTable} ba ON ba.IsDeleted=0 AND ba.Id=t.BirthAddressId");
        sbSql.LeftJoin($"{CambodiaAddress.MsSqlTable} wa ON wa.IsDeleted=0 AND wa.Id=t.WorkAddressId");
        sbSql.LeftJoin($"{CambodiaAddress.MsSqlTable} ra ON ra.IsDeleted=0 AND ra.Id=t.ResidentialAddressId");
        sbSql.LeftJoin($"{CambodiaAddress.MsSqlTable} pa ON pa.IsDeleted=0 AND pa.Id=t.PostalAddressId");

        string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;

        using var cn = DbContext.DbCxn;

        List<Person> result = (await cn.QueryAsync<Person, CambodiaAddress, CambodiaAddress, CambodiaAddress, CambodiaAddress, Person>(sql, (p, ba, wa, ra, pa) =>
        {
            if (ba != null)
                p.BirthAddress = ba;

            if (wa != null)
                p.WorkAddress = wa;

            if (ra != null)
            p.ResidentialAddress = ra;
            
            if (pa != null)
                p.PostalAddress = pa;
            return p;
        }, new { Id = id }, splitOn: "Id")).AsList();

        if (result != null && result.Any())
        {
            string sqlContactPhone = $"SELECT * FROM {ContactPhone.MsSqlTable} cp WHERE cp.IsDeleted=0 AND cp.LinkedObjectId=@LinkedObjectId AND cp.LinkedObjectType=@LinkedObjectType";
            var param = new { LinkedObjectId = id, LinkedObjectType = new DbString { Value = typeof(Person).Name, IsAnsi = true } };

            List<ContactPhone> contactPhones = (await cn.QueryAsync<ContactPhone>(sqlContactPhone, param)).AsList();
            result[0].ContactPhones = contactPhones;
            return result[0];
        }

        return null;
    }

    public async Task<int> InsertFullAsync(Person p)
    {
        using var cn = DbContext.DbCxn;

        // <!IMPORTANT> Connection required to be open before calling BeginTransaction() function
        if (cn.State != ConnectionState.Open) cn.Open();

        DateTime khTimestamp = DateTime.UtcNow.AddHours(7);

        using var tran = cn.BeginTransaction();

        try
        {
            p.CreatedDateTime = khTimestamp;
            p.ModifiedDateTime = khTimestamp;

            int objId = await cn.InsertAsync(p, tran);

            if (objId <= 0)
                throw new Exception("Failed to insert record into database.");

            p.Id = objId;

            if (p.ContactPhones.Any())
            {
                
                foreach (ContactPhone cp in p.ContactPhones)
                {
                    cp.LinkedObjectId = objId;
                    cp.LinkedObjectType = p.GetType().Name;
                    cp.CreatedUser = p.CreatedUser;
                    cp.CreatedDateTime = p.CreatedDateTime;
                    cp.ModifiedUser = p.ModifiedUser;
                    cp.ModifiedDateTime = p.ModifiedDateTime;
                    int contactPhoneId = await cn.InsertAsync(cp, tran);
                }
            }

            if (p.BirthAddress != null)
            {
                p.BirthAddress.LinkedObjectId = objId;
                p.BirthAddress.LinkedObjectType = p.GetType().Name;
                p.BirthAddress.CreatedUser = p.CreatedUser;
                p.BirthAddress.CreatedDateTime = p.CreatedDateTime;
                p.BirthAddress.ModifiedUser = p.ModifiedUser;
                p.BirthAddress.ModifiedDateTime = p.ModifiedDateTime;
				int birthAddressId = await cn.InsertAsync(p.BirthAddress, tran);
                p.BirthAddrId = birthAddressId;
            }

            if (p.PostalAddress != null)
            {
                p.PostalAddress.LinkedObjectId = objId;
                p.PostalAddress.LinkedObjectType = p.GetType().Name;
                p.PostalAddress.CreatedUser = p.CreatedUser;
                p.PostalAddress.CreatedDateTime = p.CreatedDateTime;
                p.PostalAddress.ModifiedUser = p.ModifiedUser;
                p.PostalAddress.ModifiedDateTime = p.ModifiedDateTime;
                int postalAddressId = await cn.InsertAsync(p.PostalAddress, tran);
                p.PostalAddrId = postalAddressId;
            }

            if (p.ResidentialAddress != null)
            {
                p.ResidentialAddress.LinkedObjectId = objId;
                p.ResidentialAddress.LinkedObjectType = p.GetType().Name;
                p.ResidentialAddress.CreatedUser = p.CreatedUser;
                p.ResidentialAddress.CreatedDateTime = p.CreatedDateTime;
                p.ResidentialAddress.ModifiedUser = p.ModifiedUser;
                p.ResidentialAddress.ModifiedDateTime = p.ModifiedDateTime;
                int residentialAddressId = await cn.InsertAsync(p.ResidentialAddress, tran);
                p.ResAddrId = residentialAddressId;
            }

            if (p.WorkAddress != null)
            {
                p.WorkAddress.LinkedObjectId = objId;
                p.WorkAddress.LinkedObjectType = p.GetType().Name;
                p.WorkAddress.CreatedUser = p.CreatedUser;
                p.WorkAddress.CreatedDateTime = p.CreatedDateTime;
                p.WorkAddress.ModifiedUser = p.ModifiedUser;
                p.WorkAddress.ModifiedDateTime = p.ModifiedDateTime;
                int workAddressId = await cn.InsertAsync(p.WorkAddress, tran);
                p.WorkAddrId = workAddressId;
            }

			await cn.UpdateAsync(p, tran);

            tran.Commit();
            return objId;
        }
        catch
        {
            tran.Rollback();
            throw;
        }
    }
    public async Task<bool> UpdateFullAsync(Person p)
    {
        using var cn = DbContext.DbCxn;

        // <!IMPORTANT> Connection required to be open before calling BeginTransaction() function
        if (cn.State != ConnectionState.Open) cn.Open();

        DateTime khTimestamp = DateTime.UtcNow.AddHours(7);

        using var tran = cn.BeginTransaction();

        try
        {
            p.ModifiedDateTime = khTimestamp;

            if (p.ContactPhones.Count != 0)
            {
                foreach (ContactPhone cp in p.ContactPhones)
                {
                    if (cp.Id == 0)
                    {
                        cp.LinkedObjectId = p.Id;
                        cp.LinkedObjectType = p.GetType().Name;
                        cp.CreatedUser = p.ModifiedUser;
                        cp.CreatedDateTime = p.ModifiedDateTime;
                        cp.ModifiedUser = p.ModifiedUser;
                        cp.ModifiedDateTime = p.ModifiedDateTime;
                        int contactPhoneId = await cn.InsertAsync(cp, tran);
                    }
                    else
                    {
                        cp.ModifiedUser = p.ModifiedUser;
                        cp.ModifiedDateTime = p.ModifiedDateTime;
                        bool isContactPhoneUpdated = await cn.UpdateAsync(cp, tran);
                    }
                }
            }

            if (p.BirthAddress != null)
            {
                if (p.BirthAddress.Id <= 0)
                {
                    p.BirthAddress.LinkedObjectId = p.Id;
                    p.BirthAddress.LinkedObjectType = p.GetType().Name;
                    p.BirthAddress.CreatedUser = p.ModifiedUser;
                    p.BirthAddress.CreatedDateTime = p.ModifiedDateTime;
                    p.BirthAddress.ModifiedUser = p.ModifiedUser;
                    p.BirthAddress.ModifiedDateTime = p.ModifiedDateTime;
                    int birthAddressId = await cn.InsertAsync(p.BirthAddress, tran);
                    p.BirthAddrId = birthAddressId;
                }
                else
                {
                    p.BirthAddress.ModifiedUser = p.ModifiedUser;
                    p.BirthAddress.ModifiedDateTime = p.ModifiedDateTime;
                    bool isBirthAddressUpdated = await cn.UpdateAsync(p.BirthAddress, tran);
                }
            }

            if (p.PostalAddress != null)
            {
                if (p.PostalAddress.Id <= 0)
                {
                    p.PostalAddress.LinkedObjectId = p.Id;
                    p.PostalAddress.LinkedObjectType = p.GetType().Name;
                    p.PostalAddress.CreatedUser = p.ModifiedUser;
                    p.PostalAddress.CreatedDateTime = p.ModifiedDateTime;
                    p.PostalAddress.ModifiedUser = p.ModifiedUser;
                    p.PostalAddress.ModifiedDateTime = p.ModifiedDateTime;
                    int postalAddressId = await cn.InsertAsync(p.PostalAddress, tran);
                    p.PostalAddrId = postalAddressId;
                }
                else
                {
                    p.PostalAddress.ModifiedUser = p.ModifiedUser;
                    p.PostalAddress.ModifiedDateTime = p.ModifiedDateTime;
                    bool isPostalAddressUpdated = await cn.UpdateAsync(p.PostalAddress, tran);
                }
            }

            if (p.ResidentialAddress != null)
            {
                if (p.ResidentialAddress.Id <= 0)
                {
                    p.ResidentialAddress.LinkedObjectId = p.Id;
                    p.ResidentialAddress.LinkedObjectType = p.GetType().Name;
                    p.ResidentialAddress.CreatedUser = p.ModifiedUser;
                    p.ResidentialAddress.CreatedDateTime = p.ModifiedDateTime;
                    p.ResidentialAddress.ModifiedUser = p.ModifiedUser;
                    p.ResidentialAddress.ModifiedDateTime = p.ModifiedDateTime;
                    int residentialAddressId = await cn.InsertAsync(p.ResidentialAddress, tran);
                    p.ResAddrId = residentialAddressId;
                }
                else
                {
                    p.ResidentialAddress.ModifiedUser = p.ModifiedUser;
                    p.ResidentialAddress.ModifiedDateTime = p.ModifiedDateTime;
                    bool isResidentialAddressUpdated = await cn.UpdateAsync(p.ResidentialAddress, tran);
                }
            }

            if (p.WorkAddress != null)
            {
                if (p.WorkAddress.Id <= 0)
                {
                    p.WorkAddress.LinkedObjectId = p.Id;
                    p.WorkAddress.LinkedObjectType = p.GetType().Name;
                    p.WorkAddress.CreatedUser = p.ModifiedUser;
                    p.WorkAddress.CreatedDateTime = p.ModifiedDateTime;
                    p.WorkAddress.ModifiedUser = p.ModifiedUser;
                    p.WorkAddress.ModifiedDateTime = p.ModifiedDateTime;
                    int workAddressId = await cn.InsertAsync(p.WorkAddress, tran);
                    p.WorkAddrId = workAddressId;
                }
                else
                {
                    p.WorkAddress.ModifiedUser = p.ModifiedUser;
                    p.WorkAddress.ModifiedDateTime = p.ModifiedDateTime;
                    bool isWorkAddressUpdated = await cn.UpdateAsync(p.WorkAddress, tran);
                }
            }

            bool isUpdated = await cn.UpdateAsync(p, tran);

            tran.Commit();
            return isUpdated;
        }
        catch
        {
            tran.Rollback();
            throw;
        }
    }



    public async Task<bool> IsExistSamePersonAsync(int objId, string surname, string givenName, string gender, DateTime birthDate)
    {
        SqlBuilder sbSql = new();

        sbSql.Where("t.IsDeleted=0")
            .Where("t.Surname=@Surname")
            .Where("t.GivenName=@GivenName")
            .Where("t.Gender=@Gender")
            .Where("t.BirthDate=@BirthDate")
            .Where("t.Id<>@Id");

        DynamicParameters param = new();
        param.Add("@Surname", surname, DbType.AnsiString);
        param.Add("@GivenName", givenName, DbType.AnsiString);
        param.Add("@Gender", gender, DbType.AnsiString);
        param.Add("@BirthDate", birthDate);
        param.Add("@Id", objId);

        using var cn = DbContext.DbCxn;
        string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
        int count = await cn.ExecuteScalarAsync<int>(sql, param);

        return count>0;
    }
    public async Task<bool> IsExistSamePersonKhAsync(int objId, string surnameKh, string givenNameKh, string gender, DateTime birthDate)
    {
        SqlBuilder sbSql = new();

        sbSql.Where("t.IsDeleted=0")
            .Where("t.SurnameKh=@SurnameKh")
            .Where("t.GivenNameKh=@GivenNameKh")
            .Where("t.Gender=@Gender")
            .Where("t.BirthDate=@BirthDate")
            .Where("t.Id<>@Id");

        DynamicParameters param = new();
        param.Add("@SurnameKh", surnameKh, DbType.AnsiString);
        param.Add("@GivenNameKh", givenNameKh, DbType.AnsiString);
        param.Add("@Gender", gender, DbType.AnsiString);
        param.Add("@BirthDate", birthDate);
        param.Add("@Id", objId);

        using var cn = DbContext.DbCxn;
        string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
        int count = await cn.ExecuteScalarAsync<int>(sql, param);

        return count > 0;
    }

    public async Task<int> CountExistingNationalIDAsync(int objId, string idNum, string nationalityCountryCode)
    {
        string fieldNationalIdNum = nameof(Person.NationalIdNum);
        string fieldNatlCtyCode = nameof(Person.NatlCtyCode);
        string sql = $"SELECT COUNT(*) FROM {DbObject.MsSqlTable} WHERE IsDeleted=0 AND Id<>@Id AND {fieldNationalIdNum}=@{fieldNationalIdNum} AND {fieldNatlCtyCode}=@{fieldNatlCtyCode}";
        DynamicParameters param = new();
        param.Add($"@{nameof(Person.Id)}", objId);
        param.Add($"@{fieldNationalIdNum}", idNum, DbType.AnsiString);
        param.Add($"@{fieldNatlCtyCode}", nationalityCountryCode, DbType.AnsiString);

        using var cn = DbContext.DbCxn;

        int count = await cn.ExecuteScalarAsync<int>(sql, param);

        return count;
    }

    public async Task<List<Person>> FindAsync(
        int? objId,
        string? surname,
        string? givenName,
        string? surnameKh,
        string? givenNameKh,
        string? gender,
        DateTime? birthDate,
        string? nationalIdNum,
        string? passportIdNum,
        bool? showCustomerOrNonCustomer = null)
    {
        DynamicParameters param = new();
        SqlBuilder sbSql = new();

        sbSql.Where("t.IsDeleted=0");

        if (objId != null)
        {
            sbSql.Where("t.Id<>@ObjId");
            param.Add("@ObjId", objId!.Value);
        }

        if (showCustomerOrNonCustomer.HasValue)
        {
            if (showCustomerOrNonCustomer!.Value)
                sbSql.Where("t.CustomerId IS NOT NULL");
            else
                sbSql.Where("t.CustomerId IS NULL");
        }

        StringBuilder sbNameEn = new();
        StringBuilder sbNameKh = new();

        if (!string.IsNullOrEmpty(surname))
        {
            sbNameEn.Append("t.Surname=@Surname");
            param.Add("@Surname", surname, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(givenName))
        {
            sbNameEn.Append(sbNameEn.Length > 0 ? " AND t.GivenName=@GivenName" : "t.GivenName=@GivenName");
            param.Add("@GivenName", givenName, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(surnameKh))
        {
            sbNameKh.Append("t.SurnameKh=@SurnameKh");
            param.Add("@SurnameKh", surnameKh);
        }

        if (!string.IsNullOrEmpty(givenNameKh))
        {
            sbNameKh.Append(sbNameEn.Length > 0 ? " AND t.GivenNameKh=@GivenNameKh" : "t.GivenNameKh=@GivenNameKh");
            param.Add("@GivenNameKh", givenNameKh, DbType.AnsiString);
        }

        StringBuilder sbSearchCondition = new();


        if (sbNameEn.Length > 0)
            sbSearchCondition.Append($"(({sbNameEn})");

        if (sbNameKh.Length > 0)
            sbSearchCondition.Append(sbSearchCondition.Length > 0 ? $" OR ({sbNameKh})" : $"(({sbNameKh})");

        if (birthDate != null)
        {
            if (sbNameEn.Length > 0)
                sbNameEn.Append(" AND t.BirthDate=@BirthDate");

            if (sbNameKh.Length > 0)
				sbNameKh.Append(" AND t.BirthDate=@BirthDate");

            param.Add("@BirthDate", birthDate);
        }

        if (!string.IsNullOrEmpty(gender))
        {
            if (sbNameEn.Length > 0)
                sbNameEn.Append(" AND t.Gender=@Gender");

            if (sbNameKh.Length > 0)
				sbNameKh.Append(" AND t.Gender=@Gender");

            param.Add("@Gender", gender, DbType.AnsiString);
        }

        if (sbNameEn.Length > 0)
            sbSearchCondition.Append(sbSearchCondition.Length > 0 ? $" OR ({sbNameEn})" : $"(({sbNameEn})");

        if (sbNameKh.Length > 0)
            sbSearchCondition.Append(sbSearchCondition.Length > 0 ? $" OR ({sbNameKh})" : $"(({sbNameKh})");

        if (!string.IsNullOrEmpty(nationalIdNum))
        {
            sbSearchCondition.Append(sbSearchCondition.Length > 0 ? $" OR t.NationalIdNum=@NationalIdNum" : $"(t.NationalIdNum=@NationalIdNum");
            param.Add("@NationalIdNum", nationalIdNum, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(passportIdNum))
        {
            sbSearchCondition.Append(sbSearchCondition.Length > 0 ? $" OR t.PassportNo=@PassportNo" : $"(t.PassportNo=@PassportNo");
            param.Add("@PassportNo", passportIdNum, DbType.AnsiString);
        }

        if (sbSearchCondition.Length > 0)
        {
            sbSearchCondition.Append(')');
            sbSql.Where(sbSearchCondition.ToString());
        }

        sbSql.LeftJoin($"{Country.MsSqlTable} nty ON nty.IsDeleted=0 AND nty.ObjectCode=t.NationalityCountryCode");

        using var cn = DbContext.DbCxn;

        string sql = sbSql.AddTemplate($"SELECT TOP 100 * FROM {Person.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;

        var dataList = (await cn.QueryAsync<Person, Country, Person>(sql, (p, cty) =>
        {
            p.Nationality = cty;
            return p;
        }, param, splitOn: "Id")).AsList();

        return dataList;
    }

    public async Task<List<Person>> QuickSearchAsync(int pgSize = 0,
        int pgNo = 0,
        string? searchText = null,
        bool? showCustomerOrNonCustomer = null,
        Dictionary<string, bool>? fieldRequiredToHaveValues = null,
        List<int>? excludeIdList = null)
    {
        if (pgNo < 0 && pgSize < 0)
            throw new ArgumentOutOfRangeException(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        DynamicParameters param = new();
        SqlBuilder sbSql = new();
        sbSql.Where("t.IsDeleted=0");

        #region Form Search Conditions
        Regex engNamePattern = new(@"^[a-zA-Z\s.-]{1,}$");

        if (!string.IsNullOrEmpty(searchText))
        {
            if (engNamePattern.IsMatch(searchText))
            {
                sbSql.Where("(UPPER(t.ObjectName) LIKE '%'+UPPER(@SearchText)+'%')");
                param.Add("@SearchText", searchText, DbType.AnsiString);
            }
            else
            {
                sbSql.Where("t.ObjectNameKh LIKE '%'+@SearchText+'%'");
                param.Add("@SearchText", searchText);
            }
        }

        if (showCustomerOrNonCustomer is not null)
        {
            if (showCustomerOrNonCustomer!.Value)
            {
                sbSql.Where("t.CustomerId IS NOT NULL");
                sbSql.Where("t.CustomerObjectCode IS NOT NULL");
            }
            else
            {
                sbSql.Where("t.CustomerId IS NULL");
            }
        }

        if (excludeIdList != null && excludeIdList.Count > 0)
        {
            sbSql.Where("t.Id NOT IN @ExcludeIdList");
            param.Add("@ExcludeIdList", excludeIdList);
        }

        if (fieldRequiredToHaveValues != null && fieldRequiredToHaveValues.Keys.Count > 0)
        {
            foreach (string key in fieldRequiredToHaveValues.Keys)
            {
                switch (key)
                {
                    case "Gender":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("t.Gender IS NOT NULL");
                        break;

                    case "BirthDate":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("t.BirthDate IS NOT NULL");
                        break;

                    case "NationalIdNum":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("LEN(ISNULL(t.NationalIdNum,''))>0");
                        break;

                    case "PassportNo":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("LEN(ISNULL(t.PassportNo,''))>0");
                        break;

                    case "PhoneLine1":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("LEN(ISNULL(t.PhoneLine1,''))>0");
                        break;

                    case "PhoneLine2":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("LEN(ISNULL(t.PhoneLine2,''))>0");
                        break;

                    case "NationalityCountryCode":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("t.NationalityCountryCode IS NOT NULL");
                        break;
                    default: break;
                }
            }
        }
        #endregion

        sbSql.LeftJoin($"{Country.MsSqlTable} nty ON nty.IsDeleted=0 AND nty.ObjectCode=t.NationalityCountryCode");

        sbSql.OrderBy("t.Surname ASC")
            .OrderBy("t.GivenName ASC")
            .OrderBy("t.SurnameKh ASC")
            .OrderBy("t.GivenNameKh ASC");

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

        List<Person> dataList = (await cn.QueryAsync<Person, Country, Person>(sql, (obj, nationality) =>
        {
            obj.Nationality = nationality;
            return obj;
        }, param, splitOn: "Id")).AsList();

        return dataList;
    }

    public async Task<DataPagination> GetQuickSearchPaginationAsync(
        int pgSize = 0,
        string? searchText = null,
        bool? showCustomerOrNonCustomer = null,
        Dictionary<string, bool>? fieldRequiredToHaveValues = null,
        List<int>? excludeIdList = null)
    {
        if (pgSize < 0)
            throw new ArgumentOutOfRangeException(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        DynamicParameters param = new();
        SqlBuilder sbSql = new();
        sbSql.Where("t.IsDeleted=0");

        #region Form Search Conditions
        Regex engNamePattern = new(@"^[a-zA-Z\s]{1,}$");

        if (!string.IsNullOrEmpty(searchText))
        {
            if (engNamePattern.IsMatch(searchText))
            {
                sbSql.Where("(UPPER(t.GivenName) + ' ' + UPPER(t.Surname) LIKE '%'+UPPER(@SearchText)+'%')");
                param.Add("@SearchText", searchText, DbType.AnsiString);
            }
            else
            {
                sbSql.Where("t.SurnameKh + ' ' + t.GivenNameKh LIKE '%'+@SearchText+'%'");
                param.Add("@SearchText", searchText);
            }
        }

        if (excludeIdList != null && excludeIdList.Count > 0)
        {
            sbSql.Where("t.Id NOT IN @ExcludeIdList");
            param.Add("@ExcludeIdList", excludeIdList);
        }

        if (fieldRequiredToHaveValues != null && fieldRequiredToHaveValues.Keys.Count > 0)
        {
            foreach (string key in fieldRequiredToHaveValues.Keys)
            {
                switch (key)
                {
                    case "Gender":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("t.Gender IS NOT NULL");
                        break;

                    case "BirthDate":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("t.BirthDate IS NOT NULL");
                        break;

                    case "NationalIdNum":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("LEN(ISNULL(t.NationalIdNum,''))>0");
                        break;

                    case "PassportNo":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("LEN(ISNULL(t.PassportNo,''))>0");
                        break;

                    case "PhoneLine1":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("LEN(ISNULL(t.PhoneLine1,''))>0");
                        break;

                    case "PhoneLine2":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("LEN(ISNULL(t.PhoneLine2,''))>0");
                        break;

                    case "NationalityCountryCode":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("t.NationalityCountryCode IS NOT NULL");
                        break;
                    default: break;
                }
            }
        }
        #endregion

        string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;

        using var cn = DbContext.DbCxn;

        decimal recordCount = await cn.ExecuteScalarAsync<int>(sql, param);
        int pageCount = (int)Math.Ceiling(recordCount / (pgSize == 0 ? 1 : pgSize));

        DataPagination pagination = new()
        {
            ObjectType = typeof(Person).Name,
            PageSize = pgSize,
            PageCount = pageCount,
            RecordCount = (int)recordCount
        };

        return pagination;
    }

	public override async Task<KeyValuePair<int, IEnumerable<Person>>> SearchNewAsync(
		int pgSize = 0, int pgNo = 0, string? searchText = null,
		IEnumerable<SqlSortCond>? sortConds = null,
		IEnumerable<SqlFilterCond>? filterConds = null,
		List<int>? excludeIdList = null)
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

        sbSql.LeftJoin($"{Country.MsSqlTable} natl ON natl.IsDeleted=0 AND natl.ObjectCode=t.NatlCtyCode");

		foreach (string order in GetSearchOrderbBy())
        {
            sbSql.OrderBy(order);
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

		var dataList = await cn.QueryAsync<Person, Country, Person>(sql, 
            (obj, natl) => {
                obj.Nationality = natl;
                return obj;
            }, param, splitOn: "Id");

		string sqlCount = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
		int dataCount = await cn.ExecuteScalarAsync<int>(sqlCount, param);
		return new(dataCount, dataList);
	}

	public override async Task<List<Person>> QuickSearchAsync(int pgSize = 0, int pgNo = 0, string? searchText = null, List<int>? excludeIdList = null)
    {
        if (pgNo < 0 && pgSize < 0)
            throw new ArgumentOutOfRangeException(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        DynamicParameters param = new();
        SqlBuilder sbSql = new();
        sbSql.Where("t.IsDeleted=0");

        #region Form Search Conditions
        Regex engNamePattern = new(@"^[a-zA-Z\s.-]{1,}$");

        if (!string.IsNullOrEmpty(searchText))
        {
            if (engNamePattern.IsMatch(searchText))
            {
                sbSql.Where("(UPPER(t.ObjectName) LIKE '%'+UPPER(@SearchText)+'%')");
                param.Add("@SearchText", searchText, DbType.AnsiString);
            }
            else
            {
                sbSql.Where("t.ObjectNameKh LIKE '%'+@SearchText+'%'");
                param.Add("@SearchText", searchText);
            }
        }

        if (excludeIdList != null && excludeIdList.Count > 0)
        {
            sbSql.Where("t.Id NOT IN @ExcludeIdList");
            param.Add("@ExcludeIdList", excludeIdList);
        }
        #endregion

        sbSql.LeftJoin($"{Country.MsSqlTable} nty ON nty.IsDeleted=0 AND nty.ObjectCode=t.NationalityCountryCode");

        sbSql.OrderBy("t.Surname ASC, t.GivenName ASC, t.SurnameKh ASC, t.GivenNameKh ASC");

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
                $";WITH pg AS (SELECT Id FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
                $"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ WHERE t.Id IN (SELECT Id FROM pg) /**orderby**/").RawSql;
        }

        using var cn = DbContext.DbCxn;

        List<Person> dataList = (await cn.QueryAsync<Person, Country, Person>(sql, (obj, nationality) =>
        {
            obj.Nationality = nationality;
            return obj;
        }, param, splitOn: "Id")).AsList();

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
        Regex engNamePattern = new(@"^[a-zA-Z\s]{1,}$");

        if (!string.IsNullOrEmpty(searchText))
        {
            if (engNamePattern.IsMatch(searchText))
            {
                sbSql.Where("(UPPER(t.GivenName) + ' ' + UPPER(t.Surname) LIKE '%'+UPPER(@SearchText)+'%')");
                param.Add("@SearchText", searchText, DbType.AnsiString);
            }
            else
            {
                sbSql.Where("t.SurnameKh + ' ' + t.GivenNameKh LIKE '%'+@SearchText+'%'");
                param.Add("@SearchText", searchText);
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
            ObjectType = typeof(Person).Name,
            PageSize = pgSize,
            PageCount = pageCount,
            RecordCount = (int)recordCount
        };

        return pagination;
    }

    public async Task<List<Person>> SearchAsync(
        int pgSize = 0,
        int pgNo = 0,
        string? objectCode = null,
        string? objectName = null,
        string? nameEn = null,
        string? nameKh = null,
		List<string>? genderList = null,
		List<string>? maritalStatusList = null,
		DateTime? birthDateFrom = null,
		DateTime? birthDateTo = null,
		string? nationalIdNum = null,
        string? passportNo = null,
        string? email = null,
        string? phoneNum = null,
		bool? showCustomerOrNonCustomer = null,
		string? customerId = null,
		Dictionary<string, bool>? fieldRequiredToHaveValues = null,
        List<int>? excludeIdList = null)
    {
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

        if (!string.IsNullOrEmpty(nameEn))
        {
            sbSql.Where("UPPER(ISNULL(t.Surname,'')+' '+ISNULL(t.GivenName,''))) LIKE '%'+UPPER(@NameEn)+'%'");
            param.Add("@NameEn", nameEn, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(nameKh))
        {
            sbSql.Where("(ISNULL(t.SurnameKh,'') +' '+ ISNULL(t.GivenNameKh,'')) LIKE '%'+@NameKh+'%'");
            param.Add("@NameKh", nameKh);
        }

        if (genderList != null && genderList.Any())
        {
            if (genderList.Count == 1)
            {
                sbSql.Where("t.Gender=@Gender");
                param.Add("@Gender", genderList[0], DbType.AnsiString);
            }
            else
            {
                sbSql.Where("t.Gender IN @GenderList");
                param.Add("@GenderList", genderList);
            }
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

        if (birthDateFrom != null)
        {
            sbSql.Where("t.BirthDate IS NOT NULL");
			sbSql.Where("t.BirthDate>=@BirthDateFrom");
            param.Add("@BirthDateFrom", birthDateFrom!.Value);

            if (birthDateTo.HasValue)
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

        if (!string.IsNullOrEmpty(passportNo))
        {
            sbSql.Where("t.PassportNo LIKE '%'+@PassportNo+'%'");
            param.Add("@PassportNo", passportNo, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(email))
        {
            sbSql.Where("(LOWER(t.PersonalEmail) LIKE '%'+@Email+'%' OR LOWER(t.WorkEmail) LIKE '%'+@Email+'%')");
            param.Add("@Email", email.ToLower(), DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(phoneNum))
        {
            sbSql.Where("(t.PhoneLine1 LIKE '%'+@PhoneNum+'%' OR t.PhoneLine2 LIKE '%'+@PhoneNum+'%')");
            param.Add("@PhoneNum", phoneNum, DbType.AnsiString);
        }

		if (showCustomerOrNonCustomer.HasValue)
		{
			if (showCustomerOrNonCustomer!.Value)
			{
				sbSql.Where("t.CustomerId IS NOT NULL");
				sbSql.Where("t.CustomerObjectCode IS NOT NULL");

				if (!string.IsNullOrEmpty(customerId))
				{
					sbSql.Where("t.CustomerObjectCode LIKE '%'+@CustomerObjectCode+'%'");
					param.Add("@CustomerObjectCode", customerId!, DbType.AnsiString);
				}
			}
			else
			{
				sbSql.Where("t.CustomerId IS NULL");
			}
		}

		if (excludeIdList != null && excludeIdList.Count > 0)
        {
            sbSql.Where("t.Id NOT IN @ExcludeIdList");
            param.Add("@ExcludeIdList", excludeIdList);
        }

        if (fieldRequiredToHaveValues != null && fieldRequiredToHaveValues.Keys.Count > 0)
        {
            foreach (string key in fieldRequiredToHaveValues.Keys)
            {
                switch (key)
                {
                    case "Gender":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("t.Gender IS NOT NULL");
                        break;

                    case "BirthDate":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("t.BirthDate IS NOT NULL");
                        break;

                    case "NationalIdNum":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("LEN(ISNULL(t.NationalIdNum,''))>0");
                        break;

                    case "PassportNo":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("LEN(ISNULL(t.PassportNo,''))>0");
                        break;

                    case "PhoneLine1":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("LEN(ISNULL(t.PhoneLine1,''))>0");
                        break;

                    case "PhoneLine2":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("LEN(ISNULL(t.PhoneLine2,''))>0");
                        break;

                    case "NationalityCountryCode":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("t.NationalityCountryCode IS NOT NULL");
                        break;
                    default: break;
                }
            }
        }
        #endregion

        sbSql
            .OrderBy("t.Surname ASC")
            .OrderBy("t.GivenName ASC")
            .OrderBy("t.SurnameKh ASC")
            .OrderBy("t.GivenNameKh ASC");

        sbSql.LeftJoin($"{Country.MsSqlTable} nty ON nty.IsDeleted=0 AND nty.ObjectCode=t.NationalityCountryCode");

        string sql;

        if (pgNo == 0 && pgSize == 0)
        {
            sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/ /**orderby**/").RawSql;
        }
        else
        {
            param.Add("@PageSize", pgSize);
            param.Add("@PageNo", pgNo <= 0 ? 1 : pgNo);

            sql = sbSql.AddTemplate($";WITH pg AS (SELECT t.Id FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
                                    $"SELECT t.*, nty.* FROM {DbObject.MsSqlTable} t INNER JOIN pg b ON b.Id=t.Id /**leftjoin**/ /**orderby**/").RawSql;
        }

        using var cn = DbContext.DbCxn;

        var dataList = (await cn.QueryAsync<Person, Country, Person>(sql, (obj, nationality) => {
            obj.Nationality = nationality;
            return obj;
        }, param, splitOn: "Id")).AsList();

        return dataList;
    }

    public async Task<DataPagination> GetSearchPaginationAsync(
            int pgSize = 0,
            string? objectCode = null,
            string? objectName = null,
            string? nameEn = null,
            string? nameKh = null,
			List<string>? genderList = null,
			List<string>? maritalStatusList = null,
			DateTime? birthDateFrom = null,
			DateTime? birthDateTo = null,
			string? nationalIdNum = null,
            string? passportNo = null,
            string? email = null,
            string? phoneNum = null,
			bool? showCustomerOrNonCustomer = null,
			string? customerId = null,
			Dictionary<string, bool>? fieldRequiredToHaveValues = null,
            List<int>? excludeIdList = null
        )
    {
        if (pgSize < 0)
            throw new ArgumentOutOfRangeException(nameof(pgSize), _errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

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

        if (!string.IsNullOrEmpty(nameEn))
        {
            sbSql.Where("UPPER(ISNULL(t.Surname,'')+' '+ISNULL(t.GivenName,''))) LIKE '%'+UPPER(@NameEn)+'%'");
            param.Add("@NameEn", nameEn, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(nameKh))
        {
            sbSql.Where("(ISNULL(t.SurnameKh,'') +' '+ ISNULL(t.GivenNameKh,'')) LIKE '%'+@NameKh+'%'");
            param.Add("@NameKh", nameKh);
        }

        if (genderList != null && genderList.Any())
        {
            if (genderList.Count == 1)
            {
                sbSql.Where("t.Gender=@Gender");
                param.Add("@Gender", genderList[0], DbType.AnsiString);
            }
            else
            {
                sbSql.Where("t.Gender IN @GenderList");
                param.Add("@GenderList", genderList);
            }
        }

        if (maritalStatusList != null && maritalStatusList.Count != 0)
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

		if (birthDateFrom != null)
		{
			sbSql.Where("t.BirthDate IS NOT NULL");
			sbSql.Where("t.BirthDate>=@BirthDateFrom");
			param.Add("@BirthDateFrom", birthDateFrom!.Value);

			if (birthDateTo.HasValue)
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

		if (!string.IsNullOrEmpty(passportNo))
        {
            sbSql.Where("t.PassportNo LIKE '%'+@PassportNo+'%'");
            param.Add("@PassportNo", passportNo, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(email))
        {
            sbSql.Where("(LOWER(t.PersonalEmail) LIKE '%'+@Email+'%' OR LOWER(t.WorkEmail) LIKE '%'+@Email+'%')");
            param.Add("@Email", email.ToLower(), DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(phoneNum))
        {
            sbSql.Where("(t.PhoneLine1 LIKE '%'+@PhoneNum+'%' OR t.PhoneLine2 LIKE '%'+@PhoneNum+'%')");
            param.Add("@PhoneNum", phoneNum, DbType.AnsiString);
        }

		if (showCustomerOrNonCustomer.HasValue)
		{
			if (showCustomerOrNonCustomer!.Value)
			{
				sbSql.Where("t.CustomerId IS NOT NULL");
				sbSql.Where("t.CustomerObjectCode IS NOT NULL");

				if (!string.IsNullOrEmpty(customerId))
				{
					sbSql.Where("t.CustomerObjectCode LIKE '%'+@CustomerObjectCode+'%'");
					param.Add("@CustomerObjectCode", customerId!, DbType.AnsiString);
				}
			}
			else
			{
				sbSql.Where("t.CustomerId IS NULL");
			}
		}

		if (excludeIdList != null && excludeIdList.Count > 0)
        {
            sbSql.Where("t.Id NOT IN @ExcludeIdList");
            param.Add("@ExcludeIdList", excludeIdList);
        }

        if (fieldRequiredToHaveValues != null && fieldRequiredToHaveValues.Keys.Count > 0)
        {
            foreach (string key in fieldRequiredToHaveValues.Keys)
            {
                switch (key)
                {
                    case "Gender":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("t.Gender IS NOT NULL");
                        break;

                    case "BirthDate":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("t.BirthDate IS NOT NULL");
                        break;

                    case "NationalIdNum":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("LEN(ISNULL(t.NationalIdNum,''))>0");
                        break;

                    case "PassportNo":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("LEN(ISNULL(t.PassportNo,''))>0");
                        break;

                    case "PhoneLine1":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("LEN(ISNULL(t.PhoneLine1,''))>0");
                        break;

                    case "PhoneLine2":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("LEN(ISNULL(t.PhoneLine2,''))>0");
                        break;

                    case "NationalityCountryCode":
                        if (fieldRequiredToHaveValues[key])
                            sbSql.Where("t.NationalityCountryCode IS NOT NULL");
                        break;
                    default: break;
                }
            }
        }
        #endregion

        using var cn = DbContext.DbCxn;
        string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
        decimal recordCount = await cn.ExecuteScalarAsync<int>(sql, param);
        int pageCount = pgSize == 0 ? 1 : (int)Math.Ceiling(recordCount / pgSize);

        DataPagination pagingResult = new()
        {
            ObjectType = typeof(Person).Name,
            PageSize = pgSize,
            PageCount = pageCount,
            RecordCount = (int)recordCount
        };

        return pagingResult;
    }

    public async Task<List<DropdownSelectItem>> GetForDropdownAsync(string? searchText = null)
    {
        DynamicParameters param = new();
        SqlBuilder sbSql = new();

        sbSql
            .Select("t.Id")
            .Select("'Key'=t.ObjectCode")
            .Select("'Key'=t.ObjectCode")
            .Select("'Value'=(CASE WHEN LEN(ISNULL(t.GivenName,''))> 0 THEN (t.GivenName + ' ' + t.Surname + ' (' + CASE t.Gender WHEN 'M' then N'Male' WHEN 'F' THEN N'Female' ELSE '-' END + ')') ELSE '' END)"+
                "+(CASE WHEN LEN(ISNULL(t.SurnameKh,''))> 0 OR LEN(ISNULL(t.GivenNameKh,''))> 0 THEN '  /  ' + ISNULL(t.SurnameKh, '') + ' ' + ISNULL(t.GivenNameKh, '') + ' (' + CASE t.Gender WHEN 'M' then N'ប្រុស' WHEN 'F' THEN N'ស្រី' ELSE '-' END + ')' ELSE '' END) COLLATE SQL_Latin1_General_CP1_CI_AS");
        sbSql.Where("t.IsDeleted=0");

        if (!string.IsNullOrEmpty(searchText))
        {
            Regex engNamePattern = new(@"^[a-zA-Z\s]{1,}$");

            if (engNamePattern.IsMatch(searchText))
            {
                sbSql.Where("(UPPER(ISNULL(t.GivenName,'')) + ' ' + UPPER(ISNULL(t.Surname,'')) LIKE '%'+UPPER(@SearchText)+'%')");
                param.Add("@SearchText", searchText, DbType.AnsiString);
            }
            else
            {
                sbSql.Where("ISNULL(t.SurnameKh,'') + ' ' + ISNULL(t.GivenNameKh,'') LIKE '%'+@SearchText+'%'");
                param.Add("@SearchText", searchText);
            }
        }

        using var cn = DbContext.DbCxn;
        string sql = sbSql.AddTemplate($"SELECT /**select**/ FROM {DbObject.MsSqlTable} t WHERE t.IsDeleted = 0").RawSql;
        List<DropdownSelectItem> result = (await cn.QueryAsync<DropdownSelectItem>(sql, param)).AsList();

        return result;
    }

    public async Task<List<DropdownSelectItem>> GetForDropdownEnAsync(string? searchText = null)
    {
        DynamicParameters param = new();
        SqlBuilder sbSql = new();

        sbSql
            .Select("t.Id")
            .Select("'Key'=t.ObjectCode")
            .Select("'Value'=t.GivenName + ' ' + t.Surname + ' ('+ISNULL(t.Gender,'-')+')'");

        sbSql
            .Where("t.IsDeleted=0")
            .Where("t.Surname IS NOT NULL")
            .Where("t.GivenName IS NOT NULL");

        if (!string.IsNullOrEmpty(searchText))
        {
            sbSql.Where("UPPER(t.GivenName+' '+t.Surname) LIKE '%'+UPPER(@SearchText)+'%'");
            param.Add("@SearchText", searchText, DbType.AnsiString);
        }

        using var cn = DbContext.DbCxn;
        string sql = sbSql.AddTemplate($"SELECT /**select**/ FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
        List<DropdownSelectItem> result = (await cn.QueryAsync<DropdownSelectItem>(sql, param)).AsList();

        return result;
    }
    public async Task<List<DropdownSelectItem>> GetForDropdownKhAsync(string? searchText = null)
    {
        DynamicParameters param = new();
        SqlBuilder sbSql = new();

        sbSql
            .Select("t.Id")
            .Select("'Key'=t.ObjectCode")
            .Select("'Value'=t.SurnameKh + ' ' + t.GivenNameKh + ' ('+CASE WHEN t.Gender='M' THEN N'ប្រុស' WHEN t.Gender='F' THEN N'ស្រី' ELSE N'-' END+')'");

        sbSql
            .Where("t.IsDeleted=0")
            .Where("t.Surname IS NOT NULL")
            .Where("t.GivenName IS NOT NULL");

        if (!string.IsNullOrEmpty(searchText))
        {
            sbSql.Where("UPPER(t.GivenName+' '+t.Surname) LIKE '%'+UPPER(@SearchText)+'%'");
            param.Add("@SearchText", searchText, DbType.AnsiString);
        }

        using var cn = DbContext.DbCxn;
        string sql = sbSql.AddTemplate($"SELECT /**select**/ FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
        var dataList = (await cn.QueryAsync<DropdownSelectItem>(sql, param)).AsList();
        return dataList;
    }

	public override List<string> GetSearchOrderbBy()
	{
        return ["ISNULL(t.ObjectName,'ZZZZZ') ASC"];
	}
}