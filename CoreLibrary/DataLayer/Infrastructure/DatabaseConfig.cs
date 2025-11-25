

namespace DataLayer.Infrastructure;

public class DatabaseConfig
{
    //private string _key = "*pcla@1234!pcla@1234!pcla@1234!*";
    public string? Environment { get; set; }
    public string? DatabaseType { get; set; }
	/// <summary>
	/// Posgtresql: host address
	/// </summary>
	public string? ServerUrl { set; get; }
    public string? DatabaseName { set; get; }
    public string? UserName { set; get; }
    public string? Password { get; set; }
	public string? Port { get; set; }
	public bool? MultipleActiveResultSets { get; set; }
    public bool? PersistSecurityInfo { get; set; }
    public bool? Encrypt { get; set; }
    public bool? TrustServerCertificate { get; set; }

	public string? ConnectionString
    {
        get
        {
            if (string.IsNullOrEmpty(DatabaseType) || string.IsNullOrEmpty(ServerUrl))
                return "";

            if (DatabaseType.Is(DatabaseTypes.AZURE_SQL, DatabaseTypes.MSSQL) && string.IsNullOrEmpty(DatabaseName))
                return "";

            StringBuilder sbCxnStr = new();

            if (DatabaseType.Is(DatabaseTypes.AZURE_SQL, DatabaseTypes.MSSQL))
            {
                if (string.IsNullOrEmpty(UserName))
                {
                    sbCxnStr.Append($"Server={ServerUrl};Database={DatabaseName};Trusted_Connection=False;");
                }
                else
                {
                    string pwd = Password!; //CustomCipher.DecryptString(_key, Password!);
                    sbCxnStr.Append($"Server={ServerUrl};Database={DatabaseName};User ID={UserName};Password={pwd};");
                }

                if (Encrypt.HasValue && Encrypt.Value)
                    sbCxnStr.Append("Encrypt=True;");

                if (PersistSecurityInfo.HasValue && PersistSecurityInfo.Value)
                {
                    sbCxnStr.Append("Persist Security Info=true;");
                }

                if (TrustServerCertificate.HasValue)
                    sbCxnStr.Append(TrustServerCertificate!.Value ? "TrustServerCertificate=true;" : "TrustServerCertificate=false;");

                if (MultipleActiveResultSets.HasValue && MultipleActiveResultSets.Value)
                    sbCxnStr.Append("MultipleActiveResultSets=true;");
            }
            else if (DatabaseType == DatabaseTypes.POSTGRESQL)
            {
                if (!string.IsNullOrEmpty(UserName))
                {
					string pwd = Password!; //CustomCipher.DecryptString(_key, Password!);
                    sbCxnStr.Append($"Host={ServerUrl};Database={DatabaseName};Username={UserName};Password={pwd};");
                }
                else
                {
                    sbCxnStr.Append($"Host={ServerUrl};Database={DatabaseName};");
				}
			}
			else if (DatabaseType == DatabaseTypes.MONGODB)
            {
                sbCxnStr.Append($"mongodb+srv://{UserName}:{Password}@{ServerUrl}/");
            }
          
            return sbCxnStr.ToString();
        }
    }
}

