using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace PruTech_ITSM_CMDB.Client.NonPersistentObjs;

[JsonObject]
public class QuickSearchParam
{
	[JsonProperty("search_text")]
	[JsonPropertyName("search_text")]
	public string? SearchText { get; set; }

	[JsonProperty("page_size")]
	[JsonPropertyName("page_size")]
	public int PageSize { get; set; }

	[JsonProperty("page_no")]
	[JsonPropertyName("page_no")]
	public int PageNo { get; set; }

	public QuickSearchParam(string searchText, int pgSize, int pgNo)
	{
		SearchText = searchText;
		PageSize = pgSize;
		PageNo = pgNo;
	}
}