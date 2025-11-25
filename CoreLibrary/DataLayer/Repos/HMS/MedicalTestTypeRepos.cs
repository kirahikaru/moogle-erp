using DataLayer.Models.HMS;
using static Dapper.SqlMapper;

namespace DataLayer.Repos.HMS;

public interface IMedicalTestTypeRepos : IBaseRepos<MedicalTestType>
{
	Task<List<DropDownListItem>> GetValidParentsAsync(
		int objectId,
		string? objectCode,
		int? includingId = null);

	Task<List<MedicalTestType>> SearchAsync(
		int pgSize = 0, int pgNo = 0,
		string? objectCode = null,
		string? objectName = null,
		List<int>? parentIdList = null,
		string? hierarchyPath = null);
}

public class MedicalTestTypeRepos(IDbContext dbContext) : BaseRepos<MedicalTestType>(dbContext, MedicalTestType.DatabaseObject), IMedicalTestTypeRepos
{
	public async Task<List<DropDownListItem>> GetValidParentsAsync(int objectId, string? objectCode, int? includingId = null)
    {
        SqlBuilder sbSql = new();
        DynamicParameters param = new();

        sbSql.Select("'ObjectId'=t.Id");
        sbSql.Select("t.ObjectCode");
        sbSql.Select("t.ObjectName");
        sbSql.Select("t.ObjectNameKh");
        sbSql.Select("t.HierarchyPath");

        sbSql.Where("t.IsDeleted=0");

        sbSql.OrderBy("t.ObjectName ASC");

        param.Add("@Id", objectId);
        param.Add("@ObjectCode", objectCode, DbType.AnsiString);

        if (includingId.HasValue)
        {
            sbSql.Where("(t.Id<>@Id AND t.HierarchyPath NOT LIKE '%'+@ObjectCode+'%') OR t.Id=@IncludingId)");
            param.Add("@IncludingId", includingId.Value);
        }
        else
        {
            sbSql.Where("t.Id<>@Id");
            sbSql.Where("t.HierarchyPath NOT LIKE '%'+@ObjectCode+'%'");
        }

        using var cn = DbContext.DbCxn;
        string sql = sbSql.AddTemplate($"SELECT /**select**/ FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/").RawSql;
        return (await cn.QueryAsync<DropDownListItem>(sql, param)).AsList();
    }

    public async Task<List<MedicalTestType>> SearchAsync(
        int pgSize = 0, int pgNo = 0, 
        string? objectCode = null, 
        string? objectName = null,
        List<int>? parentIdList = null,
        string? hierarchyPath = null)
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

        if (parentIdList != null && parentIdList.Any())
        {
            if (parentIdList.Count == 1)
            {
                sbSql.Where("t.ParentId=@ParentId");
                param.Add("@ParentId", parentIdList[0]);
            }
            else
            {
                sbSql.Where("t.ParentId IN @ParentIdList");
                param.Add("@ParentIdList", parentIdList);
            }
        }

        if (!string.IsNullOrEmpty(hierarchyPath))
        {
            sbSql.Where("t.HierarchyPath LIKE @HierarchyPath+'%'");
            param.Add("@HierarchyPath", hierarchyPath, DbType.AnsiString);
        }
        #endregion

        sbSql.LeftJoin($"{DbObject.MsSqlTable} pr ON pr.Id=t.ParentId");
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
                    $"SELECT t.*, pr.* FROM {DbObject.MsSqlTable} t INNER JOIN pg p ON p.Id=t.Id /**leftjoin**/ /**orderby**/").RawSql;
        }

        using var cn = DbContext.DbCxn;

        var dataList = (await cn.QueryAsync<MedicalTestType, MedicalTestType, MedicalTestType>(
                                        sql, (obj, parent) =>
                                        {
                                            obj.Parent = parent;
                                            return obj;
                                        }, param, splitOn:"Id")).AsList();

        return dataList;
    }

	public override async Task<List<MedicalTestType>> QuickSearchAsync(
        int pgSize = 0, 
        int pgNo = 0, 
        string? searchText = null, List<int>? excludeIdList = null)
	{
		if (pgNo < 0 && pgSize < 0)
			throw new ArgumentOutOfRangeException(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

		DynamicParameters param = new();
		SqlBuilder sbSql = new();
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

        sbSql.LeftJoin($"{DbObject.MsSqlTable} pr ON pr.Id=t.ParentId");
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
				$"SELECT t.*, pr.* FROM {DbObject.MsSqlTable} t INNER JOIN pg p ON p.Id=t.Id /**leftjoin**/ /**orderby**/").RawSql;
		}

		using var cn = DbContext.DbCxn;
		var dataList = (await cn.QueryAsync<MedicalTestType, MedicalTestType, MedicalTestType>(sql,
                                (obj, parent) => {
                                    obj.Parent = parent;
                                    return obj;
                                },param, splitOn:"Id")).AsList();

		return dataList;
	}
}