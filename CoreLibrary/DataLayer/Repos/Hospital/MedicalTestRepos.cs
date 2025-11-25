using DataLayer.Models.Hospital;
using DataLayer.Models.SystemCore.NonPersistent;
using static Dapper.SqlMapper;

namespace DataLayer.Repos.Hospital;

public interface IMedicalTestRepos : IBaseRepos<MedicalTest>
{
	Task<MedicalTest?> GetFullAsync(int id);

	Task<List<MedicalTest>> SearchAsync(
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

public class MedicalTestRepos(IConnectionFactory connectionFactory) : BaseRepos<MedicalTest>(connectionFactory, MedicalTest.DatabaseObject), IMedicalTestRepos
{
	public async Task<MedicalTest?> GetFullAsync(int id)
    {
		SqlBuilder sbSql = new();
		DynamicParameters param = new();

		sbSql.Where("t.IsDeleted=0");
		sbSql.Where("t.Id=@Id");

        param.Add("@Id", id);

        sbSql.LeftJoin($"{MedicalTestType.MsSqlTable} mtt ON mtt.Id=t.MedicalTestTypeId");

		using var cn = ConnectionFactory.GetDbConnection()!;

        string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;

        var dataObj = (await cn.QueryAsync<MedicalTest, MedicalTestType, MedicalTest>(sql,
                                            (obj, medicalTestType) =>
                                            {
                                                obj.TestType = medicalTestType;
                                                return obj;
                                            }, param, splitOn: "Id")).FirstOrDefault();

        return dataObj;
	}

    public override async Task<List<MedicalTest>> QuickSearchAsync(int pgSize = 0, int pgNo = 0, string? searchText = null, List<int>? excludeIdList = null)
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

        sbSql.LeftJoin($"{MedicalTestType.MsSqlTable} mtt ON mtt.Id=t.MedicalTestTypeId");

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

        using var cn = ConnectionFactory.GetDbConnection()!;

        var result = (await cn.QueryAsync<MedicalTest, MedicalTestType, MedicalTest>(sql, 
                        (obj, medicalTestType) =>
                        {
                            obj.TestType = medicalTestType;

                            return obj;
                        }, param, splitOn:"Id")).AsList();

        return result;
    }

    public async Task<List<MedicalTest>> SearchAsync(
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

        sbSql.LeftJoin($"{MedicalTestType.MsSqlTable} mtt ON mtt.Id=t.MedicalTestTypeId");
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

        using var cn = ConnectionFactory.GetDbConnection()!;

        var dataList = (await cn.QueryAsync<MedicalTest, MedicalTestType, MedicalTest>(
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

        using var cn = ConnectionFactory.GetDbConnection()!;

        decimal recordCount = await cn.ExecuteScalarAsync<int>(sql, param);
        int pageCount = (int)Math.Ceiling(recordCount / (pgSize == 0 ? 1 : pgSize));

        DataPagination pagination = new()
        {
            ObjectType = typeof(MedicalTest).Name,
            PageSize = pgSize,
            PageCount = pageCount,
            RecordCount = (int)recordCount
        };

        return pagination;
    }
}