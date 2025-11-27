using DataLayer.Models.Pru.IT;
using static Dapper.SqlMapper;

namespace DataLayer.Repos.Pru.IT;

public interface IITAssetAuditTrailRepos : IBaseRepos<ITAssetAuditTrail>
{
	Task<List<ITAssetAuditTrail>> GetAssetAsync(int assetId);
}

public class ITAssetAuditTrailRepos(IDbContext dbContext) : BaseRepos<ITAssetAuditTrail>(dbContext, ITAssetAuditTrail.DatabaseObject), IITAssetAuditTrailRepos
{
	public async Task<List<ITAssetAuditTrail>> GetAssetAsync(int assetId)
	{
		SqlBuilder sbSql = new();
		DynamicParameters param = new();

		sbSql.Where("t.IsDeleted=0");
		sbSql.Where("t.AssetId=@AssetId");
		param.Add("@AssetId", assetId);
		sbSql.OrderBy("t.RequestDate");

		using var cn = DbContext.DbCxn;
		string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/").RawSql;

		var dataList = (await cn.QueryAsync<ITAssetAuditTrail>(sql, param)).AsList();
		return dataList;
	}
}