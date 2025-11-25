using DataLayer.Models.RMS;

namespace DataLayer.Repos.RMS;

public interface ICustomerPurchaseOrderItemRepos : IBaseRepos<CustPurchaseOrderItem>
{
	Task<CustPurchaseOrderItem?> GetFullAsync(int id);

	Task<List<CustPurchaseOrderItem>> GetByOrderAsync(int customerPurchaseOrderId);

	Task<List<CustPurchaseOrderItem>> SearchAsync(
		int pgSize = 0, int pgNo = 0,
		string? objectCode = null,
		string? objectName = null,
		string? objectNameKh = null,
		string? barcode = null,
		string? orderNumber = null);

	Task<DataPagination> GetSearchPaginationAsync(
		int pgSize = 0,
		string? objectCode = null,
		string? objectName = null,
		string? objectNameKh = null,
		string? barcode = null,
		string? invoiceNumber = null);
}

public class CustPurchaseOrderItemRepos(IDbContext dbContext) : BaseRepos<CustPurchaseOrderItem>(dbContext, CustPurchaseOrderItem.DatabaseObject), ICustomerPurchaseOrderItemRepos
{
	public async Task<CustPurchaseOrderItem?> GetFullAsync(int id)
    {
        string sql = $"SELECT * FROM {DbObject.MsSqlTable} t " +
                     $"LEFT JOIN {Item.MsSqlTable} i ON i.Id=t.ItemId " +
                     $"LEFT JOIN {CustPurchaseOrder.MsSqlTable} cpo ON cpo.Id=t.CustomerPurchaseInvoiceId " +
                     $"LEFT JOIN {Currency.MsSqlTable} curr ON curr.IsDeleted=0 AND curr.ObjectCode=cpo.CurrencyCode " +
                     $"WHERE t.IsDeleted=0 AND t.Id=@Id";

        using var cn = DbContext.DbCxn;

        List<CustPurchaseOrderItem> dataList = (await cn.QueryAsync<CustPurchaseOrderItem, Item, CustPurchaseOrder, Currency, CustPurchaseOrderItem>
                                                    (sql, (obj, item, order, currency) =>
                                                    {
                                                        obj.Item = item;

                                                        if (order != null)
                                                        {
                                                            order.Currency = currency;
                                                            obj.Order = order;
                                                        }

                                                        return obj;
                                                    }, new { Id = id }, splitOn: "Id")).AsList();

        if (dataList.Any())
            return dataList[0];
        else
            return null;
    }

    public async Task<List<CustPurchaseOrderItem>> GetByOrderAsync(int customerPurchaseOrderId)
    {
        SqlBuilder sbSql = new();

        sbSql.LeftJoin($"{Item.MsSqlTable} i ON i.Id=t.ItemId");
        sbSql.LeftJoin($"{CustPurchaseOrder.MsSqlTable} cpo ON cpo.Id=t.CustomerPurchaseOrderId");
        sbSql.LeftJoin($"{Currency.MsSqlTable} curr ON curr.IsDeleted=0 AND curr.ObjectCode=t.CurrencyCode");

        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("cpo.ObjectCode IS NOT NULL");
        sbSql.Where("cpo.Id=@CustomerPurchaseOrderId");

        string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;

        using var cn = DbContext.DbCxn;

        var dataList = (await cn.QueryAsync<CustPurchaseOrderItem, Item, CustPurchaseOrder, Currency, CustPurchaseOrderItem>
                                (sql, (obj, item, order, currency) =>
                                {
                                    obj.Item = item;

                                    if (order != null)
                                    {
                                        order.Currency = currency;
                                        obj.Order = order;
                                    }

                                    return obj;
                                }, new { CustomerPurchaseOrderId = customerPurchaseOrderId }, splitOn: "Id")).AsList();

        return dataList;
    }

    public async Task<List<CustPurchaseOrderItem>> SearchAsync(
        int pgSize = 0, int pgNo = 0,
        string? objectCode = null,
        string? objectName = null,
        string? objectNameKh = null,
        string? barcode = null,
        string? orderNumber = null)
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
            sbSql.Where("UPPER(t.ObjectName) LIKE '%'+UPPER(@ObjectName)+'%'");
            param.Add("@ObjectName", objectName, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(objectNameKh))
        {
            sbSql.Where("t.ObjectNameKh LIKE '%'+@ObjectNameKh+'%'");
            param.Add("@ObjectNameKh", objectNameKh, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(barcode))
        {
            sbSql.Where("t.Barcode LIKE '%'+@Barcode+'%'");
            param.Add("@Barcode", barcode, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(orderNumber))
        {
            sbSql.Where("UPPER(cpo.ObjectCode) LIKE '%'+UPPER(@OrderNumber)+'%'");
            param.Add("@OrderNumber", orderNumber, DbType.AnsiString);
        }
        #endregion

        sbSql.LeftJoin($"{Item.MsSqlTable} i ON i.Id=t.ItemId");
        sbSql.LeftJoin($"{CustPurchaseOrder.MsSqlTable} cpo ON cpo.Id=t.CustomerPurchaseOrderId");
        sbSql.LeftJoin($"{Currency.MsSqlTable} curr ON curr.IsDeleted=0 AND curr.ObjectCode=t.CurrencyCode");

        sbSql.OrderBy("cpo.OrderDateTime DESC");
        sbSql.OrderBy("t.SequenceNo ASC");

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

        using var cn = DbContext.DbCxn;

        var dataList = (await cn.QueryAsync<CustPurchaseOrderItem, Item, CustPurchaseOrder, Currency, CustPurchaseOrderItem>
                                                    (sql, (obj, item, order, currency) =>
                                                    {
                                                        obj.Item = item;

                                                        if (order != null)
                                                        {
                                                            order.Currency = currency;
                                                            obj.Order = order;
                                                        }

                                                        return obj;
                                                    }, param, splitOn: "Id")).AsList();

        return dataList;
    }

    public async Task<DataPagination> GetSearchPaginationAsync(
        int pgSize = 0,
        string? objectCode = null,
        string? objectName = null,
        string? objectNameKh = null,
        string? barcode = null,
        string? orderNumber = null)
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
            sbSql.Where("UPPER(t.ObjectName) LIKE '%'+UPPER(@ObjectName)+'%'");
            param.Add("@ObjectName", objectName, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(objectNameKh))
        {
            sbSql.Where("t.ObjectNameKh LIKE '%'+@ObjectNameKh+'%'");
            param.Add("@ObjectNameKh", objectNameKh, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(barcode))
        {
            sbSql.Where("t.Barcode LIKE '%'+@Barcode+'%'");
            param.Add("@Barcode", barcode, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(orderNumber))
        {
            sbSql.Where("UPPER(cpo.ObjectCode) LIKE '%'+UPPER(@OrderNumber)+'%'");
            param.Add("@OrderNumber", orderNumber, DbType.AnsiString);
        }
        #endregion

        string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t LEFT JOIN {CustPurchaseOrder.MsSqlTable} cpo ON cpo.Id=t.CustomerPurchaseOrderId /**where**/").RawSql;

        using var cn = DbContext.DbCxn;

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