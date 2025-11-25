namespace DataLayer.Repos.SysCore;

public interface IPermissionRepos : IBaseRepos<Permission>
{
	Task<List<Permission>> GetValidByOrganizationAsync(int organizationId);
	Task<List<Permission>> GetByOrganizationAsync(int organizationId);
	Task<List<Permission>> SearchAsync(
		int pgSize = 0,
		int pgNo = 0,
		int? organizationId = null,
		string? objectCode = null,
		string? objectName = null,
		bool? isEnabled = null);

	Task<DataPagination> GetSearchPaginationAsync(
		int pgSize = 0,
		int? organizationId = null,
		string? objectCode = null,
		string? objectName = null,
		bool? isEnabled = null);

	Task<List<Permission>> GetByUserIdAsync(int id);
}

public class PermissionRepo(IDbContext dbContext) : BaseRepos<Permission>(dbContext, Permission.DatabaseObject), IPermissionRepos
{
	public async Task<List<Permission>> SearchAsync(
        int pgSize = 0,
        int pgNo = 0,
        int? organizationId = null,
        string? objectCode = null,
        string? objectName = null,
        bool? isEnabled = null)
    {
        if (pgNo < 0 && pgSize < 0)
            throw new Exception(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        DynamicParameters param = new();
        SqlBuilder sbSql = new();
        sbSql.Where("t.IsDeleted=0");

        #region Form Search Conditions

        //Implement lower-case convert for text-type base search to lower is to ensure searching is case-insensitive
        //since now PCLAAPP DB is a unicode enabled database, i.e. all search are case sensitive

        if (organizationId.HasValue && organizationId > 0)
        {
            sbSql.Where("t.OrganizationId=@OrganizationId");
            param.Add("@OrganizationId", organizationId);
        }

        if (!string.IsNullOrEmpty(objectCode))
        {
            sbSql.Where("LOWER(t.ObjectCode) LIKE '%'+LOWER(@ObjectCode)+'%'");
            param.Add("@ObjectCode", objectCode, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(objectName))
        {
            sbSql.Where("LOWER(t.ObjectName) LIKE '%'+LOWER(@ObjectName)+'%'");
            param.Add("@ObjectName", objectName, DbType.AnsiString);
        }

        if (isEnabled.HasValue)
        {
            sbSql.Where("t.IsEnabled=@IsEnabled");
            param.Add("@IsEnabled", isEnabled.Value);
        }
        #endregion

        sbSql.OrderBy("t.ObjectName ASC");

        string sql;

        if (pgNo == 0 && pgSize == 0)
        {
            sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/").RawSql;
        }
        else
        {
            param.Add("@PageSize", pgSize);
            param.Add("@PageNo", pgNo);

            sql = sbSql.AddTemplate($";WITH pg AS (SELECT t.Id FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
                                    $"SELECT t.* FROM {DbObject.MsSqlTable} t INNER JOIN pg p ON p.Id=t.Id /**orderby**/").RawSql;
        }

        using var cn = DbContext.DbCxn;

        return (await cn.QueryAsync<Permission>(sql, param)).AsList();
    }

    public async Task<DataPagination> GetSearchPaginationAsync(
        int pgSize = 0,
        int? organizationId = null,
        string? objectCode = null,
        string? objectName = null,
        bool? isEnabled = null)
    {
        if (pgSize < 0)
            throw new Exception(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        DynamicParameters param = new();
        SqlBuilder sbSql = new();
        sbSql.Where("t.IsDeleted=0");

        #region Form Search Conditions

        //Implement lower-case convert for text-type base search to lower is to ensure searching is case-insensitive
        //since now PCLAAPP DB is a unicode enabled database, i.e. all search are case sensitive

        if (organizationId.HasValue && organizationId > 0)
        {
            sbSql.Where("t.OrganizationId=@OrganizationId");
            param.Add("@OrganizationId", organizationId);
        }

        if (!string.IsNullOrEmpty(objectCode))
        {
            sbSql.Where("LOWER(t.ObjectCode) LIKE '%'+LOWER(@ObjectCode)+'%'");
            param.Add("@ObjectCode", objectCode, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(objectName))
        {
            sbSql.Where("LOWER(t.ObjectName) LIKE '%'+LOWER(@ObjectName)+'%'");
            param.Add("@ObjectName", objectName, DbType.AnsiString);
        }

        if (isEnabled.HasValue)
        {
            sbSql.Where("t.IsEnabled=@IsEnabled");
            param.Add("@IsEnabled", isEnabled.Value);
        }
        #endregion

        using var cn = DbContext.DbCxn;
        string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
        decimal recordCount = await cn.ExecuteScalarAsync<int>(sql, param);
        int pageCount = (int)Math.Ceiling(recordCount / (pgSize == 0 ? 1 : pgSize));

        DataPagination pagination = new()
        {
            ObjectType = typeof(Permission).Name,
            PageSize = pgSize,
            PageCount = pageCount,
            RecordCount = (int)recordCount
        };

        return pagination;
    }

    public async Task<List<Permission>> GetValidByOrganizationAsync(int organizationId)
    {
        if (organizationId <= 0)
            throw new ArgumentOutOfRangeException(nameof(organizationId), "ObjectId cannot be negative.");

        DynamicParameters param = new();
        SqlBuilder sbSql = new();
        sbSql.Where("t.IsDeleted=0")
            .Where("t.IsEnabled=1")
            .Where("t.OrganizationId=@OrganizationId");

        param.Add("@OrganizationId", organizationId);

        using var cn = DbContext.DbCxn;
        string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**where**/").RawSql;

        return (await cn.QueryAsync<Permission>(sql, param)).ToList();
    }

    public async Task<List<Permission>> GetByOrganizationAsync(int organizationId)
    {
        if (organizationId <= 0)
            throw new ArgumentOutOfRangeException(nameof(organizationId), "ObjectId cannot be negative.");

        DynamicParameters param = new();
        SqlBuilder sbSql = new();
        sbSql.Where("t.IsDeleted=0")
            .Where("t.OrganizationId=@OrganizationId");

        param.Add("@OrganizationId", organizationId);

        using var cn = DbContext.DbCxn;
        string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**where**/").RawSql;

        return (await cn.QueryAsync<Permission>(sql, param)).AsList();
    }

    public async Task<List<Permission>> GetByUserIdAsync(int id)
    {
        if (id <= 0)
            throw new ArgumentOutOfRangeException(nameof(id), "ObjectId cannot be negative.");

        DynamicParameters param = new();
        SqlBuilder sbSql = new();
        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.Id=@Id");

        sbSql.InnerJoin($"{Role.MsSqlTable} r ON r.Id=t.RoleId");
        sbSql.InnerJoin($"{RolePermission.MsSqlTable} rp ON rp.RoleId=r.Id");
        sbSql.InnerJoin($"{Permission.MsSqlTable} p on p.Id=rp.PermissionId");

        param.Add("@Id", id);

        using var cn = DbContext.DbCxn;
        string sql = sbSql.AddTemplate($"SELECT p.* FROM {User.MsSqlTable} t /**innerjoin**/ /**where**/").RawSql;
        return (await cn.QueryAsync<Permission>(sql, param)).AsList();
    }
}