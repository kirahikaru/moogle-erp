using Dapper;
using DataLayer.Models.Pru.IT;
using static Dapper.SqlMapper;

namespace DataLayer.Repos.Pru.IT;

public interface IITAssetCategoryRepos : IBaseRepos<ITAssetCategory>
{
	//Task<KeyValuePair<int, IEnumerable<ITAssetCategory>>> SearchAsync(
	//	int pgSize = 0,
	//	int pgNo = 0,
	//	string? searchText = null,
	//	IEnumerable<SqlSortCond>? sortConds = null,
	//	IEnumerable<SqlFilterCond>? filterConds = null,
	//	List<int>? excludeIdList = null);

	Task<IEnumerable<DropdownSelectItem>> GetForDropdownAsync(string assetType, int parentId = 0);
	Task<IEnumerable<DropdownSelectItem>> GetForDropdownAsync(string assetType, string parentCode);

	Task<IEnumerable<DropDownListItem>> GetValidParentsAsync(int objectId, string objectCode, int? currentParentId);
}

public class ITAssetCategoryRepos(IDbContext dbContext) : BaseRepos<ITAssetCategory>(dbContext, ITAssetCategory.DatabaseObject), IITAssetCategoryRepos
{
	public override async Task<KeyValuePair<int, IEnumerable<ITAssetCategory>>> SearchNewAsync(int pgSize = 0, int pgNo = 0, string? searchText = null, IEnumerable<SqlSortCond>? sortConds = null, IEnumerable<SqlFilterCond>? filterConds = null, List<int>? excludeIdList = null)
	{
		if (pgNo < 0 && pgSize < 0)
			throw new ArgumentOutOfRangeException(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

		SqlBuilder sbSql = new();
		DynamicParameters param = new();

		sbSql.Where("t.IsDeleted=0");
		sbSql.Where("UPPER(ISNULL(t.AssetType,''))=@AssetType");
		param.Add("@AssetType", "HARDWARE", DbType.AnsiString);

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
			else if (searchText.StartsWith("sr:"))
			{
				sbSql.Where("UPPER(t.SerialNo) LIKE '%'+UPPER(@SearchText)+'%'");
				param.Add("@SearchText", searchText.Replace("sr:", "", StringComparison.OrdinalIgnoreCase), DbType.AnsiString);
			}
			else
			{
				sbSql.Where("UPPER(t.ObjectName) LIKE '%'+UPPER(@SearchText)+'%' OR UPPER(t.ObjectCode) LIKE '%'+UPPER(@SearchText)+'%' OR UPPER(t.SerialNo) LIKE '%'+UPPER(@SearchText)+'%'");
				param.Add("@SearchText", searchText, DbType.AnsiString);
			}
		}

		if (excludeIdList != null && excludeIdList.Count > 0)
		{
			sbSql.Where("t.Id NOT IN @ExcludeIdList");
			param.Add("@ExcludeIdList", excludeIdList);
		}
		#endregion

		sbSql.LeftJoin($"{DbObject.MsSqlTable} pr ON pr.IsDeleted=0 AND pr.Id=t.ParentId");

		foreach (string orderByClause in GetSearchOrderbBy())
			sbSql.OrderBy(orderByClause);

		string sql;

		if (pgNo == 0 && pgSize == 0)
		{
			sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/").RawSql;
		}
		else
		{
			param.Add("@PageSize", pgSize);
			param.Add("@PageNo", pgNo);

			sql = sbSql.AddTemplate(
				$";WITH pg AS (SELECT t.Id FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
				$"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ WHERE t.Id IN (SELECT Id FROM pg) /**orderby**/").RawSql;
		}

		using var cn = DbContext.DbCxn;

		var dataList = (await cn.QueryAsync<ITAssetCategory, ITAssetCategory, ITAssetCategory>(sql, (obj, pr) =>
		{
			obj.Parent = pr;
			return obj;

		}, param, splitOn:"Id")).AsList();

		string countSql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
		int count = await cn.ExecuteScalarAsync<int>(countSql, param);

		return new KeyValuePair<int, IEnumerable<ITAssetCategory>>(count, dataList);

	}

	public async Task<IEnumerable<DropdownSelectItem>> GetForDropdownAsync(string assetType, int parentId)
	{
		SqlBuilder sbSql = new();
		DynamicParameters param = new();

		using var cn = DbContext.DbCxn;

		sbSql.Select("t.Id");
		sbSql.Select("'Key'=t.ObjectCode");
		sbSql.Select("'Value'=t.ObjectName");
		sbSql.Where("t.IsDeleted=0");
		sbSql.Where("t.AssetType=@AssetType");

		if (parentId>0)
		{
			sbSql.Where("t.ParentId IS NOT NULL");
			sbSql.Where("t.ParentId=@ParentId");
			param.Add("@ParentId", parentId);
		}
		else
		{
			sbSql.Where("t.ParentId IS NULL");
		}

		sbSql.OrderBy("t.ObjectName ASC");
		param.Add("@AssetType", assetType, DbType.AnsiString);

		string sql = sbSql.AddTemplate($"SELECT /**select**/ FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/").RawSql;

		return await cn.QueryAsync<DropdownSelectItem>(sql, param);
	}

	public async Task<IEnumerable<DropdownSelectItem>> GetForDropdownAsync(string assetType, string parentCode)
	{
		SqlBuilder sbSql = new();

		using var cn = DbContext.DbCxn;

		DynamicParameters param = new();

		sbSql.Select("t.Id");
		sbSql.Select("'Key'=t.ObjectCode");
		sbSql.Select("'Value'=t.ObjectName");
		sbSql.Where("t.IsDeleted=0");
		sbSql.Where("t.AssetType=@AssetType");
		param.Add("@AssetType", assetType, DbType.AnsiString);

		if (!string.IsNullOrEmpty(parentCode))
		{
			sbSql.Where("t.ParentCode IS NOT NULL");
			sbSql.Where("t.ParentCode=@ParentCode");
			param.Add("@ParentCode", parentCode, DbType.AnsiString);
		}
		else
		{
			sbSql.Where("t.ParentCode IS NULL");
		}

		sbSql.OrderBy("t.ObjectName ASC");

		string sql = sbSql.AddTemplate($"SELECT /**select**/ FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/").RawSql;

		return await cn.QueryAsync<DropdownSelectItem>(sql, param); ;
	}

	public async Task<IEnumerable<DropDownListItem>> GetValidParentsAsync(int objectId, string objectCode, int? currentParentId)
	{
		SqlBuilder sbSql = new();
		DynamicParameters param = new();

		sbSql.Select("'ObjectId'=t.Id")
			.Select("t.ObjectCode")
			.Select("t.ObjectName")
			.Select("t.HierarchyPath");

		sbSql.Where("t.IsDeleted=0");
		sbSql.Where("t.Id<>@Id");

		param.Add("@ObjectCode", objectCode, DbType.AnsiString);
		param.Add("@Id", objectId);

		if (currentParentId.HasValue)
		{
			sbSql.Where("(t.Id=@CurrentParentId OR t.HierarchyPath NOT LIKE @ObjectCode+'>%')");
			param.Add("@CurrentParentId", currentParentId.Value);
		}
		else
			sbSql.Where("t.HierarchyPath NOT LIKE @ObjectCode+'>%'");

		sbSql.OrderBy("t.HierarchyPath");

		using var cn = DbContext.DbCxn;

		string sql = sbSql.AddTemplate($"SELECT /**select**/ FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/").RawSql;

		var dataList = await cn.QueryAsync<DropDownListItem>(sql, param);

		return dataList;
	}
}