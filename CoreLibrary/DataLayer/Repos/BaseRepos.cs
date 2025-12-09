//using DapperExtensions;
using DataLayer.Models.SysCore.NonPersistent;
using System.Data.Common;
using System.Reflection;
using System.Resources;

namespace DataLayer.Repos;
public interface IBaseRepos<TEntity> where TEntity : AuditObject
{
	// warning CA1716: Rename virtual/interface member IBaseRepos<TEntity>.Get(int) so that it no longer conflicts with the reserved language keyword 'Get'. Using a reserved keyword as the name of a virtual/interface member makes it harder for consumers in other languages to override/implement the member.

	IDbContext DbContext { get; }

	TEntity? Get(int id);
	TEntity? GetByCode(string objectCode);
	List<TEntity> GetAll();
	int Insert(TEntity entity);
	bool Update(TEntity entity);
	int Delete(int id, string username);
	int HardDelete(int id);
	bool IsDuplicatedCode(int objectId, string objectCode);
	int GetExistingObjectIdByCode(int objectId, string objectCode);
	//int GetPageCount(int pageSize);

	Task<TEntity?> GetAsync(int Id);
	Task<List<TEntity>> GetManyAsync(List<int> idList);
	Task<TEntity?> GetByCodeAsync(string objectCode);
	Task<int> GetPageCountAsync(int pageSize);
	Task<List<TEntity>> GetAllAsync();
	Task<int> InsertAsync(TEntity entity);
	Task<bool> UpdateAsync(TEntity entity);
	Task<int> DeleteAsync(int id, string username);
	Task<int> HardDeleteAsync(int id);
	Task<bool> IsDuplicateCodeAsync(int objectId, string objectCode);
	Task<int> GetExistingObjectIdByCodeAsync(int objectId, string objectCode);
	Task<List<DropdownSelectItem>> GetForDropdownSelectAsync(string? objectName = null, int? includingId = null);

	Task<List<TEntity>> QuickSearchAsync(int pgSize = 0, int pgNo = 0, string? searchText = null, List<int>? excludeIdList = null);
	Task<DataPagination> GetQuickSearchPaginationAsync(int pgSize = 0, string? searchText = null, List<int>? excludeIdList = null);

	Task<KeyValuePair<int, IEnumerable<TEntity>>> SearchNewAsync(
		int pgSize = 0, int pgNo = 0,
		string? searchText = null,
		IEnumerable<SqlSortCond>? sortConds = null,
		IEnumerable<SqlFilterCond>? filterConds = null,
		List<int>? excludeIdList = null
	);

	Task<List<TEntity>> SearchAsync(int pgSize = 0, int pgNo = 0, string? objectCode = null, string? objectName = null);
	Task<DataPagination> GetSearchPaginationAsync(int pgSize = 0, string? objectCode = null, string? objectName = null);

	Task<string?> GetRunningNumberAsync(IDbConnection cn, RunNumGenParam rngParam, IDbTransaction? tran = null);
}

public class BaseRepos<TEntity>(IDbContext dbContext, DatabaseObj dbObj) : IBaseRepos<TEntity> where TEntity : AuditObject
{
    private readonly IDbContext _dbContext = dbContext;
    private readonly DatabaseObj _dbObj = dbObj;
    internal IEnumerable<PropertyInfo> ObjectProperties = typeof(TEntity).GetProperties().Where(x => !x.PropertyType.FullName!.StartsWith("DataLayerCore", StringComparison.OrdinalIgnoreCase));
    internal ResourceManager _errMsgResxMngr = new ResourceManager("ErrorMessages", Assembly.GetExecutingAssembly());


	public IDbContext DbContext => _dbContext;
    public DatabaseObj DbObject => _dbObj;

	#region SYNCRONOUS METHODS
	public TEntity? Get(int id)
    {
        using var cn = DbContext.DbCxn;

		return cn.Get<TEntity?>(id);

        //string sql = string.Format(CultureInfo.CurrentCulture, "SELECT * FROM {0} WHERE Id=@Id", TableName);
        //return cn.QuerySingleOrDefault<TEntity>(sql, new { id });
    }

    public TEntity? GetByCode(string objectCode)
    {
        SqlBuilder sbSql = new();
		DynamicParameters param = new();
		string sql;

        if (DbContext.DbType.Is(DatabaseTypes.AZURE_SQL, DatabaseTypes.MSSQL))
        {
            sbSql.Where("t.IsDeleted=0");
            sbSql.Where("LOWER(t.ObjectCode)=LOWER(@ObjectCode)");
            param.Add("@ObjectCode", objectCode, DbType.AnsiString);
            sql = sbSql.AddTemplate($"SELECT * FROM {_dbObj.MsSqlTable} t /**where**/").RawSql;
        }
        else if (DbContext.DbType.Is(DatabaseTypes.POSTGRESQL))
        {
            sbSql.Where("t.is_deleted=false");
            sbSql.Where("LOWER(t.object_code)=LOWER(@obj_code)");
            param.Add("@obj_code", objectCode, DbType.AnsiString);
            sql = sbSql.AddTemplate($"SELECT * FROM {_dbObj.PgTable} t /**where**/").RawSql;
        }
        else
            throw new NotImplementedException();

        using var cn = DbContext.DbCxn;

        return cn.QuerySingleOrDefault<TEntity?>(sql, param);
    }

    public virtual List<TEntity> GetAll()
    {
        using var cn = DbContext.DbCxn;

        return cn.GetAll<TEntity>().AsList();
    }

    public int Insert(TEntity entity)
    {
        using var cn = DbContext.DbCxn;
		return (int)cn.Insert(entity);
        //string insSql = GenerateInsertQuery();
        //return cn.QuerySingle<int>(insSql, entity);
    }

    public bool Update(TEntity entity)
    {
        using var cn = DbContext.DbCxn;
        //string updateQuery = GenerateUpdateQuery();
        //int updCount = cn.Execute(updateQuery, entity);
        //bool isUpdated = cn.Update(entity);
        //return isUpdated ? 1 : 0;

        return cn.Update(entity);
    }

    public int Delete(int id, string username)
    {
        SqlBuilder sbSql = new();
		DynamicParameters param = new();
        string sql = "";
		DateTime khTimestamp = DateTime.UtcNow.AddHours(7);

		if (DbContext.DbType.Is(DatabaseTypes.MSSQL, DatabaseTypes.AZURE_SQL))
		{
            sbSql.Set("IsDeleted=1");
			sbSql.Set("ModifiedUser=@ModifiedUser");
			sbSql.Set("ModifiedDateTime=@ModifiedDateTime");
            sbSql.Where("Id=@Id");
            
			param.Add("@Id", id);
			param.Add("@ModifiedDateTime", khTimestamp);
			param.Add("@ModifiedUser", username, DbType.AnsiString);

			sql = sbSql.AddTemplate($"UPDATE {DbObject.MsSqlTable} /**set**/ /**where**/").RawSql;
		}
		else if (DbContext.DbType.Is(DatabaseTypes.POSTGRESQL))
		{
			sbSql.Set("is_deleted=true");
			sbSql.Set("modified_user=@modified_user");
			sbSql.Set("modified_datetime=@modified_datetime");
			sbSql.Where("id=@id");

			param.Add("@id", id);
			param.Add("@modified_datetime", khTimestamp);
			param.Add("@modified_user", username, DbType.AnsiString);

			sql = sbSql.AddTemplate($"UPDATE {DbObject.PgTable} /**set**/ /**where**/").RawSql;
		}
		else
			throw new NotImplementedException();

        using var cn = DbContext.DbCxn;

        int rowAffected = cn.Execute(sql, param);

        return rowAffected;
    }

    public int HardDelete(int id)
    {
        string sql = "";

        if (DbContext.DbType.Is(DatabaseTypes.MSSQL, DatabaseTypes.AZURE_SQL))
        {
            sql = $"DELETE FROM {DbObject.MsSqlTable} WHERE Id=@Id";
        }
        else if (DbContext.DbType.Is(DatabaseTypes.POSTGRESQL))
        {
            sql = $"DELETE FROM {DbObject.PgTable} WHERE id=@Id";
        }
        else
            throw new NotImplementedException();

        using var cn = DbContext.DbCxn;

        int rowAffected = cn.Execute(sql, new { Id = id });
        return rowAffected;
    }

    public bool IsDuplicatedCode(int objectId, string objectCode)
    {
		DynamicParameters param = new();
		SqlBuilder sbSql = new();
        string sql = "";

        if (DbContext.DbType.Is(DatabaseTypes.AZURE_SQL, DatabaseTypes.MSSQL))
        {
            sbSql.Where("t.IsDeleted=0");
            sbSql.Where("t.Id<>@Id");
			sbSql.Where("UPPER(t.ObjectCode)=UPPER(@ObjectCode)");

			param.Add("@Id", objectId);
			param.Add("@ObjectCode", objectCode, DbType.AnsiString);

			sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
		}
		else if (DbContext.DbType == DatabaseTypes.POSTGRESQL)
		{
			sbSql.Where("t.is_deleted=false");
			sbSql.Where("t.id<>@id");
			sbSql.Where("LOWER(t.object_code)=@obj_code");

			param.Add("@id", objectId);
			param.Add("@obj_code", objectCode, DbType.AnsiString);

			sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.PgTable} t /**where**/").RawSql;
		}

		using var cn = DbContext.DbCxn;

        return cn.ExecuteScalar<int>(sql, param) > 0;
    }

    public int GetExistingObjectIdByCode(int objectId, string objectCode)
    {
        SqlBuilder sbSql = new();
        DynamicParameters param = new();
        string sql = "";

		if (DbContext.DbType.Is(DatabaseTypes.AZURE_SQL, DatabaseTypes.MSSQL))
		{
			sbSql.Where("t.IsDeleted=0");
			sbSql.Where("t.Id<>@Id");
			sbSql.Where("UPPER(t.ObjectCode)=UPPER(@ObjectCode)");

			param.Add("@Id", objectId);
			param.Add("@ObjectCode", objectCode, DbType.AnsiString);

			sql = sbSql.AddTemplate($"SELECT t.Id FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
		}
		else if (DbContext.DbType == DatabaseTypes.POSTGRESQL)
		{
			sbSql.Where("t.is_deleted=false");
			sbSql.Where("t.id<>@id");
			sbSql.Where("LOWER(t.object_code)=@obj_code");

			param.Add("@id", objectId);
			param.Add("@obj_code", objectCode, DbType.AnsiString);

			sql = sbSql.AddTemplate($"SELECT t.id FROM {DbObject.PgTable} t /**where**/").RawSql;
		}

        using var cn = DbContext.DbCxn;

        int id = cn.ExecuteScalar<int>(sql, param);
        return id;
    }
    #endregion

    #region ASYNCHRONOUS METHODS
    public async Task<TEntity?> GetAsync(int id)
    {
        using var cn = DbContext.DbCxn;

        return await cn.GetAsync<TEntity>(id);
    }

    public virtual async Task<List<TEntity>> GetManyAsync(List<int> idList)
    {
        SqlBuilder sbSql = new();
        DynamicParameters param = new();
		string sql = "";
		if (DbContext.DbType.Is(DatabaseTypes.AZURE_SQL, DatabaseTypes.MSSQL))
        {
            sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} WHERE IsDeleted=0 AND Id IN @IdList").RawSql;
			param.Add("@IdList", idList);
		}
		else if (DbContext.DbType == DatabaseTypes.POSTGRESQL)
        {
			sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.PgTable} WHERE is_deleted=false AND id IN @id_list").RawSql;
			param.Add("@id_list", idList);
		}
        else
        {
            throw new NotImplementedException();
        }

        using var cn = DbContext.DbCxn;

        return (await cn.QueryAsync<TEntity>(sql, param)).AsList();
    }

    public async Task<TEntity?> GetByCodeAsync(string objectCode)
    {
		SqlBuilder sbSql = new();
		DynamicParameters param = new();
		string sql;

		if (DbContext.DbType.Is(DatabaseTypes.AZURE_SQL, DatabaseTypes.MSSQL))
		{
			sbSql.Where("t.IsDeleted=0");
			sbSql.Where("LOWER(t.ObjectCode)=LOWER(@ObjectCode)");
			param.Add("@ObjectCode", objectCode, DbType.AnsiString);
			sql = sbSql.AddTemplate($"SELECT * FROM {_dbObj.MsSqlTable} t /**where**/").RawSql;
		}
		else if (DbContext.DbType.Is(DatabaseTypes.POSTGRESQL))
		{
			sbSql.Where("t.is_deleted=false");
			sbSql.Where("LOWER(t.object_code)=LOWER(@obj_code)");
			param.Add("@obj_code", objectCode, DbType.AnsiString);
			sql = sbSql.AddTemplate($"SELECT * FROM {_dbObj.PgTable} t /**where**/").RawSql;
		}
		else
			throw new NotImplementedException();

		using var cn = DbContext.DbCxn;

        return (await cn.QuerySingleOrDefaultAsync<TEntity>(sql, param));
    }

    public async Task<List<TEntity>> GetAllAsync()
    {
        string sql = "";

		if (DbContext.DbType.Is(DatabaseTypes.AZURE_SQL, DatabaseTypes.MSSQL))
		{
			sql = $"SELECT * FROM {DbObject.MsSqlTable} WHERE IsDeleted=0";
		}
		else if (DbContext.DbType == DatabaseTypes.POSTGRESQL)
		{
			sql = $"SELECT * FROM {DbObject.PgTable} WHERE is_deleted=false";
		}
		else
		{
			throw new NotImplementedException();
		}

		using var cn = DbContext.DbCxn;

        return (await cn.QueryAsync<TEntity>(sql)).AsList();
    }

    public async Task<int> GetPageCountAsync(int pageSize)
    {
        string sql;

        if (DbContext.DbType.Is(DatabaseTypes.AZURE_SQL, DatabaseTypes.MSSQL))
        {
			sql = $"SELECT COUNT(*) FROM {DbObject.MsSqlTable} WHERE IsDeleted=0";
		}
		else if (DbContext.DbType == DatabaseTypes.POSTGRESQL)
		{
			sql = $"SELECT COUNT(*) FROM {DbObject.PgTable} WHERE is_deleted=false";
			
		}
		else
		{
			throw new NotImplementedException();
		}

		

        using var cn = DbContext.DbCxn;

        return (await cn.ExecuteScalarAsync<int>(sql)) / pageSize;
    }

    public async Task<int> InsertAsync(TEntity entity)
    {

        //string insSql = GenerateInsertQuery();

        using var cn = DbContext.DbCxn;
        
        //int insCount = await cn.ExecuteAsync(insSql, entity);
        return await cn.InsertAsync(entity);
    }

    public async Task<bool> UpdateAsync(TEntity entity)
    {
        //string updateQuery = GenerateUpdateQuery();

        //using var cn = DbContext.DbCxn;
        //int updCount = await cn.ExecuteAsync(updateQuery, entity);
        //return updCount;

        using var cn = DbContext.DbCxn;
        return await cn.UpdateAsync(entity);
    }

    public async Task<int> DeleteAsync(int id, string username)
    {
		SqlBuilder sbSql = new();
		DynamicParameters param = new();
		string sql = "";
		DateTime khTimestamp = DateTime.UtcNow.AddHours(7);

		if (DbContext.DbType.Is(DatabaseTypes.MSSQL, DatabaseTypes.AZURE_SQL))
		{
			sbSql.Set("IsDeleted=1");
			sbSql.Set("ModifiedUser=@ModifiedUser");
			sbSql.Set("ModifiedDateTime=@ModifiedDateTime");
			sbSql.Where("Id=@Id");

			param.Add("@Id", id);
			param.Add("@ModifiedDateTime", khTimestamp);
			param.Add("@ModifiedUser", username, DbType.AnsiString);

			sql = sbSql.AddTemplate($"UPDATE {DbObject.MsSqlTable} /**set**/ /**where**/").RawSql;
		}
		else if (DbContext.DbType.Is(DatabaseTypes.POSTGRESQL))
		{
			sbSql.Set("is_deleted=true");
			sbSql.Set("modified_user=@modified_user");
			sbSql.Set("modified_datetime=@modified_datetime");
			sbSql.Where("id=@id");

			param.Add("@id", id);
			param.Add("@modified_datetime", khTimestamp);
			param.Add("@modified_user", username, DbType.AnsiString);

			sql = sbSql.AddTemplate($"UPDATE {DbObject.PgTable} /**set**/ /**where**/").RawSql;
		}
		else
			throw new NotImplementedException();

		using var cn = DbContext.DbCxn;
        
        int rowAffected = await cn.ExecuteAsync(sql, param);
        return rowAffected;
    }

    public async Task<int> HardDeleteAsync(int id)
    {
        string sql;

        if (DbContext.DbType.Is(DatabaseTypes.MSSQL, DatabaseTypes.AZURE_SQL))
        {
			sql = $"DELETE FROM {DbObject.MsSqlTable} WHERE Id=@Id";
		}
        else if (DbContext.DbType.Is(DatabaseTypes.POSTGRESQL))
        {
			sql = $"DELETE FROM {DbObject.PgTable} WHERE id=@Id";
		}
        else
			throw new NotImplementedException();

		using var cn = DbContext.DbCxn;
        
        int rowAffected = await cn.ExecuteAsync(sql, new { Id = id });
        return rowAffected;
    }

    public async Task<bool> IsDuplicateCodeAsync(int objectId, string objectCode)
    {
        SqlBuilder sbSql = new();
        DynamicParameters param = new();
        string sql = "";
        if (DbContext.DbType.Is(DatabaseTypes.MSSQL, DatabaseTypes.AZURE_SQL))
		{
			sbSql.Where("t.IsDeleted=0");
			sbSql.Where("t.Id<>@Id");
			sbSql.Where("UPPER(t.ObjectCode)=UPPER(@ObjectCode)");

			param.Add("@Id", objectId);
			param.Add("@ObjectCode", objectCode, DbType.AnsiString);

			sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;

		}
        else if (DbContext.DbType.Is(DatabaseTypes.POSTGRESQL))
		{
			sbSql.Where("t.is_deleted=false");
			sbSql.Where("t.id<>@id");
			sbSql.Where("UPPPER(t.object_code)=UPPER(@obj_code)");
			param.Add("@id", objectId);
			param.Add("@obj_code", objectCode, DbType.AnsiString);

			sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.PgTableName} t /**where**/").RawSql;
		}
        else
            throw new NotImplementedException();

        using var cn = DbContext.DbCxn;

        return (await cn.ExecuteScalarAsync<int>(sql, param)) > 0;
    }

    public async Task<int> GetExistingObjectIdByCodeAsync(int objectId, string objectCode)
    {
		SqlBuilder sbSql = new();
		DynamicParameters param = new();
		string sql = "";

		if (DbContext.DbType.Is(DatabaseTypes.AZURE_SQL, DatabaseTypes.MSSQL))
		{
			sbSql.Where("t.IsDeleted=0");
			sbSql.Where("t.Id<>@Id");
			sbSql.Where("UPPER(t.ObjectCode)=UPPER(@ObjectCode)");

			param.Add("@Id", objectId);
			param.Add("@ObjectCode", objectCode, DbType.AnsiString);

			sql = sbSql.AddTemplate($"SELECT t.Id FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
		}
		else if (DbContext.DbType == DatabaseTypes.POSTGRESQL)
		{
			sbSql.Where("t.is_deleted=false");
			sbSql.Where("t.id<>@id");
			sbSql.Where("LOWER(t.object_code)=@obj_code");

			param.Add("@id", objectId);
			param.Add("@obj_code", objectCode, DbType.AnsiString);

			sql = sbSql.AddTemplate($"SELECT t.id FROM {DbObject.PgTable} t /**where**/").RawSql;
		}

		using var cn = DbContext.DbCxn;

        int id = await cn.ExecuteScalarAsync<int>(sql, param);
        return id;
    }

    public async Task<List<DropdownSelectItem>> GetForDropdownSelectAsync(string? objectName = null, int? includingId = null)
    {
        SqlBuilder sbSql = new();

        using var cn = DbContext.DbCxn;
        
        List<DropdownSelectItem> result = new();
        DynamicParameters param = new();
        string sql;

		

        if (DbContext.DbType.Is(DatabaseTypes.POSTGRESQL))
        {
			sbSql.Select("t.id");
			sbSql.Select("t.object_code as \"key\"");
			sbSql.Select("t.object_name as \"value\"");

			if (includingId.HasValue)
			{
				param.Add("@including_id", includingId.Value);

				if (!string.IsNullOrEmpty(objectName))
				{
					sbSql.Where("(t.is_deleted=false AND LOWER(t.object_name) LIKE '%'+LOWER(@object_name)+'%') OR t.Id=@including_id");
					param.Add("@ObjectName", objectName, DbType.AnsiString);
				}
				else
				{
					sbSql.Where("t.is_deleted=false OR t.id=@including_id");
				}
			}
			else if (!string.IsNullOrEmpty(objectName))
			{
				sbSql.Where("t.is_deleted=false");
				sbSql.Where("LOWER(t.object_name) LIKE '%'+LOWER(@object_name)+'%'");
				param.Add("@object_name", objectName, DbType.AnsiString);
			}
			else
			{
				sbSql.Where("t.is_deleted=false");
			}

			sql = sbSql.AddTemplate($"SELECT /**select**/ FROM {DbObject.PgTable} t /**where**/ /**orderby**/").RawSql;
		}
        else if (DbContext.DbType.Is(DatabaseTypes.MSSQL, DatabaseTypes.AZURE_SQL))
        {
			sbSql.Select("t.Id");
			sbSql.Select("'Key'=t.ObjectCode");
			sbSql.Select("'Value'=t.ObjectName");

			if (includingId.HasValue)
			{
				param.Add("@IncludingId", includingId.Value);

				if (!string.IsNullOrEmpty(objectName))
				{
					sbSql.Where("(t.IsDeleted=0 AND LOWER(t.ObjectName) LIKE '%'+LOWER(@ObjectName)+'%') OR t.Id=@IncludingId");
					param.Add("@ObjectName", objectName, DbType.AnsiString);
				}
				else
				{
					sbSql.Where("t.IsDeleted=0 OR t.Id=@IncludingId");
				}
			}
			else if (!string.IsNullOrEmpty(objectName))
			{
				sbSql.Where("t.IsDeleted=0");
				sbSql.Where("LOWER(t.ObjectName) LIKE '%'+LOWER(@ObjectName)+'%'");
				param.Add("@ObjectName", objectName, DbType.AnsiString);
			}
			else
			{
				sbSql.Where("t.IsDeleted=0");
			}

			sbSql.OrderBy("t.ObjectName ASC");

			sql = sbSql.AddTemplate($"SELECT /**select**/ FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/").RawSql;
		}
        else
            throw new NotImplementedException();

        
        result = (await cn.QueryAsync<DropdownSelectItem>(sql, param)).AsList();

        return result;
    }
    #endregion

    public virtual List<string> GetSearchOrderbBy()
    {
		return DbContext.DbType switch
		{
			DatabaseTypes.POSTGRESQL => ["t.object_name ASC"],
			DatabaseTypes.AZURE_SQL or DatabaseTypes.MSSQL => ["t.ObjectName ASC"],
			_ => ["t.ObjectName ASC"],
		};
	}

	public virtual async Task<KeyValuePair<int, IEnumerable<TEntity>>> SearchNewAsync(
		int pgSize = 0, int pgNo = 0,
		string? searchText = null,
		IEnumerable<SqlSortCond>? sortConds = null,
		IEnumerable<SqlFilterCond>? filterConds = null,
		List<int>? excludeIdList = null
    )
    {
        await Task.CompletedTask; // Placeholder for async method signature, if needed
		throw new NotImplementedException();
    }

    public virtual async Task<List<TEntity>> QuickSearchAsync(
        int pgSize = 0,
        int pgNo = 0,
        string? searchText = null,
        List<int>? excludeIdList = null)
    {
        if (pgNo < 0 && pgSize < 0)
            throw new ArgumentOutOfRangeException(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        SqlBuilder sbSql = new();
        DynamicParameters param = new();
		string sql;

		if (DbContext.DbType.Is(DatabaseTypes.AZURE_SQL, DatabaseTypes.MSSQL))
        {
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

            foreach (string orderByClause in GetSearchOrderbBy())
				sbSql.OrderBy(orderByClause);

            // FORM SQL QUERY
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
					$"SELECT * FROM {DbObject.MsSqlTable} t WHERE t.Id IN (SELECT Id FROM pg) /**orderby**/").RawSql;
			}
		}
		else if (DbContext.DbType == DatabaseTypes.POSTGRESQL)
        {
			sbSql.Where("t.is_deleted=false");

			#region Form Search Conditions
			if (!string.IsNullOrEmpty(searchText))
			{
				if (searchText.StartsWith("id:", StringComparison.OrdinalIgnoreCase))
				{
					sbSql.Where("UPPER(t.object_code) LIKE '%'+UPPER(@search_text)+'%'");
					param.Add("@search_text", searchText.Replace("id:", "", StringComparison.OrdinalIgnoreCase), DbType.AnsiString);
				}
				else if (searchText.StartsWith("code:", StringComparison.OrdinalIgnoreCase))
				{
					sbSql.Where("UPPER(t.object_code) LIKE '%'+UPPER(@search_text)+'%'");
					param.Add("@search_text", searchText.Replace("code:", "", StringComparison.OrdinalIgnoreCase), DbType.AnsiString);
				}
				else
				{
					sbSql.Where("UPPER(t.object_name) LIKE '%'+UPPER(@search_text)+'%'");
					param.Add("@search_text", searchText, DbType.AnsiString);
				}
			}

			if (excludeIdList != null && excludeIdList.Count > 0)
			{
				sbSql.Where("t.id NOT IN @exclude_id_list");
				param.Add("@exclude_id_list", excludeIdList);
			}
			#endregion

			foreach (string orderByClause in GetSearchOrderbBy())
				sbSql.OrderBy(orderByClause);

			// FORM SQL QUERY
			if (pgNo == 0 && pgSize == 0)
			{
				sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.PgTable} t /**where**/ /**orderby**/").RawSql;
			}
			else
			{
				param.Add("@pg_size", pgSize);
				param.Add("@pg_no", pgNo);

				sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.PgTable} t /**where**/ /**orderby**/ LIMIT @pg_size OFFSET @pg_size * (@pg_no - 1)").RawSql;
			}
		}
		else
            throw new NotImplementedException();

        using var cn = DbContext.DbCxn;

        var dataList = (await cn.QueryAsync<TEntity>(sql, param)).AsList();

        return dataList;
    }

    public virtual async Task<DataPagination> GetQuickSearchPaginationAsync(int pgSize = 0, string? searchText = null,
        List<int>? excludeIdList = null)
    {
        if (pgSize < 0)
            throw new ArgumentOutOfRangeException(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        DynamicParameters param = new();
        SqlBuilder sbSql = new();
        string sql;


        if (DbContext.DbType.Is(DatabaseTypes.MSSQL, DatabaseTypes.AZURE_SQL))
        {
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

            sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
        }
        else if (DbContext.DbType.Is(DatabaseTypes.POSTGRESQL))
		{
			sbSql.Where("t.is_deleted=false");

			#region Form Search Conditions
			if (!string.IsNullOrEmpty(searchText))
			{
				if (searchText.StartsWith("id:", StringComparison.OrdinalIgnoreCase))
				{
					sbSql.Where("UPPER(t.object_code) LIKE '%'+UPPER(@search_text)+'%'");
					param.Add("@SearchText", searchText.Replace("id:", "", StringComparison.OrdinalIgnoreCase), DbType.AnsiString);
				}
				else if (searchText.StartsWith("code:", StringComparison.OrdinalIgnoreCase))
				{
					sbSql.Where("UPPER(t.object_code) LIKE '%'+UPPER(@search_text)+'%'");
					param.Add("@SearchText", searchText.Replace("code:", "", StringComparison.OrdinalIgnoreCase), DbType.AnsiString);
				}
				else
				{
					sbSql.Where("UPPER(t.object_name) LIKE '%'+UPPER(@search_text)+'%'");
					param.Add("@search_text", searchText, DbType.AnsiString);
				}
			}

			if (excludeIdList != null && excludeIdList.Count > 0)
			{
				sbSql.Where("t.id NOT IN @exclude_id_list");
				param.Add("@exclude_id_list", excludeIdList);
			}
			#endregion

			sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.PgTable} t /**where**/").RawSql;
		}
        else
            throw new NotImplementedException();

        using var cn = DbContext.DbCxn;

		decimal recordCount = await cn.ExecuteScalarAsync<int>(sql, param);
		int pageCount = (int)(Math.Ceiling(recordCount / (pgSize == 0 ? 1 : pgSize)));

        DataPagination pagination = new()
        {
            ObjectType = typeof(TEntity).Name,
            PageSize = pgSize,
            PageCount = pageCount,
            RecordCount = (int)recordCount
        };

        return pagination;
    }

    public virtual async Task<List<TEntity>> SearchAsync(int pgSize = 0, int pgNo = 0, 
        string? objectCode = null, 
        string? objectName = null)
    {
        if (pgNo < 0 && pgSize < 0)
            throw new ArgumentOutOfRangeException(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        DynamicParameters param = new();
        SqlBuilder sbSql = new();
        string sql;

		if (DbContext.DbType.Is(DatabaseTypes.AZURE_SQL, DatabaseTypes.MSSQL))
		{
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
			#endregion

			foreach (string orderByClause in GetSearchOrderbBy())
				sbSql.OrderBy(orderByClause);

			if (pgNo == 0 && pgSize == 0)
			{
				sql = sbSql.AddTemplate($"SELECT t.* FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/").RawSql;
			}
			else
			{
				param.Add("@PageSize", pgSize);
				param.Add("@PageNo", pgNo);

				sql = sbSql.AddTemplate(
					$";WITH pg AS (SELECT t.Id FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
					$"SELECT t.* FROM {DbObject.MsSqlTable} t INNER JOIN pg p ON p.Id=t.Id /**orderby**/").RawSql;
			}
		}
		else if (DbContext.DbType == DatabaseTypes.POSTGRESQL)
		{
			sbSql.Where("t.is_deleted=false");

			#region Form Search Conditions
			if (!string.IsNullOrEmpty(objectCode))
			{
				sbSql.Where("UPPER(t.object_code) LIKE '%'+UPPER(@obj_code)+'%'");
				param.Add("@obj_code", objectCode, DbType.AnsiString);
			}

			if (!string.IsNullOrEmpty(objectName))
			{
				sbSql.Where("LOWER(t.object_name) LIKE '%'+LOWER(@obj_name)+'%'");
				param.Add("@obj_name", objectName, DbType.AnsiString);
			}
			#endregion

			foreach (string orderByClause in GetSearchOrderbBy())
				sbSql.OrderBy(orderByClause);

			if (pgNo == 0 && pgSize == 0)
			{
				sql = sbSql.AddTemplate($"SELECT t.* FROM {DbObject.PgTable} t /**where**/ /**orderby**/").RawSql;
			}
			else
			{
				param.Add("@pg_size", pgSize);
				param.Add("@pg_no", pgNo);

				sql = sbSql.AddTemplate(
					$"SELECT * FROM {DbObject.PgTable} t /**where**/ /**orderby**/ LIMIT @pg_size OFFSET @pg_size * (@pg_no - 1)").RawSql;
			}
		}
		else
			throw new NotImplementedException();

		using var cn = DbContext.DbCxn;

        var dataList = (await cn.QueryAsync<TEntity>(sql, param)).AsList();

        return dataList;
    }

    public virtual async Task<DataPagination> GetSearchPaginationAsync(int pgSize = 0, 
        string? objectCode = null, 
        string? objectName = null)
    {
        if (pgSize < 0)
            throw new ArgumentOutOfRangeException(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        DynamicParameters param = new();
        SqlBuilder sbSql = new();
        string sql;

        if (DbContext.DbType.Is(DatabaseTypes.MSSQL, DatabaseTypes.AZURE_SQL))
        {
			sbSql.Where("t.IsDeleted=0");

			#region Form Search Conditions
			if (!string.IsNullOrEmpty(objectCode))
			{
				sbSql.Where("t.ObjectCode LIKE @ObjectCode+'%'");
				param.Add("@ObjectCode", objectCode, DbType.AnsiString);
			}

			if (!string.IsNullOrEmpty(objectName))
			{
				sbSql.Where("LOWER(t.ObjectName) LIKE '%'+LOWER(@ObjectName)+'%'");
				param.Add("@ObjectName", objectName, DbType.AnsiString);
			}
			#endregion

			sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
		}
        else if (DbContext.DbType.Is(DatabaseTypes.POSTGRESQL))
        {
			sbSql.Where("t.is_deleted=false");

			#region Form Search Conditions
			if (!string.IsNullOrEmpty(objectCode))
			{
				sbSql.Where("t.object_code LIKE @obj_code+'%'");
				param.Add("@obj_code", objectCode, DbType.AnsiString);
			}

			if (!string.IsNullOrEmpty(objectName))
			{
				sbSql.Where("LOWER(t.object_name) LIKE '%'+LOWER(@obj_name)+'%'");
				param.Add("@obj_name", objectName, DbType.AnsiString);
			}
			#endregion

			sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.PgTable} t /**where**/").RawSql;
		}
        else
            throw new NotImplementedException();
        
        using var cn = DbContext.DbCxn;

		decimal recordCount = await cn.ExecuteScalarAsync<int>(sql, param);
		int pageCount = (int)(Math.Ceiling(recordCount / (pgSize == 0 ? 1 : pgSize)));

        DataPagination pagination = new()
        {
            ObjectType = typeof(TEntity).Name,
            PageSize = pgSize,
            PageCount = pageCount,
            RecordCount = (int)recordCount
        };

        return pagination;
    }

    #region INTERNAL FUNCTIONS
    public async Task<string?> GetRunningNumberAsync(IDbConnection cn, RunNumGenParam rngParam, IDbTransaction? tran = null)
    {
        #region RUNNING NUMBER GENERATION
        SqlBuilder sbSqlRngCounter = new();

        sbSqlRngCounter.Where("rngc.IsDeleted=0");
        sbSqlRngCounter.Where("rngc.IsCurrent=1");
        sbSqlRngCounter.Where("rng.ObjectClassName=@ObjectClassName");
        sbSqlRngCounter.LeftJoin($"{RunNumGenerator.MsSqlTable} rng ON rng.Id=rngc.RunningNumberGeneratorId");

        string sqlRngCounter = sbSqlRngCounter.AddTemplate($"SELECT * FROM {RunNumGeneratorCounter.MsSqlTable} rngc /**leftjoin**/ /**where**/").RawSql;

        DynamicParameters rngCounterParam = new();
        rngCounterParam.Add("@ObjectClassName", rngParam.ObjectClassName, DbType.AnsiString);

        var rngCounter = (await cn.QueryAsync<RunNumGeneratorCounter, RunNumGenerator, RunNumGeneratorCounter>(
                            sqlRngCounter, (rngCounter, rng) =>
                            {
                                rngCounter.RunningNumberGenerator = rng;
                                return rngCounter;
                            }, rngCounterParam, splitOn: "Id", transaction: tran)).FirstOrDefault();

        if (rngCounter == null)
            throw new Exception("Running Number Generator for customer is not found.");

        SqlBuilder sbCmdRngUpd = new();
        sbCmdRngUpd.Set("CurrentNumber=CurrentNumber+1");
        sbCmdRngUpd.Set("ModifiedUser=@ModifiedUser");
        sbCmdRngUpd.Set("ModifiedDateTime=@ModifiedDateTime");
        sbCmdRngUpd.Where("IsDeleted=0");
        sbCmdRngUpd.Where("Id=@RngCounterId");

        string rngUpdCmd = sbCmdRngUpd.AddTemplate($"UPDATE {RunNumGeneratorCounter.MsSqlTable} /**set**/ OUTPUT inserted.CurrentNumber /**where**/").RawSql;

        DynamicParameters rngUpdCmdParam = new();
        rngUpdCmdParam.Add("@RngCounterId", rngCounter.Id);
        rngUpdCmdParam.Add("@ModifiedUser", rngParam.UserName);
        rngUpdCmdParam.Add("@ModifiedDateTime", DateTime.UtcNow.AddHours(7));

        int rngCounterNumber = await cn.ExecuteScalarAsync<int>(rngUpdCmd, rngUpdCmdParam, tran);
        StringBuilder runningNumber = new();
        runningNumber.Append(rngCounter.RunningNumberGenerator!.Prefix.NonNullValue());
        if (rngCounter.IntervalDay.HasValue)
        {
            runningNumber.Append(rngCounter.IntervalYear!.Value / 2000);
            runningNumber.Append(rngCounter.IntervalMonth!.Value.ToString("00"));
            runningNumber.Append(rngCounter.IntervalDay.Value.ToString("00"));
            runningNumber.Append(rngCounterNumber.ToString("0000"));
        }
        else if (rngCounter.IntervalMonth.HasValue)
        {
            runningNumber.Append(rngCounter.IntervalYear!.Value % 2000);
            runningNumber.Append(rngCounter.IntervalMonth!.Value.ToString("00"));
            runningNumber.Append(rngCounterNumber.ToString("000000"));
        }
        else if (rngCounter.IntervalYear.HasValue)
        {
            runningNumber.Append(rngCounter.IntervalYear!.Value % 2000);
            runningNumber.Append(rngCounterNumber.ToString("00000000"));
        }
        else
            runningNumber.Append(rngCounterNumber.ToString("00000000"));

        runningNumber.Append(rngCounter.RunningNumberGenerator!.Suffix.NonNullValue());
        
        return runningNumber.ToString();
        #endregion
    }

    private static List<string> GetPropertiesList(IEnumerable<PropertyInfo> properties)
    {
        return (from prop in properties
                let attributes = prop.GetCustomAttributes(typeof(DescriptionAttribute), false)
                where attributes.Length <= 0 || (attributes[0] as DescriptionAttribute)?.Description != "ignore"
				select prop.Name).AsList();
    }

    private string GenerateGetByObjectCodeQuery()
    {
        var properties = GetPropertiesList(ObjectProperties);

        int hasField = properties.Count(x => x == "ObjectCode");

        if (hasField > 0)
            return $"SELECT * FROM {DbObject.MsSqlTable} WHERE IsDeleted=0 and ObjectCode=@ObjectCode";
        else
            return "";
    }

    private string GenerateDuplicateCodeQuery()
    {
        var properties = GetPropertiesList(ObjectProperties);

        int hasField = properties.Count(x => x == "ObjectCode");

        if (hasField > 0)
            return $"SELECT COUNT(*) FROM {DbObject.MsSqlTable} WHERE IsDeleted=0 AND Id<>@Id AND UPPER(ObjectCode)=UPPER(@ObjectCode)";
        else
            return "";
    }

    private string GenerateExistingObjectIdQuery()
    {
        var properties = GetPropertiesList(ObjectProperties);

        int hasField = properties.Count(x => x == "ObjectCode");

        if (hasField > 0)
            return $"SELECT TOP 1 Id FROM {DbObject.MsSqlTable} WHERE IsDeleted=0 AND Id<>@Id AND ObjectCode=@ObjectCode ORDER BY Id DESC";
        else
            return "";
    }

    private string GenerateInsertQuery()
    {
        var insertQuery = new StringBuilder($"INSERT INTO {DbObject.MsSqlTable} ");

        insertQuery.Append('(');

        var properties = GetPropertiesList(ObjectProperties);
        properties.ForEach(property =>
        {
            if (!property.Equals("Id", StringComparison.OrdinalIgnoreCase) && !property.Equals("ObjectId", StringComparison.OrdinalIgnoreCase))
                insertQuery.Append($"[{property}],");
        });

        insertQuery
            .Remove(insertQuery.Length - 1, 1)
            .Append(") VALUES (");

        properties.ForEach(property =>
        {
            if (!property.Equals("Id", StringComparison.OrdinalIgnoreCase) && !property.Equals("ObjectId", StringComparison.OrdinalIgnoreCase))
            {
                insertQuery.Append($"@{property},");
            }
        });

        insertQuery
            .Remove(insertQuery.Length - 1, 1)
            .Append("); SELECT CAST(SCOPE_IDENTITY() as int)");

        return insertQuery.ToString();
    }

    private string GenerateUpdateQuery()
    {
        var updateQuery = new StringBuilder($"UPDATE {DbObject.MsSqlTable} SET ");
        var properties = GetPropertiesList(ObjectProperties);

        properties.ForEach(property =>
        {
            if (!property.Equals("Id", StringComparison.OrdinalIgnoreCase) && !property.Equals("ObjectId", StringComparison.OrdinalIgnoreCase))
            {
                updateQuery.Append($"{property}=@{property},");
            }
        });

        updateQuery.Remove(updateQuery.Length - 1, 1); //remove last comma
        updateQuery.Append(" WHERE Id=@Id");

        return updateQuery.ToString();
    }
    #endregion
}