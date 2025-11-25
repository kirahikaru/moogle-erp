using DataLayer.Models.FIN;
using DataLayer.Models.RMS;

namespace DataLayer.Repos.RMS;

public interface ICustomerPurchaseInvoicePaymentRepos : IBaseRepos<CustPurchaseInvPayment>
{
	Task<CustPurchaseInvPayment?> GetFullAsync(int id);

	Task<List<CustPurchaseInvPayment>> GetByInvoiceAsync(int customerPurchaseInvoiceId);
}

public class CustPurchaseInvPaymentRepos(IDbContext dbContext) : BaseRepos<CustPurchaseInvPayment>(dbContext, CustPurchaseInvPayment.DatabaseObject), ICustomerPurchaseInvoicePaymentRepos
{
	public async Task<CustPurchaseInvPayment?> GetFullAsync(int id)
    {
        SqlBuilder sbSql = new();
        sbSql.LeftJoin($"{CustPurchaseInvoice.MsSqlTable} cpi ON cpi.Id=t.CustomerPurchaseInvoiceId");
        sbSql.LeftJoin($"{Bank.MsSqlTable} b ON b.Id=t.BankId");

        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.Id=@Id");

        string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;

        using var cn = DbContext.DbCxn;

        var dataList = (await cn.QueryAsync<CustPurchaseInvPayment, CustPurchaseInvoice, Bank, CustPurchaseInvPayment>(sql, 
                                                        (obj, invoice, bank) =>
                                                        {
                                                            obj.PurchaseInvoice = invoice;
                                                            obj.Bank = bank;

                                                            return obj;
                                                        }, new { Id=id }, splitOn: "Id")).AsList();

        if (dataList.Any())
            return dataList[0];
        else
            return null;
    }

    public async Task<List<CustPurchaseInvPayment>> GetByInvoiceAsync(int customerPurchaseInvoiceId)
    {
        SqlBuilder sbSql = new();
        sbSql.LeftJoin($"{CustPurchaseInvoice.MsSqlTable} cpi ON cpi.Id=t.CustomerPurchaseInvoiceId");
        sbSql.LeftJoin($"{Bank.MsSqlTable} b ON b.Id=t.BankId");

        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.CustomerPurchaseInvoiceId=@CustomerPurchaseInvoiceId");

        string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;

        using var cn = DbContext.DbCxn;

        var dataList = (await cn.QueryAsync<CustPurchaseInvPayment, CustPurchaseInvoice, Bank, CustPurchaseInvPayment>(sql,
                                                        (obj, invoice, bank) =>
                                                        {
                                                            obj.PurchaseInvoice = invoice;
                                                            obj.Bank = bank;

                                                            return obj;
                                                        }, new { CustomerPurchaseInvoiceId = customerPurchaseInvoiceId }, splitOn: "Id")).AsList();

        return dataList;
    }
}