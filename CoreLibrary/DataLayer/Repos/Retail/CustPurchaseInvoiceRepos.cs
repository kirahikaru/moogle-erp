using DataLayer.Models.Finance;
using DataLayer.Models.Retail;
using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Repos.Retail;

public interface ICustomerPurchaseInvoiceRepos : IBaseWorkflowEnabledRepos<CustPurchaseInvoice>
{
	Task<CustPurchaseInvoice?> GetFullAsync(int id);
	Task<List<CustPurchaseInvoice>> SearchAsync(
		int pgSize = 0, int pgNo = 0,
		string? objectCode = null,
		string? customerId = null,
		string? customerName = null,
		DateTime? invoiceDateFrom = null,
		DateTime? invoiceDateTo = null,
		string? customerPurchaseOrderCode = null,
		decimal? totalPayableAmountFrom = null,
		decimal? totalPayableAmountTo = null,
		string? workflowStatus = null);

	Task<DataPagination> GetSearchPaginationAsync(
		int pgSize = 0,
		string? objectCode = null,
		string? customerId = null,
		string? customerName = null,
		DateTime? invoiceDateFrom = null,
		DateTime? invoiceDateTo = null,
		string? customerPurchaseOrderCode = null,
		decimal? totalPayableAmountFrom = null,
		decimal? totalPayableAmountTo = null,
		string? workflowStatus = null);
}

public class CustPurchaseInvoiceRepos(IConnectionFactory connectionFactory) : BaseWorkflowEnabledRepos<CustPurchaseInvoice>(connectionFactory, CustPurchaseInvoice.DatabaseObject), ICustomerPurchaseInvoiceRepos
{
	public async Task<CustPurchaseInvoice?> GetFullAsync(int id)
    {
        DynamicParameters param = new();
        SqlBuilder sbSql = new();
        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.Id=@Id");

        sbSql.LeftJoin($"{Customer.MsSqlTable} c ON c.Id=t.CustomerId");
        sbSql.LeftJoin($"{CustPurchaseOrder.MsSqlTable} cpo ON cpo.Id=t.CustomerPurchaseOrderId");
        sbSql.LeftJoin($"{Currency.MsSqlTable} curr ON curr.IsDeleted=0 AND curr.ObjectCode=t.CurrencyCode");

        var sql = sbSql.AddTemplate($"SELECT * FROM {CustPurchaseInvoice.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;

        using var cn = ConnectionFactory.GetDbConnection()!;

        var dataList = (await cn.QueryAsync<CustPurchaseInvoice, Customer, CustPurchaseOrder, Currency, CustPurchaseInvoice>
                                (sql, (obj, customer, po, currency) =>
                                {
                                    obj.Customer = customer;
                                    obj.PurchaseOrder = po;
                                    obj.Currency = currency;

                                    return obj;
                                }, new { Id = id }, splitOn: "Id")).AsList();

        if (dataList != null && dataList.Count != 0)
        {
            string itemQry = $"SELECT * FROM {CustPurchaseInvItem.MsSqlTable} cpii WHERE cpii.IsDeleted=0 AND cpii.CustomerPurhcaseInvoiceId=@CustomerPurhcaseInvoiceId; " +
                             $"SELECT * FROM {CustPurchaseInvPayment.MsSqlTable} cpip WHERE cpip.IsDeleted=0 AND cpip.CustomerPurhcaseInvoiceId=@CustomerPurhcaseInvoiceId; " +
                             $"SELECT * FROM {RetailOtherCharge.MsSqlTable} roc WHERE roc.IsDeleted=0 AND roc.LinkedObjectType='CustomerPurchaseInvoice' AND roc.LinkedObjectId=@CustomerPurhcaseInvoiceId; " +
                             $"SELECT * FROM {RetailTaxItem.MsSqlTable} rti WHERE rti.IsDeleted=0 AND rti.LinkedObjectType='CustomerPurchaseInvoice' AND rti.LinkedObjectId=@CustomerPurhcaseInvoiceId";

            using (var multi = await cn.QueryMultipleAsync(itemQry, new { CustomerPurhcaseInvoiceId = dataList[0].Id }))
            {
                dataList[0].Items = (await multi.ReadAsync<CustPurchaseInvItem>()).AsList();
                dataList[0].Payments = (await multi.ReadAsync<CustPurchaseInvPayment>()).AsList();
                dataList[0].OtherCharges = (await multi.ReadAsync<RetailOtherCharge>()).AsList();
                dataList[0].TaxItems = (await multi.ReadAsync<RetailTaxItem>()).AsList();
            }

            return dataList[0];
        }

        return null;
    }

    public override async Task<List<CustPurchaseInvoice>> QuickSearchAsync(int pgSize = 0, int pgNo = 0, string? searchText = null, List<int>? excludeIdList = null)
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
                sbSql.Where("UPPER(c.ObjectName) LIKE '%'+UPPER(@SearchText)+'%'");
                param.Add("@SearchText", searchText, DbType.AnsiString);
            }
        }

        if (excludeIdList != null && excludeIdList.Count > 0)
        {
            sbSql.Where("t.Id NOT IN @ExcludeIdList");
            param.Add("@ExcludeIdList", excludeIdList);
        }
        #endregion

        sbSql.LeftJoin($"{Customer.MsSqlTable} c ON c.Id=t.CustomerId");
        sbSql.LeftJoin($"{CustPurchaseOrder.MsSqlTable} cpo ON cpo.Id=t.CustomerPurchaseOrderId");
        sbSql.LeftJoin($"{Currency.MsSqlTable} curr ON curr.IsDeleted=0 AND curr.ObjectCode=t.CurrencyCode");

        sbSql.OrderBy("t.InvoiceDate DESC");
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
                                    $"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ WHERE t.Id IN (SELECT Id FROM pg) /**orderby**/").RawSql;
        }

        using var cn = ConnectionFactory.GetDbConnection()!;

        var data = (await cn.QueryAsync<CustPurchaseInvoice, Customer, CustPurchaseOrder, Currency, CustPurchaseInvoice>(sql,
                                        (obj, customer, po, currency) =>
                                        {
                                            obj.Customer = customer;
                                            obj.PurchaseOrder = po;
                                            obj.Currency = currency;

                                            return obj;
                                        }, param, splitOn: "Id")).AsList();

        return data;
    }

    public async Task<List<CustPurchaseInvoice>> SearchAsync(
        int pgSize = 0, int pgNo = 0,
        string? objectCode = null,
        string? customerId = null,
        string? customerName = null,
        DateTime? invoiceDateFrom = null,
        DateTime? invoiceDateTo = null,
        string? customerPurchaseOrderCode = null,
        decimal? totalPayableAmountFrom = null,
        decimal? totalPayableAmountTo = null, 
        string? workflowStatus = null)
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

        if (!string.IsNullOrEmpty(customerId))
        {
            sbSql.Where("UPPER(c.ObjectCode) LIKE '%'+UPPER(@CustomerId)+'%'");
            param.Add("@CustomerId", customerId, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(customerName))
        {
            sbSql.Where("UPPER(c.ObjectName) LIKE '%'+UPPER(@CustomerName)+'%'");
            param.Add("@CustomerName", customerName, DbType.AnsiString);
        }

        if (invoiceDateFrom.HasValue)
        {
            sbSql.Where("t.InvoiceDate IS NOT NULL AND t.InvoiceDate>=@InvoiceDateFrom");
            param.Add("@InvoiceDateFrom", invoiceDateFrom.Value);

            if (invoiceDateTo.HasValue)
            {
                sbSql.Where("t.InvoiceDate<=@InvoiceDateTo");
                param.Add("@InvoiceDateTo", invoiceDateTo.Value);
            }
        }
        else if (invoiceDateTo.HasValue)
        {
            sbSql.Where("t.InvoiceDate IS NOT NULL AND t.InvoiceDate<=@InvoiceDateTo");
            param.Add("@InvoiceDateTo", invoiceDateTo.Value);
        }

        if (!string.IsNullOrEmpty(customerPurchaseOrderCode))
        {
            sbSql.Where("UPPER(cpo.ObjectCode) LIKE '%'+UPPER(@CustomerPurchaseOrderCode)+'%'");
            param.Add("@CustomerPurchaseOrderCode", customerPurchaseOrderCode, DbType.AnsiString);
        }

        if (totalPayableAmountFrom.HasValue)
        {
            sbSql.Where("t.TotalPayableAmount IS NOT NULL AND t.TotalPayableAmount>=@TotalPayableAmountFrom");
            param.Add("@TotalPayableAmountFrom", totalPayableAmountFrom.Value);

            if (totalPayableAmountTo.HasValue)
            {
                sbSql.Where("t.TotalPayableAmount<=@TotalPayableAmountTo");
                param.Add("@TotalPayableAmountTo", totalPayableAmountTo.Value);
            }
        }
        else if (totalPayableAmountTo.HasValue)
        {
            sbSql.Where("t.TotalPayableAmount<=@TotalPayableAmountTo");
            param.Add("@TotalPayableAmountTo", totalPayableAmountTo.Value);
        }

        if (!string.IsNullOrEmpty(workflowStatus))
        {
            sbSql.Where("t.WorkflowStatus=@WorkflowStatus");
            param.Add("@WorkflowStatus", workflowStatus, DbType.AnsiString);
        }
        #endregion

        sbSql.LeftJoin($"{Customer.MsSqlTable} c ON c.Id=t.CustomerId");
        sbSql.LeftJoin($"{CustPurchaseOrder.MsSqlTable} cpo ON cpo.Id=t.CustomerPurchaseOrderId");
        sbSql.LeftJoin($"{Currency.MsSqlTable} curr ON curr.IsDeleted=0 AND curr.ObjectCode=t.CurrencyCode");
        //sbSql.LeftJoin($"{ExchangeRate.MsSqlTable} xchg ON xchg.IsDeleted=0 AND xchg.Id=t.KhrExchangeRateId");

        sbSql.OrderBy("t.InvoiceDate DESC");
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

            sql = sbSql.AddTemplate($";WITH pg AS (SELECT t.Id FROM {DbObject.MsSqlTable} t LEFT JOIN {Customer.MsSqlTable} c ON c.Id=t.CustomerId /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
                                    $"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ WHERE t.Id IN (SELECT Id FROM pg) /**orderby**/").RawSql;
        }

        using var cn = ConnectionFactory.GetDbConnection()!;

        var data = (await cn.QueryAsync<CustPurchaseInvoice, Customer, CustPurchaseOrder, Currency, CustPurchaseInvoice>(sql,
                                        (obj, customer, po, currency) =>
                                        {
                                            obj.Customer = customer;
                                            obj.PurchaseOrder = po;
                                            obj.Currency = currency;

                                            return obj;
                                        }, param, splitOn: "Id")).AsList();

        return data;
    }

    public async Task<DataPagination> GetSearchPaginationAsync(
        int pgSize = 0,
        string? objectCode = null,
        string? customerId = null,
        string? customerName = null,
        DateTime? invoiceDateFrom = null,
        DateTime? invoiceDateTo = null,
        string? customerPurchaseOrderCode = null,
        decimal? totalPayableAmountFrom = null,
        decimal? totalPayableAmountTo = null,
		string? workflowStatus = null)
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

        if (!string.IsNullOrEmpty(customerId))
        {
            sbSql.Where("UPPER(c.ObjectCode) LIKE '%'+UPPER(@CustomerId)+'%'");
            param.Add("@CustomerId", customerId, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(customerName))
        {
            sbSql.Where("UPPER(c.ObjectName) LIKE '%'+UPPER(@CustomerName)+'%'");
            param.Add("@CustomerName", customerName, DbType.AnsiString);
        }

        if (invoiceDateFrom.HasValue)
        {
            sbSql.Where("t.InvoiceDate IS NOT NULL AND t.InvoiceDate>=@InvoiceDateFrom");
            param.Add("@InvoiceDateFrom", invoiceDateFrom.Value);

            if (invoiceDateTo.HasValue)
            {
                sbSql.Where("t.InvoiceDate<=@InvoiceDateTo");
                param.Add("@InvoiceDateTo", invoiceDateTo.Value);
            }
        }
        else if (invoiceDateTo.HasValue)
        {
            sbSql.Where("t.InvoiceDate IS NOT NULL AND t.InvoiceDate<=@InvoiceDateTo");
            param.Add("@InvoiceDateTo", invoiceDateTo.Value);
        }

		if (!string.IsNullOrEmpty(workflowStatus))
		{
			sbSql.Where("t.WorkflowStatus=@WorkflowStatus");
			param.Add("@WorkflowStatus", workflowStatus, DbType.AnsiString);
		}
		#endregion

		string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;

        using var cn = ConnectionFactory.GetDbConnection()!;

        decimal recordCount = await cn.ExecuteScalarAsync<int>(sql, param);
        int pageCount = (int)Math.Ceiling(recordCount / pgSize);

        DataPagination pagination = new()
        {
            ObjectType = typeof(CustPurchaseInvoice).Name,
            PageSize = pgSize,
            PageCount = pageCount,
            RecordCount = (int)recordCount
        };

        return pagination;
    }
}