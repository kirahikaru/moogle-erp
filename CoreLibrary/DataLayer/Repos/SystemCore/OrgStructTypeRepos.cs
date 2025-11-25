using DataLayer.GlobalConstant;
using DataLayer.Models.SystemCore.NonPersistent;
using static Dapper.SqlMapper;

namespace DataLayer.Repos.SystemCore;

/// <summary>
/// Organization Structure Type
/// </summary>
public interface IOrgStructTypeRepos : IBaseRepos<OrgStructType>
{
	Task<OrgStructType?> GetFullAsync(int id);
	/// <summary>
	/// Get valid parent for an organization type. 
	/// </summary>
	Task<List<OrgStructType>> GetValidParents(OrgStructType val);

	Task<List<OrgStructType>> SearchAsync(
			int pgSize = 0, int pgNo = 0,
			string? objectCode = null,
			string? objectName = null,
			int orgLevel = -1
		);

	Task<DataPagination> GetSearchPaginationAsync(
			int pgSize = 0,
			string? objectCode = null,
			string? objectName = null,
			int orgLevel = -1
		);

	Task<IEnumerable<DropDownListItem>> GetValidParentsAsync(int objectId, string objectCode, int objOrgLevel, int? currentParentId);

	Task<List<DropDownListItem>> GetAllWithChildAsync();
}

public class OrgStructTypeRepos(IConnectionFactory connectionFactory) : BaseRepos<OrgStructType>(connectionFactory, OrgStructType.DatabaseObject), IOrgStructTypeRepos
{
	public async Task<OrgStructType?> GetFullAsync(int id)
    {
		SqlBuilder sbSql = new();
		DynamicParameters param = new();

		param.Add("@Id", id);

		sbSql.Where("t.IsDeleted=0");
		sbSql.Where("t.Id=@Id");

        sbSql.LeftJoin($"{DbObject.MsSqlTable} pr ON pr.Id=t.ParentId");

		string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;

		using var cn = ConnectionFactory.GetDbConnection()!;

		var data = (await cn.QueryAsync<OrgStructType>(sql, param)).FirstOrDefault();

		return data;
	}

	public async Task<List<OrgStructType>> GetValidParents(OrgStructType val)
    {
		ArgumentNullException.ThrowIfNull(val);

		SqlBuilder sbSql = new();
        DynamicParameters param = new();

        param.Add("@HierarchyPath", val.HierarchyPath, DbType.AnsiString);

        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.HierarchyPath NOT LIKE @HierarchyPath+'%'");

        string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**where**/").RawSql;

        using var cn = ConnectionFactory.GetDbConnection()!;

        var dataList = (await cn.QueryAsync<OrgStructType>(sql, param)).ToList();

        return dataList;
    }

    public async Task<IEnumerable<DropDownListItem>> GetValidParentsAsync(int objectId, string objectCode, int objOrgLevel, int? currentParentId)
    {
		SqlBuilder sbSql = new();
		DynamicParameters param = new();

        sbSql.Select("'ObjectId'=t.Id")
            .Select("t.ObjectCode")
            .Select("t.ObjectName")
            .Select("t.HierarchyPath");

        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.Id<>@Id");
        sbSql.Where("t.OrgLevel<@ObjOrgLevel");

        param.Add("@ObjectCode", objectCode, DbType.AnsiString);
        param.Add("@Id", objectId);
        param.Add("@ObjOrgLevel", objOrgLevel);

		if (currentParentId.HasValue)
        {
            sbSql.Where("(t.Id=@CurrentParentId OR t.HierarchyPath NOT LIKE @ObjectCode+'>%')");
            param.Add("@CurrentParentId", currentParentId.Value);
        }
        else
            sbSql.Where("t.HierarchyPath NOT LIKE @ObjectCode+'>%'");

        sbSql.OrderBy("t.HierarchyPath");

		using var cn = ConnectionFactory.GetDbConnection()!;

        string sql = sbSql.AddTemplate($"SELECT /**select**/ FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/").RawSql;

        var dataList = await cn.QueryAsync<DropDownListItem>(sql, param);

        return dataList;
	}

	public override async Task<KeyValuePair<int, IEnumerable<OrgStructType>>> SearchNewAsync(
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

		sbSql.LeftJoin($"{OrgStructType.MsSqlTable} pr ON pr.Id=t.ParentId");

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

		using var cn = ConnectionFactory.GetDbConnection()!;

		var dataList = await cn.QueryAsync<OrgStructType, OrgStructType, OrgStructType>(
				sql, (obj, parent) =>
				{
					obj.Parent = parent;
					
					return obj;
				}, param, splitOn: "Id");

		string sqlCount = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
		int dataCount = await cn.ExecuteScalarAsync<int>(sqlCount, param);
		return new(dataCount, dataList);
	}

	public async override Task<List<OrgStructType>> QuickSearchAsync(int pgSize = 0, int pgNo = 0, string? searchText = null, List<int>? excludeIdList = null)
	{
		DynamicParameters param = new();
		SqlBuilder sbSql = new();
		sbSql.Where("t.IsDeleted=0");

		#region Form Search Condition
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
		#endregion
		sbSql.LeftJoin($"{DbObject.MsSqlTable} pr ON pr.Id=t.ParentId");
		sbSql.OrderBy("t.HierarchyPath ASC");

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

		using var cn = ConnectionFactory.GetDbConnection()!;
		var dataList = (await cn.QueryAsync<OrgStructType, OrgStructType, OrgStructType>(sql,
							(obj, parent) =>
							{
								obj.Parent = parent;
								return obj;
							}, param, splitOn: "Id")).AsList();
		return dataList;
	}

	public async Task<List<OrgStructType>> SearchAsync(
            int pgSize = 0, int pgNo = 0,
            string? objectCode = null,
            string? objectName = null,
            int orgLevel = -1
        )
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
			sbSql.Where("UPPER(t.ObjectName) LIKE @ObjectName+'%'");
            param.Add("@ObjectName", objectName, DbType.AnsiString);
        }

        if (orgLevel > 0)
        {
			sbSql.Where("OrgLevel=@OrgLevel");
            param.Add("@OrgLevel", orgLevel);
        }
        #endregion

        string sql;
        sbSql.LeftJoin($"{DbObject.MsSqlTable} pr ON pr.Id=t.ParentId");

        sbSql.OrderBy("t.HierarchyPath ASC");

        if (pgNo == 0 && pgSize == 0)
        {
            sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/ /**orderby**/").RawSql;
        }
        else
        {
			//throw new NotImplementedException();
			param.Add("@PageSize", pgSize);
			param.Add("@PageNo", pgNo);

            sql = $";WITH pg AS (SELECT Id FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
                  $"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ WHERE t.Id IN (SELECT Id FROM pg) /**orderby**/";
        }

        using var cn = ConnectionFactory.GetDbConnection()!;
        
        var dataList = (await cn.QueryAsync<OrgStructType, OrgStructType, OrgStructType>(sql, 
                            (obj, parent) =>
                            {
                                obj.Parent = parent;
                                return obj;
                            }, param, splitOn:"Id")).AsList();

        return dataList;
    }

    public async Task<DataPagination> GetSearchPaginationAsync(
            int pgSize = 0,
            string? objectCode = null,
            string? objectName = null,
            int orgLevel = -1
        )
    {
        if (pgSize < 0)
            throw new Exception(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        var paramDict = new Dictionary<string, object>();

        string whereClause = "";

        #region Form Search Conditions
        if (!string.IsNullOrEmpty(objectCode))
        {
            whereClause += " AND UPPER(ObjectCode) LIKE '%'+@ObjectCode+'%'";
            paramDict.Add("@ObjectCode", new DbString { Value = objectCode.ToUpper(), IsAnsi = true });
        }

        if (!string.IsNullOrEmpty(objectName))
        {
            whereClause += " AND UPPER(ObjectName) LIKE @ObjectName+'%'";
            paramDict.Add("@ObjectName", new DbString { Value = objectName.ToUpper(), IsAnsi = true });
        }

        if (orgLevel > 0)
        {
            whereClause += " AND OrgLevel=@OrgLevel";
            paramDict.Add("@OrgLevel", orgLevel);
        }
        #endregion

        var sql = $"SELECT COUNT(*) FROM {OrgStructType.MsSqlTable} WHERE {whereClause}";

        using var cn = ConnectionFactory.GetDbConnection()!;
        var param = new DynamicParameters(paramDict);

        decimal recordCount = await cn.ExecuteScalarAsync<int>(sql, param);
        int pageCount = pgSize == 0 ? 1 : (int)Math.Ceiling(recordCount / pgSize);

        DataPagination pagingResult = new()
        {
            ObjectType = typeof(OrgStructType).Name,
            PageSize = pgSize,
            PageCount = pageCount,
            RecordCount = (int)recordCount
        };

        return pagingResult;
    }

	public async Task<List<DropDownListItem>> GetAllWithChildAsync()
	{
		SqlBuilder sbSql = new();

		sbSql.Select("'ObjectId'=t.Id")
			.Select("t.ObjectCode")
			.Select("t.ObjectName")
			.Select("t.HierarchyPath");

		sbSql.Where("t.IsDeleted=0");

		sbSql.LeftJoin($"(SELECT pos.ParentId, 'ChildCount'=COUNT(*) FROM {DbObject.MsSqlTable} pos WHERE pos.IsDeleted=0 AND pos.ParentId IS NOT NULL GROUP BY pos.ParentId) pr ON pr.ParentId=t.Id");
		sbSql.Where("pr.ChildCount IS NOT NULL");

		sbSql.OrderBy("t.HierarchyPath");
		sbSql.OrderBy("t.ObjectName");

		string sql = sbSql.AddTemplate($"SELECT /**select**/ FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;

		using var cn = ConnectionFactory.GetDbConnection()!;
		var dataList = (await cn.QueryAsync<DropDownListItem>(sql)).AsList();

		return dataList;
	}
}
