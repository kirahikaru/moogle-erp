using DataLayer.Models.HMS;

namespace DataLayer.Repos.HMS;

public interface IMedicalExamRepos : IBaseWorkflowEnabledRepos<MedicalExam>
{
	Task<List<MedicalExam>> SearchAsync(
		int pgSize = 0, int pgNo = 0,
		string? objectCode = null,
		string? objectName = null,
		List<int>? requestorUserIdList = null,
		List<int>? customerIdList = null,
		List<int>? doctorIdList = null,
		DateTime? requestDateFrom = null,
		DateTime? requestDateTo = null,
		string? customerIdCode = null,
		List<int>? assignedUserIdList = null,
		List<string>? workflowStatusList = null);

	Task<DataPagination> GetSearchPaginationAsync(
		int pgSize = 0,
		string? objectCode = null,
		string? objectName = null,
		List<int>? requestorUserIdList = null,
		List<int>? customerIdList = null,
		List<int>? doctorIdList = null,
		DateTime? requestDateFrom = null,
		DateTime? requestDateTo = null,
		string? customerIdCode = null,
		List<int>? assignedUserIdList = null,
		List<string>? workflowStatusList = null);
}

public class MedicalExamRepos(IDbContext dbContext) : BaseWorkflowEnabledRepos<MedicalExam>(dbContext, MedicalExam.DatabaseObject), IMedicalExamRepos
{
	public async Task<DataPagination> GetSearchPaginationAsync(
        int pgSize = 0, 
        string? objectCode = null, 
        string? objectName = null, 
        List<int>? requestorUserIdList = null, 
        List<int>? customerIdList = null, 
        List<int>? doctorIdList = null, 
        DateTime? requestDateFrom = null, 
        DateTime? requestDateTo = null, 
        string? customerIdCode = null, 
        List<int>? assignedUserIdList = null, 
        List<string>? workflowStatusList = null)
    {
        DynamicParameters param = new();
        SqlBuilder sbSql = new();

        sbSql.Where("t.IsDeleted=0");

        #region Form Search Conditions
        if (!string.IsNullOrEmpty(objectCode))
        {
            sbSql.Where("UPPER(t.ObjectCode) LIKE '%'+UPPER(@ObjectCode)+'%'");
            param.Add("@ObjectCode", objectCode, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(objectName))
        {
            sbSql.Where("LOWER(t.ObjectName) LIKE '%'+LOWER(@ObjectName)+'%'");
            param.Add("@ObjectName", objectName, DbType.AnsiString);
        }

        if (requestorUserIdList != null && requestorUserIdList.Any())
        {
            if (requestorUserIdList.Count == 1)
            {
                sbSql.Where("t.RequestorUserId=@RequestorUserId");
                param.Add("@RequestorUserId", requestorUserIdList[0]);
            }
            else
            {
                sbSql.Where("t.RequestorUserId IN @RequestorUserIdList");
                param.Add("@RequestorUserIdList", requestorUserIdList);
            }
        }

        if (customerIdList != null && customerIdList.Any())
        {
            if (customerIdList.Count == 1)
            {
                sbSql.Where("t.CustomerId=@CustomerId");
                param.Add("@CustomerId", customerIdList[0]);
            }
            else
            {
                sbSql.Where("t.CustomerId IN @CustomerIdList");
                param.Add("@CustomerIdList", customerIdList);
            }
        }

        if (doctorIdList != null && doctorIdList.Any())
        {
            if (doctorIdList.Count == 1)
            {
                sbSql.Where("t.DoctorId=@DoctorId");
                param.Add("@DoctorId", doctorIdList[0]);
            }
            else
            {
                sbSql.Where("t.DoctorId IN @DoctorIdList");
                param.Add("@DoctorIdList", doctorIdList);
            }
        }

        if (assignedUserIdList != null && assignedUserIdList.Any())
        {
            if (assignedUserIdList.Count == 1)
            {
                sbSql.Where("t.AssignedUserId=@AssignedUserId");
                param.Add("@AssignedUserId", assignedUserIdList[0]);
            }
            else
            {
                sbSql.Where("t.AssignedUserId IN @AssignedUserIdList");
                param.Add("@AssignedUserIdList", assignedUserIdList);
            }
        }

        if (workflowStatusList != null && workflowStatusList.Any())
        {
            if (workflowStatusList.Count == 1)
            {
                sbSql.Where("t.WorkflowStatus=@WorkflowStatus");
                param.Add("@WorkflowStatus", workflowStatusList[0]);
            }
            else
            {
                sbSql.Where("t.WorkflowStatus IN @WorkflowStatusList");
                param.Add("@WorkflowStatusList", workflowStatusList);
            }
        }

        if (requestDateFrom != null)
        {
            sbSql.Where("t.RequestDate IS NOT NULL");
            sbSql.Where("t.RequestDate>=@RequestDateFrom");
            param.Add("@RequestDateFrom", requestDateFrom);

            if (requestDateTo != null)
            {
                sbSql.Where("t.RequestDate<=@RequestDateTo");
                param.Add("@RequestDateTo", requestDateTo);
            }
        }
        else if (requestDateTo != null)
        {
            sbSql.Where("t.RequestDate IS NOT NULL");
            sbSql.Where("t.RequestDate<=@RequestDateTo");
            param.Add("@RequestDateTo", requestDateTo);
        }

        if (!string.IsNullOrEmpty(customerIdCode))
        {
            sbSql.Where("UPPER(t.CustomerIDCode) LIKE '%'+UPPER(@CustomerIDCode)+'%'");
            param.Add("@CustomerIDCode", customerIdCode, DbType.AnsiString);
        }
        #endregion

        string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;

        using var cn = DbContext.DbCxn;

        decimal recordCount = await cn.ExecuteScalarAsync<int>(sql, param);
        int pageCount = (int)Math.Ceiling(recordCount / (pgSize == 0 ? 1 : pgSize));

        DataPagination pagination = new()
        {
            ObjectType = typeof(MedicalTest).Name,
            PageSize = pgSize,
            PageCount = pageCount,
            RecordCount = (int)recordCount
        };

        return pagination;
    }

    public async Task<List<MedicalExam>> SearchAsync(
        int pgSize = 0, int pgNo = 0, 
        string? objectCode = null, 
        string? objectName = null,
        List<int>? requestorUserIdList = null,
        List<int>? customerIdList = null,
        List<int>? doctorIdList = null,
        DateTime? requestDateFrom = null,
        DateTime? requestDateTo = null,
        string? customerIdCode = null,
        List<int>? assignedUserIdList = null,
        List<string>? workflowStatusList = null)
    {
        if (pgNo < 0 && pgSize < 0)
            throw new Exception(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        SqlBuilder sbSql = new();
        DynamicParameters param = new();

        sbSql.Where("t.IsDeleted=0");

        #region Form Search Conditions
        if (!string.IsNullOrEmpty(objectCode))
        {
            sbSql.Where("UPPER(t.ObjectCode) LIKE '%'+UPPER(@ObjectCode)+'%'");
            param.Add("@ObjectCode", objectCode, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(objectName))
        {
            sbSql.Where("LOWER(t.ObjectName) LIKE '%'+LOWER(@ObjectName)+'%'");
            param.Add("@ObjectName", objectName, DbType.AnsiString);
        }

        if (requestorUserIdList != null && requestorUserIdList.Any())
        {
            if (requestorUserIdList.Count == 1)
            {
                sbSql.Where("t.RequestorUserId=@RequestorUserId");
                param.Add("@RequestorUserId", requestorUserIdList[0]);
            }
            else
            {
                sbSql.Where("t.RequestorUserId IN @RequestorUserIdList");
                param.Add("@RequestorUserIdList", requestorUserIdList);
            }
        }

        if (customerIdList != null && customerIdList.Any())
        {
            if (customerIdList.Count == 1)
            {
                sbSql.Where("t.CustomerId=@CustomerId");
                param.Add("@CustomerId", customerIdList[0]);
            }
            else
            {
                sbSql.Where("t.CustomerId IN @CustomerIdList");
                param.Add("@CustomerIdList", customerIdList);
            }
        }

        if (doctorIdList != null && doctorIdList.Any())
        {
            if (doctorIdList.Count == 1)
            {
                sbSql.Where("t.DoctorId=@DoctorId");
                param.Add("@DoctorId", doctorIdList[0]);
            }
            else
            {
                sbSql.Where("t.DoctorId IN @DoctorIdList");
                param.Add("@DoctorIdList", doctorIdList);
            }
        }

        if (assignedUserIdList != null && assignedUserIdList.Any())
        {
            if (assignedUserIdList.Count == 1)
            {
                sbSql.Where("t.AssignedUserId=@AssignedUserId");
                param.Add("@AssignedUserId", assignedUserIdList[0]);
            }
            else
            {
                sbSql.Where("t.AssignedUserId IN @AssignedUserIdList");
                param.Add("@AssignedUserIdList", assignedUserIdList);
            }
        }

        if (workflowStatusList != null && workflowStatusList.Any())
        {
            if (workflowStatusList.Count == 1)
            {
                sbSql.Where("t.WorkflowStatus=@WorkflowStatus");
                param.Add("@WorkflowStatus", workflowStatusList[0]);
            }
            else
            {
                sbSql.Where("t.WorkflowStatus IN @WorkflowStatusList");
                param.Add("@WorkflowStatusList", workflowStatusList);
            }
        }

        if (requestDateFrom != null)
        {
            sbSql.Where("t.RequestDate IS NOT NULL");
            sbSql.Where("t.RequestDate>=@RequestDateFrom");
            param.Add("@RequestDateFrom", requestDateFrom);

            if (requestDateTo != null)
            {
                sbSql.Where("t.RequestDate<=@RequestDateTo");
                param.Add("@RequestDateTo", requestDateTo);
            }
        }
        else if (requestDateTo != null)
        {
            sbSql.Where("t.RequestDate IS NOT NULL");
            sbSql.Where("t.RequestDate<=@RequestDateTo");
            param.Add("@RequestDateTo", requestDateTo);
        }

        if (!string.IsNullOrEmpty(customerIdCode))
        {
            sbSql.Where("UPPER(t.CustomerIDCode) LIKE '%'+UPPER(@CustomerIDCode)+'%'");
            param.Add("@CustomerIDCode", customerIdCode, DbType.AnsiString);
        }
        #endregion

        sbSql.LeftJoin($"{User.MsSqlTable} reqUsr ON reqUsr.Id=t.RequestorUserId");
        sbSql.LeftJoin($"{User.MsSqlTable} assUsr ON assUsr.Id=t.AssignedUserId");
        sbSql.LeftJoin($"{Doctor.MsSqlTable} dr ON dr.Id=t.DoctorId");
        sbSql.LeftJoin($"{Customer.MsSqlTable} cust ON cust.Id=t.CustomerId");
        sbSql.OrderBy("t.ObjectName ASC");

        string sql;

        if (pgNo == 0 && pgSize == 0)
        {
            sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/ /**orderby**/").RawSql;
        }
        else
        {
            param.Add("@PageSize", pgSize);
            param.Add("@PageNo", pgNo);

            sql = sbSql.AddTemplate(
                    $";WITH pg AS (SELECT Id FROM {DbObject.MsSqlTable} /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
                    $"SELECT t.*, regUsr.*, assUsr.*, dr.*, cust.* FROM {DbObject.MsSqlTable} t INNER JOIN pg p ON p.Id=t.Id /**leftjoin**/ /**orderby**/").RawSql;
        }

        using var cn = DbContext.DbCxn;

        var dataList = (await cn.QueryAsync<MedicalExam, User, User, Doctor, Customer, MedicalExam>(
                                        sql, (obj, reqUsr, assUsr, dr, cust) =>
                                        {
                                            obj.RequestorUser = reqUsr;
                                            obj.AssignedUser = assUsr;
                                            obj.Doctor = dr;
                                            obj.Customer = cust;

                                            return obj;
                                        }, param, splitOn: "Id")).AsList();

        return dataList;
    }
}