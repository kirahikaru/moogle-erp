using DataLayer.Models.Retail;
using DataLayer.Models.SystemCore.NonPersistent;
using DataLayer.GlobalConstant;

namespace DataLayer.Repos.Retail;

public interface IInventoryCheckOutRepos : IBaseWorkflowEnabledRepos<InventoryCheckOut>
{
	Task<InventoryCheckOut?> GetFullAsync(int objId);
	Task<bool> UpdateFullAsync(InventoryCheckOut obj);
	Task<int> SaveAndTransitWorkflowAsync(InventoryCheckOut obj, WorkflowTransitionDetail wtd);

	Task<List<InventoryCheckOut>> SearchAsync(
		int pgSize = 0, int pgNo = 0,
		string? objectCode = null,
		string? objectName = null,
		DateTime? CheckedOutDateTimeFrom = null,
		DateTime? CheckedOutDateTimeTo = null,
		List<int>? assignedUserIdList = null,
		List<int>? requestorUserIdList = null,
		List<int>? approverUserIdList = null,
		List<string>? workflowStatusList = null);

	Task<DataPagination> GetSearchPaginationAsync(
		int pgSize = 0,
		string? objectCode = null,
		string? objectName = null,
		DateTime? CheckedOutDateTimeFrom = null,
		DateTime? CheckedOutDateTimeTo = null,
		List<int>? assignedUserIdList = null,
		List<int>? requestorUserIdList = null,
		List<int>? approverUserIdList = null,
		List<string>? workflowStatusList = null);
}

public class InventoryCheckOutRepos(IConnectionFactory connectionFactory) : BaseWorkflowEnabledRepos<InventoryCheckOut>(connectionFactory, InventoryCheckOut.DatabaseObject), IInventoryCheckOutRepos
{
	public async Task<InventoryCheckOut?> GetFullAsync(int objId)
    {
        SqlBuilder sbSql = new();

        sbSql.LeftJoin($"{User.MsSqlTable} ru ON ru.Id=t.RequestorUserId");
        sbSql.LeftJoin($"{User.MsSqlTable} au ON au.Id=t.ApprovedUserId");
        sbSql.LeftJoin($"{User.MsSqlTable} assu ON assu.Id=t.AssignedUserId");

        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.Id=@Id");

        string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;

        using var cn = ConnectionFactory.GetDbConnection()!;

        InventoryCheckOut? data = (await cn.QueryAsync<InventoryCheckOut, User, User, User, InventoryCheckOut>(
                                          sql, (obj, requestor, approver, assignedUser) =>
                                          {
                                              obj.RequestorUser = requestor;
                                              obj.ApprovedUser = approver;
                                              obj.AssignedUser = assignedUser;

                                              return obj;
                                          }, new { Id = objId }, splitOn: "Id")).SingleOrDefault();

        if (data != null)
        {
            SqlBuilder sbSqlItem = new();

            sbSqlItem.Where("icoi.IsDeleted=0");
            sbSqlItem.Where("icoi.InventoryCheckOutId=@ObjectId");

            sbSqlItem.LeftJoin($"{Item.MsSqlTable} i ON i.Id=icoi.ItemId");
            sbSqlItem.LeftJoin($"{Location.MsSqlTable} loc ON loc.Id=icoi.LocationId");
            sbSqlItem.LeftJoin($"{ItemCategory.MsSqlTable} ctg ON ctg.Id=i.ItemCategoryId");
            sbSqlItem.LeftJoin($"{UnitOfMeasure.MsSqlTable} uom ON uom.IsDeleted=0 AND uom.ObjectCode=icoi.UnitCode");

            string itemSql = sbSqlItem.AddTemplate($"SELECT * FROM {InventoryCheckOutItem.MsSqlTable} icoi /**leftjoin**/ /**where**/").RawSql;

            data.Items = (await cn.QueryAsync<InventoryCheckOutItem, Item, Location, ItemCategory, UnitOfMeasure, InventoryCheckOutItem>(sql,
                                (obj, item, location, itemCtg, uom) =>
                                {
                                    item.Category = itemCtg;
                                    obj.Item = item;
                                    obj.Location = location;
                                    obj.Unit = uom;

                                    return obj;
                                }, new { ObjectId = objId }, splitOn: "Id")).AsList();
        }

        return data;
    }

    public async Task<bool> UpdateFullAsync(InventoryCheckOut obj)
    {
        using var cn = ConnectionFactory.GetDbConnection()!;

        // <!IMPORTANT> Connection required to be open before calling BeginTransaction() function
        if (cn.State != ConnectionState.Open) cn.Open();

        using var tran = cn.BeginTransaction();

        bool isError = !await cn.UpdateAsync(obj, tran);

        if (obj.Items != null && obj.Items.Count != 0 && !isError)
        {
            foreach (InventoryCheckOutItem item in obj.Items)
            {
                if (isError) break;

                item.InventoryCheckOutId = obj.Id;

                if (item.Id == 0 && !item.IsDeleted)
                {
                    item.CreatedUser = obj.ModifiedUser;
                    item.CreatedDateTime = obj.ModifiedDateTime;
                    item.ModifiedUser = obj.ModifiedUser;
                    item.ModifiedDateTime = obj.ModifiedDateTime;
                    isError = await cn.InsertAsync(item, tran) <= 0;
                }
                else if (item.Id > 0)
                {
                    item.ModifiedUser = obj.ModifiedUser;
                    item.ModifiedDateTime = obj.ModifiedDateTime;
                    isError = !await cn.UpdateAsync(item, tran);
                }
            }
        }

        return !isError;
    }

    public async Task<int> SaveAndTransitWorkflowAsync(InventoryCheckOut obj, WorkflowTransitionDetail wtd)
    {
        if (string.IsNullOrEmpty(wtd.CurrentWorkflowStatus) || string.IsNullOrEmpty(wtd.WorkflowAction))
            throw new Exception("Current Workflow Status and Workflow Action cannot be null.");
        else if (!WFC_InventoryCheckOut.IsValidWorkflowTransit(wtd.CurrentWorkflowStatus!, wtd.WorkflowAction!))
            throw new Exception($"Invalid current workflow status ({wtd.CurrentWorkflowStatus}) and workflow action ({wtd.WorkflowAction}) combination.");

        string endWorkflowStatus = WFC_InventoryCheckOut.GetResultingWorkflowStatus(wtd.WorkflowAction!);

        if (string.IsNullOrEmpty(endWorkflowStatus))
            throw new Exception($"Invalid workflow action ({wtd.WorkflowAction})");

        using var cn = ConnectionFactory.GetDbConnection()!;

        // <!IMPORTANT> Connection required to be open before calling BeginTransaction() function
        if (cn.State != ConnectionState.Open) cn.Open();

        DateTime khTimestamp = DateTime.UtcNow.AddHours(7);

        using var tran = cn.BeginTransaction();
        try
        {
            int objId = 0;
            obj.WorkflowStatus = endWorkflowStatus;
            obj.ModifiedDateTime = khTimestamp;
            bool isError = false;

            #region Running Number Generation 
            if (wtd.WorkflowAction.Is(WorkflowActions.SUBMIT_AND_APRPOVE, WorkflowActions.SUBMIT_FOR_APPROVAL))
            {
                SqlBuilder sbSqlRngCounter = new();

                sbSqlRngCounter.Where("rngc.IsDeleted=0");
                sbSqlRngCounter.Where("rngc.IsCurrent=1");
                sbSqlRngCounter.Where("rngc.ObjectClassName=@ObjectClassName");

                sbSqlRngCounter.LeftJoin($"{RunNumGenerator.MsSqlTable} rng ON rng.Id=rngc.RunningNumberGeneratorId");

                string sqlRngCounter = sbSqlRngCounter.AddTemplate($"SELECT * FROM {RunNumGeneratorCounter.MsSqlTable} rngc /**leftjoin**/ /**where**/").RawSql;

                DynamicParameters rngCounterParam = new();
                rngCounterParam.Add("@ObjectClassName", obj.GetType().Name, DbType.AnsiString);

                var rngCounter = (await cn.QueryAsync<RunNumGeneratorCounter, RunNumGenerator, RunNumGeneratorCounter>(
                                    sqlRngCounter, (rngCounter, rng) =>
                                    {
                                        rngCounter.RunningNumberGenerator = rng;
                                        return rngCounter;
                                    }, rngCounterParam, splitOn: "Id", transaction: tran)).FirstOrDefault();

                if (rngCounter == null)
                    throw new Exception($"Running Number Generator Counter for {obj.GetType().Name} is not found.");

                SqlBuilder sbCmdUpdRngCounter = new();
                sbCmdUpdRngCounter.Where("IsDeleted=0");
                sbCmdUpdRngCounter.Where("Id=@RngCounterId");
                sbCmdUpdRngCounter.Set("CurrentNumber=CurrentNumber + 1");
                sbCmdUpdRngCounter.Set("ModifiedUser=@ModifiedUser");
                sbCmdUpdRngCounter.Set("ModifiedDateTime=@ModifiedDateTime");

                string cmdUpdRngCounter = sbCmdUpdRngCounter.AddTemplate($"UPDATE {RunNumGeneratorCounter.MsSqlTable} /**set**/ OUTPUT inserted.CurrentNumber /**where**/").RawSql;
                
                DynamicParameters cmdUpdRngCounterParam = new();
                cmdUpdRngCounterParam.Add("@RngCounterId", rngCounter.Id);
                cmdUpdRngCounterParam.Add("@ModifiedUser", obj.ModifiedUser);
                cmdUpdRngCounterParam.Add("@ModifiedDateTime", obj.ModifiedDateTime);

                int rngCounterNumber = await cn.ExecuteScalarAsync<int>(cmdUpdRngCounter, cmdUpdRngCounterParam, tran);

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
                    runningNumber.Append(rngCounter.IntervalYear!.Value / 2000);
                    runningNumber.Append(rngCounter.IntervalMonth!.Value.ToString("00"));
                    runningNumber.Append(rngCounterNumber.ToString("000000"));
                }
                else if (rngCounter.IntervalDay.HasValue)
                {
                    runningNumber.Append((rngCounter.IntervalYear!.Value / 2000).ToString());
                    runningNumber.Append(rngCounterNumber.ToString("00000000"));
                }
                else
                    runningNumber.Append(rngCounterNumber.ToString("00000000"));

                runningNumber.Append(rngCounter.RunningNumberGenerator!.Suffix.NonNullValue());

                obj.ObjectCode = runningNumber.ToString();
                obj.CheckOutDateTime = khTimestamp;
            }
            #endregion

            if (obj.Id > 0) // UPDATE
            {
                WorkflowHistory wh = new()
                {
                    LinkedObjectId = obj.Id,
                    LinkedObjectType = obj.GetType().Name,
                    UserId = wtd.TargetUserId,
                    StartStatus = wtd.CurrentWorkflowStatus,
                    EndStatus = endWorkflowStatus,
                    OrgStructId  = wtd.TargetUser?.OrgStructId,
                    Action = wtd.WorkflowAction,
                    CreatedUser = obj.ModifiedUser,
                    CreatedDateTime = obj.ModifiedDateTime,
                    ModifiedUser = obj.ModifiedUser,
                    ModifiedDateTime = obj.ModifiedDateTime,
                    Remark = wtd.TransitionRemark
                };

                int workflowHistoryId = await cn.InsertAsync(wh, tran);
                isError = !await cn.UpdateAsync(obj, tran);

                if (wtd.WorkflowAction.Is(WorkflowActions.SAVE_AS_DRAFT, WorkflowActions.SUBMIT_FOR_APPROVAL, WorkflowActions.SUBMIT_AND_APRPOVE) && !isError)
                {
                    if (obj.Items != null && obj.Items.Count != 0)
                    {
                        foreach (InventoryCheckOutItem item in obj.Items)
                        {
                            if (isError) break;
                            item.InventoryCheckOutId = obj.Id;

                            if (item.Id > 0)
                            {
                                item.ModifiedUser = obj.ModifiedUser;
                                item.ModifiedDateTime = obj.ModifiedDateTime;
                                isError = !await cn.UpdateAsync(item, tran);
                            }
                            else
                            {
                                item.CreatedUser = obj.ModifiedUser;
                                item.CreatedDateTime = obj.ModifiedDateTime;
                                item.ModifiedUser = obj.ModifiedUser;
                                item.ModifiedDateTime = obj.ModifiedDateTime;
                                isError = await cn.InsertAsync(item, tran) <= 0;
                            }
                        }
                    }
                }

                objId = isError ? 0 : 1;
            }
            else     // INSERT
            {
                objId = await cn.InsertAsync(obj, tran);

                if (objId > 0)
                {
                    foreach (InventoryCheckOutItem item in obj.Items)
                    {
                        if (isError) break;
                        if (item.Id != 0 || item.IsDeleted) continue;

                        int itemId = await cn.InsertAsync(item, tran);

                        item.InventoryCheckOutId = objId;
                        item.CreatedUser = obj.CreatedUser;
                        item.CreatedDateTime = obj.CreatedDateTime;
                        item.ModifiedUser = obj.ModifiedUser;
                        item.ModifiedDateTime = obj.ModifiedDateTime;

                        if (itemId > 0)
                            item.Id = itemId;
                        else
                            isError = true;
                    }

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

                    if (isError)
                        throw new Exception("There is an error inserting InventoryCheckOutItem");
                }
                else
                {
                    throw new Exception("There is an error inserting InventoryCheckOut.");
                }
            }


            // When status is approved, then update item balance
            if (endWorkflowStatus == WorkflowStatuses.APPROVED)
            {
                foreach (InventoryCheckOutItem item in obj.Items!)
                {
                    #region UPDATE ItemStockBalance
                    string cmdUpdBalance = $"EXEC [rms].SP_UpdateItemStockBalance @itemId, @itemBarcode, @tranType, @user, @quantity";
                    DynamicParameters cmdUpdBalanceParam = new();
                    cmdUpdBalanceParam.Add("@itemId", item.ItemId!.Value);
                    cmdUpdBalanceParam.Add("@itemBarcode", item.Barcode, DbType.AnsiString);
                    cmdUpdBalanceParam.Add("@tranType", "CHECK-OUT", DbType.AnsiString);
                    cmdUpdBalanceParam.Add("@user", item.ModifiedUser, DbType.AnsiString);
                    cmdUpdBalanceParam.Add("@quantity", item.Quantity!.Value);

                    isError = await cn.ExecuteAsync(cmdUpdBalance, cmdUpdBalanceParam, tran) == 0;
                    #endregion
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

    public async Task<List<InventoryCheckOut>> SearchAsync(
        int pgSize = 0, int pgNo = 0,
        string? objectCode = null,
        string? objectName = null,
        DateTime? checkedOutDateTimeFrom = null,
        DateTime? checkedOutDateTimeTo = null,
        List<int>? assignedUserIdList = null,
        List<int>? requestorUserIdList = null,
        List<int>? approverUserIdList = null,
        List<string>? workflowStatusList = null)
    {
        if (pgNo < 0 && pgSize < 0)
            throw new Exception(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        DynamicParameters param = new();
        SqlBuilder sbSql = new();
        sbSql.Where("t.IsDeleted=0");

        #region Form Search Condition
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

        if (checkedOutDateTimeFrom.HasValue)
        {
            sbSql.Where("t.CheckedOutDateTime IS NOT NULL AND t.CheckedOutDateTime>=@CheckedOutDateTimeFrom");
            param.Add("@CheckedOutDateTimeFrom", checkedOutDateTimeFrom);

            if (checkedOutDateTimeTo.HasValue)
            {
                sbSql.Where("t.CheckedOutDateTime<=@CheckedOutDateTimeTo");
                param.Add("@CheckedOutDateTimeTo", checkedOutDateTimeTo);
            }
        }
        else if (checkedOutDateTimeTo.HasValue)
        {
            sbSql.Where("t.CheckedOutDateTime IS NOT NULL AND t.CheckedOutDateTime<=@CheckedOutDateTimeTo");
            param.Add("@CheckedOutDateTimeTo", checkedOutDateTimeTo.Value);
        }

        if (assignedUserIdList != null && assignedUserIdList.Any())
        {
            if (assignedUserIdList.Count == 1)
            {
                sbSql.Where("t.AssignedUserId=@AssignedUserId");
                param.Add("@AssignedUserId", assignedUserIdList[0]);
            }
            else
            {
                sbSql.Where("t.AssignedUserId IN @AssignedUserIdList");
                param.Add("@AssignedUserIdList", assignedUserIdList);
            }
        }

        if (requestorUserIdList != null && requestorUserIdList.Any())
        {
            if (requestorUserIdList.Count == 1)
            {
                sbSql.Where("t.RequestorUserId=@RequestorUserId");
                param.Add("@RequestorUserId", requestorUserIdList[0]);
            }
            else
            {
                sbSql.Where("t.RequestorUserId IN @RequestorUserIdList");
                param.Add("@RequestorUserIdList", requestorUserIdList);
            }
        }

        if (approverUserIdList != null && approverUserIdList.Any())
        {
            if (approverUserIdList.Count == 1)
            {
                sbSql.Where("t.ApprovedUserId=@RequestorUserId");
                param.Add("@ApprovedUserId", approverUserIdList[0]);
            }
            else
            {
                sbSql.Where("t.ApprovedUserId IN @ApprovedUserIdList");
                param.Add("@ApprovedUserIdList", approverUserIdList);
            }
        }

        if (workflowStatusList != null && workflowStatusList.Any())
        {
            if (workflowStatusList.Count == 1)
            {
                sbSql.Where("t.WorkflowStatus=@WorkflowStatus");
                param.Add("@WorkflowStatus", workflowStatusList[0], DbType.AnsiString);
            }
            else
            {
                sbSql.Where("t.WorkflowStatus IN @WorkflowStatusList");
                param.Add("@WorkflowStatusList", workflowStatusList);
            }
        }
        #endregion

        sbSql.LeftJoin($"{User.MsSqlTable} ru ON ru.Id=t.RequestorUserId");
        sbSql.LeftJoin($"{User.MsSqlTable} au ON au.Id=t.ApprovedUserId");

        sbSql.OrderBy("t.CheckOutDateTime DESC");
        sbSql.OrderBy("t.ObjectCode DESC");

        string sql;

        if (pgNo == 0 && pgSize == 0)
        {
            sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/ /**orderby**/").RawSql;
        }
        else
        {
            param.Add("@PageSize", pgSize);

            param.Add("@PageNo", pgNo);
            sql = sbSql.AddTemplate($";WITH pg AS (SELECT t.Id FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
                                              $"SELECT t.*, ru.*, au.* FROM {DbObject.MsSqlTable} t INNER JOIN pg p ON p.Id=t.Id /**leftjoin**/ /**orderby**/").RawSql;
        }

        using var cn = ConnectionFactory.GetDbConnection()!;

        List<InventoryCheckOut> data = (await cn.QueryAsync<InventoryCheckOut, User, User, InventoryCheckOut>(sql, 
                                        (obj, requestor, approver) => 
                                        {
                                            obj.RequestorUser = requestor;
                                            obj.ApprovedUser = approver;

                                            return obj;
                                        }, param, splitOn: "Id")).AsList();

        return data;
    }

    public async Task<DataPagination> GetSearchPaginationAsync(
        int pgSize = 0,
        string? objectCode = null,
        string? objectName = null,
        DateTime? checkOutDateTimeFrom = null,
        DateTime? checkOutDateTimeTo = null,
        List<int>? assignedUserIdList = null,
        List<int>? requestorUserIdList = null,
        List<int>? approverUserIdList = null,
        List<string>? workflowStatusList = null)
    {
        if (pgSize < 0)
            throw new Exception(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        DynamicParameters param = new();
        SqlBuilder sbSql = new();
        sbSql.Where("t.IsDeleted=0");

        #region Form Search Condition
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

        if (checkOutDateTimeFrom.HasValue)
        {
            sbSql.Where("t.CheckOutDateTime IS NOT NULL AND t.CheckOutDateTime>=@CheckOutDateTimeFrom");
            param.Add("@CheckOutDateTimeFrom", checkOutDateTimeFrom);

            if (checkOutDateTimeTo.HasValue)
            {
                sbSql.Where("t.CheckOutDateTime<=@CheckOutDateTimeTo");
                param.Add("@CheckOutDateTimeTo", checkOutDateTimeTo);
            }
        }
        else if (checkOutDateTimeTo.HasValue)
        {
            sbSql.Where("t.CheckOutDateTime IS NOT NULL AND t.CheckOutDateTime<=@CheckOutDateTimeTo");
            param.Add("@CheckOutDateTimeTo", checkOutDateTimeTo.Value);
        }

        if (assignedUserIdList != null && assignedUserIdList.Any())
        {
            if (assignedUserIdList.Count == 1)
            {
                sbSql.Where("t.AssignedUserId=@AssignedUserId");
                param.Add("@AssignedUserId", assignedUserIdList[0]);
            }
            else
            {
                sbSql.Where("t.AssignedUserId IN @AssignedUserIdList");
                param.Add("@AssignedUserIdList", assignedUserIdList);
            }
        }

        if (requestorUserIdList != null && requestorUserIdList.Any())
        {
            if (requestorUserIdList.Count == 1)
            {
                sbSql.Where("t.RequestorUserId=@RequestorUserId");
                param.Add("@RequestorUserId", requestorUserIdList[0]);
            }
            else
            {
                sbSql.Where("t.RequestorUserId IN @RequestorUserIdList");
                param.Add("@RequestorUserIdList", requestorUserIdList);
            }
        }

        if (approverUserIdList != null && approverUserIdList.Any())
        {
            if (approverUserIdList.Count == 1)
            {
                sbSql.Where("t.ApprovedUserId=@RequestorUserId");
                param.Add("@ApprovedUserId", approverUserIdList[0]);
            }
            else
            {
                sbSql.Where("t.ApprovedUserId IN @ApprovedUserIdList");
                param.Add("@ApprovedUserIdList", approverUserIdList);
            }
        }

        if (workflowStatusList != null && workflowStatusList.Any())
        {
            if (workflowStatusList.Count == 1)
            {
                sbSql.Where("t.WorkflowStatus=@WorkflowStatus");
                param.Add("@WorkflowStatus", workflowStatusList[0], DbType.AnsiString);
            }
            else
            {
                sbSql.Where("t.WorkflowStatus IN @WorkflowStatusList");
                param.Add("@WorkflowStatusList", workflowStatusList);
            }
        }
        #endregion

        string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;

        using var cn = ConnectionFactory.GetDbConnection()!;

        decimal recordCount = await cn.ExecuteScalarAsync<int>(sql, param);
        int pageCount = (int)Math.Ceiling(recordCount / pgSize);

        DataPagination pagination = new()
        {
            ObjectType = typeof(InventoryCheckOut).Name,
            PageSize = pgSize,
            PageCount = pageCount,
            RecordCount = (int)recordCount
        };

        return pagination;
    }
}