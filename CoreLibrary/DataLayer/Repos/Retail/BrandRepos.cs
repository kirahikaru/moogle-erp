using DataLayer.Models.Retail;

namespace DataLayer.Repos.Retail;

public interface IBrandRepos : IBaseRepos<Brand>
{
	Task<IEnumerable<string>> GetAllBrandNamesAsync();
}

public class BrandRepos(IConnectionFactory connectionFactory) : BaseRepos<Brand>(connectionFactory, Brand.DatabaseObject), IBrandRepos
{
	public async Task<IEnumerable<string>> GetAllBrandNamesAsync()
	{
		//DynamicParameters param = new();
		SqlBuilder sbSql = new();
		sbSql.Where("t.IsDeleted=0");
		sbSql.Select("t.ObjectName");
		sbSql.OrderBy("t.ObjectName");

		string sql = sbSql.AddTemplate($"SELECT /**select**/ FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/").RawSql;

		using var cn = ConnectionFactory.GetDbConnection()!;

		var dataList = await cn.QueryAsync<string>(sql);

		return dataList;
	}
}