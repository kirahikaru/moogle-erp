using DataLayer.Models.RMS;
using DataLayer.GlobalConstant;

namespace DataLayer.Repos.RMS;

public interface IInventoryCheckInRepos : IBaseWorkflowEnabledRepos<InventoryCheckIn>
{
	Task<InventoryCheckIn?> GetFullAsync(int objId);
	Task<int> InsertFullAsync(InventoryCheckIn obj);
	Task<bool> UpdateFullAsync(InventoryCheckIn obj);
	Task<int> SaveAndTransitWorkflowAsync(InventoryCheckIn obj, WorkflowTransitionDetail wtd);

	Task<List<InventoryCheckIn>> SearchAsync(
		int pgSize = 0, int pgNo = 0,
		string? objectCode = null,
		string? objectName = null,
		DateTime? checkInDateTimeFrom = null,
		DateTime? checkInDateTimeTo = null,
		decimal? totalAmountFrom = null,
		decimal? totalAmountTo = null,
		List<int>? supplierIdList = null,
		string? supplierInvoiceRefNum = null,
		List<int>? assignedUserIdList = null,
		List<string>? workflowStatusList = null);

	Task<DataPagination> GetSearchPaginationAsync(
		int pgSize = 0,
		string? objectCode = null,
		string? objectName = null,
		DateTime? checkInDateTimeFrom = null,
		DateTime? checkInDateTimeTo = null,
		decimal? totalAmountFrom = null,
		decimal? totalAmountTo = null,
		List<int>? supplierIdList = null,
		string? supplierInvoiceRefNum = null,
		List<int>? assignedUserIdList = null,
		List<string>? workflowStatusList = null);
}

public class InventoryCheckInRepos(IDbContext dbContext) : BaseWorkflowEnabledRepos<InventoryCheckIn>(dbContext, InventoryCheckIn.DatabaseObject), IInventoryCheckInRepos
{
	public async Task<InventoryCheckIn?> GetFullAsync(int objId)
    {
        SqlBuilder sbSql = new();

        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.Id=@Id");

        sbSql.LeftJoin($"{Currency.MsSqlTable} ccy ON ccy.IsDeleted=0 AND ccy.ObjectCode=t.CurrencyCode");
        sbSql.LeftJoin($"{User.MsSqlTable} iu ON iu.Id=t.InitiatorUserId");
        sbSql.LeftJoin($"{User.MsSqlTable} au ON au.Id=t.ApprovedUserId");
        sbSql.LeftJoin($"{User.MsSqlTable} assu ON assu.Id=t.AssignedUserId");
        sbSql.LeftJoin($"{Supplier.MsSqlTable} s ON s.Id=t.SupplierId");

        string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;

        using var cn = DbContext.DbCxn;

        InventoryCheckIn? data = (await cn.QueryAsync<InventoryCheckIn, Currency, User, User, User, Supplier, InventoryCheckIn>(
                                          sql, (obj, currency, initiator, approver, assignedUser, supplier) =>
                                          {
                                              obj.Currency = currency;
                                              obj.InitiatorUser = initiator;
                                              obj.ApprovedUser = approver;
                                              obj.AssignedUser = assignedUser;
                                              obj.Supplier = supplier;

                                              return obj;
                                          }, new { Id=objId }, splitOn: "Id")).FirstOrDefault();

        //if (data != null)
        //{
        //    string itemSql = $"SELECT * FROM {InventoryCheckInItem.MsSqlTable} icii " +
        //                     $"LEFT JOIN {Item.MsSqlTable} i ON i.Id=icii.ItemId " +
        //                     $"LEFT JOIN {Location.MsSqlTable} loc ON loc.Id=icii.LocationId " +
        //                     $"LEFT JOIN {Country.MsSqlTable} mfgCty ON mfgCty.IsDeleted=0 AND mfgCty.ObjectCode=icii.MfgCountryCode " +
        //                     $"LEFT JOIN {Manufacturer.MsSqlTable} mfg ON mfg.Id=icii.ManufacturerId " +
        //                     $"LEFT JOIN {UnitOfMeasure.MsSqlTable} uom ON uom.IsDeleted=0 AND uom.ObjectCode=icii.UnitCode " +
        //                     $"WHERE icii.IsDeleted=0 AND icii.InventoryCheckInId=@ObjectId";

        //    data.Items = (await cn.QueryAsync<InventoryCheckInItem, Item, Location, Country, Manufacturer, UnitOfMeasure, InventoryCheckInItem>(itemSql,
                             
        //        (obj, item, location, country, manufacturer, uom) =>
        //                                              {
        //                                                  obj.Item = item;
        //                                                  obj.Location = location;
        //                                                  obj.MfgCountry = country;
        //                                                  obj.Manufacturer = manufacturer;
        //                                                  obj.Unit = uom;

        //                                                  return obj;
        //                                              }, new { ObjectId = objId }, splitOn: "Id")).AsList();
        //}

        return data;
    }

    public async Task<int> InsertFullAsync(InventoryCheckIn obj)
    {
        using var cn = DbContext.DbCxn;

        // <!IMPORTANT> Connection required to be open before calling BeginTransaction() function
        if (cn.State != ConnectionState.Open) cn.Open();

        using var tran = cn.BeginTransaction();

        try
        {
            bool isError = false;
            int objId = await cn.InsertAsync(obj, tran);

            if (obj.Items != null && obj.Items.Count != 0 && !isError)
            {
                foreach (InventoryCheckInItem item in obj.Items)
                {
                    if (isError) break;
                    if (item.IsDeleted) continue;

                    if (item.Id == 0)
                    {
                        item.InventoryCheckInId = objId;
                        item.CreatedUser = obj.ModifiedUser;
                        item.CreatedDateTime = obj.ModifiedDateTime;
                        item.ModifiedUser = obj.ModifiedUser;
                        item.ModifiedDateTime = obj.ModifiedDateTime;
                        isError = await cn.InsertAsync(item, tran) <= 0;
                    }
                }
            }

            if (isError)
                throw new Exception("Application encountered error while doing UpdateFull for InventoryCheckIn");
            else
                tran.Commit();

            return objId;
        }
        catch
        {
            tran.Rollback();
            throw;
        }
    }

    public async Task<bool> UpdateFullAsync(InventoryCheckIn obj)
    {
        using var cn = DbContext.DbCxn;

        // <!IMPORTANT> Connection required to be open before calling BeginTransaction() function
        if (cn.State != ConnectionState.Open) cn.Open();

        using var tran = cn.BeginTransaction();

        try
        {
            bool isError = !await cn.UpdateAsync(obj, tran);

            if (obj.Items != null && obj.Items.Count != 0 && !isError)
            {
                foreach (InventoryCheckInItem item in obj.Items)
                {
                    if (isError) break;

                    if (item.Id == 0 && !item.IsDeleted)
                    {
                        item.InventoryCheckInId = obj.Id;
                        item.CreatedUser = obj.ModifiedUser;
                        item.CreatedDateTime = obj.ModifiedDateTime;
                        item.ModifiedUser = obj.ModifiedUser;
                        item.ModifiedDateTime = obj.ModifiedDateTime;
                        isError = await cn.InsertAsync(item, tran) <= 0;
                    }
                    else if (item.Id > 0)
                    {
                        item.InventoryCheckInId = obj.Id;
                        item.ModifiedUser = obj.ModifiedUser;
                        item.ModifiedDateTime = obj.ModifiedDateTime;
                        isError = !await cn.UpdateAsync(item, tran);
                    }
                }
            }

            if (isError)
                throw new Exception("Application encountered error while doing UpdateFull for InventoryCheckIn");
            else
                tran.Commit();

            return true;
        }
        catch
        {
            tran.Rollback();
            throw;
        }
    }

    public override async Task<List<InventoryCheckIn>> QuickSearchAsync(
        int pgSize = 0, int pgNo = 0, 
        string? searchText = null, 
        List<int>? excludeIdList = null)
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
                param.Add("@SearchText", searchText, DbType.AnsiString);
            }
            else
            {
                sbSql.Where("(t.ObjectName LIKE '%'+@SearchText+'%'");
                param.Add("@SearchText", searchText, DbType.AnsiString);
            }
        }

        if (excludeIdList != null && excludeIdList.Count > 0)
        {
            sbSql.Where("t.Id NOT IN @ExcludeIdList");
            param.Add("@ExcludeIdList", excludeIdList);
        }
        #endregion

        sbSql.LeftJoin($"{Supplier.MsSqlTable} s ON s.Id=t.SupplierId");
        sbSql.LeftJoin($"{User.MsSqlTable} iu ON iu.Id=t.InitiatorUserId");
        sbSql.LeftJoin($"{User.MsSqlTable} au ON au.Id=t.ApprovedUserId");

        sbSql.OrderBy("t.CheckInDateTime DESC");
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
                    $"SELECT t.*, s.*, iu.*, au.* FROM {DbObject.MsSqlTable} t INNER JOIN pg p ON p.Id=t.Id /**leftjoin**/ /**orderby**/").RawSql;
        }

        using var cn = DbContext.DbCxn;

        var dataList = (await cn.QueryAsync<InventoryCheckIn, Supplier, User, User, InventoryCheckIn>(sql,
                                        (obj, s, initUsr, apprUsr) =>
                                        {
                                            obj.Supplier = s;
                                            obj.InitiatorUser = initUsr;
                                            obj.ApprovedUser = apprUsr;

                                            return obj;
                                        }, param, splitOn: "Id")).AsList();

        return dataList;
    }

    public async Task<List<InventoryCheckIn>> SearchAsync(
        int pgSize = 0, int pgNo = 0,
        string? objectCode = null,
        string? objectName = null,
        DateTime? checkInDateTimeFrom = null,
        DateTime? checkInDateTimeTo = null,
        decimal? totalAmountFrom = null,
        decimal? totalAmountTo = null,
        List<int>? supplierIdList = null,
        string? supplierInvoiceRefNum = null,
        List<int>? assignedUserIdList = null,
        List<string>? workflowStatusList = null)
    {
        if (pgNo < 0 && pgSize < 0)
            throw new Exception(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

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

        if (checkInDateTimeFrom.HasValue)
        {
            sbSql.Where("t.CheckInDateTime IS NOT NULL AND t.CheckedInDateTime>=@CheckInDateTimeFrom");
            param.Add("@CheckInDateTimeFrom", checkInDateTimeFrom.Value);

            if (checkInDateTimeTo.HasValue)
            {
                sbSql.Where("t.CheckedInDateTime<=@CheckInDateTimeTo");
                param.Add("@CheckInDateTimeTo", checkInDateTimeTo.Value);
            }
        }
        else if (checkInDateTimeTo.HasValue)
        {
            sbSql.Where("t.CheckInDateTime IS NOT NULL AND t.CheckInDateTime<=@CheckInDateTimeTo");
            param.Add("@CheckInDateTimeTo", checkInDateTimeTo.Value);
        }

        if (totalAmountFrom.HasValue)
        {
            sbSql.Where("t.TotalAmount IS NOT NULL AND t.TotalAmount>=@TotalAmountFrom");
            param.Add("@TotalAmountFrom", totalAmountFrom.Value);

            if (totalAmountTo.HasValue)
            {
                sbSql.Where("t.TotalAmount<=@TotalAmountTo");
                param.Add("@TotalAmountTo", totalAmountTo.Value);
            }
        }

        if (supplierIdList != null && supplierIdList.Any())
        {
            if (supplierIdList.Count == 1)
            {
                sbSql.Where("t.SupplierId=@SupplierId");
                param.Add("@SupplierId", supplierIdList[0]);
            }
            else
            {
                sbSql.Where("t.SupplierId IN @SupplierIdList");
                param.Add("@SupplierIdList", supplierIdList);
            }
        }

        if (!string.IsNullOrEmpty(supplierInvoiceRefNum))
        {
            sbSql.Where("t.SupplierInvoiceRefNum=@SupplierInvoiceRefNum");
            param.Add("@SupplierInvoiceRefNum", supplierInvoiceRefNum, DbType.AnsiString);
        }

        if (assignedUserIdList != null && assignedUserIdList.Any())
        {
            if (assignedUserIdList.Count == 1)
            {
                sbSql.Where("t.AssignedUserId IS NOT NULL AND t.AssignedUserId=@AssignedUserId");
                param.Add("@AssignedUserId", assignedUserIdList[0]);
            }
            else
            {
                sbSql.Where("t.AssignedUserId IN @AssignedUserIdList");
                param.Add("@AssignedUserIdList", assignedUserIdList);
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

        sbSql.LeftJoin($"{Supplier.MsSqlTable} s ON s.Id=t.SupplierId");
        sbSql.LeftJoin($"{User.MsSqlTable} iu ON iu.Id=t.InitiatorUserId");
        sbSql.LeftJoin($"{User.MsSqlTable} au ON au.Id=t.ApprovedUserId");

        sbSql.OrderBy("t.CheckInDateTime DESC");
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
                    $"SELECT t.*, s.*, iu.*, au.* FROM {DbObject.MsSqlTable} t INNER JOIN pg p ON p.Id=t.Id /**leftjoin**/ /**orderby**/").RawSql;
        }

        using var cn = DbContext.DbCxn;

        var dataList = (await cn.QueryAsync<InventoryCheckIn, Supplier, User, User, InventoryCheckIn>(sql, 
                                        (obj, s, initUsr, apprUsr) =>
                                        {
                                            obj.Supplier = s;
                                            obj.InitiatorUser = initUsr;
                                            obj.ApprovedUser = apprUsr;

                                            return obj;
                                        }, param, splitOn: "Id")).AsList();

        return dataList;
    }

    public async Task<DataPagination> GetSearchPaginationAsync(
        int pgSize = 0,
        string? objectCode = null,
        string? objectName = null,
        DateTime? checkInDateTimeFrom = null,
        DateTime? checkInDateTimeTo = null,
        decimal? totalAmountFrom = null,
        decimal? totalAmountTo = null,
        List<int>? supplierIdList = null,
        string? supplierInvoiceRefNum = null,
        List<int>? assignedUserIdList = null,
        List<string>? workflowStatusList = null)
    {
        if (pgSize < 0)
            throw new Exception(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        DynamicParameters param = new();
        SqlBuilder sbSql = new();
        sbSql.Where("t.IsDeleted = 0");

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

        if (checkInDateTimeFrom.HasValue)
        {
            sbSql.Where("t.CheckInDateTime IS NOT NULL AND t.CheckedInDateTime>=@CheckInDateTimeFrom");
            param.Add("@CheckInDateTimeFrom", checkInDateTimeFrom.Value);

            if (checkInDateTimeTo.HasValue)
            {
                sbSql.Where("t.CheckedInDateTime<=@CheckInDateTimeTo");
                param.Add("@CheckInDateTimeTo", checkInDateTimeTo.Value);
            }
        }
        else if (checkInDateTimeTo.HasValue)
        {
            sbSql.Where("t.CheckInDateTime IS NOT NULL AND t.CheckInDateTime<=@CheckInDateTimeTo");
            param.Add("@CheckInDateTimeTo", checkInDateTimeTo.Value);
        }

        if (totalAmountFrom.HasValue)
        {
            sbSql.Where("t.TotalAmount IS NOT NULL AND t.TotalAmount>=@TotalAmountFrom");
            param.Add("@TotalAmountFrom", totalAmountFrom.Value);

            if (totalAmountTo.HasValue)
            {
                sbSql.Where("t.TotalAmount<=@TotalAmountTo");
                param.Add("@TotalAmountTo", totalAmountTo.Value);
            }
        }

        if (supplierIdList != null && supplierIdList.Any())
        {
            if (supplierIdList.Count == 1)
            {
                sbSql.Where("t.SupplierId=@SupplierId");
                param.Add("@SupplierId", supplierIdList[0]);
            }
            else
            {
                sbSql.Where("t.SupplierId IN @SupplierIdList");
                param.Add("@SupplierIdList", supplierIdList);
            }
        }

        if (!string.IsNullOrEmpty(supplierInvoiceRefNum))
        {
            sbSql.Where("t.SupplierInvoiceRefNum=@SupplierInvoiceRefNum");
            param.Add("@SupplierInvoiceRefNum", supplierInvoiceRefNum, DbType.AnsiString);
        }

        if (assignedUserIdList != null && assignedUserIdList.Any())
        {
            if (assignedUserIdList.Count == 1)
            {
                sbSql.Where("t.AssignedUserId IS NOT NULL AND t.AssignedUserId=@AssignedUserId");
                param.Add("@AssignedUserId", assignedUserIdList[0]);
            }
            else
            {
                sbSql.Where("t.AssignedUserId IN @AssignedUserIdList");
                param.Add("@AssignedUserIdList", assignedUserIdList);
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

        using var cn = DbContext.DbCxn;

        decimal recordCount = await cn.ExecuteScalarAsync<int>(sql, param);
        int pageCount = (int)Math.Ceiling(recordCount / pgSize);
        DataPagination pagination = new()
        {
            ObjectType = typeof(InventoryCheckIn).Name,
            PageSize = pgSize,
            PageCount = pageCount,
            RecordCount = (int)recordCount
        };

        return pagination;
    }

    public async Task<int> SaveAndTransitWorkflowAsync(InventoryCheckIn obj, WorkflowTransitionDetail wtd)
    {
        if (string.IsNullOrEmpty(wtd.CurrentWorkflowStatus) || string.IsNullOrEmpty(wtd.WorkflowAction))
            throw new Exception("Current Workflow Status and Workflow Action cannot be null.");
        else if (!WFC_InventoryCheckIn.IsValidWorkflowTransit(wtd.CurrentWorkflowStatus!, wtd.WorkflowAction!))
            throw new Exception($"Invalid current workflow status ({wtd.CurrentWorkflowStatus}) and workflow action ({wtd.WorkflowAction}) combination.");

        string endWorkflowStatus = WFC_InventoryCheckIn.GetResultingWorkflowStatus(wtd.WorkflowAction!);

        if (string.IsNullOrEmpty(endWorkflowStatus))
            throw new Exception($"Invalid workflow action ({wtd.WorkflowAction})");

        using var cn = DbContext.DbCxn;

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
            if (wtd.WorkflowAction == WorkflowActions.REGISTER)
            {
                string rngCounterQry = $"SELECT * FROM {RunNumGeneratorCounter.MsSqlTable} rngc " +
                               $"LEFT JOIN {RunNumGenerator.MsSqlTable} rng ON rng.Id=rngc.RunningNumberGeneratorId " +
                               $"WHERE rngc.IsDeleted=0 AND rngc.IsCurrent=1 AND rng.ObjectClassName=@ObjectClassName";

                DynamicParameters rngCounterParam = new();
                rngCounterParam.Add("@ObjectClassName", obj.GetType().Name, DbType.AnsiString);

                RunNumGeneratorCounter? rngCounter = (await cn.QueryAsync<RunNumGeneratorCounter, RunNumGenerator, RunNumGeneratorCounter>(
                                                            rngCounterQry, (rngCounter, rng) =>
                                                            {
                                                                rngCounter.RunningNumberGenerator = rng;
                                                                return rngCounter;
                                                            }, rngCounterParam, splitOn: "Id", transaction: tran)).FirstOrDefault();

                if (rngCounter == null)
                    throw new Exception("Running Number Generator for customer is not found.");

                string rngUpdCmd = $"UPDATE {RunNumGeneratorCounter.MsSqlTable} " +
                                   $"SET CurrentNumber=CurrentNumber + 1, ModifiedUser=@ModifiedUser, ModifiedDateTime=@ModifiedDateTime " +
                                   $"OUTPUT inserted.CurrentNumber " +
                                   $"WHERE IsDeleted=0 AND Id=@RngCounterId";

                DynamicParameters rngUpdCmdParam = new();
                rngUpdCmdParam.Add("@RngCounterId", rngCounter.Id);
                rngUpdCmdParam.Add("@ModifiedUser", obj.ModifiedUser);
                rngUpdCmdParam.Add("@ModifiedDateTime", obj.ModifiedDateTime);

                int rngCounterNumber = await cn.ExecuteScalarAsync<int>(rngUpdCmd, rngUpdCmdParam, tran);
                StringBuilder runningNumber = new();
                runningNumber.Append(rngCounter.RunningNumberGenerator!.Prefix.NonNullValue());
                if (rngCounter.IntervalDay.HasValue)
                {
                    runningNumber.Append((rngCounter.IntervalYear!.Value / 2000).ToString());
                    runningNumber.Append(rngCounter.IntervalMonth!.Value.ToString("00"));
                    runningNumber.Append(rngCounter.IntervalDay.Value.ToString("00"));
                    runningNumber.Append(rngCounterNumber.ToString("0000"));
                }
                else if (rngCounter.IntervalMonth.HasValue)
                {
                    runningNumber.Append((rngCounter.IntervalYear!.Value / 2000).ToString());
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
                obj.CheckInDateTime = khTimestamp;
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
                    OrgStructId = wtd.TargetUser?.OrgStructId,
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
                    // [SECTION] Update and save all items
                    if (obj.Items != null && obj.Items.Count != 0)
                    {
                        foreach (InventoryCheckInItem item in obj.Items)
                        {
                            if (isError) break;

                            item.InventoryCheckInId = obj.Id;

                            if (item.Id > 0)
                            {
                                item.ModifiedUser = obj.ModifiedUser;
                                item.ModifiedDateTime= obj.ModifiedDateTime;
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
                    foreach (InventoryCheckInItem item in obj.Items)
                    {
                        if (isError) break;
                        if (item.Id != 0 || item.IsDeleted) continue;

                        item.InventoryCheckInId = objId;
                        item.CreatedUser = obj.CreatedUser;
                        item.CreatedDateTime = obj.CreatedDateTime;
                        item.ModifiedUser = obj.ModifiedUser;
                        item.ModifiedDateTime = obj.ModifiedDateTime;

                        int itemId = await cn.InsertAsync(item, tran);

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
                        throw new Exception("There is an error tryning to insert/update InventoryCheckInItem record.");
                }
                else
                {
                    throw new Exception("There is an error inserting InventoryCheckIn record");
                }
            }

            // When status is approved, then update item balance
            if (endWorkflowStatus == WorkflowStatuses.APPROVED)
            {
                foreach (InventoryCheckInItem item in obj.Items!)
                {
                    #region UPDATE ItemStockBalance
                    //string cmdUpdBalance = $"EXEC [rms].SP_UpdateItemStockBalance @itemId, @itemBarcode, @unitCode, @tranType, @user, @quantity";
                    string cmdUpdBalance = $"EXEC [rms].SP_UpdateItemStockBalance @itemId, @itemBarcode, @tranType, @user, @quantity";
                    DynamicParameters cmdUpdBalanceParam = new();
                    cmdUpdBalanceParam.Add("@itemId", item.ItemId!.Value);
                    //cmdUpdBalanceParam.Add("@unitCode", item.UnitCode!, DbType.AnsiString);
                    cmdUpdBalanceParam.Add("@itemBarcode", item.Barcode, DbType.AnsiString);
                    cmdUpdBalanceParam.Add("@tranType", "CHECK-IN", DbType.AnsiString);
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
}