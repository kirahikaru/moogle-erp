using DataLayer.GlobalConstant;
using DataLayer.Models.EMS;
using System.Text.RegularExpressions;

namespace DataLayer.Repos.EMS;

public interface IEventRegistrationRepos : IBaseRepos<EventRegistration>
{
	Task<int> GetExistingCountAsync(int objId, int eventId, int invitationId);
	Task<int> GetExistingCountAsync(int objId, int eventId, string walkInRef);

	Task<EventRegistration?> GetFullAsync(int id);
	Task<EventRegistration?> GetFullByInvitationAsync(int eventInvitationId);

	Task<int> InsertFullAsync(EventRegistration obj);

	Task<List<EventRegistration>> GetByEventAsync(int eventId, bool includeInvitationLink = false);

	Task<List<EventRegistration>> SearchByEventAsync(int eventId,
		int pgSize = 0,
		int pgNo = 0,
		string? name = null,
		string? assignedBarcode = null,
		string? guestOf = null,
		string? grouping = null,
		List<string>? namePrefixList = null,
		List<string>? genderList = null,
		List<string>? maritalStatusList = null,
		bool? isWalkIn = null,
		decimal? feeAmountUsdFrom = null,
		decimal? feeAmountUsdTo = null,
		decimal? feeAmountKhrFrom = null,
		decimal? feeAmountKhrTo = null);

	Task<DataPagination> GetSearchPaginationByEventAsync(int eventId,
		int pgSize = 0,
		string? name = null,
		string? assignedBarcode = null,
		string? guestOf = null,
		string? grouping = null,
		List<string>? namePrefixList = null,
		List<string>? genderList = null,
		List<string>? maritalStatusList = null,
		bool? isWalkIn = null,
		decimal? feeAmountUsdFrom = null,
		decimal? feeAmountUsdTo = null,
		decimal? feeAmountKhrFrom = null,
		decimal? feeAmountKhrTo = null);

	Task<List<EventRegistration>> SearchAsync(
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
		string? grouping = null,
		bool? isWalkIn = null,
		decimal? feeAmountUsdFrom = null,
		decimal? feeAmountUsdTo = null,
		decimal? feeAmountKhrFrom = null,
		decimal? feeAmountKhrTo = null);

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
		string? grouping = null,
		bool? isWalkIn = null,
		decimal? feeAmountUsdFrom = null,
		decimal? feeAmountUsdTo = null,
		decimal? feeAmountKhrFrom = null,
		decimal? feeAmountKhrTo = null);
}

public class EventRegistrationRepos(IDbContext dbContext) : BaseRepos<EventRegistration>(dbContext, EventRegistration.DatabaseObject), IEventRegistrationRepos
{
	public async Task<int> GetExistingCountAsync(int objId, int eventId, int invitationId)
    {
		DynamicParameters param = new();
		SqlBuilder sbSql = new();
		sbSql.Where("t.IsDeleted=0");
		sbSql.Where("t.IsCancelled=0");
		sbSql.Where("t.Id<>0");
		sbSql.Where("t.EventId=@EventId");
		sbSql.Where("t.InvitationId IS NOT NULL");
		sbSql.Where("t.InvitationId=@InvitationId");

		param.Add("@CurrentObjectId", objId);
		param.Add("@EventId", eventId);
		param.Add("@InvitationId", invitationId);

		string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;

		using var cn = DbContext.DbCxn;

		int count = await cn.ExecuteScalarAsync<int>(sql, param);
		return count;
	}

	public async Task<int> GetExistingCountAsync(int objId, int eventId, string walkInRef)
    {
		DynamicParameters param = new();
		SqlBuilder sbSql = new();
        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.IsCancelled=0");
        sbSql.Where("t.EventId=@EventId");
        sbSql.Where("t.ObjectCode=@ObjectCode");
        sbSql.Where("t.InvitationId IS NULL");

        param.Add("@CurrentObjectId", objId);
		param.Add("@EventId", eventId);
		param.Add("@ObjectCode", walkInRef, DbType.AnsiString);

        string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;

		using var cn = DbContext.DbCxn;

        int count = await cn.ExecuteScalarAsync<int>(sql, param);
        return count;
	}


	public async Task<EventRegistration?> GetFullAsync(int id)
    {
        DynamicParameters param = new();
        SqlBuilder sbSql = new();
        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.Id=@Id");

        sbSql.LeftJoin($"{EventInvitation.MsSqlTable} ei ON ei.Id=t.EventInvitationId");
        sbSql.LeftJoin($"{Event.MsSqlTable} e ON e.Id=t.EventId");

        string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;

        using var cn = DbContext.DbCxn;

        
        var dataList = (await cn.QueryAsync<EventRegistration, EventInvitation, Event, EventRegistration>(sql,
                                (obj, ei, e) =>
                                {
                                    obj.Invitation = ei;
                                    obj.Event = e;

                                    return obj;
                                }, new { Id = id }, splitOn: "Id")).AsList();

		if (dataList != null && dataList.Count != 0)
            return dataList[0];
        else
            return null;
    }

    public async Task<EventRegistration?> GetFullByInvitationAsync(int eventInvitationId)
    {
        DynamicParameters param = new();
        SqlBuilder sbSql = new();
        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.EventInvitationId IS NOT NULL");
        sbSql.Where("t.EventInvitationId=@EventInvitationId");

        sbSql.LeftJoin($"{EventInvitation.MsSqlTable} ei ON ei.Id=t.EventInvitationId");
        sbSql.LeftJoin($"{Event.MsSqlTable} e ON e.Id=t.EventId");

        string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;

        using var cn = DbContext.DbCxn;

        var objList = (await cn.QueryAsync<EventRegistration, EventInvitation, Event, EventRegistration>(sql,
                                (obj, ei, e) =>
                                {
                                    obj.Invitation = ei;
                                    obj.Event = e;

                                    return obj;
                                }, new { EventInvitationId = eventInvitationId }, splitOn: "Id")).AsList();

        if (objList != null && objList.Any())
            return objList[0];
        else
            return null;
    }

    public async Task<int> InsertFullAsync(EventRegistration obj)
    {
        DateTime khTimestamp = DateTime.UtcNow.AddHours(7);

        using var cn = DbContext.DbCxn;

        // <!IMPORTANT> Connection required to be open before calling BeginTransaction() function
        if (cn.State != ConnectionState.Open) cn.Open();

        using var tran = cn.BeginTransaction();

        try
        {
            obj.CreatedDateTime = khTimestamp;
            obj.ModifiedDateTime = khTimestamp;
            int objId = await cn.InsertAsync(obj, tran);
            //string insSql = QueryGenerator.GenerateInsertQuery(receipt.GetType(), this.DatabaseObject);
            //int objId = cn.QuerySingleOrDefault<int>(insSql, receipt, tran);
            if (objId > 0)
            {
                if (obj.EventInvitationId.HasValue)
                {
                    string updInvitationCmd = $"UPDATE {EventInvitation.MsSqlTable} SET [Status]=@Status, ModifiedDateTime=@ModifiedDateTime, ModifiedUser=@ModifiedUser WHERE Id=@InvitationId";
                    DynamicParameters updInvitationParam = new();
                    updInvitationParam.Add("@Status", EventInvitationStatuses.REGISTERED);
                    updInvitationParam.Add("@ModifiedDateTime", khTimestamp);
                    updInvitationParam.Add("@ModifiedUser", obj.ModifiedUser);
                    updInvitationParam.Add("@InvitationId", obj.EventInvitationId!.Value);

                    int updInvitationCount = await cn.ExecuteAsync(updInvitationCmd, updInvitationParam, tran);

                    if (updInvitationCount <= 0)
                        throw new Exception("Failed to update invitation status to registered.");
                }

                tran.Commit();
                return objId;
            }
            else
                throw new Exception($"failed to insert object to database.");
        }
        catch
        {
            tran.Rollback();
            throw;
        }
    }

    public async Task<List<EventRegistration>> GetByEventAsync(int eventId, bool includeInvitationLink = false)
    {
		DynamicParameters param = new();
		SqlBuilder sbSql = new();

        sbSql.Where("t.IsDeleted=0");
		sbSql.Where("t.EventId=@EventId");

        param.Add("@EventId", eventId);

		using var cn = DbContext.DbCxn;

        if (includeInvitationLink)
        {
            sbSql.LeftJoin($"{EventInvitation.MsSqlTable} ei ON ei.Id=t.EventInvitationId");
			string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;
            var dataList = (await cn.QueryAsync<EventRegistration, EventInvitation, EventRegistration>(sql, (r, i) =>
            {
                r.Invitation = i;

                return r;
            }, param, splitOn:"Id")).AsList();

            return dataList;
		}
        else
        {
			string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;
            var dataList = (await cn.QueryAsync<EventRegistration>(sql, param)).AsList();
            return dataList;
		}
	}


	public async Task<List<EventRegistration>> SearchByEventAsync(int eventId,
        int pgSize = 0,
        int pgNo = 0,
        string? name = null,
        string? assignedBarcode = null,
        string? guestOf = null,
        string? grouping = null,
        List<string>? namePrefixList = null,
        List<string>? genderList = null,
        List<string>? maritalStatusList = null,
        bool? isWalkIn = null,
        decimal? feeAmountUsdFrom = null,
        decimal? feeAmountUsdTo = null,
        decimal? feeAmountKhrFrom = null,
        decimal? feeAmountKhrTo = null)
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
            sbSql.Where("t.Barcode LIKE '%'+@Barcode+'%'");
            param.Add("@Barcode", assignedBarcode, DbType.AnsiString);
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

        if (isWalkIn is not null)
        {
            if (isWalkIn!.Value)
                sbSql.Where("t.EventInvitationId IS NULL");
            else
                sbSql.Where("t.EventInvitationId IS NOT NULL");
        }

        if (feeAmountUsdFrom.HasValue)
        {
            sbSql.Where("t.FeeAmountPaidUsd IS NOT NULL");
            sbSql.Where("t.FeeAmountPaidUsd>=@FeeAmountUsdFrom");
            param.Add("@FeeAmountUsdFrom", feeAmountUsdFrom!.Value);

            if (feeAmountUsdTo.HasValue)
            {
                sbSql.Where("t.FeeAmountPaidUsd<=@FeeAmountUsdTo");
                param.Add("@FeeAmountUsdTo", feeAmountUsdTo!.Value);
            }
        }
        else if (feeAmountUsdTo.HasValue)
        {
            sbSql.Where("t.FeeAmountPaidUsd IS NOT NULL");
            sbSql.Where("t.FeeAmountPaidUsd<=@FeeAmountUsdTo");
            param.Add("@FeeAmountUsdTo", feeAmountUsdTo!.Value);
        }

        if (feeAmountKhrFrom.HasValue)
        {
            sbSql.Where("t.FeeAmountPaidKhr IS NOT NULL");
            sbSql.Where("t.FeeAmountPaidKhr>=@FeeAmountKhrFrom");
            param.Add("@FeeAmountKhrFrom", feeAmountKhrFrom!.Value);

            if (feeAmountKhrTo.HasValue)
            {
                sbSql.Where("t.FeeAmountPaidKhr<=@FeeAmountKhrTo");
                param.Add("@FeeAmountKhrTo", feeAmountKhrTo!.Value);
            }
        }
        else if (feeAmountKhrTo.HasValue)
        {
            sbSql.Where("t.FeeAmountPaidKhr IS NOT NULL");
            sbSql.Where("t.FeeAmountPaidKhr<=@FeeAmountKhrTo");
            param.Add("@FeeAmountKhrTo", feeAmountKhrTo!.Value);
        }

        sbSql.OrderBy("t.FullDisplayNameEn ASC");
        sbSql.OrderBy("t.FullDisplayNameKh ASC");
        #endregion

        string sql;

        if (pgNo == 0 && pgSize == 0)
        {
            sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/").RawSql;
        }
        else
        {
            param.Add("@PageSize", pgSize);
            param.Add("@PageNo", pgNo);

            sql = sbSql.AddTemplate(
                $";WITH pg AS (SELECT t.Id FROM {DbObject.MsSqlTable} t /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
                $"SELECT t.* FROM {DbObject.MsSqlTable} t INNER JOIN pg z ON z.Id=t.Id /**orderby**/").RawSql;
        }

        using var cn = DbContext.DbCxn;

        var result = (await cn.QueryAsync<EventRegistration>(sql, param)).AsList();

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
        bool? isWalkIn = null,
        decimal? feeAmountUsdFrom = null,
        decimal? feeAmountUsdTo = null,
        decimal? feeAmountKhrFrom = null,
        decimal? feeAmountKhrTo = null)
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

        if (namePrefixList != null && namePrefixList.Count != 0)
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

        sbSql.OrderBy("t.FullDisplayNameEn ASC");
        sbSql.OrderBy("t.FullDisplayNameKh ASC");
        #endregion

        string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;

        using var cn = DbContext.DbCxn;

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
    public override async Task<List<EventRegistration>> QuickSearchAsync(int pgSize = 0, int pgNo = 0, string? searchText = null, List<int>? excludeIdList = null)
    {
        if (pgNo < 0 && pgSize < 0)
            throw new ArgumentOutOfRangeException(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        DynamicParameters param = new();
        SqlBuilder sbSql = new();
        sbSql.Where("t.IsDeleted=0");

        #region Form Search Conditions
        if (!string.IsNullOrEmpty(searchText))
        {
            if (searchText.StartsWith("id:", StringComparison.OrdinalIgnoreCase) || searchText.StartsWith("barcode:", StringComparison.OrdinalIgnoreCase))
            {
                sbSql.Where("t.Barcode IS NOT NUL AND UPPER(t.Barcode) LIKE '%'+UPPER(@SearchText)+'%'");
                param.Add("@SearchText", searchText, DbType.AnsiString);
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

        sbSql.LeftJoin($"{EventInvitation.MsSqlTable} ei ON ei.Id=t.EventInvitationId");
        sbSql.LeftJoin($"{Event.MsSqlTable} e ON e.Id=t.EventId");

        sbSql.OrderBy("ei.FullDisplayNameEn ASC")
            .OrderBy("ei.FullDisplayNameKh ASC");

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
                $"SELECT t.*, ei.*, e.* FROM {DbObject.MsSqlTable} t INNER JOIN pg p ON p.Id=t.Id /**leftjoin**/ /**orderby**/").RawSql;
        }

        using var cn = DbContext.DbCxn;

        var result = (await cn.QueryAsync<EventRegistration, EventInvitation, Event, EventRegistration>(sql,
                        (obj, ei, e) =>
                        {
                            obj.Invitation = ei;
                            obj.Event = e;

                            return obj;
                        }, param, splitOn: "Id")).AsList();

        return result;
    }

    public override async Task<DataPagination> GetQuickSearchPaginationAsync(int pgSize = 0, string? searchText = null, List<int>? excludeIdList = null)
    {
        if (pgSize < 0)
            throw new ArgumentOutOfRangeException(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        DynamicParameters param = new();
        SqlBuilder sbSql = new();
        sbSql.Where("t.IsDeleted=0");

        #region Form Search Conditions
        if (!string.IsNullOrEmpty(searchText))
        {
            if (searchText.StartsWith("id:", StringComparison.OrdinalIgnoreCase) || searchText.StartsWith("barcode:", StringComparison.OrdinalIgnoreCase))
            {
                sbSql.Where("ei.AssignedBarcode IS NOT NUL AND UPPER(ei.AssignedBarcode) LIKE '%'+UPPER(@SearchText)+'%'");
                param.Add("@SearchText", searchText, DbType.AnsiString);
            }
            else
            {
                sbSql.Where("(ei.FullDisplayNameEn LIKE '%'+@SearchText+'%' OR ei.FullDisplayNameKh LIKE '%'+@SearchText+'%')");
                param.Add("@SearchText", searchText);
            }
        }

        if (excludeIdList != null && excludeIdList.Count > 0)
        {
            sbSql.Where("t.Id NOT IN @ExcludeIdList");
            param.Add("@ExcludeIdList", excludeIdList);
        }
        #endregion

        sbSql.LeftJoin($"{EventInvitation.MsSqlTable} ei ON ei.Id=t.EventInvitationId");
        sbSql.LeftJoin($"{Event.MsSqlTable} e ON e.Id=t.EventId");

        string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;

        using var cn = DbContext.DbCxn;

        decimal recordCount = await cn.ExecuteScalarAsync<int>(sql, param);
        int pageCount = (int)Math.Ceiling(recordCount / (pgSize == 0 ? 1 : pgSize));

        DataPagination pagination = new()
        {
            ObjectType = typeof(Event).Name,
            PageSize = pgSize,
            PageCount = pageCount,
            RecordCount = (int)recordCount
        };

        return pagination;
    }

    public async Task<List<EventRegistration>> SearchAsync(
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
        string? grouping = null,
        bool? isWalkIn = null,
        decimal? feeAmountUsdFrom = null,
        decimal? feeAmountUsdTo = null,
        decimal? feeAmountKhrFrom = null,
        decimal? feeAmountKhrTo = null)
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

        if (!string.IsNullOrEmpty(assignedBarcode))
        {
            sbSql.Where("LOWER(ei.AssignedBarcode) LIKE '%'+LOWER(@AssignedBarcode)+'%'");
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

        if (isWalkIn is not null)
        {
            if (isWalkIn!.Value)
                sbSql.Where("t.EventInvitationId IS NULL");
            else
                sbSql.Where("t.EventInvitationId IS NOT NULL");
        }

        if (feeAmountUsdFrom.HasValue)
        {
            sbSql.Where("t.FeeAmountPaidUsd IS NOT NULL");
            sbSql.Where("t.FeeAmountPaidUsd>=@FeeAmountUsdFrom");
            param.Add("@FeeAmountUsdFrom", feeAmountUsdFrom!.Value);

            if (feeAmountUsdTo.HasValue)
            {
                sbSql.Where("t.FeeAmountPaidUsd<=@FeeAmountUsdTo");
                param.Add("@FeeAmountUsdTo", feeAmountUsdTo!.Value);
            }
        }
        else if (feeAmountUsdTo.HasValue)
        {
            sbSql.Where("t.FeeAmountPaidUsd IS NOT NULL");
            sbSql.Where("t.FeeAmountPaidUsd<=@FeeAmountUsdTo");
            param.Add("@FeeAmountUsdTo", feeAmountUsdTo!.Value);
        }

        if (feeAmountKhrFrom.HasValue)
        {
            sbSql.Where("t.FeeAmountPaidKhr IS NOT NULL");
            sbSql.Where("t.FeeAmountPaidKhr>=@FeeAmountKhrFrom");
            param.Add("@FeeAmountKhrFrom", feeAmountKhrFrom!.Value);

            if (feeAmountKhrTo.HasValue)
            {
                sbSql.Where("t.FeeAmountPaidKhr<=@FeeAmountKhrTo");
                param.Add("@FeeAmountKhrTo", feeAmountKhrTo!.Value);
            }
        }
        else if (feeAmountKhrTo.HasValue)
        {
            sbSql.Where("t.FeeAmountPaidKhr IS NOT NULL");
            sbSql.Where("t.FeeAmountPaidKhr<=@FeeAmountKhrTo");
            param.Add("@FeeAmountKhrTo", feeAmountKhrTo!.Value);
        }
        #endregion

        sbSql.LeftJoin($"{EventInvitation.MsSqlTable} ei ON ei.Id=t.EventInvitationId");
        sbSql.LeftJoin($"{Event.MsSqlTable} e ON e.Id=t.EventId");

        sbSql.OrderBy("e.EventName ASC")
            .OrderBy("ei.AssignedBarcode ASC");

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
                $"SELECT t.*, e.*, p.* FROM {DbObject.MsSqlTable} t INNER JOIN pg p ON p.Id=t.Id /**leftjoin**/ /**orderby**/").RawSql;
        }

        using var cn = DbContext.DbCxn;

        var result = (await cn.QueryAsync<EventRegistration, EventInvitation, Event, EventRegistration>(sql,
                                (obj, ei, e) =>
                                {
                                    obj.Invitation = ei;
                                    obj.Event = e;

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
        string? grouping = null,
        bool? isWalkIn = null,
        decimal? feeAmountUsdFrom = null,
        decimal? feeAmountUsdTo = null,
        decimal? feeAmountKhrFrom = null,
        decimal? feeAmountKhrTo = null)
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

        if (!string.IsNullOrEmpty(assignedBarcode))
        {
            sbSql.Where("LOWER(ei.AssignedBarcode) LIKE '%'+LOWER(@AssignedBarcode)+'%'");
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

        if (isWalkIn is not null)
        {
            if (isWalkIn!.Value)
                sbSql.Where("t.EventInvitationId IS NULL");
            else
                sbSql.Where("t.EventInvitationId IS NOT NULL");
        }

        if (feeAmountUsdFrom.HasValue)
        {
            sbSql.Where("t.FeeAmountPaidUsd IS NOT NULL");
            sbSql.Where("t.FeeAmountPaidUsd>=@FeeAmountUsdFrom");
            param.Add("@FeeAmountUsdFrom", feeAmountUsdFrom!.Value);

            if (feeAmountUsdTo.HasValue)
            {
                sbSql.Where("t.FeeAmountPaidUsd<=@FeeAmountUsdTo");
                param.Add("@FeeAmountUsdTo", feeAmountUsdTo!.Value);
            }
        }
        else if (feeAmountUsdTo.HasValue)
        {
            sbSql.Where("t.FeeAmountPaidUsd IS NOT NULL");
            sbSql.Where("t.FeeAmountPaidUsd<=@FeeAmountUsdTo");
            param.Add("@FeeAmountUsdTo", feeAmountUsdTo!.Value);
        }

        if (feeAmountKhrFrom.HasValue)
        {
            sbSql.Where("t.FeeAmountPaidKhr IS NOT NULL");
            sbSql.Where("t.FeeAmountPaidKhr>=@FeeAmountKhrFrom");
            param.Add("@FeeAmountKhrFrom", feeAmountKhrFrom!.Value);

            if (feeAmountKhrTo.HasValue)
            {
                sbSql.Where("t.FeeAmountPaidKhr<=@FeeAmountKhrTo");
                param.Add("@FeeAmountKhrTo", feeAmountKhrTo!.Value);
            }
        }
        else if (feeAmountKhrTo.HasValue)
        {
            sbSql.Where("t.FeeAmountPaidKhr IS NOT NULL");
            sbSql.Where("t.FeeAmountPaidKhr<=@FeeAmountKhrTo");
            param.Add("@FeeAmountKhrTo", feeAmountKhrTo!.Value);
        }
        #endregion

        sbSql.LeftJoin($"{EventInvitation.MsSqlTable} ei ON ei.Id=t.EventInvitationId");
        sbSql.LeftJoin($"{Event.MsSqlTable} e ON e.Id=t.EventId");

        string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;

        using var cn = DbContext.DbCxn;

        decimal recordCount = await cn.ExecuteScalarAsync<int>(sql, param);
        int pageCount = (int)(Math.Ceiling(recordCount / (pgSize == 0 ? 1 : pgSize)));

        DataPagination pagination = new()
        {
            ObjectType = typeof(EventRegistration).Name,
            PageSize = pgSize,
            PageCount = pageCount,
            RecordCount = (int)recordCount
        };

        return pagination;
    }
}