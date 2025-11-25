namespace DataLayer.Repos.SysCore;
public interface IOrgBranchRepos : IBaseRepos<OrganizationBranch>
{
	Task<List<OrganizationBranch>> SearchAsync(
		int pgSize = 0, int pgNo = 0,
		int? organizationId = null,
		string? name = null,
		string? provinceCode = null,
		bool? isEnabled = null);

	Task<DataPagination> GetSearchPaginationAsync(
		int pgSize = 0,
		int? organizationId = null,
		string? name = null,
		string? provinceCode = null,
		bool? isEnabled = null);

	//Task<List<OrganizationBranch>> GetByValidOrganizationId(int organizationId);
	//Task<List<OrganizationBranch>> GetByAllOrganizationId(int organizationId);
	Task<List<DropDownListItem>> GetForDropdownByOrganizationAsync(int organizationId);
}

public class OrgBranchRepos(IDbContext dbContext) : BaseRepos<OrganizationBranch>(dbContext, OrganizationBranch.DatabaseObject), IOrgBranchRepos
{
	public async Task<List<OrganizationBranch>> SearchAsync(
        int pgSize = 0,
        int pgNo = 0,
        int? organizationId = null,
        string? name = null,
        string? provinceCode = null,
        bool? isEnabled = null)
    {
        if (pgNo < 0 && pgSize < 0)
            throw new Exception(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        DynamicParameters param = new();
        SqlBuilder sbSql = new();
        sbSql.Where("t.IsDeleted=0");

        #region Form Search Conditions
        if (organizationId.HasValue && organizationId > 0)
        {
            sbSql.Where("t.OrganizationId=@OrganizationId");
            param.Add("@OrganizationId", organizationId);
        }

        if (!string.IsNullOrEmpty(name))
        {
            sbSql.Where("LOWER(t.ObjectName) LIKE '%'+LOWER(@Name)+'%'");
            param.Add("@Name", name);
        }

        if (!string.IsNullOrEmpty(provinceCode))
        {
            sbSql.Where("LOWER(t.ProvinceCode)=@ProvinceCode");
            param.Add("@ProvinceCode", provinceCode, DbType.AnsiString);
        }

        if (isEnabled.HasValue)
        {
            sbSql.Where("t.IsEnabled=@isEnabled");
            param.Add("@IsEnabled", isEnabled.Value);
        }
        #endregion
        sbSql.OrderBy("t.ObjectName ASC");

        using var cn = DbContext.DbCxn;
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

        var data = (await cn.QueryAsync<OrganizationBranch>(sql, param)).AsList();
        return data;
    }

    public async Task<DataPagination> GetSearchPaginationAsync(
        int pgSize = 0,
        int? organizationId = null,
        string? name = null,
        string? provinceCode = null,
        bool? isEnabled = null)
    {
        if (pgSize < 0)
            throw new Exception(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        DynamicParameters param = new();
        SqlBuilder sbSql = new();
        sbSql.Where("t.IsDeleted=0");

        #region Form Search Conditions
        if (organizationId.HasValue && organizationId > 0)
        {
            sbSql.Where("t.OrganizationId=@OrganizationId");
            param.Add("@OrganizationId", organizationId);
        }

        if (!string.IsNullOrEmpty(name))
        {
            sbSql.Where("LOWER(t.ObjectName) LIKE '%'+LOWER(@Name)+'%'");
            param.Add("@Name", name);
        }

        if (!string.IsNullOrEmpty(provinceCode))
        {
            sbSql.Where("LOWER(t.ProvinceCode)=@ProvinceCode");
            param.Add("@ProvinceCode", provinceCode, DbType.AnsiString);
        }

        if (isEnabled.HasValue)
        {
            sbSql.Where("t.IsEnabled=@isEnabled");
            param.Add("@IsEnabled", isEnabled.Value);
        }
        #endregion
        using var cn = DbContext.DbCxn;
        string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
        decimal recordCount = await cn.ExecuteScalarAsync<int>(sql, param);
        int pageCount = (int)Math.Ceiling(recordCount / (pgSize == 0 ? 1 : pgSize));

        DataPagination pagination = new()
        {
            ObjectType = typeof(OrganizationBranch).Name,
            PageSize = pgSize,
            PageCount = pageCount,
            RecordCount = (int)recordCount
        };

        return pagination;
    }

    public async Task<List<DropDownListItem>> GetForDropdownByOrganizationAsync(int organizationId)
    {
        DynamicParameters param = new();
        SqlBuilder sbSql = new();

        param.Add("@OrganizationId", organizationId);

        sbSql
            .Select("t.Id")
            .Select("t.ObjectCode")
            .Select("t.ObjecName")
            .Select("t.ObjectNameKh")
            .Where("t.IsDeleted=0")
            .Where("t.IsEnabled=1")
            .Where("t.OrganizationId=@OrganizationId");

        var sql = sbSql.AddTemplate($"SELECT /**select**/ FROM {DbObject.MsSqlTable} t /**where**/").RawSql;

        using var cn = DbContext.DbCxn;

        return (await cn.QueryAsync<DropDownListItem>(sql, param)).AsList();
    }
}