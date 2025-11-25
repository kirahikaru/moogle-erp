using DataLayer.Models.Finance;
using DataLayer.Models.Retail;
using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Repos.Retail;

public interface ICustomerPurchaseOrderRepos : IBaseWorkflowEnabledRepos<CustPurchaseOrder>
{
	Task<CustPurchaseOrder?> GetFullAsync(int id);

	Task<List<CustPurchaseOrder>> SearchAsync(
		int pgSize = 0, int pgNo = 0,
		string? objectCode = null,
		DateTime? orderDateTimeFrom = null,
		DateTime? orderDateTimeTo = null,
		string? customerId = null,
		string? customerName = null,
		decimal? totalPayableAmountFrom = null,
		decimal? totalPayableAmountTo = null,
		List<int>? deliveryOptionIdList = null,
		string? workflowStatus = null);

	Task<DataPagination> GetSearchPaginationAsync(
		int pgSize = 0,
		string? objectCode = null,
		DateTime? orderDateTimeFrom = null,
		DateTime? orderDateTimeTo = null,
		string? customerId = null,
		string? customerName = null,
		decimal? totalPayableAmountFrom = null,
		decimal? totalPayableAmountTo = null,
		List<int>? deliveryOptionIdList = null,
		string? workflowStatus = null);
}

public class CustPurchaseOrderRepos(IConnectionFactory connectionFactory) : BaseWorkflowEnabledRepos<CustPurchaseOrder>(connectionFactory, CustPurchaseOrder.DatabaseObject), ICustomerPurchaseOrderRepos
{
	public async Task<CustPurchaseOrder?> GetFullAsync(int id)
    {
        DynamicParameters param = new();
        SqlBuilder sbSql = new();
        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.Id=@Id");

        sbSql.LeftJoin($"{Customer.MsSqlTable} c ON c.Id=t.CustomerId");
        sbSql.LeftJoin($"{DeliveryOption.MsSqlTable} do ON do.Id=t.DeliveryOptionId");
        sbSql.LeftJoin($"{Currency.MsSqlTable} curr ON curr.IsDeleted=0 AND curr.ObjectCode=t.CurrencyCode");
        sbSql.LeftJoin($"{CustPurchaseInvoice.MsSqlTable} cpi ON cpi.Id=t.CustomerPurchaseInvoiceId");

        string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;

        using var cn = ConnectionFactory.GetDbConnection()!;

        var dataList = (await cn.QueryAsync<CustPurchaseOrder, Customer, DeliveryOption, Currency, CustPurchaseInvoice, CustPurchaseOrder>(
                                                sql, (obj, customer, dlo, curr, cpi) =>
                                                {
                                                    obj.Customer = customer;
                                                    obj.DeliveryOption = dlo;
                                                    obj.Currency = curr;
                                                    obj.Invoice = cpi;

                                                    return obj;
                                                }, new { Id = id }, splitOn: "Id")).AsList();

        if (dataList != null && dataList.Any())
        {
            string itemQry = $"SELECT * FROM {CustPurchaseOrderItem.MsSqlTable} WHERE IsDeleted=0 AND CustomerPurchaseOrderId=@CustomerPurchaseOrderId";

            var items = (await cn.QueryAsync<CustPurchaseOrderItem>(itemQry, new { CustomerPurchaseOrderId = dataList[0].Id })).AsList();
            dataList[0].Items = items;
            return dataList[0];
        }

        return null;
    }

    public override async Task<List<CustPurchaseOrder>> QuickSearchAsync(int pgSize = 0, int pgNo = 0, string? searchText = null, List<int>? excludeIdList = null)
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
                sbSql.Where("(UPPER(t.ObjectCode) LIKE '%'+UPPER(@SearchText)+'%' OR UPPER(c.ObjectName) LIKE '%'+UPPER(@SearchText)+'%')");
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
        sbSql.LeftJoin($"{DeliveryOption.MsSqlTable} do ON do.Id=t.DeliveryOptionId");
        sbSql.LeftJoin($"{Currency.MsSqlTable} curr ON curr.IsDeleted=0 AND curr.ObjectCode=t.CurrencyCode");
        sbSql.LeftJoin($"{CustPurchaseInvoice.MsSqlTable} cpi ON cpi.Id=t.CustomerPurchaseInvoiceId");

        sbSql.OrderBy("t.OrderDateTime DESC");
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
                                              $"SELECT t.*, c.*, do.*, curr.*, cpi.* FROM {DbObject.MsSqlTable} t INNER JOIN pg p ON p.Id=t.Id /**leftjoin**/ /**orderby**/").RawSql;
        }

        using var cn = ConnectionFactory.GetDbConnection()!;

        var data = (await cn.QueryAsync<CustPurchaseOrder, Customer, DeliveryOption, Currency, CustPurchaseInvoice, CustPurchaseOrder>(sql,
                                        (obj, customer, delo, curr, invoice) =>
                                        {
                                            obj.DeliveryOption = delo;
                                            obj.Customer = customer;
                                            obj.Currency = curr;
                                            obj.Invoice = invoice;

                                            return obj;
                                        }, param, splitOn: "Id")).AsList();

        return data;
    }

    public async Task<List<CustPurchaseOrder>> SearchAsync(
        int pgSize = 0, int pgNo = 0,
        string? objectCode = null,
        DateTime? orderDateTimeFrom = null,
        DateTime? orderDateTimeTo = null,
        string? customerId = null,
        string? customerName = null,
        decimal? totalPayableAmountFrom = null,
        decimal? totalPayableAmountTo = null,
        List<int>? deliveryOptionIdList = null,
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

        if (orderDateTimeFrom.HasValue)
        {
            sbSql.Where("t.OrderDateTime IS NOT NULL AND t.OrderDateTime>=@OrderDateTimeFrom");
            param.Add("@OrderDateTimeFrom", orderDateTimeFrom.Value);

            if (orderDateTimeTo.HasValue)
            {
                sbSql.Where("t.OrderDateTime<=@OrderDateTimeTo");
                param.Add("@OrderDateTimeTo", orderDateTimeTo.Value);
            }
        }
        else if (orderDateTimeTo.HasValue)
        {
            sbSql.Where("t.OrderDateTime IS NOT NULL AND t.OrderDateTime<=@OrderDateTimeTo");
            param.Add("@OrderDateTimeTo", orderDateTimeTo.Value);
        }

        if (!string.IsNullOrEmpty(customerId))
        {
            sbSql.Where("UPPER(c.ObjectCode) LIKE '%'+UPPER(@CustomerId)+'%'");
            param.Add("@CustomerId", customerId, DbType.AnsiString);
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

        if (deliveryOptionIdList != null && deliveryOptionIdList.Any())
        {
            if (deliveryOptionIdList.Count == 1)
            {
                sbSql.Where("t.DeliveryOptionId=@DeliveryOptionId");
                param.Add("@DeliveryOptionId", deliveryOptionIdList[0]);
            }
            else
            {
                sbSql.Where("t.DeliveryOptionId IN @DeliveryOptionIdList");
                param.Add("@DeliveryOptionIdList", deliveryOptionIdList);
            }
        }

		if (!string.IsNullOrEmpty(workflowStatus))
		{
			sbSql.Where("t.WorkflowStatus=@WorkflowStatus");
			param.Add("@WorkflowStatus", workflowStatus, DbType.AnsiString);
		}
		#endregion

		sbSql.LeftJoin($"{Customer.MsSqlTable} c ON c.Id=t.CustomerId");
        sbSql.LeftJoin($"{DeliveryOption.MsSqlTable} do ON do.Id=t.DeliveryOptionId");
        sbSql.LeftJoin($"{Currency.MsSqlTable} curr ON curr.IsDeleted=0 AND curr.ObjectCode=t.CurrencyCode");
        sbSql.LeftJoin($"{CustPurchaseInvoice.MsSqlTable} cpi ON cpi.Id=t.CustomerPurchaseInvoiceId");

        sbSql.OrderBy("t.OrderDateTime DESC");
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
                                    $"SELECT t.*, c.*, curr.* FROM {DbObject.MsSqlTable} t INNER JOIN pg p ON p.Id=t.Id /**leftjoin**/ /**orderby**/").RawSql;
        }

        using var cn = ConnectionFactory.GetDbConnection()!;

        var data = (await cn.QueryAsync<CustPurchaseOrder, Customer, DeliveryOption, Currency, CustPurchaseInvoice, CustPurchaseOrder>(sql,
                                        (obj, customer, delo, curr, invoice) =>
                                        {
                                            obj.DeliveryOption = delo;
                                            obj.Customer = customer;
                                            obj.Currency = curr;
                                            obj.Invoice = invoice;

                                            return obj;
                                        }, param, splitOn: "Id")).AsList();

        return data;
    }

    public async Task<DataPagination> GetSearchPaginationAsync(
        int pgSize = 0,
        string? objectCode = null,
        DateTime? orderDateTimeFrom = null,
        DateTime? orderDateTimeTo = null,
        string? customerId = null,
        string? customerName = null,
        decimal? totalPayableAmountFrom = null,
        decimal? totalPayableAmountTo = null,
        List<int>? deliveryOptionIdList = null,
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

        if (orderDateTimeFrom.HasValue)
        {
            sbSql.Where("t.OrderDateTime IS NOT NULL AND t.OrderDateTime>=@OrderDateTimeFrom");
            param.Add("@OrderDateTimeFrom", orderDateTimeFrom.Value);

            if (orderDateTimeTo.HasValue)
            {
                sbSql.Where("t.OrderDateTime<=@OrderDateTimeTo");
                param.Add("@OrderDateTimeTo", orderDateTimeTo.Value);
            }
        }
        else if (orderDateTimeTo.HasValue)
        {
            sbSql.Where("t.OrderDateTime IS NOT NULL AND t.OrderDateTime<=@OrderDateTimeTo");
            param.Add("@OrderDateTimeTo", orderDateTimeTo.Value);
        }

        if (!string.IsNullOrEmpty(customerId))
        {
            sbSql.Where("UPPER(c.ObjectCode) LIKE '%'+UPPER(@CustomerId)+'%'");
            param.Add("@CustomerId", customerId, DbType.AnsiString);
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

		if (deliveryOptionIdList != null && deliveryOptionIdList.Count != 0)
        {
            if (deliveryOptionIdList.Count == 1)
            {
                sbSql.Where("t.DeliveryOptionId=@DeliveryOptionId");
                param.Add("@DeliveryOptionId", deliveryOptionIdList[0]);
            }
            else
            {
                sbSql.Where("t.DeliveryOptionId IN @DeliveryOptionIdList");
                param.Add("@DeliveryOptionIdList", deliveryOptionIdList);
            }
        }

        if (!string.IsNullOrEmpty(workflowStatus))
        {
            sbSql.Where("t.WorkflowStatus=@WorkflowStatus");
            param.Add("@WorkflowStatus", workflowStatus, DbType.AnsiString);
        }
        #endregion

        string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t LEFT JOIN {Customer.MsSqlTable} c ON c.Id=t.CustomerId /**where**/").RawSql;

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