using DataLayer.Models.RMS;

namespace DataLayer.Repos.RMS;

public interface IBrandRepos : IBaseRepos<Brand>
{
	Task<IEnumerable<string>> GetAllBrandNamesAsync();
}

public class BrandRepos(IDbContext dbContext) : BaseRepos<Brand>(dbContext, Brand.DatabaseObject), IBrandRepos
{
	public async Task<IEnumerable<string>> GetAllBrandNamesAsync()
	{
		//DynamicParameters param = new();
		SqlBuilder sbSql = new();
		sbSql.Where("t.IsDeleted=0");
		sbSql.Select("t.ObjectName");
		sbSql.OrderBy("t.ObjectName");

		string sql = sbSql.AddTemplate($"SELECT /**select**/ FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/").RawSql;

		using var cn = DbContext.DbCxn;

		var dataList = await cn.QueryAsync<string>(sql);

		return dataList;
	}
}