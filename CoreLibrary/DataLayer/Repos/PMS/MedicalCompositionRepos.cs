using DataLayer.Models.PMS;

namespace DataLayer.Repos.PMS;

public interface IMedicalCompositionRepos : IBaseRepos<MedicalComposition>
{
	Task<List<MedicalComposition>> SearchAsync(
		int pgSize = 0, int pgNo = 0,
		string? objName = null,
		string? frenchName = null,
		string? treatmentDesc = null);

	Task<DataPagination> GetSearchPaginationAsync(
		int pgSize = 0,
		string? objName = null,
		string? frenchName = null,
		string? treatmentDesc = null);
}

public class MedicalCompositionRepos(IDbContext dbContext) : BaseRepos<MedicalComposition>(dbContext, MedicalComposition.DatabaseObject), IMedicalCompositionRepos
{
	public override async Task<KeyValuePair<int, IEnumerable<MedicalComposition>>> SearchNewAsync(
		int pgSize = 0, int pgNo = 0,
		string? searchText = null,
		IEnumerable<SqlSortCond>? sortConds = null,
		IEnumerable<SqlFilterCond>? filterConds = null,
		List<int>? excludeIdList = null
	)
	{
		DynamicParameters param = new();
		SqlBuilder sbSql = new();

		sbSql.Where("t.IsDeleted=0");

		#region Form Search Conditions

		if (!string.IsNullOrEmpty(searchText))
		{
			sbSql.Where("(UPPER(t.ObjectName) LIKE '%'+UPPER(@SearchText)+'%' OR UPPER(t.ObjectCode) LIKE '%'+UPPER(@SearchText)+'%')");
			param.Add("@SearchText", searchText);
		}

		if (excludeIdList != null && excludeIdList.Count != 0)
		{
			sbSql.Where("t.Id NOT IN @ExcludeIdList");
			param.Add("@ExcludeIdList", excludeIdList);
		}

		if (filterConds != null && filterConds.Any())
		{
			foreach (var filterCond in filterConds)
			{

			}
		}

		#endregion

		if (sortConds is null || !sortConds.Any())
		{
			foreach (string order in GetSearchOrderbBy())
			{
				sbSql.OrderBy(order);
			}
		}
		else
		{
			foreach (SqlSortCond sortCond in sortConds)
			{
				sbSql.OrderBy(sortCond.GetSortCommand("t"));
			}
		}

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
				$";WITH pg AS (SELECT Id FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
				$"SELECT * FROM {DbObject.MsSqlTable} t WHERE t.Id IN (SELECT Id FROM pg) /**orderby**/").RawSql;
		}

		using var cn = DbContext.DbCxn;

		var dataList = await cn.QueryAsync<MedicalComposition>(sql, param);

		string sqlCount = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
		int dataCount = await cn.ExecuteScalarAsync<int>(sqlCount, param);
		return new(dataCount, dataList);
	}

	public async Task<List<MedicalComposition>> SearchAsync(
        int pgSize = 0, int pgNo = 0,
        string? objName = null,
        string? frenchName = null,
        string? treatmentDesc = null)
    {
        DynamicParameters param = new();
        SqlBuilder sbSql = new();

        #region Form Search Conditions
        sbSql.Where("t.IsDeleted=0");

        if (!string.IsNullOrEmpty(objName))
        {
            sbSql.Where("t.[ObjectName] LIKE '%'+@ObjectName+'%'");
            param.Add("@ObjectName", objName, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(frenchName))
        {
            sbSql.Where("t.FrenchName LIKE '%'+@FrenchName+'%'");
            param.Add("@FrenchName", frenchName, DbType.String);
        }

        if (!string.IsNullOrEmpty(treatmentDesc))
        {
            sbSql.Where("t.TreatmentDescription LIKE '%'+@TreatmentDescription"+'%');
            param.Add("@TreatmentDescription", treatmentDesc, DbType.String);
        }

        sbSql.OrderBy("t.ObjectName ASC");
        #endregion

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
                    $";WITH pg AS (SELECT Id FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
                    $"SELECT t.* FROM {DbObject.MsSqlTable} t INNER JOIN pg ON pg.Id=t.Id /**orderby**/").RawSql;
        }
        using var cn = DbContext.DbCxn;

        List<MedicalComposition> result = (await cn.QueryAsync<MedicalComposition>(sql, param)).ToList();
        
        return result;
    }

    public async Task<DataPagination> GetSearchPaginationAsync(
        int pgSize = 0,
        string? objName = null,
        string? frenchName = null,
        string? treatmentDesc = null)
    {
        if (pgSize < 0)
            throw new ArgumentOutOfRangeException(nameof(pgSize), _errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        DynamicParameters param = new();
        SqlBuilder sbSql = new();

        //Implement lower-case convert for text-type base search to lower is to ensure searching is case-insensitive
        //since now PCLAAPP DB is a unicode enabled database, i.e. all search are case sensitive

        sbSql.Where("t.IsDeleted=0");

        #region Form Search Conditions
        sbSql.Where("t.IsDeleted=0");

        if (!string.IsNullOrEmpty(objName))
        {
            sbSql.Where("t.[ObjectName] LIKE '%'+@ObjectName+'%'");
            param.Add("@ObjectName", objName, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(frenchName))
        {
            sbSql.Where("t.FrenchName LIKE '%'+@FrenchName+'%'");
            param.Add("@FrenchName", frenchName, DbType.String);
        }

        if (!string.IsNullOrEmpty(treatmentDesc))
        {
            sbSql.Where("t.TreatmentDescription LIKE '%'+@TreatmentDescription" + '%');
            param.Add("@TreatmentDescription", treatmentDesc, DbType.String);
        }

        sbSql.OrderBy("t.ObjectName ASC");
        #endregion

        string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;

        using var cn = DbContext.DbCxn;

        decimal recordCount = await cn.ExecuteScalarAsync<int>(sql, param);
        int pageCount = (int)Math.Ceiling(recordCount / pgSize);

        DataPagination pagination = new()
        {
            ObjectType = typeof(MedicalComposition).Name,
            RecordCount = (int)recordCount,
            PageCount = pageCount,
            PageSize = pgSize
        };

        return pagination;
    }
}