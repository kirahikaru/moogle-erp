using DataLayer.GlobalConstant;
using DataLayer.Models.EventManagement;
using DataLayer.Models.SystemCore.NonPersistent;
using System.Text.RegularExpressions;

namespace DataLayer.Repos.EventManagement;

public interface IEventInvitationRepos : IBaseRepos<EventInvitation>
{
	Task<EventInvitation?> GetFullAsync(int id);

	Task<EventInvitation?> GetByAssignedBarcodeAsync(int eventId, string assignedBarcode);

	Task<int> GetExistingByBarcodeCountAsync(int objId, int eventId, string barcode);

	Task<int> GetExistingByNameCountAsync(int objId, int eventId, string objectName, string? gender);

	Task<bool> IsAssignedBarcodeDuplicateAsync(int eventId, int objId, string assignedBarcode);

	Task<int> GetAssignedBarcodeCountAsync(int eventId);

	Task<List<EventInvitation>> GetByEventAsync(int eventId);

	Task<List<DropdownSelectItem>> GetCurrentInvitationGroupingAsync(int eventId);

	Task<List<DropdownSelectItem>> GetCurrentInvitationGuestOfAsync(int eventId);

	Task<List<DropdownSelectItem>> GetNotYetInvitedAsync(int eventId, string? searchText = null);

	Task<List<DropdownSelectItem>> GetForRegistrationAsync(int eventId, int pgSize = 0, int pgNo = 0, string? searchText = null);

	Task<List<EventInvitation>> SearchByEventAsync(int eventId,
		int pgSize = 0,
		int pgNo = 0,
		string? name = null,
		string? assignedBarcode = null,
		string? guestOf = null,
		string? grouping = null,
		List<string>? namePrefixList = null,
		List<string>? genderList = null,
		List<string>? maritalStatusList = null,
		bool? onlyNotYetRegistered = null,
		List<string>? statuses = null);

	Task<DataPagination> GetSearchPaginationByEventAsync(int eventId,
		int pgSize = 0,
		string? name = null,
		string? assignedBarcode = null,
		string? guestOf = null,
		string? grouping = null,
		List<string>? namePrefixList = null,
		List<string>? genderList = null,
		List<string>? maritalStatusList = null,
		bool? onlyNotYetRegistered = null,
		List<string>? statuses = null);

	Task<List<EventInvitation>> SearchAsync(
		int pgSize = 0,
		int pgNo = 0,
		string? objectCode = null,
		string? objectName = null,
		string? eventCode = null,
		string? eventName = null,
		string? nameEn = null,
		string? nameKh = null,
		string? callName = null,
		List<string>? genderList = null,
		List<string>? maritalStatusList = null,
		string? assignedBarcode = null,
		string? guestOf = null,
		string? grouping = null);

	Task<DataPagination> GetSearchPaginationAsync(
		int pgSize = 0,
		string? objectCode = null,
		string? objectName = null,
		string? eventCode = null,
		string? eventName = null,
		string? nameEn = null,
		string? nameKh = null,
		string? callName = null,
		List<string>? genderList = null,
		List<string>? maritalStatusList = null,
		string? assignedBarcode = null,
		string? guestOf = null,
		string? grouping = null);
}

public class EventInvitationRepos(IConnectionFactory connectionFactory) : BaseRepos<EventInvitation>(connectionFactory, EventInvitation.DatabaseObject), IEventInvitationRepos
{
	public async Task<EventInvitation?> GetFullAsync(int id)
    {
		SqlBuilder sbSql = new();
        string sql = string.Empty;

		if (ConnectionFactory.DatabaseType.Is(DatabaseTypes.MSSQL, DatabaseTypes.AZURE_SQL))
        {
			sbSql.LeftJoin($"{Event.MsSqlTable} e ON e.Id=t.EventId");
			sbSql.LeftJoin($"{Person.MsSqlTable} p ON p.Id=t.PersonId");
			sbSql.Where("t.IsDeleted=0 AND t.Id=@Id");

			sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;
		}
        else if (ConnectionFactory.DatabaseType.Is(DatabaseTypes.POSTGRESQL))
        {
			
		}
        else
            throw new NotImplementedException();

        using var cn = ConnectionFactory.GetDbConnection()!;

        var objList = (await cn.QueryAsync<EventInvitation, Event, Person, EventInvitation>(sql,
                                (ei, e, p) =>
                                {
                                    ei.Event = e;
                                    ei.Person = p;

                                    return ei;
                                }, new { Id=id }, splitOn: "Id")).AsList();

		if (objList != null && objList.Count != 0)
            return objList[0];
        else 
            return null;
    }

	public async Task<int> GetExistingByBarcodeCountAsync(int objId, int eventId, string barcode)
    {
		SqlBuilder sbSql = new();
		DynamicParameters param = new();
        string sql = string.Empty;

        if (ConnectionFactory.DatabaseType.Is(DatabaseTypes.MSSQL, DatabaseTypes.AZURE_SQL))
        {
            sbSql.Where("t.IsDeleted=0");
            sbSql.Where("t.Id<>@Id");
            sbSql.Where("t.EventId=@EventId");
            sbSql.Where("t.AssignedBarcode=@AssignedBarcode");

            param.Add("@Id", objId);
            param.Add("@EventId", eventId);
            param.Add("@AssignedBarcode", barcode, DbType.AnsiString);

			sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
		}
        if (ConnectionFactory.DatabaseType.Is(DatabaseTypes.MSSQL, DatabaseTypes.AZURE_SQL))
        {

        }
        else
			throw new NotImplementedException();

		using var cn = ConnectionFactory.GetDbConnection()!;

        int count = await cn.ExecuteScalarAsync<int>(sql, param);

        return count;
	}

	public async Task<int> GetExistingByNameCountAsync(int objId, int eventId, string objectName, string? gender)
    {
		DynamicParameters param = new();
		SqlBuilder sbSql = new();
        List<string> excludeStatusList = [
            EventInvitationStatuses.CANCELLED
        ];
		sbSql.Where("t.IsDeleted=0");
		sbSql.Where("t.Id<>@Id");
		sbSql.Where("t.EventId=@EventId");

        if (!string.IsNullOrEmpty(gender))
        {
            param.Add("@Gender", gender, DbType.AnsiString);
            sbSql.Where("t.Gender=@Gender");
        }
        
        sbSql.Where("t.[Status] NOT IN @ExcludeStatusList");
		sbSql.Where("UPPER(t.ObjectName)=UPPER(@ObjectName) OR UPPER(@ObjectName) LIKE '%'+UPPER(t.FullDisplayNameEn)+'%' OR @ObjectName LIKE '%'+t.FullDisplayNameKh+'%'");

		param.Add("@Id", objId);
		param.Add("@EventId", eventId);
		param.Add("@ObjectName", objectName);
        param.Add("@ExcludeStatusList", excludeStatusList);

		string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;

		using var cn = ConnectionFactory.GetDbConnection()!;

		int count = await cn.ExecuteScalarAsync<int>(sql, param);

		return count;
	}

	public async Task<EventInvitation?> GetByAssignedBarcodeAsync(int eventId, string assignedBarcode)
    {
        SqlBuilder sbSql = new();
        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.EventId=@EventId");
        sbSql.Where("t.AssignedBarcode IS NOT NULL");
        sbSql.Where("t.AssignedBarcode=@AssignedBarcode");

        DynamicParameters param = new();
        param.Add("@EventId", eventId);
        param.Add("@AssignedBarcode", assignedBarcode, DbType.AnsiString);

        string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**where**/").RawSql;

        using var cn = ConnectionFactory.GetDbConnection()!;

        EventInvitation? data = await cn.QuerySingleOrDefaultAsync<EventInvitation>(sql, param);

        return data;
    }

    public async Task<bool> IsAssignedBarcodeDuplicateAsync(int eventId, int objId, string assignedBarcode)
    {
        SqlBuilder sbSql = new();
        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.EventId=@EventId");
        sbSql.Where("t.Id<>@Id");
        sbSql.Where("t.AssignedBarcode IS NOT NULL");
        sbSql.Where("t.AssignedBarcode=@AssignedBarcode");

        DynamicParameters param = new();
        param.Add("@EventId", eventId);
        param.Add("@Id", objId);
        param.Add("@AssignedBarcode", assignedBarcode, DbType.AnsiString);

        using var cn = ConnectionFactory.GetDbConnection()!;
        string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
        int count = await cn.ExecuteScalarAsync<int>(sql, param);
        return count > 0;
    }

	public async Task<List<EventInvitation>> GetByEventAsync(int eventId)
    {
		DynamicParameters param = new();
		SqlBuilder sbSql = new();

		sbSql.Where("t.IsDeleted=0");
		sbSql.Where("t.EventId=@EventId");

		param.Add("@EventId", eventId);

		using var cn = ConnectionFactory.GetDbConnection()!;
		string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
		var dataList = (await cn.QueryAsync<EventInvitation>(sql, param)).AsList();
		return dataList;
	}

	public async Task<List<DropdownSelectItem>> GetForRegistrationAsync(int eventId, int pgSize = 0, int pgNo = 0, string? searchText = null)
    {
        if (pgNo < 0 && pgSize < 0)
            throw new ArgumentOutOfRangeException(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        SqlBuilder sbSql = new();
        DynamicParameters param = new();
        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.EventId=@EventId");
        sbSql.Where("t.[Status] NOT IN ('REGISTERED','CANCELLED','REJECTED')");

        param.Add("@EventId", eventId);

        Regex engPattern = new(@"^[a-zA-Z\s.,-]{1,}$");

        if (!string.IsNullOrEmpty(searchText))
        {
            if (engPattern.IsMatch(searchText))
            {
                sbSql.Where("UPPER(t.FullDisplayNameEn) LIKE '%'+UPPER(@SearchText)+'%' ");
                param.Add("@SearchText", searchText, DbType.AnsiString);
            }
            else
            {
                sbSql.Where("UPPER(t.FullDisplayNameKh) LIKE '%'+UPPER(@SearchText)+'%' ");
                param.Add("@SearchText", searchText);
            }
        }

        sbSql.Select("t.Id");
        sbSql.Select("'Key'=t.ObjectCode");
        sbSql.Select("'Value'=ISNULL(t.FullDisplayNameEn,'')+' / '+ISNULL(t.FullDisplayNameKh,'')+(CASE WHEN LEN(TRIM(ISNULL(t.CallName,'')))>0 THEN ' ('+t.CallName+')' ELSE '' END)");

        using var cn = ConnectionFactory.GetDbConnection()!;

        string sql;

        if (pgNo == 0 && pgSize == 0)
        {
            //SELECT t.Id, 'Key'=t.ObjectCode, 'Value'=ISNULL(t.FullDisplayNameEn,'')+' / '+ISNULL(t.FullDisplayNameKh,'')+(CASE WHEN LEN(TRIM(ISNULL(t.CallName,'')))>0 THEN ' ('+t.CallName+')' ELSE '' END)" FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/
            sql = sbSql.AddTemplate($"SELECT /**select**/ FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/").RawSql;
        }
        else
        {
            param.Add("@PageSize", pgSize);
            param.Add("@PageNo", pgNo);

            sql = sbSql.AddTemplate(
                $";WITH pg AS (SELECT t.Id FROM {DbObject.MsSqlTable} t LEFT JOIN {Event.MsSqlTable} e ON e.Id=t.EventId /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
                $"SELECT /**select**/ FROM {DbObject.MsSqlTable} t INNER JOIN pg p ON p.Id=t.Id /**orderby**/").RawSql;
        }

        var result = (await cn.QueryAsync<DropdownSelectItem>(sql, param)).AsList();

        return result;
    }

    public async Task<List<DropdownSelectItem>> GetCurrentInvitationGroupingAsync(int eventId)
    {
        SqlBuilder sbSql = new();
        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.EventId=@EventId");
        sbSql.Where("t.Grouping IS NOT NULL");
        sbSql.Where("LEN(t.Grouping)>0");

        sbSql.Select("'Id'=CAST(NULL AS INT)");
        sbSql.Select("'Key'=a.Grouping");
        sbSql.Select("'Value'=a.Grouping");

        DynamicParameters param = new();
        param.Add("@EventId", eventId);

        string sql = sbSql.AddTemplate($"SELECT /**select**/ FROM (SELECT DISTINCT t.Grouping FROM {DbObject.MsSqlTable} t /**where**/) a").RawSql;

        using var cn = ConnectionFactory.GetDbConnection()!;
        var dataList = (await cn.QueryAsync<DropdownSelectItem>(sql, param)).AsList();

        return dataList;
    }

    public async Task<List<DropdownSelectItem>> GetCurrentInvitationGuestOfAsync(int eventId)
    {
        SqlBuilder sbSql = new();
        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.EventId=@EventId");
        sbSql.Where("t.GuestOf IS NOT NULL");
        sbSql.Where("LEN(t.GuestOf)>0");

        sbSql.Select("'Id'=CAST(NULL AS INT)");
        sbSql.Select("'Key'=a.GuestOf");
        sbSql.Select("'Value'=a.GuestOf");

        DynamicParameters param = new();
        param.Add("@EventId", eventId);

        string sql = sbSql.AddTemplate($"SELECT /**select**/ FROM (SELECT DISTINCT t.GuestOf FROM {DbObject.MsSqlTable} t /**where**/) a ").RawSql;

        using var cn = ConnectionFactory.GetDbConnection()!;
        var dataList = (await cn.QueryAsync<DropdownSelectItem>(sql, param)).AsList();

        return dataList;
    }

    public override async Task<List<EventInvitation>> QuickSearchAsync(
        int pgSize = 0, int pgNo = 0, 
        string? searchText = null, 
        List<int> ? excludeIdList = null)
    {
        if (pgNo < 0 && pgSize < 0)
            throw new ArgumentOutOfRangeException(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        DynamicParameters param = new();
        SqlBuilder sbSql = new();
        sbSql.Where("t.IsDeleted=0");

        #region Form Search Conditions
        if (!string.IsNullOrEmpty(searchText))
        {
            if (searchText.StartsWith("id:", StringComparison.OrdinalIgnoreCase))
            {
                sbSql.Where("t.AssignedBarcode IS NOT NUL AND UPPER(t.AssignedBarcode) LIKE '%'+UPPER(@SearchText)+'%'");
                
				param.Add("@SearchText", searchText.Replace("id:", "", StringComparison.OrdinalIgnoreCase), DbType.AnsiString);
            }
            else if (searchText.StartsWith("barcode:", StringComparison.OrdinalIgnoreCase))
            {
				sbSql.Where("t.AssignedBarcode IS NOT NUL AND UPPER(t.AssignedBarcode) LIKE '%'+UPPER(@SearchText)+'%'");
				param.Add("@SearchText", searchText.Replace("barcode:", "", StringComparison.OrdinalIgnoreCase), DbType.AnsiString);
			}
			else if (searchText.StartsWith("event-id:", StringComparison.OrdinalIgnoreCase))
			{
				sbSql.Where("e.ObjectCode IS NOT NUL AND UPPER(e.ObjectCode) LIKE '%'+UPPER(@SearchText)+'%'");
				param.Add("@SearchText", searchText.Replace("event-id:","", StringComparison.OrdinalIgnoreCase), DbType.AnsiString);
			}
			else
            {
                sbSql.Where("(t.FullDisplayNameEn LIKE '%'+@SearchText+'%' OR t.FullDisplayNameKh LIKE '%'+@SearchText+'%')");
                param.Add("@SearchText", searchText);
            }
        }

        if (excludeIdList != null && excludeIdList.Count > 0)
        {
            sbSql.Where("t.Id NOT IN @ExcludeIdList");
            param.Add("@ExcludeIdList", excludeIdList);
        }
        #endregion

        sbSql.OrderBy("e.StartDateTime DESC");
        sbSql.OrderBy("e.EndDateTime DESC");
        sbSql.OrderBy("t.FullDisplayNameEn ASC");
        sbSql.OrderBy("t.FullDisplayNameKh ASC");

        sbSql.LeftJoin($"{Event.MsSqlTable} e ON e.Id=t.EventId");
        sbSql.LeftJoin($"{Person.MsSqlTable} p ON p.Id=t.PersonId");

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
                $";WITH pg AS (SELECT t.Id FROM {DbObject.MsSqlTable} t LEFT JOIN {Event.MsSqlTable} e ON e.Id=t.EventId /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
                $"SELECT t.*, e.*, p.* FROM {DbObject.MsSqlTable} t INNER JOIN pg z ON z.Id=t.Id /**leftjoin**/ /**orderby**/").RawSql;
        }

        using var cn = ConnectionFactory.GetDbConnection()!;

        List<EventInvitation> result = (await cn.QueryAsync<EventInvitation, Event, Person, EventInvitation>(sql,
                                (ei, e, p) =>
                                {
                                    ei.Event = e;
                                    ei.Person = p;

                                    return ei;
                                }, param, splitOn: "Id")).AsList();

        return result;
    }

    public override async Task<DataPagination> GetQuickSearchPaginationAsync(int pgSize = 0, string? searchText = null, List<int>? excludeIdList = null)
    {
        if (pgSize < 0)
            throw new ArgumentOutOfRangeException(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        SqlBuilder sbSql = new();
        DynamicParameters param = new();
        sbSql.Where("t.IsDeleted=0");

		#region Form Search Conditions
		if (!string.IsNullOrEmpty(searchText))
		{
			if (searchText.StartsWith("id:", StringComparison.OrdinalIgnoreCase))
			{
				sbSql.Where("t.AssignedBarcode IS NOT NUL AND UPPER(t.AssignedBarcode) LIKE '%'+UPPER(@SearchText)+'%'");

				param.Add("@SearchText", searchText.Replace("id:", "", StringComparison.OrdinalIgnoreCase), DbType.AnsiString);
			}
			else if (searchText.StartsWith("barcode:", StringComparison.OrdinalIgnoreCase))
			{
				sbSql.Where("t.AssignedBarcode IS NOT NUL AND UPPER(t.AssignedBarcode) LIKE '%'+UPPER(@SearchText)+'%'");
				param.Add("@SearchText", searchText.Replace("barcode:", "", StringComparison.OrdinalIgnoreCase), DbType.AnsiString);
			}
			else if (searchText.StartsWith("event-id:", StringComparison.OrdinalIgnoreCase))
			{
				sbSql.Where("e.ObjectCode IS NOT NUL AND UPPER(e.ObjectCode) LIKE '%'+UPPER(@SearchText)+'%'");
				param.Add("@SearchText", searchText.Replace("event-id:", "", StringComparison.OrdinalIgnoreCase), DbType.AnsiString);
			}
			else
			{
				sbSql.Where("(t.FullDisplayNameEn LIKE '%'+@SearchText+'%' OR t.FullDisplayNameKh LIKE '%'+@SearchText+'%')");
				param.Add("@SearchText", searchText);
			}
		}

		if (excludeIdList != null && excludeIdList.Count > 0)
		{
			sbSql.Where("t.Id NOT IN @ExcludeIdList");
			param.Add("@ExcludeIdList", excludeIdList);
		}
		#endregion

		string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;

        using var cn = ConnectionFactory.GetDbConnection()!;

        decimal recordCount = await cn.ExecuteScalarAsync<int>(sql, param);
        int pageCount = (int)(Math.Ceiling(recordCount / (pgSize == 0 ? 1 : pgSize)));

        DataPagination pagination = new()
        {
            ObjectType = typeof(Event).Name,
            PageSize = pgSize,
            PageCount = pageCount,
            RecordCount = (int)recordCount
        };

        return pagination;
    }

    public async Task<List<EventInvitation>> SearchByEventAsync(int eventId,
        int pgSize = 0,
        int pgNo = 0,
        string? name = null,
        string? assignedBarcode = null,
        string? guestOf = null,
        string? grouping = null,
        List<string>? namePrefixList = null,
        List<string>? genderList = null,
        List<string>? maritalStatusList = null,
        bool? onlyNotYetRegistered = null,
        List<string>? statuses = null)
    {
        if (pgNo < 0 && pgSize < 0)
            throw new ArgumentOutOfRangeException(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        DynamicParameters param = new();
        SqlBuilder sbSql = new();

        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.EventId=@EventId");
        param.Add("@EventId", eventId);

        #region Form Search Conditions
        Regex engNamePattern = new(@"^[a-zA-Z\s\W.,-]{1,}$");

        if (!string.IsNullOrEmpty(name))
        {
            if (engNamePattern.IsMatch(name))
            {
                sbSql.Where("((t.FullDisplayNameEn IS NOT NULL AND LOWER(t.FullDisplayNameEn) LIKE '%'+@FullDisplayNameEn+'%') OR t.ObjectName LIKE '%'+@InvitationName+'%')");
                param.Add("@FullDisplayNameEn", name, DbType.AnsiString);
                param.Add("@InvitationName", name);
            }
            else
            {
                sbSql.Where("((t.FullDisplayNameKh IS NOT NULL AND t.FullDisplayNameKh LIKE '%'+@FullDisplayNameEn+'%') OR t.ObjectName LIKE '%'+@InvitationName+'%')");
                param.Add("@FullDisplayNameEn", name);
                param.Add("@InvitationName", name);
            }
        }
        
        if (!string.IsNullOrEmpty(assignedBarcode))
        {
            sbSql.Where("t.AssignedBarcode LIKE '%'+@AssignedBarcode+'%'");
            param.Add("@AssignedBarcode", assignedBarcode, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(guestOf))
        {
            sbSql.Where("t.GuestOf LIKE '%'+@GuestOf+'%'");
            param.Add("@GuestOf", guestOf);
        }

        if (!string.IsNullOrEmpty(grouping))
        {
            sbSql.Where("t.Grouping LIKE '%'+@Grouping+'%'");
            param.Add("@Grouping", grouping);
        }

        if (namePrefixList != null && namePrefixList.Any())
        {
            if (namePrefixList.Count == 1)
            {
                sbSql.Where("t.NamePrefix=@NamePrefix");
                param.Add("@NamePrefix", namePrefixList[0], DbType.AnsiString);
            }
            else
            {
                sbSql.Where("t.NamePrefix IN @NamePrefixList");
                param.Add("@GNamePrefixList", namePrefixList);
            }
        }

        if (genderList != null && genderList.Any())
        {
            if (genderList.Count == 1)
            {
                sbSql.Where("t.Gender=@Gender");
                param.Add("@Gender", genderList[0], DbType.AnsiString);
            }
            else
            {
                sbSql.Where("t.Gender IN @GenderList");
                param.Add("@GenderList", genderList);
            }
        }

        if (maritalStatusList != null && maritalStatusList.Any())
        {
            if (maritalStatusList.Count == 1)
            {
                sbSql.Where("t.MaritalStatus=@MaritalStatus");
                param.Add("@MaritalStatus", maritalStatusList[0], DbType.AnsiString);
            }
            else
            {
                sbSql.Where("t.MaritalStatus IN @MaritalStatusList");
                param.Add("@MaritalStatusList", maritalStatusList);
            }
        }

        if (onlyNotYetRegistered.HasValue && onlyNotYetRegistered!.Value)
        {
            sbSql.Where("t.[Status] IS NOT NULL");
            sbSql.Where("t.[Status]<>@RegisteredStatus");
            param.Add("@RegisteredStatus", EventInvitationStatuses.REGISTERED);
        }

        if (statuses != null && statuses.Any())
        {
            if (statuses.Count == 1)
            {
                sbSql.Where("t.[Status]=@Status");
                param.Add("@Status", statuses[0], DbType.AnsiString);
            }
            else
            {
                sbSql.Where("t.[Status] IN @StatusList");
                param.Add("@StatusList", statuses);
            }
        }

        sbSql.LeftJoin($"{Person.MsSqlTable} p ON p.Id=t.PersonId");

        sbSql.OrderBy("t.FullDisplayNameEn ASC");
        sbSql.OrderBy("t.FullDisplayNameKh ASC");
        #endregion

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
                $"SELECT t.*, ps.* FROM {DbObject.MsSqlTable} t INNER JOIN pg p ON p.Id=t.Id /**leftjoin**/ /**orderby**/").RawSql;
        }

        using var cn = ConnectionFactory.GetDbConnection()!;

        var result = (await cn.QueryAsync<EventInvitation, Person, EventInvitation>(sql,
                                (obj, p) =>
                                {
                                    obj.Person = p;

                                    return obj;
                                }, param, splitOn: "Id")).AsList();

        return result;
    }

    public async Task<DataPagination> GetSearchPaginationByEventAsync(int eventId,
        int pgSize = 0,
        string? name = null,
        string? assignedBarcode = null,
        string? guestOf = null,
        string? grouping = null,
        List<string>? namePrefixList = null,
        List<string>? genderList = null,
        List<string>? maritalStatusList = null,
        bool? onlyNotYetRegistered = null,
        List<string>? statuses = null)
    {
        if (pgSize < 0)
            throw new ArgumentOutOfRangeException(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        DynamicParameters param = new();
        SqlBuilder sbSql = new();

        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.EventId=@EventId");
        param.Add("@EventId", eventId);

        #region Form Search Conditions
        Regex engNamePattern = new(@"^[a-zA-Z\s\W.,-]{1,}$");

        if (!string.IsNullOrEmpty(name))
        {
            if (engNamePattern.IsMatch(name))
            {
                sbSql.Where("((t.FullDisplayNameEn IS NOT NULL AND LOWER(t.FullDisplayNameEn) LIKE '%'+@FullDisplayNameEn+'%') OR t.ObjectName LIKE '%'+@InvitationName+'%')");
                param.Add("@FullDisplayNameEn", name, DbType.AnsiString);
                param.Add("@InvitationName", name);
            }
            else
            {
                sbSql.Where("((t.FullDisplayNameKh IS NOT NULL AND t.FullDisplayNameKh LIKE '%'+@FullDisplayNameEn+'%') OR t.ObjectName LIKE '%'+@InvitationName+'%')");
                param.Add("@FullDisplayNameEn", name);
                param.Add("@InvitationName", name);
            }
        }

        if (!string.IsNullOrEmpty(assignedBarcode))
        {
            sbSql.Where("t.AssignedBarcode LIKE '%'+@AssignedBarcode+'%'");
            param.Add("@AssignedBarcode", assignedBarcode, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(guestOf))
        {
            sbSql.Where("t.GuestOf LIKE '%'+@GuestOf+'%'");
            param.Add("@GuestOf", guestOf);
        }

        if (!string.IsNullOrEmpty(grouping))
        {
            sbSql.Where("t.Grouping LIKE '%'+@Grouping+'%'");
            param.Add("@Grouping", grouping);
        }

        if (namePrefixList != null && namePrefixList.Any())
        {
            if (namePrefixList.Count == 1)
            {
                sbSql.Where("t.NamePrefix=@NamePrefix");
                param.Add("@NamePrefix", namePrefixList[0], DbType.AnsiString);
            }
            else
            {
                sbSql.Where("t.NamePrefix IN @NamePrefixList");
                param.Add("@NamePrefixList", namePrefixList);
            }
        }

		if (genderList != null && genderList.Count != 0)
        {
            if (genderList.Count == 1)
            {
                sbSql.Where("t.Gender=@Gender");
                param.Add("@Gender", genderList[0], DbType.AnsiString);
            }
            else
            {
                sbSql.Where("t.Gender IN @GenderList");
                param.Add("@GenderList", genderList);
            }
        }

        if (maritalStatusList != null && maritalStatusList.Any())
        {
            if (maritalStatusList.Count == 1)
            {
                sbSql.Where("t.MaritalStatus=@MaritalStatus");
                param.Add("@MaritalStatus", maritalStatusList[0], DbType.AnsiString);
            }
            else
            {
                sbSql.Where("t.MaritalStatus IN @MaritalStatusList");
                param.Add("@MaritalStatusList", maritalStatusList);
            }
        }

        if (statuses != null && statuses.Any())
        {
            if (statuses.Count == 1)
            {
                sbSql.Where("t.[Status]=@Status");
                param.Add("@Status", statuses[0], DbType.AnsiString);
            }
            else
            {
                sbSql.Where("t.[Status] IN @StatusList");
                param.Add("@StatusList", statuses);
            }
        }

        sbSql.OrderBy("t.FullDisplayNameEn ASC");
        sbSql.OrderBy("t.FullDisplayNameKh ASC");
        #endregion

        string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;

        using var cn = ConnectionFactory.GetDbConnection()!;

        decimal recordCount = await cn.ExecuteScalarAsync<int>(sql, param);
        int pageCount = (int)(Math.Ceiling(recordCount / (pgSize == 0 ? 1 : pgSize)));

        DataPagination pagination = new()
        {
            ObjectType = typeof(Event).Name,
            PageSize = pgSize,
            PageCount = pageCount,
            RecordCount = (int)recordCount
        };

        return pagination;
    }

    public async Task<List<EventInvitation>> SearchAsync(
        int pgSize = 0,
        int pgNo = 0,
        string? objectCode = null,
        string? objectName = null,
        string? eventCode = null,
        string? eventName = null,
        string? nameEn = null,
        string? nameKh = null,
        string? callName = null,
        List<string>? genderList = null,
        List<string>? maritalStatusList = null,
        string? assignedBarcode = null,
        string? guestOf = null,
        string? grouping = null)
    {
        if (pgNo < 0 && pgSize < 0)
            throw new ArgumentOutOfRangeException(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        DynamicParameters param = new();
        SqlBuilder sbSql = new();

        sbSql.Where("t.IsDeleted=0");

        #region Form Search Conditions
        if (!string.IsNullOrEmpty(objectCode))
        {
            sbSql.Where("t.ObjectCode LIKE @ObjectCode+'%'");
            param.Add("@ObjectCode", objectCode, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(objectName))
        {
            sbSql.Where("LOWER(t.ObjectName) LIKE '%'+@ObjectName+'%'");
            param.Add("@ObjectName", objectName.ToLower(), DbType.AnsiString);
        }

		if (!string.IsNullOrEmpty(eventCode))
		{
			sbSql.Where("UPPER(e.ObjectCode) LIKE '%'+UPPER(@EventCode)+'%'");
			param.Add("@EventCode", eventCode, DbType.AnsiString);
		}

		if (!string.IsNullOrEmpty(eventName))
        {
            sbSql.Where("LOWER(e.ObjectName) LIKE '%'+LOWER(@EventName)+'%'");
            param.Add("@EventName", eventName, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(nameEn))
        {
            sbSql.Where("LOWER(t.FullDisplayNameEn) LIKE '%'+LOWER(@NameEn)+'%'");
            param.Add("@NameEn", nameEn, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(nameKh))
        {
            sbSql.Where("LOWER(t.FullDisplayNameKh) LIKE '%'+LOWER(@NameKh)+'%'");
            param.Add("@NameKh", nameKh);
        }

        if (!string.IsNullOrEmpty(callName))
        {
            sbSql.Where("LOWER(t.CallName) LIKE '%'+LOWER(@CallName)+'%'");
            param.Add("@CallName", callName);
        }

        if (genderList != null && genderList.Count != 0)
        {
            if (genderList.Count == 1)
            {
                sbSql.Where("t.Gender=@Gender");
                param.Add("@Gender", genderList[0], DbType.AnsiString);
            }
            else
            {
                sbSql.Where("t.Gender IN @GenderList");
                param.Add("@GenderList", genderList);
            }
        }

		if (maritalStatusList != null && maritalStatusList.Count != 0)
        {
            if (maritalStatusList.Count == 1)
            {
                sbSql.Where("t.MaritalStatus=@MaritalStatus");
                param.Add("@MaritalStatus", maritalStatusList[0], DbType.AnsiString);
            }
            else
            {
                sbSql.Where("t.MaritalStatus IN @MaritalStatusList");
                param.Add("@MaritalStatusList", maritalStatusList);
            }
        }

        if (!string.IsNullOrEmpty(assignedBarcode))
        {
            sbSql.Where("LOWER(t.AssignedBarcode) LIKE '%'+LOWER(@AssignedBarcode)+'%'");
            param.Add("@AssignedBarcode", assignedBarcode);
        }

        if (!string.IsNullOrEmpty(guestOf))
        {
            sbSql.Where("LOWER(t.GuestOf) LIKE '%'+LOWER(@GuestOf)+'%'");
            param.Add("@GuestOf", guestOf);
        }

        if (!string.IsNullOrEmpty(grouping))
        {
            sbSql.Where("LOWER(t.Grouping) LIKE '%'+LOWER(@Grouping)+'%'");
            param.Add("@Grouping", grouping);
        }
        #endregion

        sbSql.OrderBy("e.StartDateTime DESC");
        sbSql.OrderBy("e.EndDateTime DESC");

        sbSql.LeftJoin($"{Event.MsSqlTable} e ON e.Id=t.EventId");
        sbSql.LeftJoin($"{Person.MsSqlTable} p ON p.Id=t.PersonId");

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
                $";WITH pg AS (SELECT t.Id FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
                $"SELECT t.*, e.*, p.* FROM {DbObject.MsSqlTable} t INNER JOIN pg z ON z.Id=t.Id /**leftjoin**/ /**orderby**/").RawSql;
        }

        using var cn = ConnectionFactory.GetDbConnection()!;

        List<EventInvitation> result = (await cn.QueryAsync<EventInvitation, Event, Person, EventInvitation>(sql,
                                (obj, e, p) =>
                                {
                                    obj.Event = e;
                                    obj.Person = p;

                                    return obj;
                                }, param, splitOn: "Id")).AsList();

        return result;
    }

    public async Task<DataPagination> GetSearchPaginationAsync(
        int pgSize = 0,
        string? objectCode = null,
        string? objectName = null,
		string? eventCode = null,
		string? eventName = null,
        string? nameEn = null,
        string? nameKh = null,
        string? callName = null,
        List<string>? genderList = null,
        List<string>? maritalStatusList = null,
        string? assignedBarcode = null,
        string? guestOf = null,
        string? grouping = null)
    {
        if (pgSize < 0)
            throw new ArgumentOutOfRangeException(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        DynamicParameters param = new();
        SqlBuilder sbSql = new();

        sbSql.Where("t.IsDeleted=0");

        #region Form Search Conditions
        if (!string.IsNullOrEmpty(objectCode))
        {
            sbSql.Where("t.ObjectCode LIKE @ObjectCode+'%'");
            param.Add("@ObjectCode", objectCode, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(objectName))
        {
            sbSql.Where("LOWER(t.ObjectName) LIKE '%'+@ObjectName+'%'");
            param.Add("@ObjectName", objectName.ToLower(), DbType.AnsiString);
        }

		if (!string.IsNullOrEmpty(eventCode))
		{
			sbSql.Where("UPPER(e.ObjectCode) LIKE '%'+UPPER(@EventCode)+'%'");
			param.Add("@EventCode", eventCode, DbType.AnsiString);
		}

		if (!string.IsNullOrEmpty(eventName))
		{
			sbSql.Where("LOWER(e.ObjectName) LIKE '%'+LOWER(@EventName)+'%'");
			param.Add("@EventName", eventName, DbType.AnsiString);
		}

		if (!string.IsNullOrEmpty(nameEn))
        {
            sbSql.Where("LOWER(t.FullDisplayNameEn) LIKE '%'+LOWER(@NameEn)+'%'");
            param.Add("@NameEn", nameEn, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(nameKh))
        {
            sbSql.Where("LOWER(t.FullDisplayNameKh) LIKE '%'+LOWER(@NameKh)+'%'");
            param.Add("@NameKh", nameKh);
        }

        if (!string.IsNullOrEmpty(callName))
        {
            sbSql.Where("LOWER(t.CallName) LIKE '%'+LOWER(@CallName)+'%'");
            param.Add("@CallName", callName);
        }

        if (genderList != null && genderList.Any())
        {
            if (genderList.Count == 1)
            {
                sbSql.Where("t.Gender=@Gender");
                param.Add("@Gender", genderList[0], DbType.AnsiString);
            }
            else
            {
                sbSql.Where("t.Gender IN @GenderList");
                param.Add("@GenderList", genderList);
            }
        }

        if (maritalStatusList != null && maritalStatusList.Any())
        {
            if (maritalStatusList.Count == 1)
            {
                sbSql.Where("t.MaritalStatus=@MaritalStatus");
                param.Add("@MaritalStatus", maritalStatusList[0], DbType.AnsiString);
            }
            else
            {
                sbSql.Where("t.MaritalStatus IN @MaritalStatusList");
                param.Add("@MaritalStatusList", maritalStatusList);
            }
        }

        if (!string.IsNullOrEmpty(assignedBarcode))
        {
            sbSql.Where("LOWER(t.AssignedBarcode) LIKE '%'+LOWER(@AssignedBarcode)+'%'");
            param.Add("@AssignedBarcode", assignedBarcode);
        }

        if (!string.IsNullOrEmpty(guestOf))
        {
            sbSql.Where("LOWER(t.GuestOf) LIKE '%'+LOWER(@GuestOf)+'%'");
            param.Add("@GuestOf", guestOf);
        }

        if (!string.IsNullOrEmpty(grouping))
        {
            sbSql.Where("LOWER(t.Grouping) LIKE '%'+LOWER(@Grouping)+'%'");
            param.Add("@Grouping", grouping);
        }
		#endregion

		sbSql.LeftJoin($"{Event.MsSqlTable} e ON e.Id=t.EventId");

		string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;

        using var cn = ConnectionFactory.GetDbConnection()!;

        decimal recordCount = await cn.ExecuteScalarAsync<int>(sql, param);
        int pageCount = (int)(Math.Ceiling(recordCount / (pgSize == 0 ? 1 : pgSize)));

        DataPagination pagination = new()
        {
            ObjectType = typeof(Event).Name,
            PageSize = pgSize,
            PageCount = pageCount,
            RecordCount = (int)recordCount
        };

        return pagination;
    }

    public async Task<int> GetAssignedBarcodeCountAsync(int eventId)
    {
        string sql = $"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t WHERE t.IsDeleted=0 AND t.EventId=@EventId AND LEN(ISNULL(t.AssignedBarcode,''))>0";

        using var cn = ConnectionFactory.GetDbConnection()!;

        int count = await cn.ExecuteScalarAsync<int>(sql, new { EventId = eventId });
        return count;
    }

    public async Task<List<DropdownSelectItem>> GetNotYetInvitedAsync(int eventId, string? searchText = null)
    {
        SqlBuilder sbSql = new();
        DynamicParameters param = new();
        sbSql.Select("t.Id");
        sbSql.Select("'Key' = t.ObjectCode");
        sbSql.Select("'Value'=dbo.GetCompleteDipslayName(t.Surname, t.GivenName, t.SurnameKh, t.GivenNameKh, t.Gender) COLLATE SQL_Latin1_General_CP1_CI_AS");

        sbSql.LeftJoin($"{EventInvitation.MsSqlTable} ei ON ei.IsDeleted=0 AND ei.EventId=@EventId AND ei.PersonId=t.Id");
        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("ei.Id IS NULL");
        
        param.Add("@EventId", eventId);

        if (!string.IsNullOrEmpty(searchText))
        {
            Regex engNamePattern = new(@"^[a-zA-Z\s]{1,}$");

            if (engNamePattern.IsMatch(searchText))
            {
                sbSql.Where("(UPPER(ISNULL(t.GivenName,'')) + ' ' + UPPER(ISNULL(t.Surname,'')) LIKE '%'+UPPER(@SearchText)+'%')");
                param.Add("@SearchText", searchText, DbType.AnsiString);
            }
            else
            {
                sbSql.Where("ISNULL(t.SurnameKh,'') + ' ' + ISNULL(t.GivenNameKh,'') LIKE '%'+@SearchText+'%'");
                param.Add("@SearchText", searchText);
            }
        }

        using var cn = ConnectionFactory.GetDbConnection()!;

        string sql = sbSql.AddTemplate($"SELECT /**select**/ FROM {Person.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;
        var result = (await cn.QueryAsync<DropdownSelectItem>(sql, param)).AsList();

        return result;
    }
}