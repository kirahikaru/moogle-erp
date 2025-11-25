using DataLayer.Models.Retail;
using System.Text.RegularExpressions;

namespace DataLayer.Repos.Retail;

public interface IGs1CompanyPrefixRepos : IBaseRepos<Gs1CompanyPrefix>
{
	Task<Country?> GetCountryByGs1CodeAsync(string gs1Code);
}

public class Gs1CompanyPrefixRepos(IConnectionFactory connectionFactory) : BaseRepos<Gs1CompanyPrefix>(connectionFactory, Gs1CompanyPrefix.DatabaseObject), IGs1CompanyPrefixRepos
{
	public async Task<Country?> GetCountryByGs1CodeAsync(string? gs1Code)
    {
        Regex gs1CodeFormat = new(@"[0-9]{3}");

        if (string.IsNullOrEmpty(gs1Code))
            throw new ArgumentNullException(nameof(gs1Code));
        else if (!gs1CodeFormat.IsMatch(gs1Code!))
            throw new Exception("GS1Code parameter provide invalid format.");

        int gs1CodeInt = int.Parse(gs1Code!);
        var sql = $"SELECT cty.* FROM {DbObject.MsSqlTable} gs1 LEFT JOIN {Country.MsSqlTable} cty ON cty.IsDeleted=0 AND cty.ObjectCode=gs1.CountryCode WHERE gs1.IsDeleted=0 AND StartNumber<=@GS1Code AND EndNumber>=@GS1Code";

        using var cn = ConnectionFactory.GetDbConnection()!;

        Country? data = await cn.QueryFirstOrDefaultAsync<Country>(sql, new { GS1Code = gs1CodeInt });
        return data;
    }
}