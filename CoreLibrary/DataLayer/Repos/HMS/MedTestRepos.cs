using DataLayer.Models.HMS;
using System.Text.RegularExpressions;
using static Dapper.SqlMapper;

namespace DataLayer.Repos.HMS;

public interface IMedTestRepos : IBaseRepos<MedTest>
{
	Task<MedTest?> GetFullAsync(int id);

	Task<List<MedTest>> SearchAsync(
		int pgSize = 0, int pgNo = 0,
		string? objectCode = null,
		string? objectName = null,
		List<int>? medicalTestTypeIdList = null);

	Task<DataPagination> GetSearchPaginationAsync(
		int pgSize = 0,
		string? objectCode = null,
		string? objectName = null,
		List<int>? medicalTestTypeIdList = null);
}

public class MedTestRepos(IDbContext dbContext) : BaseRepos<MedTest>(dbContext, MedTest.DatabaseObject), IMedTestRepos
{
	public async Task<MedTest?> GetFullAsync(int id)
    {
		SqlBuilder sbSql = new();
		DynamicParameters param = new();

		sbSql.Where("t.IsDeleted=0");
		sbSql.Where("t.Id=@Id");

        param.Add("@Id", id);

        sbSql.LeftJoin($"{MedTestType.MsSqlTable} mtt ON mtt.Id=t.MedicalTestTypeId");

		using var cn = DbContext.DbCxn;

        string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;

        var dataObj = (await cn.QueryAsync<MedTest, MedTestType, MedTest>(sql,
                                            (obj, medicalTestType) =>
                                            {
                                                obj.TestType = medicalTestType;
                                                return obj;
                                            }, param, splitOn: "Id")).FirstOrDefault();

        return dataObj;
	}

	public override async Task<KeyValuePair<int, IEnumerable<MedTest>>> SearchNewAsync(
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
			if (Regex.IsMatch(searchText, @"^[0-9]{5,}$"))
			{
				sbSql.Where("t.Barcode=@SearchText");
				param.Add("@SearchText", searchText);
			}
			else
			{
				sbSql.Where("(UPPER(t.ObjectName) LIKE '%'+UPPER(@SearchText)+'%' OR UPPER(t.ObjectCode) LIKE '%'+UPPER(@SearchText)+'%')");
				param.Add("@SearchText", searchText);
			}
		}

		if (excludeIdList != null && excludeIdList.Count != 0)
		{
			sbSql.Where("t.Id NOT IN @ExcludeIdList");
			param.Add("@ExcludeIdList", excludeIdList);
		}

		if (filterConds != null && filterConds.Any())
		{
			foreach (SqlFilterCond filterCond in filterConds)
			{

			}
		}

		sbSql.LeftJoin($"{MedTestType.MsSqlTable} mtt ON mtt.Id=t.MedTestTypeId");

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

		var dataList = await cn.QueryAsync<MedTest, MedTestType, MedTest>(
										sql, (obj, testType) =>
										{
											obj.TestType = testType;

											return obj;
										}, param, splitOn: "Id");

		string sqlCount = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
		int dataCount = await cn.ExecuteScalarAsync<int>(sqlCount, param);
		return new(dataCount, dataList);
	}

	public override async Task<List<MedTest>> QuickSearchAsync(int pgSize = 0, int pgNo = 0, string? searchText = null, List<int>? excludeIdList = null)
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

        sbSql.LeftJoin($"{MedTestType.MsSqlTable} mtt ON mtt.Id=t.MedicalTestTypeId");

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

			sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) ROWS FETCH NEXT @PageSize ROW ONLY;").RawSql;
		}

        using var cn = DbContext.DbCxn;

        var result = (await cn.QueryAsync<MedTest, MedTestType, MedTest>(sql, 
                        (obj, medicalTestType) =>
                        {
                            obj.TestType = medicalTestType;

                            return obj;
                        }, param, splitOn:"Id")).AsList();

        return result;
    }

    public async Task<List<MedTest>> SearchAsync(
        int pgSize = 0, int pgNo = 0,
        string? objectCode = null,
        string? objectName = null,
        List<int>? medicalTestTypeIdList = null)
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

        if (medicalTestTypeIdList != null && medicalTestTypeIdList.Any())
        {
            if (medicalTestTypeIdList.Count == 1)
            {
                sbSql.Where("t.MedicalTestTypeId=@MedicalTestTypeId");
                param.Add("@MedicalTestTypeId", medicalTestTypeIdList[0]);
            }
            else
            {
                sbSql.Where("t.MedicalTestTypeId IN @MedicalTestTypeIdList");
                param.Add("@MedicalTestTypeIdList", medicalTestTypeIdList);
            }
        }
        #endregion

        sbSql.LeftJoin($"{MedTestType.MsSqlTable} mtt ON mtt.Id=t.MedicalTestTypeId");
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
                    $"SELECT t.*, mtt.* FROM {DbObject.MsSqlTable} t INNER JOIN pg p ON p.Id=t.Id /**leftjoin**/ /**orderby**/").RawSql;
        }

        using var cn = DbContext.DbCxn;

        var dataList = (await cn.QueryAsync<MedTest, MedTestType, MedTest>(
                                        sql, (obj, testType) =>
                                        {
                                            obj.TestType = testType;
                                            return obj;
                                        }, param, splitOn: "Id")).AsList();

        return dataList;
    }

    public async Task<DataPagination> GetSearchPaginationAsync(
        int pgSize = 0,
        string? objectCode = null,
        string? objectName = null,
        List<int>? medicalTestTypeIdList = null)
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

        if (medicalTestTypeIdList != null && medicalTestTypeIdList.Any())
        {
            if (medicalTestTypeIdList.Count == 1)
            {
                sbSql.Where("t.MedicalTestTypeId=@MedicalTestTypeId");
                param.Add("@MedicalTestTypeId", medicalTestTypeIdList[0]);
            }
            else
            {
                sbSql.Where("t.MedicalTestTypeId IN @MedicalTestTypeIdList");
                param.Add("@MedicalTestTypeIdList", medicalTestTypeIdList);
            }
        }
        #endregion

        string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;

        using var cn = DbContext.DbCxn;

        decimal recordCount = await cn.ExecuteScalarAsync<int>(sql, param);
        int pageCount = (int)Math.Ceiling(recordCount / (pgSize == 0 ? 1 : pgSize));

        DataPagination pagination = new()
        {
            ObjectType = typeof(MedTest).Name,
            PageSize = pgSize,
            PageCount = pageCount,
            RecordCount = (int)recordCount
        };

        return pagination;
    }
}