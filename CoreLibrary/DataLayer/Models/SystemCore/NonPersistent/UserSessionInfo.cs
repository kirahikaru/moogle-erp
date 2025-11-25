using DataLayer.AuxComponents.Extensions;
using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace DataLayer.Models.SystemCore.NonPersistent;

[JsonObject]
public class UserSessionInfo
{
    [JsonProperty("user_id")]
    [JsonPropertyName("user_id")]
    public int UserId { get; set; }

    [JsonProperty("user_user_id")]
    [JsonPropertyName("user_user_id")]
    public string? UserUserID { get; set; }

    [JsonProperty("user_type")]
    [JsonPropertyName("user_type")]
    public string? UserType { get; set; }

    [JsonProperty("username")]
    [JsonPropertyName("username")]
    public string? UserName { get; set; }

    [JsonProperty("employee_id")]
    [JsonPropertyName("employee_id")]
    public string? EmployeeId { get; set; }

    [JsonProperty("terminated_datetime")]
    [JsonPropertyName("terminated_datetime")]
    public DateTime? TerminatedDateTime { get; set; }

    [JsonProperty("enabled")]
    [JsonPropertyName("enabled")]
    public bool IsEnabled { get; set; }

    [JsonProperty("report_to_userid")]
    [JsonPropertyName("report_to_userid")]
    public int? ReportToUserId { get; set; }

    [JsonProperty("confidentiality_level")]
    [JsonPropertyName("confidentiality_level")]
    public int? ConfidentialityLevel { get; set; }

    [Newtonsoft.Json.JsonIgnore]
    public string UserNameAndUserID => $"{UserName.NonNullValue("-")} ({UserUserID.NonNullValue("-")})";
}