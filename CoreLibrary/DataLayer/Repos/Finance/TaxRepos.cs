using DataLayer.Models.Finance;

namespace DataLayer.Repos.Finance;

public interface ITaxRepos : IBaseRepos<Tax>
{
	Task<Tax?> GetFullAsync(int id, DateTime effectiveDate, bool isForeigner);
	Task<Tax?> GetFullAsync(string objectCode, DateTime effectiveDate, bool isForeigner);
}

public class TaxRepos(IConnectionFactory connectionFactory) : BaseRepos<Tax>(connectionFactory, Tax.DatabaseObject), ITaxRepos
{
	public async Task<Tax?> GetFullAsync(int id, DateTime effectiveDate, bool isForeigner)
    {
        string sql = $"SELECT * FROM {DbObject.MsSqlTable} t WHERE t.IsDeleted=0 AND t.Id=@Id";

        SqlBuilder sbSqlTaxRate = new();

        sbSqlTaxRate.Where("tr.IsDeleted=0");
        sbSqlTaxRate.Where("tr.TaxId=@Id");
        sbSqlTaxRate.Where("tr.IsForForeigner=@IsForeigner");
        sbSqlTaxRate.Where("(tr.StartDate IS NULL OR tr.StartDate<=@EffectiveDate)");
        sbSqlTaxRate.Where("(tr.EndDate IS NULL OR tr.EndDate>=@EffectiveDate)");
        sbSqlTaxRate.OrderBy("tr.StartDate DESC");
        sbSqlTaxRate.OrderBy("tr.MinApplicableAmount DESC");

        string taxRateQry = sbSqlTaxRate.AddTemplate($"SELECT * FROM {TaxRate.MsSqlTable} tr /**where**/ /**orderby**/").RawSql;

        DynamicParameters taxRateQryParam = new();
        taxRateQryParam.Add("@IsForeigner", isForeigner);
        taxRateQryParam.Add("@EffectiveDate", effectiveDate);

        using var cn = ConnectionFactory.GetDbConnection()!;

        Tax? obj = await cn.QuerySingleOrDefaultAsync<Tax?>(sql, new { Id=id });

        if (obj != null)
        {
            List<TaxRate> taxRates = (await cn.QueryAsync<TaxRate>(taxRateQry, taxRateQryParam)).AsList();
            obj.Rates = taxRates;
        }

        return obj;
    }

    public async Task<Tax?> GetFullAsync(string objectCode, DateTime effectiveDate, bool isForeigner)
    {
        string sql = $"SELECT * FROM {DbObject.MsSqlTable} t WHERE t.IsDeleted=0 AND t.ObjectCode=@ObjectCode";

        SqlBuilder sbSqlTaxRate = new();

        using var cn = ConnectionFactory.GetDbConnection()!;

        Tax? obj = await cn.QuerySingleOrDefaultAsync<Tax?>(sql, new { ObjectCode=objectCode });

        if (obj != null)
        {
            sbSqlTaxRate.Where("tr.IsDeleted=0");
            sbSqlTaxRate.Where("tr.TaxId=@Id");
            sbSqlTaxRate.Where("tr.IsForForeigner=@IsForeigner");
            sbSqlTaxRate.Where("(tr.StartDate IS NULL OR tr.StartDate<=@EffectiveDate)");
            sbSqlTaxRate.Where("(tr.EndDate IS NULL OR tr.EndDate>=@EffectiveDate)");
            sbSqlTaxRate.OrderBy("tr.StartDate DESC");
            sbSqlTaxRate.OrderBy("tr.MinApplicableAmount DESC");

            string taxRateQry = sbSqlTaxRate.AddTemplate($"SELECT * FROM {TaxRate.MsSqlTable} tr /**where**/ /**orderby**/").RawSql;


            DynamicParameters param = new();
            param.Add("@ObjectCode", objectCode, DbType.AnsiString);

            DynamicParameters taxRateQryParam = new();
            taxRateQryParam.Add("@Id", obj.Id);
            taxRateQryParam.Add("@IsForeigner", isForeigner);
            taxRateQryParam.Add("@EffectiveDate", effectiveDate);

            List<TaxRate> taxRates = (await cn.QueryAsync<TaxRate>(taxRateQry, taxRateQryParam)).AsList();
            obj.Rates = taxRates;
        }

        return obj;
    }
}