using DataLayer.Models.HMS;

namespace DataLayer.Repos.HMS;

/// <summary>
/// Medical Prescription Repository
/// </summary>
public interface IMedicalPrescriptionRepos : IBaseRepos<MedRx>
{
	Task<List<MedRx>> SearchAsync(
		int pgSize = 0, int pgNo = 0,
		string? objectCode = null,
		string? objectName = null,
		List<int>? healthcareFacilityIdList = null,
		List<int>? customerIdList = null,
		List<int>? doctorIdList = null,
		DateTime? issueDateTimeFrom = null,
		DateTime? issueDateTimeTo = null);

	Task<DataPagination> GetSearchPaginationAsync(
		int pgSize = 0,
		string? objectCode = null,
		string? objectName = null,
		List<int>? healthcareFacilityIdList = null,
		List<int>? customerIdList = null,
		List<int>? doctorIdList = null,
		DateTime? issueDateTimeFrom = null,
		DateTime? issueDateTimeTo = null);
}

public class MedRxRepos(IDbContext dbContext) : BaseRepos<MedRx>(dbContext, MedRx.DatabaseObject), IMedicalPrescriptionRepos
{
	public async Task<DataPagination> GetSearchPaginationAsync(
        int pgSize = 0, 
        string? objectCode = null, 
        string? objectName = null, 
        List<int>? healthcareFacilityIdList = null, 
        List<int>? customerIdList = null, 
        List<int>? doctorIdList = null, 
        DateTime? issueDateTimeFrom = null, 
        DateTime? issueDateTimeTo = null)
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

        if (healthcareFacilityIdList != null && healthcareFacilityIdList.Any())
        {
            if (healthcareFacilityIdList.Count == 1)
            {
                sbSql.Where("t.HealthcareFacilityId=@HealthcareFacilityId");
                param.Add("@HealthcareFacilityId", healthcareFacilityIdList[0]);
            }
            else
            {
                sbSql.Where("t.HealthcareFacilityId IN @HealthcareFacilityIdList");
                param.Add("@HealthcareFacilityIdList", healthcareFacilityIdList);
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

        if (issueDateTimeFrom != null)
        {
            sbSql.Where("t.IssueDateTime IS NOT NULL");
            sbSql.Where("t.IssueDateTime>=@IssueDateTimeFrom");
            param.Add("@IssueDateTimeFrom", issueDateTimeFrom);

            if (issueDateTimeTo != null)
            {
                sbSql.Where("t.IssueDateTime<=@IssueDateTimeTo");
                param.Add("@IssueDateTimeTo", issueDateTimeTo);
            }
        }
        else if (issueDateTimeTo != null)
        {
            sbSql.Where("t.IssueDateTime IS NOT NULL");
            sbSql.Where("t.IssueDateTime<=@IssueDateTimeTo");
            param.Add("@IssueDateTimeTo", issueDateTimeTo);
        }
        #endregion

        string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;

        using var cn = DbContext.DbCxn;

        decimal recordCount = await cn.ExecuteScalarAsync<int>(sql, param);
        int pageCount = (int)Math.Ceiling(recordCount / (pgSize == 0 ? 1 : pgSize));

        DataPagination pagination = new()
        {
            ObjectType = typeof(MedTest).Name,
            PageSize = pgSize,
            PageCount = pageCount,
            RecordCount = (int)recordCount
        };

        return pagination;
    }

    public async Task<List<MedRx>> SearchAsync(
        int pgSize = 0, 
        int pgNo = 0, 
        string? objectCode = null, 
        string? objectName = null, 
        List<int>? healthcareFacilityIdList = null, 
        List<int>? customerIdList = null, 
        List<int>? doctorIdList = null, 
        DateTime? issueDateTimeFrom = null, 
        DateTime? issueDateTimeTo = null)
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

        if (healthcareFacilityIdList != null && healthcareFacilityIdList.Any())
        {
            if (healthcareFacilityIdList.Count == 1)
            {
                sbSql.Where("t.HealthcareFacilityId=@HealthcareFacilityId");
                param.Add("@HealthcareFacilityId", healthcareFacilityIdList[0]);
            }
            else
            {
                sbSql.Where("t.HealthcareFacilityId IN @HealthcareFacilityIdList");
                param.Add("@HealthcareFacilityIdList", healthcareFacilityIdList);
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

        if (issueDateTimeFrom != null)
        {
            sbSql.Where("t.IssueDateTime IS NOT NULL");
            sbSql.Where("t.IssueDateTime>=@IssueDateTimeFrom");
            param.Add("@IssueDateTimeFrom", issueDateTimeFrom);

            if (issueDateTimeTo != null)
            {
                sbSql.Where("t.IssueDateTime<=@IssueDateTimeTo");
                param.Add("@IssueDateTimeTo", issueDateTimeTo);
            }
        }
        else if (issueDateTimeTo != null)
        {
            sbSql.Where("t.IssueDateTime IS NOT NULL");
            sbSql.Where("t.IssueDateTime<=@IssueDateTimeTo");
            param.Add("@IssueDateTimeTo", issueDateTimeTo);
        }
        #endregion

        sbSql.LeftJoin($"{HealthcareFacility.MsSqlTable} hcf ON hcf.Id=t.HealthcareFacilityId");
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
                    $";WITH pg AS (SELECT t.Id FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
                    $"SELECT t.*, hcf.*, dr.*, cust.* FROM {DbObject.MsSqlTable} t INNER JOIN pg p ON p.Id=t.Id /**leftjoin**/ /**orderby**/").RawSql;
        }

        using var cn = DbContext.DbCxn;

        var dataList = (await cn.QueryAsync<MedRx, HealthcareFacility, Doctor, Customer, MedRx>(
                                        sql, (obj, healthcareFacility, dr, cust) =>
                                        {
                                            obj.HealthcareFacility = healthcareFacility;
                                            obj.Doctor = dr;
                                            obj.Customer = cust;

                                            return obj;
                                        }, param, splitOn: "Id")).AsList();

        return dataList;
    }
}