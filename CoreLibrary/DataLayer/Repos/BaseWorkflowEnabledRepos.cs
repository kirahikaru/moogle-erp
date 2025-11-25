using DataLayer.GlobalConstant;
using DataLayer.Models.SysCore.NonPersistent;
using System.Reflection;

namespace DataLayer.Repos;

public interface IBaseWorkflowEnabledRepos<TEntity> : IBaseRepos<TEntity> where TEntity : AuditObject
{
	Task<int> SaveAndTransitWorkflowAsync(TEntity entity, User fromUser, User toUser, string workflowAction, string workflowRemark = "");
	Task<int> TransitWorkflowAsync(TEntity entity, User fromUser, User toUser, string workflowAction, string workflowRemark = "");
}

public class BaseWorkflowEnabledRepos<TEntity>(IDbContext dbContext, DatabaseObj dbObj) : BaseRepos<TEntity>(dbContext, dbObj), IBaseWorkflowEnabledRepos<TEntity> where TEntity : AuditObject
{
    public virtual async Task<int> SaveAndTransitWorkflowAsync(TEntity entity, User fromUser, User toUser, string workflowAction, string workflowRemark = "")
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity), _errMsgResxMngr.GetString("Null", CultureInfo.CurrentUICulture));
        else if (entity is not WorkflowEnabledObject || !(typeof(WorkflowEnabledObject).IsAssignableFrom(entity.GetType())))
            throw new ArgumentException(_errMsgResxMngr.GetString("InvalidParameterNotWorkflowObject", CultureInfo.CurrentUICulture));

        if (fromUser == null || fromUser.Id <= 0)
            throw new ArgumentNullException(nameof(fromUser), _errMsgResxMngr.GetString("Null", CultureInfo.CurrentUICulture));

        string endStatus = WorkflowController.GetResultingWorkflowStatus(workflowAction);
        int objId = (entity as WorkflowEnabledObject)!.Id;
        string currentStatus = (entity as WorkflowEnabledObject)!.WorkflowStatus;

        if (string.IsNullOrEmpty(currentStatus))
        {
            string errMsg = _errMsgResxMngr.GetString("InvalidWorkflowAction", CultureInfo.CurrentUICulture)!;
            throw new Exception(string.Format(CultureInfo.CurrentCulture, errMsg, workflowAction, entity.GetType().Name, currentStatus));
        }

        DateTime khTimeNow = DateTime.UtcNow.AddHours(7);

        WorkflowHistory wh = new()
        {
            LinkedObjectId = (objId > 0 ? objId : (int?)null),
            LinkedObjectType = entity.GetType().Name,
            UserId = fromUser.Id,
            StartStatus = currentStatus,
            EndStatus = endStatus,
            OrgStructId = fromUser.OrgStructId,
            Action = workflowAction,
            CreatedUser = fromUser.UserName,
            CreatedDateTime = khTimeNow,
            ModifiedUser = fromUser.UserName,
            ModifiedDateTime = khTimeNow,
            Remark = workflowRemark
        };

        (entity as WorkflowEnabledObject)!.WorkflowStatus = endStatus;
        (entity as WorkflowEnabledObject)!.ModifiedUser = fromUser.UserName;
        (entity as WorkflowEnabledObject)!.ModifiedDateTime = khTimeNow;

        if (workflowAction.Is(WorkflowActions.ASSIGN, WorkflowActions.TRANSFER, WorkflowActions.SELF_PICKUP))
        {
            PropertyInfo? propAssignedDateTime = entity.GetType().GetProperty("AssignedDateTime", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            PropertyInfo? propAssignedUserId = entity.GetType().GetProperty("AssignedUserId", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

            propAssignedDateTime?.SetValue(entity, khTimeNow);

            propAssignedUserId?.SetValue(entity, toUser.Id);
        }

        //var insWfCmd = QueryGenerator.GenerateInsertQuery(typeof(WorkflowHistory), WorkflowHistory.DatabaseObject);

        using var cn = DbContext.DbCxn;

        // <!IMPORTANT> Connection required to be open before calling BeginTransaction() function
        if (cn.State != ConnectionState.Open) cn.Open();

        using var tran = cn.BeginTransaction();
        try
        {
            if (objId <= 0)   // create lead
            {
                (entity as WorkflowEnabledObject)!.CreatedUser = fromUser.UserName;
                (entity as WorkflowEnabledObject)!.CreatedDateTime = khTimeNow;

                //var insCmd = QueryGenerator.GenerateInsertQuery(entity.GetType(), this.DatabaseObject);
                //objId = await cn.QuerySingleAsync<int>(insCmd, entity, transaction: tran);
                objId = await cn.InsertAsync(entity, tran);

                if (objId > 0)
                {
                    //Claim running number(if any has been locked)
                    var sql = QueryGenerator.GenerateClaimRunningNumberQuery(entity.GetType());

                    if (!string.IsNullOrEmpty(sql))
                    {
                        string? objectCode = (string)entity.GetType()!.GetProperty("ObjectCode")!.GetValue(entity)!;

                        var param = new { LinkedObjectId = objId, ModifiedUser = fromUser.UserName, ModifiedDateTime = khTimeNow, ObjectCode = objectCode, fromUser.UserId };
                        int claimCount = await cn.ExecuteAsync(sql, param, tran);
                    }

                    wh.LinkedObjectId = objId;
                    //await cn.QuerySingleAsync<int>(insWfCmd, wh, transaction: tran);
                    await cn.InsertAsync(wh, tran);
                }
                else
                    throw new Exception(_errMsgResxMngr.GetString("WorkflowUpdateFailed", CultureInfo.CurrentUICulture));
            }
            else // update lead + workflow transaction
            {
                //int updCount = 0;
                //var updCmd = QueryGenerator.GenerateUpdateQuery(entity.GetType(), this.DatabaseObject);
                //updCount = await cn.ExecuteAsync(updCmd, entity, transaction: tran);

                bool isUpdated = await cn.UpdateAsync(entity, tran);

                //insert workflow history
                //await cn.QuerySingleAsync<int>(insWfCmd, wh, transaction: tran);
                if (isUpdated)
                    await cn.InsertAsync(wh, tran);

                else
                    throw new Exception(_errMsgResxMngr.GetString("WorkflowUpdateFailed", CultureInfo.CurrentUICulture));
            }

            tran.Commit();
            return objId;
        }
        catch
        {
            tran.Rollback();
            throw;
        }
    }

    public async Task<int> TransitWorkflowAsync(TEntity entity, User fromUser, User toUser, string workflowAction, string workflowRemark = "")
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));
        ArgumentNullException.ThrowIfNull(fromUser, nameof(fromUser));
        
        if (entity is not WorkflowEnabledObject || !(typeof(WorkflowEnabledObject).IsAssignableFrom(entity.GetType())))
            throw new ArgumentException(_errMsgResxMngr.GetString("InvalidParameterNotWorkflowObject", CultureInfo.CurrentUICulture));
        else if ((entity as WorkflowEnabledObject)!.Id <= 0)
            throw new ArgumentNullException(nameof(entity), _errMsgResxMngr.GetString("Null", CultureInfo.CurrentUICulture));

        if (fromUser.Id <= 0)
            throw new ArgumentNullException(nameof(fromUser), _errMsgResxMngr.GetString("Null", CultureInfo.CurrentUICulture));

        string endStatus = WorkflowController.GetResultingWorkflowStatus(workflowAction);
        int objId = (entity as WorkflowEnabledObject)!.Id;
        string currentStatus = (entity as WorkflowEnabledObject)!.WorkflowStatus;

        if (string.IsNullOrEmpty(endStatus))
        {
            string? errMsg = _errMsgResxMngr.GetString("InvalidWorkflowAction", CultureInfo.CurrentUICulture);
            throw new Exception(string.Format(CultureInfo.CurrentCulture, errMsg!, workflowAction, entity.GetType().Name, currentStatus));
        }

        DateTime khTimeNow = DateTime.UtcNow.AddHours(7);

        WorkflowHistory wh = new()
        {
            LinkedObjectId = (objId > 0 ? objId : (int?)null),
            LinkedObjectType = entity.GetType().Name,
            UserId = fromUser.Id,
            StartStatus = currentStatus,
            EndStatus = endStatus,
            OrgStructId = fromUser.OrgStructId,
            Action = workflowAction,
            CreatedUser = fromUser.UserName,
            CreatedDateTime = khTimeNow,
            ModifiedUser = fromUser.UserName,
            ModifiedDateTime = khTimeNow,
            Remark = workflowRemark
        };

        //(entity as WorkflowEnabledObject).WorkflowStatus = endStatus;
        //(entity as WorkflowEnabledObject).ModifiedUser = fromUser.UserName;
        //(entity as WorkflowEnabledObject).ModifiedDateTime = khTimeNow;

        var updCmd = $"UPDATE {DbObject.MsSqlTable} SET ";

        DynamicParameters param = new();

        if (workflowAction.Is(WorkflowActions.ASSIGN, WorkflowActions.TRANSFER, WorkflowActions.SELF_PICKUP, WorkflowActions.FORWARD, WorkflowActions.RE_OPEN))
        {
            PropertyInfo? propAssignedDateTime = entity.GetType().GetProperty("AssignedDateTime", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            PropertyInfo? propAssignedUserId = entity.GetType().GetProperty("AssignedUserId", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

            if (propAssignedDateTime != null)
            {
                //propAssignedDateTime.SetValue(entity, khTimeNow);
                updCmd += "AssignedDateTime=@AssignedDateTime, ";
                param.Add("@AssignedDateTime", khTimeNow);
            }

            if (propAssignedUserId != null)
            {
                //propAssignedUserId.SetValue(entity, user.UserId);
                updCmd += "AssignedUserId=@AssignedUserId, ";
                param.Add("@AssignedUserId", toUser.Id);
            }
        }

        updCmd += @"WorkflowStatus=@WorkflowStatus,
                        ModifiedUser=@ModifiedUser,
                        ModifiedDateTime=@ModifiedDateTime
                        WHERE Id=@Id";

        param.Add("@Id", objId);
        param.Add("@WorkflowStatus", endStatus);
        param.Add("@ModifiedUser", fromUser.UserName!);
        param.Add("@ModifiedDateTime", khTimeNow);

        //var updParam = new { Id = objId, WorkflowStatus = endStatus, ModifiedUser = user.UserName, ModifiedDateTime = khTimeNow };

        //var insWfCmd = QueryGenerator.GenerateInsertQuery(typeof(WorkflowHistory), "[dbo].[WorkflowHistory]");

        using var cn = DbContext.DbCxn;

        // <!IMPORTANT> Connection required to be open before calling BeginTransaction() function
        if (cn.State != ConnectionState.Open) cn.Open();

        using var tran = cn.BeginTransaction();
        try
        {
            int result = cn.Execute(updCmd, param, tran);

            //insert workflow history
            if (result > 0)
            {
                //await cn.QuerySingleAsync<int>(insWfCmd, wh, transaction: tran);
                await cn.InsertAsync(wh, tran);
            }
            else
                throw new Exception(_errMsgResxMngr.GetString("WorkflowUpdateFailed", CultureInfo.CurrentUICulture));

            tran.Commit();
            return result;
        }
        catch
        {
            tran.Rollback();
            throw;
        }
    }
}