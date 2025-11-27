using DataLayer.Models.EMS;

namespace DataLayer.Repos.EMS;

public interface IEventTypeRepos : IBaseRepos<EventType>
{
	Task<EventType?> GetFullAsync(int id);
	Task<int> InsertFullAsync(EventType obj);
	Task<bool> UpdateFullAsync(EventType obj);

	Task<List<EventType>> SearchAsync(
		int pgSize = 0, int pgNo = 0,
		string? objectCode = null,
		string? objectName = null,
		string? objectNameKh = null,
		bool? isEnabled = null);

	Task<DataPagination> GetSearchPaginationAsync(
		int pgSize = 0,
		string? objectCode = null,
		string? objectName = null,
		string? objectNameKh = null,
		bool? isEnabled = null);
}

public class EventTypeRepos(IDbContext dbContext) : BaseRepos<EventType>(dbContext, EventType.DatabaseObject), IEventTypeRepos
{
	public async Task<EventType?> GetFullAsync(int id)
    {
        string sql = $"SELECT * FROM {DbObject.MsSqlTable} t WHERE t.IsDeleted=0 AND t.Id=@Id; " +
                     $"SELECT * FROM {EventOrganizerRole.MsSqlTable} eor ON eor.IsDeleted=0 AND eor.EventTypeId=@Id; ";

        var param = new { Id = id };

        using var cn = DbContext.DbCxn;

        EventType? data = null;

        using (var multi = await cn.QueryMultipleAsync(sql, param))
        {
            data = await multi.ReadSingleOrDefaultAsync<EventType>();
            
            if (data != null)
            {
                data.ValidRoles = (await multi.ReadAsync<EventOrganizerRole>()).AsList();
            }
        }

        return data;
    }
    public async Task<int> InsertFullAsync(EventType obj)
    {
        using var cn = DbContext.DbCxn;
        // <!IMPORTANT> Connection required to be open before calling BeginTransaction() function
        if (cn.State != ConnectionState.Open) cn.Open();
        using var tran = cn.BeginTransaction();
        try
        {
            DateTime khTimestamp = DateTime.UtcNow.AddHours(7);

            obj.CreatedDateTime = khTimestamp;
            obj.ModifiedDateTime = khTimestamp;

            int objId = await cn.InsertAsync(obj, tran);

            if (objId>0 && obj.ValidRoles != null && obj.ValidRoles.Any())
            {
                foreach (EventOrganizerRole eor in obj.ValidRoles)
                {
                    eor.EventTypeId = objId;
                    eor.CreatedUser = obj.CreatedUser;
                    eor.CreatedDateTime = obj.CreatedDateTime;
                    eor.ModifiedUser = obj.ModifiedUser;
                    eor.ModifiedDateTime = obj.ModifiedDateTime;
                    int eorId = await cn.InsertAsync(eor, tran);
                }
            }

            tran.Commit();
            return objId;
        }
        catch
        {
            tran.Rollback();
            throw;
        }
    }

    public async Task<bool> UpdateFullAsync(EventType obj)
    {
        using var cn = DbContext.DbCxn;
        // <!IMPORTANT> Connection required to be open before calling BeginTransaction() function
        if (cn.State != ConnectionState.Open) cn.Open();
        using var tran = cn.BeginTransaction();
        try
        {
            DateTime khTimestamp = DateTime.UtcNow.AddHours(7);
            obj.ModifiedDateTime = khTimestamp;

            bool isUpdated = await cn.UpdateAsync(obj, tran);

            if (obj.ValidRoles != null && obj.ValidRoles.Any())
            {
                foreach (EventOrganizerRole eor in obj.ValidRoles)
                {
                    if (eor.Id <= 0)
                    {
                        eor.EventTypeId = obj.Id;
                        eor.CreatedUser = obj.ModifiedUser;
                        eor.CreatedDateTime = obj.ModifiedDateTime;
                        eor.ModifiedUser = obj.ModifiedUser;
                        eor.ModifiedDateTime = obj.ModifiedDateTime;
                        int eorId = await cn.InsertAsync(eor, tran);
                    }
                    else
                    {
                        eor.ModifiedUser = obj.ModifiedUser;
                        eor.ModifiedDateTime = obj.ModifiedDateTime;
                        bool isEorUpdated = await cn.UpdateAsync(eor, tran);
                    }
                }
            }

            tran.Commit();
            return isUpdated;
        }
        catch
        {
            tran.Rollback();
            throw;
        }
    }

    public async Task<List<EventType>> SearchAsync(
        int pgSize = 0, int pgNo = 0,
        string? objectCode = null,
        string? objectName = null,
        string? objectNameKh = null,
        bool? isEnabled = null)
    {
        if (pgNo < 0 && pgSize < 0)
            throw new ArgumentOutOfRangeException(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

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

        if (!string.IsNullOrEmpty(objectNameKh))
        {
            sbSql.Where("t.ObjectNameKh LIKE '%'+@ObjectNameKh+'%'");
            param.Add("@NameEn", objectNameKh, DbType.String);
        }

        if (isEnabled != null)
        {
            sbSql.Where("t.IsEnabled=@IsEnabled");
            param.Add("@IsEnabled", isEnabled.Value, DbType.Boolean);
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
            sql = sbSql.AddTemplate(
                $";WITH pg AS (SELECT Id FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
                $"SELECT t.* FROM {DbObject.MsSqlTable} t INNER JOIN pg p ON p.Id=t.Id /**orderby**/").RawSql;
        }

        using var cn = DbContext.DbCxn;

        List<EventType> result = (await cn.QueryAsync<EventType>(sql, param)).AsList();

        return result;
    }

    public async Task<DataPagination> GetSearchPaginationAsync(
        int pgSize = 0,
        string? objectCode = null,
        string? objectName = null,
        string? objectNameKh = null,
        bool? isEnabled = null)
    {
        if (pgSize < 0)
            throw new ArgumentOutOfRangeException(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

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

        if (!string.IsNullOrEmpty(objectNameKh))
        {
            sbSql.Where("t.ObjectNameKh LIKE '%'+@ObjectNameKh+'%'");
            param.Add("@NameEn", objectNameKh, DbType.String);
        }

        if (isEnabled != null)
        {
            sbSql.Where("t.IsEnabled=@IsEnabled");
            param.Add("@IsEnabled", isEnabled.Value, DbType.Boolean);
        }
        #endregion

        string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;

        using var cn = DbContext.DbCxn;

        decimal recordCount = await cn.ExecuteScalarAsync<int>(sql, param);
        int pageCount = (int)Math.Ceiling(recordCount / (pgSize == 0 ? 1 : pgSize));

        DataPagination pagination = new()
        {
            ObjectType = typeof(EventType).Name,
            PageSize = pgSize,
            PageCount = pageCount,
            RecordCount = (int)recordCount
        };

        return pagination;
    }
}