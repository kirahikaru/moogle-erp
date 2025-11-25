namespace DataLayer.Repos.SystemCore;

public interface IWorkflowConfigRepos : IBaseRepos<WorkflowConfig>
{
	Task<WorkflowConfig?> GetConfigAsync(string objectClassName, DateTime busnDate);

	Task<List<WorkflowApprovalHierarchy>> GetApprovalHierarchyAsync(int workflowConfigId);

	Task<WorkflowApprovalHistory?> GetApprovalHistoryAsync(int linkedObjectId, int linkedObjectType);
}


public class WorkflowConfigRepos(IConnectionFactory connectionFactory) : BaseRepos<WorkflowConfig>(connectionFactory, WorkflowConfig.DatabaseObject), IWorkflowConfigRepos
{
	public async Task<WorkflowConfig?> GetConfigAsync(string objectClassName, DateTime busnDate)
    {
        SqlBuilder sbSql = new();
        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.ObjectName=@ObjectClassName");
        sbSql.Where("t.StartDate>=@BusnDate");
        sbSql.Where("(t.EndDate IS NULL OR t.EndDate>@BusnDate)");

        DynamicParameters param = new();
        param.Add("@ObjectClassName", objectClassName, DbType.AnsiString);
        param.Add("@BusnDate", busnDate);

        string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**where**/").RawSql;

        using var cn = ConnectionFactory.GetDbConnection()!;
        var data = await cn.QueryFirstOrDefaultAsync<WorkflowConfig?>(sql, param);

        return data;
    }

    public async Task<List<WorkflowApprovalHierarchy>> GetApprovalHierarchyAsync(int workflowConfigId)
    {
        SqlBuilder sbSql = new();
        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.WorkflowConfigId=@WorkflowConfigId");

        DynamicParameters param = new();
        param.Add("@WorkflowConfigId", workflowConfigId);

        string sql = sbSql.AddTemplate($"SELECT * FROM {WorkflowApprovalHierarchy.MsSqlTable} t /**where**/").RawSql;

        using var cn = ConnectionFactory.GetDbConnection()!;
        var dataList = (await cn.QueryAsync<WorkflowApprovalHierarchy>(sql, param)).AsList();

        return dataList;
    }

    public async Task<WorkflowApprovalHistory?> GetApprovalHistoryAsync(int linkedObjectId, int linkedObjectType)
    {
        SqlBuilder sbSql = new();
        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.LinkedObjectId=@LinkedObjectId");
        sbSql.Where("t.LinkedObjectType=@LinkedObjectType");

        DynamicParameters param = new();
        param.Add("@LinkedObjectId", linkedObjectId);
        param.Add("@LinkedObjectType", linkedObjectType, DbType.AnsiString);

        string sql = sbSql.AddTemplate($"SELECT * FROM {WorkflowApprovalHistory.MsSqlTable} t /**where**/").RawSql;

        using var cn = ConnectionFactory.GetDbConnection()!;
        var data = await cn.QueryFirstOrDefaultAsync<WorkflowApprovalHistory?>(sql, param);

        return data;
    }
}