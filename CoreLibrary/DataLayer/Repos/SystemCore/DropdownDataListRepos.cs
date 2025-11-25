using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Repos.SystemCore;

public interface IDropdownDataListRepos : IBaseRepos<DropdownDataList>
{
	Task<bool> HasExistingAsync(string systemName, string objectName, string fieldName, string objectCode, int objId);

	/// <summary>
	/// 
	/// </summary>
	/// <param name="systemName">GlobalConstant > DropdownDataSystems</param>
	/// <param name="objectName"></param>
	/// <param name="objectFieldName"></param>
	/// <param name="includingCode"></param>
	/// <returns></returns>
	Task<List<DropdownDataList>> GetForDropdownListAsync(
		string systemName,
		string objectName,
		string objectFieldName,
		string? includingCode = null);

	Task<List<DropdownDataList>> SearchAsync(
			int pgSize = 0, int pgNo = 0,
			string? objectCode = null,
			string? objectName = null,
			string? obejctFieldName = null,
			string? systemName = null,
			string? nameEn = null,
			string? nameKh = null,
			bool? isEnabled = null);

	Task<DataPagination> GetSearchPaginationAsync(
			int pgSize = 0,
			string? objectCode = null,
			string? objectName = null,
			string? obejctFieldName = null,
			string? systemName = null,
			string? nameEn = null,
			string? nameKh = null,
			bool? isEnabled = null);
}

public class DropdownDataListRepos(IConnectionFactory connectionFactory) : BaseRepos<DropdownDataList>(connectionFactory, DropdownDataList.DatabaseObject), IDropdownDataListRepos
{
	public async Task<bool> HasExistingAsync(string systemName, string objectName, string fieldName, string objectCode, int objId)
    {
        SqlBuilder sbSql = new();
        DynamicParameters param = new();

        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.SystemName=@SystemName");
        sbSql.Where("t.ObjectName=@ObjectName");
        sbSql.Where("t.ObjectFieldName=@ObjectFieldName");
        sbSql.Where("t.ObjectCode=@ObjectCode");
        sbSql.Where("t.Id<>@Id");

        param.Add("@SystemName", systemName, DbType.AnsiString);
        param.Add("@ObjectName", objectName, DbType.AnsiString);
        param.Add("@ObjectFieldName", fieldName, DbType.AnsiString);
        param.Add("@ObjectCode", objectCode, DbType.AnsiString);
        param.Add("@Id", objId, DbType.AnsiString);

        using var cn = ConnectionFactory.GetDbConnection()!;
        string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
        int count = await cn.ExecuteScalarAsync<int>(sql, param);
        return count > 0;
    }

    public async Task<List<DropdownDataList>> GetForDropdownListAsync(
        string systemName,
        string objectName,
        string objectFieldName,
        string? includingCode = null)
    {
        SqlBuilder sbSql = new();
        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.IsEnabled=1");
        sbSql.Where("t.ObjectName=@ObjectName");
        sbSql.Where("t.ObjectFieldName=@ObjectFieldName");
        sbSql.Where("t.SystemName=@SystemName");

        sbSql.OrderBy("t.NameEn ASC, t.NameKh ASC");

        DynamicParameters param = new();
        param.Add("@ObjectName", objectName, DbType.AnsiString);
        param.Add("@ObjectFieldName", objectFieldName, DbType.AnsiString);
        param.Add("@SystemName", systemName, DbType.AnsiString);

        SqlBuilder.Template sbSqlTempl = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/");

        using var cn = ConnectionFactory.GetDbConnection()!;

        List<DropdownDataList> dataList = (await cn.QueryAsync<DropdownDataList>(sbSqlTempl.RawSql, param)).AsList();

		if (!string.IsNullOrEmpty(includingCode) && !dataList.Any(x => x.ObjectCode == includingCode))
        {
            DynamicParameters includingParam = new();
            includingParam.Add("@ObjectCode", includingCode, DbType.AnsiString);

            DropdownDataList? item = cn.QuerySingleOrDefault<DropdownDataList?>($"SELECT * FROM {DbObject.MsSqlTable} t WHERE t.IsDeleted=0 AND t.ObjectCode=@ObjectCode", includingParam);

            if (item != null)
                dataList.Add(item);
        }

        return dataList;
    }

    public async Task<List<DropdownDataList>> SearchAsync(
            int pgSize = 0, int pgNo = 0,
            string? objectCode = null,
            string? objectName = null,
            string? obejctFieldName = null,
            string? systemName = null,
            string? nameEn = null,
            string? nameKh = null,
            bool? isEnabled = null)
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
            sbSql.Where("UPPER(t.ObjectName) LIKE '%'+UPPER(@ObjectName)+'%'");
            param.Add("@ObjectName", objectName, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(obejctFieldName))
        {
            sbSql.Where("UPPER(t.ObjectFieldName) LIKE '%'+UPPER(@ObjectFieldName)+'%'");
            param.Add("@ObjectFieldName", obejctFieldName, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(systemName))
        {
            sbSql.Where("UPPER(t.SystemName)=UPPER(@SystemName)");
            param.Add("@SystemName", systemName, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(nameEn))
        {
            sbSql.Where("UPPER(t.NameEn) LIKE '%'+UPPER(@NameEn)+'%'");
            param.Add("@NameEn", nameEn, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(nameKh))
        {
            sbSql.Where("t.NameKh LIKE '%'+@NameKh+'%'");
            param.Add("@NameKh", nameKh);
        }

        if (isEnabled != null)
        {
            sbSql.Where("t.IsEnabled=@IsEnabled");
            param.Add("@IsEnabled", isEnabled.Value);
        }
        #endregion

        sbSql.OrderBy("t.NameEn ASC");
        sbSql.OrderBy("t.NameKh ASC");

        SqlBuilder.Template? sbSqlTempl;

        if (pgNo == 0 && pgSize == 0)
        {
            sbSqlTempl = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/");
        }
        else
        {
            param.Add("@PageSize", pgSize);
            param.Add("@PageNo", pgNo);

            sbSqlTempl = sbSql.AddTemplate(
                $";WITH pg AS (SELECT Id FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
                $"SELECT t.* FROM {DbObject.MsSqlTable} t INNER JOIN pg p ON p.Id=a.Id /**orderby**/");
        }

        using var cn = ConnectionFactory.GetDbConnection()!;
        
        List<DropdownDataList> dataList = (await cn.QueryAsync<DropdownDataList>(sbSqlTempl.RawSql, param)).AsList();

        return dataList;
    }

    public async Task<DataPagination> GetSearchPaginationAsync(
            int pgSize = 0,
            string? objectCode = null,
            string? objectName = null,
            string? obejctFieldName = null,
            string? systemName = null,
            string? nameEn = null,
            string? nameKh = null,
            bool? isEnabled = null)
    {
        if (pgSize < 0)
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
			sbSql.Where("UPPER(t.ObjectName) LIKE '%'+UPPER(@ObjectName)+'%'");
			param.Add("@ObjectName", objectName, DbType.AnsiString);
		}

		if (!string.IsNullOrEmpty(obejctFieldName))
		{
			sbSql.Where("UPPER(t.ObjectFieldName) LIKE '%'+UPPER(@ObjectFieldName)+'%'");
			param.Add("@ObjectFieldName", obejctFieldName, DbType.AnsiString);
		}

		if (!string.IsNullOrEmpty(systemName))
		{
			sbSql.Where("UPPER(t.SystemName)=UPPER(@SystemName)");
			param.Add("@SystemName", systemName, DbType.AnsiString);
		}

		if (!string.IsNullOrEmpty(nameEn))
		{
			sbSql.Where("UPPER(t.NameEn) LIKE '%'+UPPER(@NameEn)+'%'");
			param.Add("@NameEn", nameEn, DbType.AnsiString);
		}

		if (!string.IsNullOrEmpty(nameKh))
		{
			sbSql.Where("t.NameKh LIKE '%'+@NameKh+'%'");
			param.Add("@NameKh", nameKh);
		}

		if (isEnabled != null)
		{
			sbSql.Where("t.IsEnabled=@IsEnabled");
			param.Add("@IsEnabled", isEnabled.Value);
		}
		#endregion

		var sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;

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