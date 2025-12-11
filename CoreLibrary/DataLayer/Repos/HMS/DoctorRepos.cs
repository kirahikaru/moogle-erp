using DataLayer.GlobalConstant;
using DataLayer.Models.HMS;
using System.Xml.Schema;

namespace DataLayer.Repos.HMS;

public interface IDoctorRepos : IBaseRepos<Doctor>
{
	Task<Doctor?> GetFullAsync(int id);

	Task<int> InsertFullAsync(Doctor obj, UserSessionInfo user, string endStatus = "", string? remark = null);

	Task<bool> UpdateFullAsync(Doctor obj, UserSessionInfo user, string endStatus = "", string? remark = null);

	Task<List<Doctor>> SearchAsync(
		int pgSize = 0,
		int pgNo = 0,
		string? objectCode = null,
		string? objectName = null,
		int? healthcareFacilityId = null,
		List<string>? statusList = null);

	Task<DataPagination> GetSearchPaginationAsync(
		int pgSize = 0,
		string? objectCode = null,
		string? objectName = null,
		int? healthcareFacilityId = null,
		List<string>? statusList = null);
}

public class DoctorRepos(IDbContext dbContext) : BaseRepos<Doctor>(dbContext, Doctor.DatabaseObject), IDoctorRepos
{
	public async Task<Doctor?> GetFullAsync(int id)
    {
        SqlBuilder sbSql = new();
        DynamicParameters param = new();

        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.Id=0");

        sbSql.LeftJoin($"{Employee.MsSqlTable} emp ON emp.Id=t.EmployeeId");
        sbSql.LeftJoin($"{Country.MsSqlTable} nty ON nty.IsDeleted=0 AND nty.ObjectCode=t.NationalityCountryCode");

        using var cn = DbContext.DbCxn;

        string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;

        var data = (await cn.QueryAsync<Doctor, Employee, Country, Doctor>(sql, (obj, employee, nty) =>
        {
            employee.Nationality = nty;
            obj.Employee = employee;
            return obj;
        }, param, splitOn: "Id")).FirstOrDefault();

        return data;
    }

    public async Task<int> InsertFullAsync(Doctor obj, UserSessionInfo user, string endStatus = "", string? remark = null)
    {
        using var cn = DbContext.DbCxn;
        // <!IMPORTANT> Connection required to be open before calling BeginTransaction() function
        if (cn.State != ConnectionState.Open) cn.Open();
        using var tran = cn.BeginTransaction();

        try
        {
            if (obj.Status == EmployeeStatuses.ACTIVE)
            {
                obj.ObjectCode = await GetRunningNumberAsync(cn, new RunNumGenParam()
                {
                    UserName = obj.ModifiedUser,
                    ObjectClassName = obj.GetType().Name
                }, tran);

                if (string.IsNullOrEmpty(obj.ObjectCode))
                {
                    throw new Exception("Cannot get Doctor.ObjectCode from Running Number Generator.");
                }
            }

            int objId = await cn.InsertAsync(obj, tran);

            if (objId > 0)
            {
                ObjectStatusAuditTrail statusChgAudit = new()
                {
                    ObjectCode = obj.ObjectCode,
                    ObjectName = obj.GetType().Name,
                    ObjectId = objId,
                    ActionCode = "SAFE-DRAFT",
                    FromStatus = "",
                    ToStatus = obj.Status,
                    Remark = remark,
                    TriggeredUserId = user.UserId,
                    CreatedDateTime = obj.CreatedDateTime,
                    CreatedUser = user.UserName,
                    ModifiedDateTime = obj.ModifiedDateTime,
                    ModifiedUser = obj.ModifiedUser
                };

                int statusChgAuditId = await cn.InsertAsync(statusChgAudit, tran);

                if (statusChgAuditId <= 0)
                    throw new Exception("Failed to inster Customer Change History ('Register').");

                if (endStatus.Length > 0 && endStatus == EmployeeStatuses.ACTIVE)
                {
                    #region EMPLOYEE ACTIVATION
                    string updEmployeeCmd = $"UPDATE {Employee.MsSqlTable} SET [Status]=@Status WHERE Id=@Id";

                    DynamicParameters updCustomerParam = new();
                    updCustomerParam.Add("@Status", endStatus, DbType.AnsiString);
                    updCustomerParam.Add("@Id", objId);

                    int updEmployeeCount = await cn.ExecuteAsync(updEmployeeCmd, updCustomerParam, tran);

                    if (updEmployeeCount > 0)
                    {
                        ObjectStatusAuditTrail activateStatusChgAudit = new()
                        {
                            ObjectCode = obj.ObjectCode,
                            ObjectName = obj.GetType().Name,
                            ObjectId = objId,
                            ActionCode = "ACTIVATE",
                            FromStatus = obj.Status,
                            ToStatus = endStatus,
                            Remark = remark,
                            TriggeredUserId = user.UserId,
                            CreatedDateTime = obj.CreatedDateTime,
                            CreatedUser = user.UserName,
                            ModifiedDateTime = obj.ModifiedDateTime,
                            ModifiedUser = obj.ModifiedUser
                        };

                        int activateStatusChgAuditId = await cn.InsertAsync(activateStatusChgAudit, tran);

                        if (activateStatusChgAuditId <= 0)
                            throw new Exception("Failed to inster Customer Change History ('Activate').");
                    }
                    #endregion
                }
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

    public async Task<bool> UpdateFullAsync(Doctor obj, UserSessionInfo user, string endStatus = "", string? remark = null)
    {
        using var cn = DbContext.DbCxn;
        // <!IMPORTANT> Connection required to be open before calling BeginTransaction() function
        if (cn.State != ConnectionState.Open) cn.Open();
        using var tran = cn.BeginTransaction();

        try
        {
            ObjectStatusAuditTrail? statusChgAudit = null;

            if (EmployeeStatuses.IsValid(endStatus))
            {
                statusChgAudit = new()
                {
                    ObjectCode = obj.ObjectCode,
                    ObjectName = obj.GetType().Name,
                    ObjectId = obj.Id,
                    TriggeredUserId = user.UserId,
                    FromStatus = obj.Status,
                    ToStatus = endStatus,
                    Remark = remark,
                    CreatedDateTime = obj.ModifiedDateTime,
                    CreatedUser = obj.ModifiedUser,
                    ModifiedDateTime = obj.ModifiedDateTime,
                    ModifiedUser = obj.ModifiedUser
                };
            }

            switch (endStatus)
            {
                case EmployeeStatuses.INVALID:
                    statusChgAudit!.ActionCode = "CANCEL";
                    break;
                case EmployeeStatuses.ACTIVE:
                    statusChgAudit!.ActionCode = "ACTIVATE";
                    break;
                case EmployeeStatuses.BLACKLIST:
                    statusChgAudit!.ActionCode = "BLACKLIST";
                    break;
                case EmployeeStatuses.TERMINATED:
                    statusChgAudit!.ActionCode = "TERMINATE";
                    break;
                case EmployeeStatuses.SUSPENDED:
                    statusChgAudit!.ActionCode = "SUSPEND";
                    break;
                case EmployeeStatuses.RESIGNED:
                    statusChgAudit!.ActionCode = "RESIGN";
                    break;
                default:
                    break;
            }

            if (statusChgAudit != null && !statusChgAudit.FromStatus!.Equals(statusChgAudit.ToStatus))
            {
                int statusChgAuditId = await cn.InsertAsync(statusChgAudit, tran);
                if (statusChgAuditId <= 0)
                    throw new Exception("Failed to insert customer change history.");
                else obj.Status = endStatus;
            }

            if (string.IsNullOrEmpty(obj.ObjectCode) && obj.Status == EmployeeStatuses.ACTIVE)
            {
                obj.ObjectCode = await GetRunningNumberAsync(cn, new RunNumGenParam()
                {
                    UserName = obj.ModifiedUser,
                    ObjectClassName = obj.GetType().Name
                }, tran);

                if (string.IsNullOrEmpty(obj.ObjectCode))
                {
                    throw new Exception("Cannot get Doctor.ObjectCode from Running Number Generator.");
                }
            }

            bool isUpdated = await cn.UpdateAsync(obj, tran);
            tran.Commit();
            return isUpdated;
        }
        catch
        {
            tran.Rollback();
            throw;
        }
    }

	public override async Task<KeyValuePair<int, IEnumerable<Doctor>>> SearchNewAsync(
		int pgSize = 0, int pgNo = 0, string? searchText = null,
		IEnumerable<SqlSortCond>? sortConds = null,
		IEnumerable<SqlFilterCond>? filterConds = null, List<int>? excludeIdList = null)
	{
		DynamicParameters param = new();
		SqlBuilder sbSql = new();

		sbSql.Where("t.IsDeleted=0");

		#region Form Search Conditions
		if (!string.IsNullOrEmpty(searchText))
		{
			if (searchText.StartsWith("id:"))
			{
				sbSql.Where("UPPER(t.ObjectCode) LIKE '%'+@SearchText+'%'");
				param.Add("@SearchText", searchText.Replace("id:", "", StringComparison.CurrentCultureIgnoreCase), DbType.AnsiString);
			}
			else
			{
				sbSql.Where("(UPPER(t.ObjectName) LIKE '%'+UPPER(@SearchText)+'%' OR UPPER(t.ObjectCode) LIKE '%'+UPPER(@SearchText)+'%')");
				param.Add("@SearchText", searchText, DbType.AnsiString);
			}
		}

		if (excludeIdList != null && excludeIdList.Count != 0)
		{
			sbSql.Where("t.Id NOT IN @ExcludeIdList");
			param.Add("@ExcludeIdList", excludeIdList);
		}
		#endregion

        //ANY JOIN CONDITIONS
        sbSql.LeftJoin($"{Employee.MsSqlTable} emp ON emp.Id=t.EmployeeId");
        sbSql.LeftJoin($"{HealthcareFacility.MsSqlTable} hcf ON hcf.Id=t.HealthcareFacilityId");

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
			sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) ROWS FETCH NEXT @PageSize ROW ONLY;").RawSql;
		}

		using var cn = DbContext.DbCxn;

		var dataList = await cn.QueryAsync<Doctor, Employee, HealthcareFacility, Doctor>(sql, 
            (obj, emp, hcf) =>
            {
                obj.Employee = emp;
                obj.HealthcareFacility = hcf;

                return obj;
            }, param, splitOn: "Id");

		string sqlCount = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
		int dataCount = await cn.ExecuteScalarAsync<int>(sqlCount, param);
		return new(dataCount, dataList);
	}

	public async Task<DataPagination> GetSearchPaginationAsync(
        int pgSize = 0, 
        string? objectCode = null, 
        string? objectName = null, 
        int? healthcareFacilityId = null, 
        List<string>? statusList = null)
    {
        if (pgSize < 0)
            throw new ArgumentOutOfRangeException(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

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

        if (healthcareFacilityId != null)
        {
            sbSql.Where("t.HealthcareFacilityId=@HealthcareFacilityId");
            param.Add("@HealthcareFacilityId", healthcareFacilityId);
        }

        if (statusList is not null && statusList.Any())
        {
            if (statusList.Count == 1)
            {
                sbSql.Where("t.[Status]=@Status");
                param.Add("@Status", statusList[0]);
            }
            else
            {
                sbSql.Where("t.[Status] IN @StatusList");
                param.Add("@StatusList", statusList);
            }
        }
        #endregion

        string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;

        using var cn = DbContext.DbCxn;

        decimal recordCount = await cn.ExecuteScalarAsync<int>(sql, param);
        int pageCount = (int)Math.Ceiling(recordCount / (pgSize == 0 ? 1 : pgSize));

        DataPagination pagination = new()
        {
            ObjectType = typeof(HealthcareFacility).Name,
            PageSize = pgSize,
            PageCount = pageCount,
            RecordCount = (int)recordCount
        };

        return pagination;
    }

    public async Task<List<Doctor>> SearchAsync(
        int pgSize = 0, 
        int pgNo = 0, 
        string? objectCode = null, 
        string? objectName = null, 
        int? healthcareFacilityId = null, 
        List<string>? statusList = null)
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

        if (healthcareFacilityId != null)
        {
            sbSql.Where("t.HealthcareFacilityId=@HealthcareFacilityId");
            param.Add("@HealthcareFacilityId", healthcareFacilityId);
        }

        if (statusList is not null && statusList.Any())
        {
            if (statusList.Count == 1)
            {
                sbSql.Where("t.[Status]=@Status");
                param.Add("@Status", statusList[0]);
            }
            else
            {
                sbSql.Where("t.[Status] IN @StatusList");
                param.Add("@StatusList", statusList);
            }
        }
        #endregion

        sbSql.LeftJoin($"{Employee.MsSqlTable} emp ON emp.Id=t.EmployeeId");
        sbSql.LeftJoin($"{HealthcareFacility.MsSqlTable} hcf ON hcf.Id=t.HealthcareFacilityId");

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

            sql = sbSql.AddTemplate($";WITH pg AS (SELECT t.Id FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
                  $"SELECT t.*, emp.*, hcf.* FROM {DbObject.MsSqlTable} t INNER JOIN pg p ON p.Id=t.Id /**leftjoin**/ /**orderby**/").RawSql;
        }

        using var cn = DbContext.DbCxn;

        var dataList = (await cn.QueryAsync<Doctor, Employee, HealthcareFacility, Doctor>(
                sql, (obj, employee, hcf) =>
                {
                    obj.Employee = employee;
                    obj.HealthcareFacility = hcf;

                    return obj;
                }, param, splitOn: "Id")).AsList();

        return dataList;
    }
}