using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Repos.SystemCore;

public interface ILocationTypeRepos : IBaseRepos<LocationType>
{
	Task<List<DropDownListItem>> GetValidParentsAsync(
		string hierarchyPath,
		string? searchText = null,
		int? includingObjId = null,
		List<int>? excludingObjectIdList = null);

	Task<List<LocationType>> GetChildsAsync(int objId, string hierarchyPath);

	Task<List<LocationType>> SearchAsync(
		int pgSize = 0, int pgNo = 0,
		string? objectCode = null,
		string? objectName = null,
		string? localName = null);

	Task<DataPagination> GetSearchPaginationAsync(
		int pgSize = 0,
		string? objectCode = null,
		string? objectName = null,
		string? localName = null);
}

public class LocationTypeRepos(IConnectionFactory connectionFactory) : BaseRepos<LocationType>(connectionFactory, LocationType.DatabaseObject), ILocationTypeRepos
{
	public async Task<List<LocationType>> GetChildsAsync(int objId, string hierarchyPath)
    {
        DynamicParameters param = new();
        SqlBuilder sbSql = new();

        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.Id<>@Id");
        sbSql.Where("t.HierarchyPath IS NOT NULL");
        sbSql.Where("t.HierarchyPath LIKE @HierarchyPath+'>%'");
        sbSql.LeftJoin($"{LocationType.MsSqlTable} p ON p.Id=t.ParentId");

        string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;

        param.Add("@Id", objId);
        param.Add("@HierarchyPath", hierarchyPath, DbType.AnsiString);

        using var cn = ConnectionFactory.GetDbConnection()!;

        List<LocationType> dataList = (await cn.QueryAsync<LocationType, LocationType, LocationType>(sql,
                                       (obj, parent) =>
                                       {
                                           obj.Parent = parent;
                                           return obj;
                                       }, param, splitOn: "Id")).AsList();
        return dataList;
    }

    public async Task<List<DropDownListItem>> GetValidParentsAsync(
        string hierarchyPath,
        string? searchText = null,
        int? includingObjId = null,
        List<int>? excludingObjectIdList = null)
    {
        if (string.IsNullOrEmpty(hierarchyPath))
            return [];

        DynamicParameters param = new();
        SqlBuilder sbSql = new();

        sbSql
            .Select("t.Id")
            .Select("'ObjectType' = 'LocationType'")
            .Select("t.ObjectCode")
            .Select("t.ObjectName")
            .Select("t.HierarchyPath");

        sbSql
            .Where("t.IsDeleted=0")
            .Where("t.HierarchyPath NOT LIKE @HierarchyPath+'%'");

        param.Add("@HierarchyPath", hierarchyPath, DbType.AnsiString);

        if (!string.IsNullOrEmpty(searchText))
        {
            sbSql.Where("UPPER(t.ObjectName) LIKE '%'+UPPER(@SearchText)+'%'");
            param.Add("@SearchText", searchText, DbType.AnsiString);
        }

        if (excludingObjectIdList != null && excludingObjectIdList.Count > 0)
        {
            sbSql.Where("t.Id NOT IN @ExcludingObjectIdList");
            param.Add("@ExcludingObjectIdList", excludingObjectIdList, DbType.AnsiString);
        }

        sbSql.OrderBy("t.ObjectName ASC");

        string sql = sbSql.AddTemplate($"SELECT TOP 100 /**select**/ FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/").RawSql;

        using var cn = ConnectionFactory.GetDbConnection()!;

        var dataList = (await cn.QueryAsync<DropDownListItem>(sql, param)).AsList();

        return dataList;
    }

    public async Task<List<LocationType>> SearchAsync(
        int pgSize = 0, int pgNo = 0,
        string? objectCode = null,
        string? objectName = null,
        string? localName = null)
    {
        if (pgNo < 0 && pgSize < 0)
            throw new Exception(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

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

        if (!string.IsNullOrEmpty(localName))
        {
            sbSql.Where("t.LocalName LIKE '%'+@LocalName+'%'");
            param.Add("@LocalName", localName);
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
                                    $"SELECT t.* FROM {LocationType.MsSqlTable} t INNER JOIN pg p ON p.Id=t.Id /**orderby**/").RawSql;
        }

        using var cn = ConnectionFactory.GetDbConnection()!;

        List<LocationType> dataList = (await cn.QueryAsync<LocationType>(sql, param)).AsList();

        return dataList;
    }

    public async Task<DataPagination> GetSearchPaginationAsync(int pgSize = 0, string? objectCode = null, string? objectName = null, string? localName = null)
    {
        if (pgSize < 0)
            throw new Exception(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

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

        if (!string.IsNullOrEmpty(localName))
        {
            sbSql.Where("t.LocalName LIKE '%'+@LocalName+'%'");
            param.Add("@LocalName", localName);
        }
        #endregion

        var sql = $"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/";

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