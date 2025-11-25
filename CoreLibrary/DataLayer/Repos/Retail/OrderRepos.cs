// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using DataLayer.Models.Finance;
using DataLayer.Models.Retail;
using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Repos.Retail;

public interface IOrderRepos : IBaseRepos<Order>
{
	Task<List<Order>> SearchAsync(
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

public class OrderRepos(IConnectionFactory connectionFactory) : BaseRepos<Order>(connectionFactory, Order.DatabaseObject), IOrderRepos
{
	public async Task<List<Order>> SearchAsync(
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
        sbSql.LeftJoin($"{Receipt.MsSqlTable} rcpt ON rcpt.Id=t.ReceiptId");

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
                                    $"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ WHERE t.Id IN (SELECT Id FROM pg) /**orderby**/").RawSql;
        }

        using var cn = ConnectionFactory.GetDbConnection()!;

        var data = (await cn.QueryAsync<Order, Customer, DeliveryOption, Currency, Receipt, Order>(sql,
                                        (obj, customer, delo, curr, receipt) =>
                                        {
                                            obj.DeliveryOption = delo;
                                            obj.Customer = customer;
                                            obj.Currency = curr;
                                            obj.Receipt = receipt;

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
        int pageCount = (int)(Math.Ceiling(recordCount / pgSize));

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