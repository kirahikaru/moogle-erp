using DataLayer.GlobalConstant;
using DataLayer.Models.EMS;
using DataLayer.Models.EMS.NonPersistent;
using static Dapper.SqlMapper;

namespace DataLayer.Repos.EMS;

public interface IEventRepos : IBaseRepos<Event>
{
	Task<Event?> GetFullAsync(int id);

	Task<List<Event>> SearchAsync(
		int pgSize = 0,
		int pgNo = 0,
		string? objectCode = null,
		string? objectName = null,
		List<string>? feeTypeCodeList = null,
		List<int>? eventTypeIdList = null,
		DateTime? fromDateTime = null,
		DateTime? toDateTime = null);

	Task<DataPagination> GetSearchPaginationAsync(
		int pgSize = 0,
		string? objectCode = null,
		string? objectName = null,
		List<string>? feeTypeCodeList = null,
		List<int>? eventTypeIdList = null,
		DateTime? fromDateTime = null,
		DateTime? toDateTime = null);

	Task<int> SaveAndTransitWorkflowAsync(Event obj, WorkflowTransitionDetail wtd);

	Task<List<DropdownSelectItem>> GetValidEventForInvitationAsync(string? searchText = null);
	Task<List<DropdownSelectItem>> GetValidEventForRegistrationAsync(string? searchText = null);

	Task<EventRegSumm?> GetEventRegistrationSummaryAsync(int eventId);
	Task<List<EventOtherFeeItem>> GetOtherFeeItemsAsync(int eventId);
}

public class EventRepos(IDbContext dbContext) : BaseRepos<Event>(dbContext, Event.DatabaseObject), IEventRepos
{
	public async Task<Event?> GetFullAsync(int id)
    {
        SqlBuilder sbSql = new();

        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.Id=@Id");

        sbSql.LeftJoin($"{EventType.MsSqlTable} et ON et.Id=t.EventTypeId");
        sbSql.LeftJoin($"{Currency.MsSqlTable} fc ON fc.IsDeleted=0 AND fc.ObjectCode=t.FeeCurrencyCode");

        string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;

        //string itemQry = $"SELECT ei.* FROM {EventInvitation.MsSqlTable} ei WHERE ei.IsDeleted=0 AND ei.EventId=@Id; " +
        //                 $"SELECT er.* FROM {EventRegistration.MsSqlTable} er WHERE er.IsDeleted=0 AND er.EventId=@Id;";

        var param = new { Id = id };

        using var cn = DbContext.DbCxn;
        
        var dataList = (await cn.QueryAsync<Event, EventType, Currency, Event>(sql, (obj, et, curr) =>
        {
            obj.EventType = et;
            obj.FeeCurrency = curr;
            return obj;
        }, param, splitOn: "Id")).AsList();
        
        if (dataList != null && dataList.Count != 0)
        {
            //using var multi = await cn.QueryMultipleAsync(itemQry, param);

            //dataList[0].Invitations = (await multi.ReadAsync<EventInvitation>()).AsList();
            //dataList[0].Registrations = (await multi.ReadAsync<EventRegistration>()).AsList();

            return dataList[0];
        }

        return null;
    }

    public override async Task<List<Event>> QuickSearchAsync(int pgSize = 0, int pgNo = 0, string? searchText = null, List<int>? excludeIdList = null)
    {
        if (pgNo < 0 && pgSize < 0)
            throw new ArgumentOutOfRangeException(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        DynamicParameters param = new();
        SqlBuilder sbSql = new();
        sbSql.Where("t.IsDeleted=0");

        #region Form Search Conditions
        if (!string.IsNullOrEmpty(searchText))
        {
            sbSql.Where("(UPPER(t.ObjectCode) LIKE '%'+UPPER(@SearchText)+'%' OR UPPER(t.ObjectName) LIKE '%'+UPPER(@SearchText)+'%')");
            param.Add("@SearchText", searchText, DbType.AnsiString);
        }

        if (excludeIdList != null && excludeIdList.Count > 0)
        {
            sbSql.Where("t.Id NOT IN @ExcludeIdList");
            param.Add("@ExcludeIdList", excludeIdList);
        }
        #endregion

        sbSql.LeftJoin($"{EventType.MsSqlTable} et ON et.Id=t.EventTypeId");

        sbSql.OrderBy("t.StartDateTime DESC, t.EndDateTime DESC");

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
                $"SELECT t.*, et.* FROM {DbObject.MsSqlTable} t INNER JOIN pg p ON p.Id=t.Id /**leftjoin**/ /**orderby**/").RawSql;
        }

        using var cn = DbContext.DbCxn;

        var result = (await cn.QueryAsync<Event, EventType, Event>(sql,
                                (e, et) =>
                                {
                                    e.EventType = et;
                                    return e;
                                }, param, splitOn: "Id")).AsList();

        return result;
    }

    public async Task<List<Event>> SearchAsync(
        int pgSize = 0,
        int pgNo = 0,
        string? objectCode = null,
        string? objectName = null,
        List<string>? feeTypeCodeList = null,
        List<int>? eventTypeIdList = null,
        DateTime? fromDateTime = null,
        DateTime? toDateTime = null)
    {
        if (pgNo < 0 && pgSize < 0)
            throw new ArgumentOutOfRangeException(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        DynamicParameters param = new();
        SqlBuilder sbSql = new();

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

        if (feeTypeCodeList != null && feeTypeCodeList.Any())
        {
            if (feeTypeCodeList.Count == 1)
            {
                sbSql.Where("t.FeeTypeCode=@FeeTypeCode");
                param.Add("@FeeTypeCode", feeTypeCodeList[0], DbType.AnsiString);
            }
            else
            {
                sbSql.Where("t.FeeTypeCode IN @FeeTypeCodeList");
                param.Add("@FeeTypeCodeList", feeTypeCodeList);
            }
        }

        if (eventTypeIdList != null && eventTypeIdList.Any())
        {
            if (eventTypeIdList.Count == 1)
            {
                sbSql.Where("t.EventTypeId=@EventTypeId");
                param.Add("@EventTypeId", eventTypeIdList[0]);
            }
            else
            {
                sbSql.Where("t.EventTypeId IN @EventTypeIdList");
                param.Add("@EventTypeIdList", eventTypeIdList);
            }
        }

        if (fromDateTime != null)
        {
            sbSql.Where("t.EndDateTime IS NOT NULL");
            sbSql.Where("t.EndDateTime>=@FromDateTime");
            param.Add("@FromDateTime", fromDateTime.Value);

            
        }
        if (toDateTime != null)
        {
            sbSql.Where("t.StartDateTime IS NOT NULL");
            sbSql.Where("t.StartDateTime<=@ToDateTime");
            param.Add("@ToDateTime", toDateTime.Value);
        }
        #endregion

        sbSql.LeftJoin($"{EventType.MsSqlTable} et ON et.Id=t.EventTypeId");

        sbSql.OrderBy("t.StartDateTime DESC, t.EndDateTime DESC");
        
        string sql;

        if (pgNo == 0 && pgSize == 0)
        {
            sql = sbSql.AddTemplate($"SELECT t.*, et.* FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/ /**orderby**/").RawSql;
        }
        else
        {
            param.Add("@PageSize", pgSize);
            param.Add("@PageNo", pgNo);

            sql = sbSql.AddTemplate(
                $";WITH pg AS (SELECT Id FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
                $"SELECT t.*, et.* FROM {DbObject.MsSqlTable} t INNER JOIN pg p ON p.Id=t.Id /**leftjoin**/ /**orderby**/").RawSql;
        }

        using var cn = DbContext.DbCxn;

        var dataList = (await cn.QueryAsync<Event, EventType, Event>(sql,
                                (e, et) =>
                                {
                                    e.EventType = et;

                                    return e;
                                }, param, splitOn: "Id")).AsList();

        return dataList;
    }

    public async Task<DataPagination> GetSearchPaginationAsync(
        int pgSize = 0,
        string? objectCode = null,
        string? objectName = null,
        List<string>? feeTypeCodeList = null,
        List<int>? eventTypeIdList = null,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        if (pgSize < 0)
            throw new ArgumentOutOfRangeException(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        DynamicParameters param = new();
        SqlBuilder sbSql = new();

        sbSql.Where("t.IsDeleted=0");

        #region Form Search Conditions
        if (!string.IsNullOrEmpty(objectCode))
        {
            sbSql.Where("t.ObjectCode LIKE @ObjectCode+'%'");
            param.Add("@ObjectCode", objectCode, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(objectName))
        {
            sbSql.Where("LOWER(t.ObjectName) LIKE '%'+@ObjectName+'%'");
            param.Add("@ObjectName", objectName.ToLower(), DbType.AnsiString);
        }

        if (feeTypeCodeList != null && feeTypeCodeList.Any())
        {
            if (feeTypeCodeList.Count == 1)
            {
                sbSql.Where("t.FeeTypeCode=@FeeTypeCode");
                param.Add("@FeeTypeCode", feeTypeCodeList[0], DbType.AnsiString);
            }
            else
            {
                sbSql.Where("t.FeeTypeCode IN @FeeTypeCodeList");
                param.Add("@FeeTypeCodeList", feeTypeCodeList);
            }
        }

        if (eventTypeIdList != null && eventTypeIdList.Any())
        {
            if (eventTypeIdList.Count == 1)
            {
                sbSql.Where("t.EventTypeId=@EventTypeId");
                param.Add("@EventTypeId", eventTypeIdList[0]);
            }
            else
            {
                sbSql.Where("t.EventTypeId IN @EventTypeIdList");
                param.Add("@EventTypeIdList", eventTypeIdList);
            }
        }

        if (fromDate != null)
        {
            sbSql.Where("t.EndDateTime IS NOT NULL");
            sbSql.Where("t.EndDateTime>=@FromDateTime");
            param.Add("@FromDateTime", fromDate.Value);


        }
        if (toDate != null)
        {
            sbSql.Where("t.StartDateTime IS NOT NULL");
            sbSql.Where("t.StartDateTime<=@ToDate");
            param.Add("@ToDate", toDate.Value);
        }
        #endregion

        string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;

        using var cn = DbContext.DbCxn;

        decimal recordCount = await cn.ExecuteScalarAsync<int>(sql, param);
        int pageCount = (int)Math.Ceiling(recordCount / (pgSize == 0 ? 1 : pgSize));

        DataPagination pagination = new()
        {
            ObjectType = typeof(Event).Name,
            PageSize = pgSize,
            PageCount = pageCount,
            RecordCount = (int)recordCount
        };

        return pagination;
    }

    public async Task<int> SaveAndTransitWorkflowAsync(Event obj, WorkflowTransitionDetail wtd)
    {
        if (string.IsNullOrEmpty(wtd.CurrentWorkflowStatus) || string.IsNullOrEmpty(wtd.WorkflowAction))
            throw new Exception("Current Workflow Status and Workflow Action cannot be null.");
        else if (!EventWorkflowController.IsValidWorkflowTransit(wtd.CurrentWorkflowStatus!, wtd.WorkflowAction!))
            throw new Exception($"Invalid current workflow status ({wtd.CurrentWorkflowStatus}) and workflow action ({wtd.WorkflowAction}) combination.");

        string endWorkflowStatus = EventWorkflowController.GetResultingWorkflowStatus(wtd.WorkflowAction!);

        if (string.IsNullOrEmpty(endWorkflowStatus))
            throw new Exception($"Invalid workflow action ({wtd.WorkflowAction})");

        using var cn = DbContext.DbCxn;
        
        DateTime khTimestamp = DateTime.UtcNow.AddHours(7);
        // <!IMPORTANT> Connection required to be open before calling BeginTransaction() function
        if (cn.State != ConnectionState.Open) cn.Open();

        using var tran = cn.BeginTransaction();
        try
        {
            int objId = 0;
            obj.WorkflowStatus = endWorkflowStatus;
            obj.ModifiedDateTime = khTimestamp;

            if (obj.Id > 0) // UPDATE
            {
                WorkflowHistory wh = new()
                {
                    LinkedObjectId = obj.Id,
                    LinkedObjectType = obj.GetType().Name,
                    UserId = wtd.TargetUserId,
                    StartStatus = wtd.CurrentWorkflowStatus,
                    EndStatus = endWorkflowStatus,
                    OrgStructId = wtd.TargetUser?.OrgStructId,
                    Action = wtd.WorkflowAction,
                    CreatedUser = obj.ModifiedUser,
                    CreatedDateTime = obj.ModifiedDateTime,
                    ModifiedUser = obj.ModifiedUser,
                    ModifiedDateTime = obj.ModifiedDateTime,
                    Remark = wtd.TransitionRemark
                };

                int workflowHistoryId = await cn.InsertAsync(wh, tran);

                bool isUpdated = await cn.UpdateAsync(obj, tran);
                objId = isUpdated ? obj.Id : 0;
            }
            else     // INSERT
            {
                objId = await cn.InsertAsync(obj, tran);

                if (objId > 0)
                {
                    WorkflowHistory wh = new()
                    {
                        LinkedObjectId = objId,
                        LinkedObjectType = obj.GetType().Name,
                        UserId = wtd.TargetUserId,
                        StartStatus = wtd.CurrentWorkflowStatus,
                        EndStatus = endWorkflowStatus,
                        OrgStructId = wtd.TargetUser?.OrgStructId,
                        Action = wtd.WorkflowAction,
                        CreatedUser = obj.CreatedUser,
                        CreatedDateTime = obj.CreatedDateTime,
                        ModifiedUser = obj.ModifiedUser,
                        ModifiedDateTime = obj.ModifiedDateTime,
                        Remark = wtd.TransitionRemark
                    };

                    int workflowHistoryId = await cn.InsertAsync(wh, tran);

                    if (workflowHistoryId < 0)
                        throw new Exception("Failed to insert worklfow history.");
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

    public async Task<List<DropdownSelectItem>> GetValidEventForInvitationAsync(string? searchText = null)
    {
        SqlBuilder sbSql = new();

        sbSql.Select("t.Id")
            .Select("'Key'=t.ObjectCode")
            .Select("'Value'=t.ObjectName");

        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.WorkflowStatus=@WorkflowStatus");

        string sql = sbSql.AddTemplate($"SELECT /**select**/ FROM {DbObject.MsSqlTable} t /**where**/").RawSql;

        DynamicParameters param = new();
        param.Add("@WorkflowStatus", WorkflowStatuses.INVITATION_OPEN);
        
        if (!string.IsNullOrEmpty(searchText))
        {
            sbSql.Where("UPPER(t.ObjectName) LIKE '%'+UPPER(@ObjectName)+'%'");
            param.Add("@ObjectName", searchText, DbType.AnsiString);
        }

        sbSql.OrderBy("t.ObjectName ASC");

        using var cn = DbContext.DbCxn;

        List<DropdownSelectItem> dataList = (await cn.QueryAsync<DropdownSelectItem>(sql, param)).AsList();

        return dataList;
    }

    public async Task<List<DropdownSelectItem>> GetValidEventForRegistrationAsync(string? searchText = null)
    {
        SqlBuilder sbSql = new();

        sbSql.Select("t.Id")
            .Select("'Key'=t.ObjectCode")
            .Select("'Value'=t.ObjectName");

        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.WorkflowStatus=@WorkflowStatus");

        string sql = sbSql.AddTemplate($"SELECT /**select**/ FROM {DbObject.MsSqlTable} t /**where**/").RawSql;

        DynamicParameters param = new();
        param.Add("@WorkflowStatus", WorkflowStatuses.REGISTRATION_OPEN);

        if (!string.IsNullOrEmpty(searchText))
        {
            sbSql.Where("UPPER(t.ObjectName) LIKE '%'+UPPER(@ObjectName)+'%'");
            param.Add("@ObjectName", searchText, DbType.AnsiString);
        }

        sbSql.OrderBy("t.ObjectName ASC");

        using var cn = DbContext.DbCxn;

        var dataList = (await cn.QueryAsync<DropdownSelectItem>(sql, param)).AsList();

        return dataList;
    }

    public async Task<EventRegSumm?> GetEventRegistrationSummaryAsync(int eventId)
    {
		string sql = "EXEC ems.[GetEventRegistrationSummary] @EventId";
        
        using var cn = DbContext.DbCxn;

        var obj = (await cn.QuerySingleOrDefaultAsync<EventRegSumm>(sql, new { EventId = eventId }));

        return obj;
    }

    public async Task<List<EventOtherFeeItem>> GetOtherFeeItemsAsync(int eventId)
    {
        SqlBuilder sbSql = new();

        sbSql.Select("t.FeeCurrencyCode")
            .Select("'FeeCurrencySymbol'=curr.CurrencySymbol")
            .Select("'TotalFeeAmount'=SUM(t.FeeAmountPaid)");

        sbSql.LeftJoin($"{Currency.MsSqlTable} curr ON curr.IsDeleted=0 AND curr.ObjectCode=t.FeeCurrencyCode");

        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.FeeCurrencyCode IS NOT NULL");
        sbSql.Where("t.FeeAmountPaid IS NOT NULL");
        sbSql.Where("t.IsCancelled=0");
        sbSql.Where("t.EventId=@EventId");


        sbSql.GroupBy("t.FeeCurrencyCode")
            .GroupBy("curr.CurrencySymbol");

        sbSql.OrderBy("t.FeeCurrencyCode ASC");

        string sql = sbSql.AddTemplate($"SELECT /**select**/ FROM ems.EventRegistration t /**leftjoin**/ /**where**/ /**groupby**/ /**orderby**/").RawSql;

        using var cn = DbContext.DbCxn;

        var dataList = (await cn.QueryAsync<EventOtherFeeItem>(sql, new { EventId = eventId })).AsList();

        return dataList;
    }
}