using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Repos.SystemCore;

public interface IAddressRepos : IBaseRepos<Address>
{
	Task<List<Address>> GetByLinkedObjectAsync(int linkedObjectId, string linkedObejctType);
	Task<List<Address>> GetByLinkedObjectAsync(string linkedRecordID, string linkedObejctType);
	Task<Address?> GetByGeoLocationAsync(double latitude, double longitue);

	Task<List<Address>> SearchAsync(
			int pgSize = 0, int pgNo = 0,
			string? objectCode = null,
			string? objectName = null,
			string? type = null,
			string? city = null,
			string? district = null,
			string? subDistrict = null,
			string? zipCode = null,
			string? countryCode = null,
			string? landmark = null);

	Task<DataPagination> GetSearchPaginationAsync(
			int pgSize = 0,
			string? objectCode = null,
			string? objectName = null,
			string? type = null,
			string? city = null,
			string? district = null,
			string? subDistrict = null,
			string? zipCode = null,
			string? countryCode = null,
			string? landmark = null);
}

public class AddressRepos(IConnectionFactory connectionFactory) : BaseRepos<Address>(connectionFactory, new DatabaseObj(AuditObject.SchemaName, Address.MsSqlTableName, Address.PgTableName)), IAddressRepos
{
	public async Task<List<Address>> GetByLinkedObjectAsync(int linkedObjectId, string linkedObejctType)
    {
        SqlBuilder sbSql = new();
        DynamicParameters param = new();
        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.LinkedObjectId=@LinkedObjectId");
        sbSql.Where("t.LinkedObjectType=@LinkedObjectType");

        param.Add("@LinkedObjectId", linkedObjectId);
        param.Add("@LinkedObjectType", linkedObejctType);

        
        using var cn = ConnectionFactory.GetDbConnection()!;

        string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} /**where**/").RawSql;
        return (await cn.QueryAsync<Address>(sql, param)).AsList();
    }

    public async Task<List<Address>> GetByLinkedObjectAsync(string linkedRecordID, string linkedObejctType)
    {
        SqlBuilder sbSql = new();
        DynamicParameters param = new();
        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.LinkedRecordID=@LinkedRecordID");
        sbSql.Where("t.LinkedObjectType=@LinkedObjectType");

        var sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
        
        param.Add("@LinkedRecordID", linkedRecordID, DbType.AnsiString);
        param.Add("@LinkedObjectType", linkedObejctType, DbType.AnsiString);

        using var cn = ConnectionFactory.GetDbConnection()!;

        return (await cn.QueryAsync<Address>(sql, param)).AsList();
    }

    public async Task<Address?> GetByGeoLocationAsync(double latitude, double longitue)
    {
        SqlBuilder sbSql = new();
        DynamicParameters param = new();

        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.Latitude=@Latitude");
        sbSql.Where("t.Longitude=@Longitude");

        var sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
        param.Add("@Latitude", latitude);
        param.Add("@Longitue", longitue);

        using var cn = ConnectionFactory.GetDbConnection()!;
        
        return await cn.QuerySingleOrDefaultAsync<Address?>(sql, param).ConfigureAwait(false);
    }

    public async Task<List<Address>> SearchAsync(
            int pgSize = 0, int pgNo = 0,
            string? objectCode = null,
            string? objectName = null,
            string? type = null,
            string? city = null,
            string? district = null,
            string? subDistrict = null,
            string? zipCode = null,
            string? countryCode = null,
            string? landmark = null)
    {
        if (pgNo < 0 && pgSize < 0)
            throw new Exception(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        SqlBuilder sbSql = new();
        DynamicParameters param = new();

        sbSql.Where("t.IsDeleted=0");

        #region Form Search Conditions
        if (!string.IsNullOrEmpty(objectCode))
        {
            sbSql.Where("t.ObjectCode LIKE '%'+@ObjectCode+'%'");
            param.Add("@ObjectCode", objectCode, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(objectName))
        {
            sbSql.Where("LOWER(t.ObjectName) LIKE '%'+@ObjectName+'%'");
            param.Add("@ObjectName", objectName, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(type))
        {
            sbSql.Where("t.[Type]=@Type");
            param.Add("@Type", type);
        }

        if (!string.IsNullOrEmpty(city))
        {
            sbSql.Where("t.City=@City");
            param.Add("@City", city, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(district))
        {
            sbSql.Where("t.District=@District");
            param.Add("@District", district, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(subDistrict))
        {
            sbSql.Where("t.SubDistrict=@SubDistrict");
            param.Add("@SubDistrict", subDistrict, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(zipCode))
        {
            sbSql.Where("t.Zipcode=@Zipcode");
            param.Add("@Zipcode", zipCode, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(countryCode))
        {
            sbSql.Where("t.CountryCode=@CountryCode");
            param.Add("@CountryCode", countryCode, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(landmark))
        {
            sbSql.Where("t.Landmark LIKE '%'+@Landmark+'%'");
            param.Add("@Landmark", landmark);
        }
        #endregion

        sbSql.OrderBy("t.CountryCode ASC");
        sbSql.OrderBy("t.City ASC");
        sbSql.OrderBy("t.District ASC");
        sbSql.OrderBy("t.SubDistrict ASC");

		string sql;

		if (pgNo == 0 && pgSize == 0)
        {
            sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/").RawSql;
        }
        else
        {
            param.Add("@PageSize", pgSize);
            param.Add("@PageNo", pgNo);

            sql = sbSql.AddTemplate($";WITH pg AS (SELECT Id FROM {DbObject.MsSqlTable} /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
                  $"SELECT t.* FROM {DbObject.MsSqlTable} t INNER JOIN pg p ON p.Id=t.Id /**orderby**/").RawSql;
        }

        using var cn = ConnectionFactory.GetDbConnection()!;

        List<Address> dataList = (await cn.QueryAsync<Address>(sql, param)).AsList();

        return dataList;
    }

    public async Task<DataPagination> GetSearchPaginationAsync(
            int pgSize = 0,
            string? objectCode = null,
            string? objectName = null,
            string? type = null,
            string? city = null,
            string? district = null,
            string? subDistrict = null,
            string? zipCode = null,
            string? countryCode = null,
            string? landmark = null)
    {
        if (pgSize < 0)
            throw new Exception(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        SqlBuilder sbSql = new();
        DynamicParameters param = new();

        sbSql.Where("t.IsDeleted=0");

        #region Form Search Conditions
        if (!string.IsNullOrEmpty(objectCode))
        {
            sbSql.Where("t.ObjectCode LIKE '%'+@ObjectCode+'%'");
            param.Add("@ObjectCode", objectCode, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(objectName))
        {
            sbSql.Where("LOWER(t.ObjectName) LIKE '%'+@ObjectName+'%'");
            param.Add("@ObjectName", objectName, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(type))
        {
            sbSql.Where("t.[Type]=@Type");
            param.Add("@Type", type);
        }

        if (!string.IsNullOrEmpty(city))
        {
            sbSql.Where("t.City=@City");
            param.Add("@City", city, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(district))
        {
            sbSql.Where("t.District=@District");
            param.Add("@District", district, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(subDistrict))
        {
            sbSql.Where("t.SubDistrict=@SubDistrict");
            param.Add("@SubDistrict", subDistrict, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(zipCode))
        {
            sbSql.Where("t.Zipcode=@Zipcode");
            param.Add("@Zipcode", zipCode, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(countryCode))
        {
            sbSql.Where("t.CountryCode=@CountryCode");
            param.Add("@CountryCode", countryCode, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(landmark))
        {
            sbSql.Where("t.Landmark LIKE '%'+@Landmark+'%'");
            param.Add("@Landmark", landmark);
        }
        #endregion

        string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;

        using var cn = ConnectionFactory.GetDbConnection()!;

        decimal recordCount = await cn.ExecuteScalarAsync<int>(sql, param);
        int pageCount = pgSize == 0 ? 1 : (int)Math.Ceiling(recordCount / pgSize);

        DataPagination pagination = new()
        {
            ObjectType = typeof(CambodiaProvince).Name,
            PageSize = pgSize,
            PageCount = pageCount,
            RecordCount = (int)recordCount
        };

        return pagination;
    }
}