using DataLayer.Models.SystemCore.NonPersistent;

namespace DataLayer.Repos.SystemCore;

public interface IBusinessSectorRepos : IBaseRepos<BusinessSector>
{
	Task<List<DropDownListItem>> GetValidParentAsync(int objectId, string objectCode, int? includingId = null);
	Task<List<BusinessSector>> GetByParentAsync(int id);
	Task<List<BusinessSector>> GetAllChildrenAsync(string objectCode);
}

public class BusinessSectorRepos(IConnectionFactory connectionFactory) : BaseRepos<BusinessSector>(connectionFactory, BusinessSector.DatabaseObject), IBusinessSectorRepos
{
	public async Task<List<BusinessSector>> GetByParentAsync(int id)
    {
        DynamicParameters param = new();
        SqlBuilder sbSql = new();

        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.ParentId=@ParentId");
        sbSql.LeftJoin($"{DbObject.MsSqlTable} p ON p.IsDeleted=0 AND p.Id=t.ParentId");

        string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;

        using var cn = ConnectionFactory.GetDbConnection()!;

        List<BusinessSector> dataList = (await cn.QueryAsync<BusinessSector, BusinessSector, BusinessSector>(sql,
                                                (obj, p) =>
                                                {
                                                    obj.Parent = p;
                                                    return obj;
                                                }, new { ParentId = id }, splitOn: "Id")).ToList();

        return dataList;
    }

    public async Task<List<BusinessSector>> GetAllChildrenAsync(string objectCode)
    {
        DynamicParameters param = new();
        SqlBuilder sbSql = new();

        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.HierarchyPath=@ObjectCode+'%'");
        sbSql.LeftJoin($"{DbObject.MsSqlTable} p ON p.IsDeleted=0 AND p.Id=t.ParentId");

        string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;

        param.Add("@ObjectCode", objectCode, DbType.AnsiString);

        using var cn = ConnectionFactory.GetDbConnection()!;

        var dataList = (await cn.QueryAsync<BusinessSector, BusinessSector, BusinessSector>(sql,
                                                (obj, p) =>
                                                {
                                                    obj.Parent = p;
                                                    return obj;
                                                }, param, splitOn: "Id")).ToList();

        return dataList;
    }

    public async Task<List<DropDownListItem>> GetValidParentAsync(int objectId, string objectCode, int? includingId = null)
    {
        SqlBuilder sbSql = new();
        DynamicParameters param = new();
        param.Add("@Id", objectId);
        param.Add("@ObjectCode", objectCode, DbType.AnsiString);

        sbSql.Select("'ObjectId'=t.Id");
        sbSql.Select("t.ObjectCode");
        sbSql.Select("t.ObjectName");
        sbSql.Select("'ObjectNameEn'=t.ObjectName");
        sbSql.Select("'ObjectNameKh'=t.ObjectNameKh");
        sbSql.Select("t.HierarchyPath");

        if (includingId.HasValue)
        {
            sbSql.Where("(t.IsDeleted=0 AND t.Id<>@Id AND t.HierarchyPath NOT LIKE '%'+@ObjectCode+'%') OR t.Id=@IncludingId");
            param.Add("@IncludingId", includingId.Value);
        }
        else
        {
            sbSql.Where("t.IsDeleted=0 AND t.Id<>@Id AND t.HierarchyPath NOT LIKE '%'+@ObjectCode+'%'");
        }

        sbSql.OrderBy("t.ObjectName ASC");

        using var cn = ConnectionFactory.GetDbConnection()!;
        string sql = sbSql.AddTemplate($"SELECT /**select**/ FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/").RawSql;
        var dataList = (await cn.QueryAsync<DropDownListItem>(sql, param)).AsList();

        return dataList;
    }
}