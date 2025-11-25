using DataLayer.Models.Retail;
using DataLayer.Models.SystemCore.NonPersistent;
namespace DataLayer.Repos.Retail;

public interface ICustPurchaseInvItemRepos : IBaseRepos<CustPurchaseInvItem>
{
	Task<CustPurchaseInvItem?> GetFullAsync(int id);

	Task<List<CustPurchaseInvItem>> GetByInvoiceAsync(int customerPurchaseInvoiceId);

	Task<List<CustPurchaseInvItem>> SearchAsync(
		int pgSize = 0, int pgNo = 0,
		string? objectCode = null,
		string? objectName = null,
		string? objectNameKh = null,
		string? barcode = null,
		string? invoiceNumber = null,
		bool? isPaid = null,
		string? paymentRefNo = null);

	Task<DataPagination> GetSearchPaginationAsync(
		int pgSize = 0,
		string? objectCode = null,
		string? objectName = null,
		string? objectNameKh = null,
		string? barcode = null,
		string? invoiceNumber = null,
		bool? isPaid = null,
		string? paymentRefNo = null);
}

public class CustPurchaseInvItemRepos(IConnectionFactory connectionFactory) : BaseRepos<CustPurchaseInvItem>(connectionFactory, CustPurchaseInvItem.DatabaseObject), ICustPurchaseInvItemRepos
{
	public async Task<CustPurchaseInvItem?> GetFullAsync(int id)
	{
		string sql = $"SELECT * FROM {DbObject.MsSqlTable} t " +
						$"LEFT JOIN {Item.MsSqlTable} i ON i.Id=t.ItemId " +
						$"LEFT JOIN {CustPurchaseInvoice.MsSqlTable} cpi ON cpi.Id=i.CustomerPurhcaseInvoiceId " +
						$"WHERE t.IsDeleted=0 AND t.Id=@Id";

		using var cn = ConnectionFactory.GetDbConnection()!;

		List<CustPurchaseInvItem> dataList = (await cn.QueryAsync<CustPurchaseInvItem, Item, CustPurchaseInvoice, CustPurchaseInvItem>(
												sql, (obj, item, invoice) =>
												{
													obj.Item = item;
													obj.Invoice = invoice;

													return obj;
												}, new { Id = id }, splitOn: "Id")).AsList();

		if (dataList.Count != 0)
			return dataList[0];
		else
			return null;
	}

	public async Task<List<CustPurchaseInvItem>> GetByInvoiceAsync(int customerPurchaseInvoiceId)
	{
		string sql = $"SELECT * FROM {DbObject.MsSqlTable} t " +
						$"LEFT JOIN {Item.MsSqlTable} i ON i.Id=t.ItemId " +
						$"LEFT JOIN {CustPurchaseInvoice.MsSqlTable} cpi ON cpi.Id=i.CustomerPurhcaseInvoiceId " +
						$"WHERE t.IsDeleted=0 AND t.CustomerPurhcaseInvoiceId=@CustomerPurhcaseInvoiceId";

		using var cn = ConnectionFactory.GetDbConnection()!;

		List<CustPurchaseInvItem> dataList = (await cn.QueryAsync<CustPurchaseInvItem, Item, CustPurchaseInvoice, CustPurchaseInvItem>(
												sql, (obj, item, invoice) =>
												{
													obj.Item = item;
													obj.Invoice = invoice;

													return obj;
												}, new { CustomerPurhcaseInvoiceId = customerPurchaseInvoiceId }, splitOn: "Id")).AsList();

		return dataList;
	}

	public async Task<List<CustPurchaseInvItem>> SearchAsync(
		int pgSize = 0, int pgNo = 0,
		string? objectCode = null,
		string? objectName = null,
		string? objectNameKh = null,
		string? barcode = null,
		string? invoiceNumber = null,
		bool? isPaid = null,
		string? paymentRefNo = null)
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

		if (!string.IsNullOrEmpty(barcode))
		{
			sbSql.Where("t.Barcode=@Barcode");
			param.Add("@Barcode", barcode, DbType.AnsiString);
		}

		if (!string.IsNullOrEmpty(invoiceNumber))
		{
			sbSql.Where("UPPER(cpi.ObjectCode) LIKE '%'+UPPER(@InvoiceNumber)+'%'");
			param.Add("@InvoiceNumber", invoiceNumber, DbType.AnsiString);
		}

		if (isPaid.HasValue)
		{
			if (isPaid.Value)
				sbSql.Where("t.PaidDateTime IS NOT NULL");
			else
				sbSql.Where("t.PaidDateTime IS NULL");

		}

		if (!string.IsNullOrEmpty(paymentRefNo))
		{
			sbSql.Where("t.PaymentRefNo LIKE '%'+@PaymentRefNo+'%'");
			param.Add("@PaymentRefNo", paymentRefNo, DbType.AnsiString);
		}
		#endregion

		sbSql.LeftJoin($"{Item.MsSqlTable} i ON c.Id=t.CustomerId");
		sbSql.LeftJoin($"{CustPurchaseInvoice.MsSqlTable} cpi ON cpi.Id=t.CustomerPurhcaseInvoiceId");

		sbSql.OrderBy("cpi.InvoiceDate DESC");
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

			sql = sbSql.AddTemplate($";WITH pg AS (SELECT t.Id FROM {DbObject.MsSqlTable} t LEFT JOIN {CustPurchaseInvoice.MsSqlTable} cpi ON cpi.Id=t.CustomerPurhcaseInvoiceId /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
									$"SELECT t.*, i.*, cpi.* FROM {DbObject.MsSqlTable} t INNER JOIN pg p ON p.Id=t.Id /**leftjoin**/ /**orderby**/").RawSql;
		}

		using var cn = ConnectionFactory.GetDbConnection()!;

		List<CustPurchaseInvItem> data = (await cn.QueryAsync<CustPurchaseInvItem, Item, CustPurchaseInvoice, CustPurchaseInvItem>(sql,
										(obj, item, invoice) =>
										{
											obj.Item = item;
											obj.Invoice = invoice;

											return obj;
										}, param, splitOn: "Id")).AsList();

		return data;
	}

	public async Task<DataPagination> GetSearchPaginationAsync(
		int pgSize = 0,
		string? objectCode = null,
		string? objectName = null,
		string? objectNameKh = null,
		string? barcode = null,
		string? invoiceNumber = null,
		bool? isPaid = null,
		string? paymentRefNo = null)
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

		if (!string.IsNullOrEmpty(barcode))
		{
			sbSql.Where("t.Barcode=@Barcode");
			param.Add("@Barcode", barcode, DbType.AnsiString);
		}

		if (!string.IsNullOrEmpty(invoiceNumber))
		{
			sbSql.Where("UPPER(cpi.ObjectCode) LIKE '%'+UPPER(@InvoiceNumber)+'%'");
			param.Add("@InvoiceNumber", invoiceNumber, DbType.AnsiString);
		}

		if (isPaid.HasValue)
		{
			if (isPaid.Value)
				sbSql.Where("t.PaidDateTime IS NOT NULL");
			else
				sbSql.Where("t.PaidDateTime IS NULL");
		}

		if (!string.IsNullOrEmpty(paymentRefNo))
		{
			sbSql.Where("t.PaymentRefNo LIKE '%'+@PaymentRefNo+'%'");
			param.Add("@PaymentRefNo", paymentRefNo, DbType.AnsiString);
		}
		#endregion

		sbSql.LeftJoin($"{CustPurchaseInvoice.MsSqlTable} cpi ON cpi.Id=t.CustomerPurhcaseInvoiceId");

		string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;

		using var cn = ConnectionFactory.GetDbConnection()!;

		decimal recordCount = await cn.ExecuteScalarAsync<int>(sql, param);
		int pageCount = (int)Math.Ceiling(recordCount / pgSize);

		DataPagination pagination = new()
		{
			ObjectType = typeof(CustPurchaseInvItem).Name,
			PageSize = pgSize,
			PageCount = pageCount,
			RecordCount = (int)recordCount
		};

		return pagination;
	}
}