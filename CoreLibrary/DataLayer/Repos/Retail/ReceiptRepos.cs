using DataLayer.Models.Retail;
using DataLayer.Models.Finance;
using DataLayer.Models.SystemCore.NonPersistent;
using DataLayer.GlobalConstant;

namespace DataLayer.Repos.Retail;

public interface IReceiptRepos : IBaseRepos<Receipt>
{
	Task<Receipt?> GetFullAsync(int id);

	Task<int> InsertFullAsync(Receipt obj);

	Task<bool> UpdateFullAsync(Receipt obj);

	Task<List<Receipt>> GetByDateAsync(DateTime receiptDate);

	Task<List<Receipt>> SearchAsync(
			int pgSize = 0, int pgNo = 0,
			string? objectCode = null,
			DateTime? receiptDateForm = null,
			DateTime? receiptDateTo = null,
			string? currencyCode = null,
			decimal? totalAmountFrom = null,
			decimal? totalAmountTo = null,
			List<string>? statuses = null,
			List<int>? cashierUserIds = null,
			List<int>? customerIds = null,
			string? customerName = null,
			string? customerID = null);

	Task<DataPagination> GetSearchPaginationAsync(
			int pgSize = 0,
			string? objectCode = null,
			DateTime? receiptDateForm = null,
			DateTime? receiptDateTo = null,
			string? currencyCode = null,
			decimal? totalAmountFrom = null,
			decimal? totalAmountTo = null,
			List<string>? statuses = null,
			List<int>? cashierUserIds = null,
			List<int>? customerIds = null,
			string? customerName = null,
			string? customerID = null);
}

public class ReceiptRepos(IConnectionFactory connectionFactory) : BaseRepos<Receipt>(connectionFactory, Receipt.DatabaseObject), IReceiptRepos
{
	public async Task<Receipt?> GetFullAsync(int id)
    {
        SqlBuilder sbSql = new();
        DynamicParameters param = new();

        sbSql.LeftJoin($"{User.MsSqlTable} usr ON usr.Id=t.CashierUserId");
        sbSql.LeftJoin($"{Currency.MsSqlTable} curr ON curr.IsDeleted=0 AND curr.ObjectCode=t.CurrencyCode");
        sbSql.LeftJoin($"{Customer.MsSqlTable} c ON c.IsDeleted=0 AND c.Id=t.CustomerId");
        sbSql.LeftJoin($"{ExchangeRate.MsSqlTable} xchg ON xchg.Id=t.KhrExchangeRateId");

        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.Id=@Id");
        param.Add("@Id", id);


        string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;

        using var cn = ConnectionFactory.GetDbConnection()!;

        Receipt? receipt = (await cn.QueryAsync<Receipt, User, Currency, Customer, ExchangeRate, Receipt>(
                                        sql, (obj, user, curr, cust, xchg) => 
                                        {
                                            obj.CashierUser = user;
                                            obj.Currency = curr;
                                            obj.Customer = cust;
                                            obj.KhrExchangeRate = xchg;

                                            return obj;
                                        }, param, splitOn: "Id")).SingleOrDefault();

        if (receipt != null)
        {
            string itemQry = $"SELECT * FROM {ReceiptItem.MsSqlTable} ri WHERE ri.IsDeleted=0 AND ri.ReceiptId=@ReceiptId; " +
                             $"SELECT * FROM {RetailTaxItem.MsSqlTable} rti WHERE rti.IsDeleted=0 AND rti.LinkedObjectType='Receipt' AND rti.LinkedObjectId=@ReceiptId; " +
                             $"SELECT * FROM {RetailOtherCharge.MsSqlTable} roc WHERE roc.IsDeleted=0 AND roc.LinkedObjectType='Receipt' AND roc.LinkedObjectId=@ReceiptId; " +
                             $"SELECT TOP 1 * FROM {ReceiptPayment.MsSqlTable} rp WHERE rp.IsDeleted=0 AND rp.ReceiptId=@ReceiptId; ";

            using var multi = await cn.QueryMultipleAsync(itemQry, new { ReceiptId = id });

            receipt.Items = (await multi.ReadAsync<ReceiptItem>()).AsList();
            receipt.TaxItems = (await multi.ReadAsync<RetailTaxItem>()).AsList();
            receipt.OtherCharges = (await multi.ReadAsync<RetailOtherCharge>()).AsList();
            receipt.Payment = await multi.ReadFirstOrDefaultAsync<ReceiptPayment>();
        }

        return receipt;
    }

    public async Task<int> InsertFullAsync(Receipt obj)
    {
        using var cn = ConnectionFactory.GetDbConnection()!;

        // <!IMPORTANT> Connection required to be open before calling BeginTransaction() function
        if (cn.State != ConnectionState.Open) cn.Open();

        using var tran = cn.BeginTransaction();

        try
        {
            if (string.IsNullOrEmpty(obj.ObjectCode) && obj.Status == ReceiptStatuses.PAID)
            {
                #region Running Number Generation
                SqlBuilder sbSqlRngCounter = new();

                sbSqlRngCounter.Where("rngc.IsDeleted=0");
                sbSqlRngCounter.Where("rngc.IsCurrent=1");
                sbSqlRngCounter.Where("rng.ObjectClassName=@ObjectClassName");
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
                rngUpdCmdParam.Add("@ModifiedUser", obj.ModifiedUser);
                rngUpdCmdParam.Add("@ModifiedDateTime", obj.ModifiedDateTime);

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

                obj.ObjectCode = runningNumber.ToString();
                #endregion
            }

            int objId = await cn.InsertAsync(obj, tran);
            //string insSql = QueryGenerator.GenerateInsertQuery(receipt.GetType(), this.DatabaseObject);
            //int objId = cn.QuerySingleOrDefault<int>(insSql, receipt, tran);
            if (objId > 0)
            {
                foreach (ReceiptItem item in obj.Items)
                {
                    if (item.Id > 0)
                        throw new Exception($"{typeof(ReceiptItem).Name} is not new.");

                    item.ReceiptNumber = obj.ObjectCode;
                    item.ReceiptId = objId;
                    item.CreatedUser = obj.CreatedUser;
                    item.CreatedDateTime = obj.CreatedDateTime;
                    item.ModifiedUser = obj.ModifiedUser;
                    item.ModifiedDateTime = obj.ModifiedDateTime;

                    //string insSql
                    int itemId = await cn.InsertAsync(item, tran);

                    if (itemId > 0)
                        item.Id = itemId;
                }

                foreach (RetailOtherCharge otherChargeItem in obj.OtherCharges)
                {
                    if (otherChargeItem.Id > 0)
                        throw new Exception($"{typeof(RetailOtherCharge).Name} is not new.");

                    otherChargeItem.LinkedObjectId = objId;
                    otherChargeItem.LinkedObjectType = obj.GetType().Name;
                    otherChargeItem.CreatedUser = obj.CreatedUser;
                    otherChargeItem.CreatedDateTime = obj.CreatedDateTime;
                    otherChargeItem.ModifiedUser = obj.ModifiedUser;
                    otherChargeItem.ModifiedDateTime = obj.ModifiedDateTime;

                    //string insSql
                    int otherChargeItemId = await cn.InsertAsync(otherChargeItem, tran);

                    if (otherChargeItemId > 0)
                        otherChargeItem.Id = otherChargeItemId;
                }

                foreach (RetailTaxItem taxItem in obj.TaxItems)
                {
                    if (taxItem.Id > 0)
                        throw new Exception($"{typeof(RetailOtherCharge).Name} is not new.");

                    taxItem.LinkedObjectId = objId;
                    taxItem.LinkedObjectType = obj.GetType().Name;
                    taxItem.CreatedUser = obj.CreatedUser;
                    taxItem.CreatedDateTime = obj.CreatedDateTime;
                    taxItem.ModifiedUser = obj.ModifiedUser;
                    taxItem.ModifiedDateTime = obj.ModifiedDateTime;

                    //string insSql
                    int taxItemId = await cn.InsertAsync(taxItem, tran);

                    if (taxItemId > 0)
                        taxItem.Id = taxItemId;
                }

                if (obj.Status == ReceiptStatuses.PAID)
                {
                    obj.Payment!.ReceiptId = obj.Id;
                    obj.CreatedUser = obj.CreatedUser;
                    obj.CreatedDateTime = obj.CreatedDateTime;
                    obj.ModifiedUser = obj.ModifiedUser;
                    obj.ModifiedDateTime = obj.ModifiedDateTime;
                    int receiptPaymentId = await cn.InsertAsync(obj.Payment, tran);
                }

                tran.Commit();
                return objId;
            }
            else
                throw new Exception($"failed to insert object to database.");
        }
        catch
        {
            tran.Rollback();
            throw;
        }
    }

    public async Task<bool> UpdateFullAsync(Receipt obj)
    {
        if (obj.Id <= 0)
            throw new Exception("Object is not existing.");

        using var cn = ConnectionFactory.GetDbConnection()!;

        // <!IMPORTANT> Connection required to be open before calling BeginTransaction() function
        if (cn.State != ConnectionState.Open) cn.Open();

        using var tran = cn.BeginTransaction();

        try
        {
            if (string.IsNullOrEmpty(obj.ObjectCode) && obj.Status == ReceiptStatuses.PAID)
            {
                #region Running Number Generation
                SqlBuilder sbSqlRngCounter = new();

                sbSqlRngCounter.Where("rngc.IsDeleted=0");
                sbSqlRngCounter.Where("rngc.IsCurrent=1");
                sbSqlRngCounter.Where("rng.ObjectClassName=@ObjectClassName");
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
                rngUpdCmdParam.Add("@ModifiedUser", obj.ModifiedUser);
                rngUpdCmdParam.Add("@ModifiedDateTime", obj.ModifiedDateTime);

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

                obj.ObjectCode = runningNumber.ToString();
                #endregion
            }

            bool isMainObjUpdated = await cn.UpdateAsync(obj, tran);
            //string insSql = QueryGenerator.GenerateInsertQuery(receipt.GetType(), this.DatabaseObject);
            //int objId = cn.QuerySingleOrDefault<int>(insSql, receipt, tran);
            if (isMainObjUpdated)
            {
                foreach (ReceiptItem item in obj.Items)
                {
                    item.ReceiptId = obj.Id;

                    if (item.Id > 0)
                    {
                        item.ModifiedUser = obj.ModifiedUser;
                        item.ModifiedDateTime = obj.ModifiedDateTime;
                        bool isItemUpdated = await cn.UpdateAsync(item, tran);

                        if (!isItemUpdated)
                            throw new Exception($"Failed to update ReceiptItem (Id={item.Id}.");
                    }
                    else
                    {
                        item.CreatedUser = obj.ModifiedUser;
                        item.CreatedDateTime = obj.ModifiedDateTime;
                        item.ModifiedUser = obj.ModifiedUser;
                        item.ModifiedDateTime = obj.ModifiedDateTime;

                        //string insSql
                        int itemId = await cn.InsertAsync(item, tran);

                        if (itemId <= 0)
                            throw new Exception("Failed to insert ReceiptItem.");
                        else
                            item.Id = itemId;
                    }
                }

                foreach (RetailOtherCharge otherChargeItem in obj.OtherCharges)
                {
                    otherChargeItem.LinkedObjectId = obj.Id;
                    otherChargeItem.LinkedObjectType = obj.GetType().Name;

                    if (otherChargeItem.Id > 0)
                    {
                        otherChargeItem.ModifiedUser = obj.ModifiedUser;
                        otherChargeItem.ModifiedDateTime = obj.ModifiedDateTime;
                        bool isOtherChargeItemUpdated = await cn.UpdateAsync(otherChargeItem, tran);

                        if (!isOtherChargeItemUpdated)
                            throw new Exception($"Failed to update OtherChargeItem (Id={otherChargeItem.Id})");
                    }
                    else
                    {
                        otherChargeItem.CreatedUser = obj.CreatedUser;
                        otherChargeItem.CreatedDateTime = obj.CreatedDateTime;
                        otherChargeItem.ModifiedUser = obj.ModifiedUser;
                        otherChargeItem.ModifiedDateTime = obj.ModifiedDateTime;
                        int otherChargeItemId = await cn.InsertAsync(otherChargeItem, tran);

                        if (otherChargeItemId > 0)
                            otherChargeItem.Id = otherChargeItemId;
                        else
                            throw new Exception("Failed to insert OtherChargeItem.");
                    }
                }

                foreach (RetailTaxItem taxItem in obj.TaxItems)
                {
                    taxItem.LinkedObjectId = obj.Id;
                    taxItem.LinkedObjectType = obj.GetType().Name;

                    if (taxItem.Id > 0)
                    {
                        taxItem.ModifiedUser = obj.ModifiedUser;
                        taxItem.ModifiedDateTime = obj.ModifiedDateTime;
                        bool isTaxItemUpdated = await cn.UpdateAsync(taxItem, tran);

                        if (!isTaxItemUpdated)
                            throw new Exception($"Failed to update OtherChargeItem (Id={taxItem.Id})");
                    }
                    else
                    {
                        taxItem.CreatedUser = obj.CreatedUser;
                        taxItem.CreatedDateTime = obj.CreatedDateTime;
                        taxItem.ModifiedUser = obj.ModifiedUser;
                        taxItem.ModifiedDateTime = obj.ModifiedDateTime;
                        int taxItemId = await cn.InsertAsync(taxItem, tran);

                        if (taxItemId > 0)
                            taxItem.Id = taxItemId;
                        else
                            throw new Exception("Failed to insert OtherChargeItem.");
                    }
                }

                if (obj.Status == ReceiptStatuses.PAID)
                {
                    obj.Payment!.ReceiptId = obj.Id;
                    obj.CreatedUser = obj.ModifiedUser;
                    obj.CreatedDateTime = obj.ModifiedDateTime;
                    obj.ModifiedUser = obj.ModifiedUser;
                    obj.ModifiedDateTime = obj.ModifiedDateTime;
                    int receiptPaymentId = await cn.InsertAsync(obj.Payment);
                }

                tran.Commit();
                return isMainObjUpdated;
            }
            else
                throw new Exception($"failed to insert object to database.");
        }
        catch
        {
            tran.Rollback();
            throw;
        }
    }

    public async Task<List<Receipt>> GetByDateAsync(DateTime receiptDate)
    {
        DynamicParameters param = new();
        SqlBuilder sbSql = new();
        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.ReceiptDate=@ReceiptDate");

        sbSql.LeftJoin($"{Currency.MsSqlTable} curr ON curr.IsDeleted=0 AND curr.ObjectCode=t.CurrencyCode");
        sbSql.LeftJoin($"{User.MsSqlTable} c ON c.Id=t.CashierUserId");

        var sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;

        using var cn = ConnectionFactory.GetDbConnection()!;

        var dataList = (await cn.QueryAsync<Receipt, Currency, User, Receipt>(sql, 
                                        (obj, currency, cashier) =>
                                        {
                                            obj.Currency = currency;
                                            obj.CashierUser = cashier;

                                            return obj;
                                        }, new { ReceiptDate = receiptDate }, splitOn: "Id")).ToList();
        return dataList;
    }

    public override async Task<List<Receipt>> QuickSearchAsync(int pgSize = 0, int pgNo = 0, string? searchText = null, List<int>? excludeIdList = null)
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
                sbSql.Where("UPPER(t.ObjectCode) LIKE '%'+UPPER(@ObjectCode)+'%'");
                param.Add("@SearchText", searchText, DbType.AnsiString);
            }
        }

        if (excludeIdList != null && excludeIdList.Count > 0)
        {
            sbSql.Where("t.Id NOT IN @ExcludeIdList");
            param.Add("@ExcludeIdList", excludeIdList);
        }
        #endregion

        sbSql.LeftJoin($"{Currency.MsSqlTable} curr ON curr.IsDeleted=0 AND curr.ObjectCode=t.CurrencyCode");
        sbSql.LeftJoin($"{ReceiptPayment.MsSqlTable} rp ON rp.IsDeleted=0 AND rp.ReceiptId=t.Id");
        sbSql.LeftJoin($"{User.MsSqlTable} c ON c.Id=t.CashierUserId");
		sbSql.LeftJoin($"{Customer.MsSqlTable} cust ON cust.Id=t.CustomerId");

		sbSql.OrderBy("t.ReceiptDate DESC");

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
                $"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ WHERE t.Id IN (SELECT Id FROM pg) /**orderby**/").RawSql;
        }

        using var cn = ConnectionFactory.GetDbConnection()!;

        var dataList = (await cn.QueryAsync<Receipt, Currency, ReceiptPayment, User, Customer, Receipt>(sql,
                                                    (obj, currency, payment, cashier, customer) =>
                                                    {
                                                        obj.Currency = currency;
                                                        obj.CashierUser = cashier;
                                                        obj.Payment = payment;
                                                        obj.Customer = customer;

                                                        return obj;
                                                    }, param, splitOn: "Id")).AsList();

        return dataList;
    }

    public async Task<List<Receipt>> SearchAsync(
                            int pgSize = 0, int pgNo = 0,
                            string? objectCode = null,
                            DateTime? receiptDateForm = null,
                            DateTime? receiptDateTo = null,
                            string? currencyCode = null,
                            decimal? totalAmountFrom = null,
                            decimal? totalAmountTo = null,
                            List<string>? statuses = null,
                            List<int>? cashierUserIds = null,
							List<int>? customerIds = null,
							string? customerName = null,
							string? customerID = null)
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

        if (receiptDateForm.HasValue)
        {
            sbSql.Where("t.ReceiptDate IS NOT NULL AND t.ReceiptDate>=@ReceiptDateFrom");
            param.Add("@ReceiptDateFrom", receiptDateForm.Value);

            if (receiptDateTo.HasValue)
            {
                sbSql.Where("t.ReceiptDate<=@ReceiptDateTo");
                param.Add("@ReceiptDateTo", receiptDateTo.Value);
            }
        }
        else if (receiptDateTo.HasValue)
        {
            sbSql.Where("t.ReceiptDate IS NOT NULL AND t.ReceiptDate>=@ReceiptDateTo");
            param.Add("@ReceiptDateTo", receiptDateTo.Value);
        }

        if (!string.IsNullOrEmpty(currencyCode))
        {
            sbSql.Where("t.CurrencyCode=@CurrencyCode");
            param.Add("@CurrencyCode", currencyCode, DbType.AnsiString);
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

        if (totalAmountTo.HasValue)
        {
            sbSql.Where("t.TotalAmount IS NOT NULL AND t.TotalAmount<=@TotalAmountTo");
            param.Add("@TotalAmountTo", totalAmountTo.Value);
        }

        if (statuses != null && statuses.Count > 0)
        {
            if (statuses.Count == 1)
            {
                sbSql.Where("t.[Status]=@Status");
                param.Add("@Status", statuses[0]);
            }
            else
            {
                sbSql.Where("t.Status IN @Status");
                param.Add("@Status", statuses);
            }
        }

        if (cashierUserIds != null && cashierUserIds.Count > 0)
        {
            if (cashierUserIds.Count == 1)
            {
                sbSql.Where("t.CashierUserId=@CashierUserId");
                param.Add("@CashierUserId", cashierUserIds[0]);
            }
            else
            {
                sbSql.Where("t.CashierUserId IN @CashierUserIds");
                param.Add("@CashierUserIds", cashierUserIds);
            }
        }

        if (customerIds != null && customerIds.Count > 0)
        {
            if (customerIds.Count == 1)
            {
                sbSql.Where("t.CustomerId=@CustomerId");
                param.Add("@CustomerId", customerIds[0]);
            }
            else
            {
				sbSql.Where("t.CustomerId IN @CustomerIdList");
				param.Add("@CustomerIdList", customerIds);
			}
        }

        if (!string.IsNullOrEmpty(customerName))
        {
            sbSql.Where("(UPPER(cust.ObjectName) LIKE '%'+@CustomerName+'%' OR cust.ObjectNameKh LIKE '%'+@CustomerName+'%')");
            param.Add("@CustomerName", customerName);
        }

		if (!string.IsNullOrEmpty(customerID))
		{
			sbSql.Where("UPPER(cust.ObjectCode) LIKE '%'+UPPER(@CustomerObjectCode)+'%'");
			param.Add("@CustomerObjectCode", customerName);
		}
		#endregion

		sbSql.LeftJoin($"{Currency.MsSqlTable} curr ON curr.IsDeleted=0 AND curr.ObjectCode=t.CurrencyCode");
		sbSql.LeftJoin($"{ReceiptPayment.MsSqlTable} rp ON rp.IsDeleted=0 AND rp.ReceiptId=t.Id");
		sbSql.LeftJoin($"{User.MsSqlTable} cash ON cash.Id=t.CashierUserId");
		sbSql.LeftJoin($"{Customer.MsSqlTable} cust ON cust.Id=t.CustomerId");

		sbSql.OrderBy("t.ReceiptDate DESC");
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
			sql = sbSql.AddTemplate(
				$";WITH pg AS (SELECT t.Id FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
				$"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ WHERE t.Id IN (SELECT Id FROM pg) /**orderby**/").RawSql;
		}

        using var cn = ConnectionFactory.GetDbConnection()!;

		var dataList = (await cn.QueryAsync<Receipt, Currency, ReceiptPayment, User, Customer, Receipt>(sql,
													(obj, currency, payment, cashier, customer) =>
													{
														obj.Currency = currency;
														obj.CashierUser = cashier;
														obj.Payment = payment;
														obj.Customer = customer;

														return obj;
													}, param, splitOn: "Id")).AsList();

		return dataList;
    }

    public async Task<DataPagination> GetSearchPaginationAsync(
                            int pgSize = 0,
                            string? objectCode = null,
                            DateTime? receiptDateForm = null,
                            DateTime? receiptDateTo = null,
                            string? currencyCode = null,
                            decimal? totalAmountFrom = null,
                            decimal? totalAmountTo = null,
                            List<string>? statuses = null,
                            List<int>? cashierUserIds = null,
							List<int>? customerIds = null,
							string? customerName = null,
							string? customerID = null)
    {
        if (pgSize < 0)
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

		if (receiptDateForm.HasValue)
		{
			sbSql.Where("t.ReceiptDate IS NOT NULL AND t.ReceiptDate>=@ReceiptDateFrom");
			param.Add("@ReceiptDateFrom", receiptDateForm.Value);

			if (receiptDateTo.HasValue)
			{
				sbSql.Where("t.ReceiptDate<=@ReceiptDateTo");
				param.Add("@ReceiptDateTo", receiptDateTo.Value);
			}
		}
		else if (receiptDateTo.HasValue)
		{
			sbSql.Where("t.ReceiptDate IS NOT NULL AND t.ReceiptDate>=@ReceiptDateTo");
			param.Add("@ReceiptDateTo", receiptDateTo.Value);
		}

		if (!string.IsNullOrEmpty(currencyCode))
		{
			sbSql.Where("t.CurrencyCode=@CurrencyCode");
			param.Add("@CurrencyCode", currencyCode, DbType.AnsiString);
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

		if (totalAmountTo.HasValue)
		{
			sbSql.Where("t.TotalAmount IS NOT NULL AND t.TotalAmount<=@TotalAmountTo");
			param.Add("@TotalAmountTo", totalAmountTo.Value);
		}

		if (statuses != null && statuses.Count > 0)
		{
			if (statuses.Count == 1)
			{
				sbSql.Where("t.[Status]=@Status");
				param.Add("@Status", statuses[0]);
			}
			else
			{
				sbSql.Where("t.Status IN @Status");
				param.Add("@Status", statuses);
			}
		}

		if (cashierUserIds != null && cashierUserIds.Count > 0)
		{
			if (cashierUserIds.Count == 1)
			{
				sbSql.Where("t.CashierUserId=@CashierUserId");
				param.Add("@CashierUserId", cashierUserIds[0]);
			}
			else
			{
				sbSql.Where("t.CashierUserId IN @CashierUserIds");
				param.Add("@CashierUserIds", cashierUserIds);
			}
		}

		if (customerIds != null && customerIds.Count > 0)
		{
			if (customerIds.Count == 1)
			{
				sbSql.Where("t.CustomerId=@CustomerId");
				param.Add("@CustomerId", customerIds[0]);
			}
			else
			{
				sbSql.Where("t.CustomerId IN @CustomerIdList");
				param.Add("@CustomerIdList", customerIds);
			}
		}

		if (!string.IsNullOrEmpty(customerName))
		{
			sbSql.Where("(UPPER(cust.ObjectName) LIKE '%'+@CustomerName+'%' OR cust.ObjectNameKh LIKE '%'+@CustomerName+'%')");
			param.Add("@CustomerName", customerName);
		}

		if (!string.IsNullOrEmpty(customerID))
		{
			sbSql.Where("UPPER(cust.ObjectCode) LIKE '%'+UPPER(@CustomerObjectCode)+'%'");
			param.Add("@CustomerObjectCode", customerName);
		}
		#endregion

		string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;

        using var cn = ConnectionFactory.GetDbConnection()!;

        decimal recordCount = await cn.ExecuteScalarAsync<int>(sql, param);
        int pageCount = pgSize == 0 ? 1 : (int)Math.Ceiling(recordCount / pgSize);

        DataPagination pagination = new()
        {
            ObjectType = typeof(Receipt).Name,
            PageSize = pgSize,
            PageCount = pageCount,
            RecordCount = (int)recordCount
        };

        return pagination;
    }
}