namespace DataLayer.Repos.SysCore;

public interface IWorkflowHistoryRepos : IBaseRepos<WorkflowHistory>
{
	Task<List<WorkflowHistory>> GetByLinkedObjectAsync(int linkedObjectId, string linkedObjectType);

	Task<int> GetCountByActionAsync(int linkedObjectId, string linkedObjectType, string action);
	Task<List<WorkflowHistory>> GetByActionAsync(int linkedObjectId, string linkedObjectType, string action);
}


public class WorkflowHistoryRepos(IDbContext dbContext) : BaseRepos<WorkflowHistory>(dbContext, WorkflowHistory.DatabaseObject), IWorkflowHistoryRepos
{
	public async Task<List<WorkflowHistory>> GetByLinkedObjectAsync(int linkedObjectId, string linkedObjectType)
    {
        SqlBuilder sbSql = new();

        sbSql.Where("wf.IsDeleted=0");
        sbSql.Where("wf.LinkedObjectId=@LinkedObjectId");
        sbSql.Where("wf.LinkedObjectType=@LinkedObjectType");

        sbSql.LeftJoin($"{OrgStruct.MsSqlTable} os ON os.IsDeleted=0 AND os.Id=wf.LinkedObjectId");
        sbSql.LeftJoin($"{User.MsSqlTable} u ON u.IsDeleted=0 AND u.Id=wf.UserId");

        var sql = sbSql.AddTemplate($"SELECT * FROM {WorkflowHistory.MsSqlTable} wf /**leftjoin**/ /**where**/").RawSql;

        DynamicParameters param = new();

        param.Add("@LinkedObjectId", linkedObjectId);
        param.Add("@LinkedObjectType", linkedObjectType, DbType.AnsiString);

        using var cn = DbContext.DbCxn;

        var dataList = (await cn.QueryAsync<WorkflowHistory, OrgStruct, User, WorkflowHistory>(
                    sql, (workflow, orgStruct, user) =>
                    {
                        if (orgStruct != null)
                            workflow.OrgStruct = orgStruct;

                        if (user != null)
                            workflow.User = user;

                        return workflow;
                    }, param, splitOn: "Id").ConfigureAwait(false)).ToList();

        return dataList;
    }

    public async Task<int> GetCountByActionAsync(int linkedObjectId, string linkedObjectType, string action)
    {
        SqlBuilder sbSql = new();
        DynamicParameters param = new();
        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.LinkedObjectId=@LinkedObjectId");
        sbSql.Where("t.LinkedObjectType=@LinkedObjectType");
        sbSql.Where("ISNULL(t.Action,'')=@Action");

        param.Add("@LinkedObjectId", linkedObjectId);
        param.Add("@LinkedObjectType", linkedObjectType, DbType.AnsiString);
        param.Add("@Action", action, DbType.AnsiString);

        string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;

        using var cn = DbContext.DbCxn;

        int count = await cn.ExecuteScalarAsync<int>(sql, param);
        return count;
    }
    public async Task<List<WorkflowHistory>> GetByActionAsync(int linkedObjectId, string linkedObjectType, string action)
    {
        SqlBuilder sbSql = new();
        DynamicParameters param = new();
        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.LinkedObjectId=@LinkedObjectId");
        sbSql.Where("t.LinkedObjectType=@LinkedObjectType");
        sbSql.Where("ISNULL(t.Action,'')=@Action");

        param.Add("@LinkedObjectId", linkedObjectId);
        param.Add("@LinkedObjectType", linkedObjectType, DbType.AnsiString);
        param.Add("@Action", action, DbType.AnsiString);

        string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**where**/").RawSql;

        using var cn = DbContext.DbCxn;

        var dataList = (await cn.QueryAsync<WorkflowHistory>(sql, param)).AsList();
        return dataList;
    }
}