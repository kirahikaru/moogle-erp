using DataLayer.Models.Pru.Finance;
using static Dapper.SqlMapper;

namespace DataLayer.Repos.Pru.Finance;

public interface IQuotationItemRepos : IBaseRepos<QuotationItem>
{
	Task<IEnumerable<QuotationItem>> GetByQuotationAsync(string quotationCode);
}

public class QuotationItemRepos(IDbContext dbContext) : BaseRepos<QuotationItem>(dbContext, QuotationItem.DatabaseObject), IQuotationItemRepos
{

	public async Task<IEnumerable<QuotationItem>> GetByQuotationAsync(string quotationCode)
	{
		string sql = $"SELECT * FROM {DbObject.MsSqlTable} t WHERE t.IsDeleted=0 AND t.QuotationCode=@QuotationCode ORDER BY t.OrderNo";
		DynamicParameters param = new();
		param.Add("@QuotationCode", quotationCode, DbType.AnsiString);
		using var cn = DbContext.DbCxn;
		var dataList = await cn.QueryAsync<QuotationItem>(sql, param);
		return dataList;
	}
}