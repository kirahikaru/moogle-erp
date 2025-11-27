using DataLayer.AuxComponents.Extensions;
using DataLayer.GlobalConstant;
using DataLayer.Models;
using DataLayer.Models.SysCore.NonPersistent;
using DataLayer.Repos;
using Microsoft.AspNetCore.Components;
//using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using MudBlazor;
using Toolbelt.Blazor.HotKeys2;

namespace PruTech_ITSM_CMDB.Client;

public class MainPageBase<T> : ComponentBase, IAsyncDisposable
{
    [Inject]
	public required IJSRuntime JsRuntime { get; set; }

	//[Inject]
	//public required ProtectedSessionStorage SessionStorage { get; set; }

	[Inject]
	public required NavigationManager NavManager { get; set; }

	[Inject]
	public required HotKeys HotKeys { get; set; }

	[Inject]
	public required IUowPruIT Uow { get; set; }

	[CascadingParameter(Name = "AuthUser")]
	public UserSessionInfo? LoggedInUser { get; set; }

	public string ObjectDisplayName { get; set; }

	public string? HeaderTitle { get; set; }

	public SysModPerm CurrentSysModPerm { get; set; }
	public AppModulePermission CurrentAppModPerm { get; set; }
	public HotKeysContext? CurrentHotKeyContext { get; set; }

	
	public IEnumerable<T> MainDataList { get; set; }
	public T? SelectedObject { get; set; }
	public IList<T> SelectedObjects { get; set; }
	public int DataCount { get; set; }
	public int SelectedRowNo { get; set; }
	public bool IsTblDense { get; set; }
	public bool HasTblRowHoverEffect { get; set; }
	public bool ShowTblRowStripe { get; set; }
	public bool ShowTblBorder { get; set; }
    public string DataTableHeight { get; set; }

    public int PageSize { get; set; }
	//public int CurrentPage { get; set; }

	public string? SearchText { get; set; }
	public bool IsSearching { get; set; }
	public bool IsAdvSearch { get; set; }
	public bool IsAdvSearchPanelOpen { get; set; }
	public MudDataGrid<T> MainDataGrid { get; set; }
	public readonly string DataGridHdrStyle = "font-weight:700; background-color:var(--pru-gray-default); color:#FFFFFF; border-right:1px solid #FFFFFF; padding-inline-start: 10px; padding-inline-end:5px; line-height:100%; padding-top:4px; padding-bottom:4px";

	public string? SearchParamName { get; set; }
	public string? UrlPrefix { get; set; }

	public string AuditTrailUser => LoggedInUser != null ? LoggedInUser.UserNameAndUserID : "Public";
	public MudTextField<string>? UITextBoxSearch { get; set; }

	public MainPageBase()
	{
		CurrentAppModPerm = new();
		CurrentSysModPerm = new();
		SelectedRowNo = -1;
		ObjectDisplayName = typeof(T).GetDisplayName();
		HeaderTitle = ObjectDisplayName;
		IsTblDense = false;
		HasTblRowHoverEffect = true;
		PageSize = 50;
		ShowTblRowStripe = true;
		ShowTblBorder = false;
		DataTableHeight = "70vh";
		MainDataGrid = new MudDataGrid<T>();
		MainDataList = [];
		SelectedObjects = [];
	}

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (firstRender)
		{
			await UITextBoxSearch!.FocusAsync();

			CurrentHotKeyContext ??= HotKeys.CreateContext()
				.Add(ModCode.Alt, Code.N, CreateRecord, new() { Exclude = Exclude.None })
				.Add(ModCode.Ctrl, Code.E, EditHotKeyPressed, new() { Exclude = Exclude.None });
		}

		await base.OnAfterRenderAsync(firstRender);
	}

	public virtual async Task OnSearchBoxKeyDown(KeyboardEventArgs args)
	{
		if (IsSearching) return;

		switch (args.Key)
		{
			case "Enter":
				await UITextBoxSearch!.FocusAsync();
				await MainDataGrid!.ReloadServerData();
				break;
			case "Escape":
				await UITextBoxSearch!.Clear();
				await UITextBoxSearch!.FocusAsync();
				await MainDataGrid!.ReloadServerData();
				break;
			default:
				return;
		}
	}

	public virtual async ValueTask SearchHotKeyPressed()
	{
		SearchText = "";
		await JsRuntime!.InvokeVoidAsync("controlFocus.focusElement", "searchBox");
	}

	protected string SelectedRowClassFunc(T element, int rowNumber)
	{
		if (SelectedRowNo == rowNumber)
		{
			SelectedRowNo = -1;
			return string.Empty;
		}
		else if (MainDataGrid.SelectedItem != null && MainDataGrid.SelectedItem.Equals(element))
		{
			SelectedRowNo = rowNumber;
			return "pru-mud-datagrid-row selected";
		}
		else
		{
			return string.Empty;
		}
	}

	protected virtual async Task<GridData<T>> ServerDataFunc(GridState<T> gridState)
	{
		try
		{
			IsSearching = true;

			//if (gridState.SortDefinitions.Count > 0)
			//{
			//	var firstSort = gridState.SortDefinitions.First();
			//	result = firstSort.Descending
			//		? result.OrderByDescending(firstSort.SortFunc).ToList()
			//		: result.OrderBy(firstSort.SortFunc).ToList();
			//}

			if (gridState.FilterDefinitions.Any())
			{
				//var filterFunctions = gridState.FilterDefinitions.Select(x => x.GenerateFilterFunction());
				//result = result
				//	.Where(x => filterFunctions.All(f => f(x)))
				//	.ToList();
			}

			DataPagination pagination = new();

			//var totalNumberOfFilteredItems = result.Count;

			//result = result
			//	.Skip(gridState.StartIndex)
			//	.Take(gridState.Count)
			//	.ToList();
			await Task.CompletedTask;
			return new GridData<T>
			{
				Items = MainDataList,
				TotalItems = pagination.RecordCount
			};
		}
		catch (TaskCanceledException)
		{
			return new GridData<T>
			{
				Items = [],
				TotalItems = 0
			};
		}
		finally
		{
			IsSearching = false;
		}
	}

	protected virtual async Task<GridData<T>> VirtualizedServerDataFunc(GridStateVirtualize<T> gridState, CancellationToken token)
	{
		try
		{
			IsSearching = true;
			
			await Task.Delay(1000, token);

			//if (gridState.SortDefinitions.Count > 0)
			//{
			//	var firstSort = gridState.SortDefinitions.First();
			//	result = firstSort.Descending
			//		? result.OrderByDescending(firstSort.SortFunc).ToList()
			//		: result.OrderBy(firstSort.SortFunc).ToList();
			//}

			if (gridState.FilterDefinitions.Any())
			{
				//var filterFunctions = gridState.FilterDefinitions.Select(x => x.GenerateFilterFunction());
				//result = result
				//	.Where(x => filterFunctions.All(f => f(x)))
				//	.ToList();
			}

			DataPagination pagination = new();

			//var totalNumberOfFilteredItems = result.Count;

			//result = result
			//	.Skip(gridState.StartIndex)
			//	.Take(gridState.Count)
			//	.ToList();

			return new GridData<T>
			{
				Items = MainDataList,
				TotalItems = pagination.RecordCount
			};
		}
		catch (TaskCanceledException)
		{
			return new GridData<T>
			{
				Items = [],
				TotalItems = 0
			};
		}
		finally
		{
			IsSearching = false;
		}
	}

	public virtual async Task OnSearchClicked()
	{
		await UITextBoxSearch!.Clear();
		await UITextBoxSearch!.FocusAsync();
		await MainDataGrid!.ReloadServerData();
	}

	public async Task OnSearchTextBoxKeyDown(KeyboardEventArgs e)
	{
		if (e == null || string.IsNullOrEmpty(e.Key))
			return;

		switch (e.Key.ToUpper())
		{
			case KeyboardKeys.ENTER:
				{
					IsAdvSearch = false;
					//await DataTable!.ReloadServerData();
				}
				break;
			case KeyboardKeys.ESCAPE:
				{
					SearchText = "";
					//RtbSearchTextBox.Value = SearchText;
					IsAdvSearch = false;
					//await DataTable!.ReloadServerData();
				}
				break;
		}

		await Task.CompletedTask;
	}

	public async ValueTask OnAdvSearchPanelHotKeyPressed() => await OnAdvancedSearchPanelBtnClicked();

	public virtual async Task OnAdvancedSearchPanelBtnClicked()
	{
		IsAdvSearchPanelOpen = true;

		try
		{
			await JsRuntime!.InvokeVoidAsync("controlFocus.focusElement", "filterObjectCode");
		}
		catch { }
	}

	public virtual async Task OnAdvanceSearchClicked()
	{
		CheckAdvancedSearch();
		//await DataTable!.ReloadServerData();
		IsAdvSearchPanelOpen = false;
		await Task.CompletedTask;
	}

	public virtual void CheckAdvancedSearch()
	{
		IsAdvSearch = false;
	}

	public async ValueTask ClearAdvancedSearchFilterHotKeyPressed() => await ClearAdvancedSearchFilter();

	public virtual async Task ClearAdvancedSearchFilter()
	{
		IsAdvSearch = false;
		SearchText = "";
		//RtbSearchTextBox.Value = SearchText;
		ClearAdvancedSearchFilterValues();
		//await DataTable!.ReloadServerData();
		await Task.CompletedTask;
	}

	public virtual void ClearAdvancedSearchFilterValues()
	{
		throw new NotImplementedException("Please implement clear advance search filter values.");
	}

	//begin::MudBlazor Table Function
	public void OnRowClickDummy(MouseEventArgs args)
	{

	}

	public virtual async Task ViewFullRecord(int objId)
	{
		// Save quick search parameter before leaving to edit page
		if (!string.IsNullOrEmpty(SearchText) || MainDataGrid!.CurrentPage > 0)
		{
			//QuickSearchParam quickSearchParam = new(SearchText.NonNullValue(), MainDataGrid!.RowsPerPage, MainDataGrid.CurrentPage);
			//await SessionStorage!.SetAsync(SearchParamName!, JsonConvert.SerializeObject(quickSearchParam, Formatting.Indented));
		}

		//NavManager!.NavigateTo($"{UrlPrefix!}/edit/{objId}/{true}");
		NavManager!.NavigateTo($"{UrlPrefix!}/cruc/read/{objId}");
		await Task.CompletedTask;
	}

	public async ValueTask CreateRecord() => await CreateOrEditRecord(0);

	public virtual void CloneAndEditRecord(int objId)
	{
		NavManager!.NavigateTo($"{UrlPrefix!}/cruc/clone/{objId}");
	}

	public virtual async ValueTask EditHotKeyPressed()
	{
		if (SelectedObject != null && SelectedObject is AuditObject && (SelectedObject as AuditObject)!.Id > 0)
		{
			await CreateOrEditRecord((SelectedObject as AuditObject)!.Id);
		}
	}

	public virtual async Task CreateOrEditRecord(int objId)
	{
		// Save quick search parameter before leaving to edit page
		if (!string.IsNullOrEmpty(SearchText) || MainDataGrid!.CurrentPage > 0)
		{
			//QuickSearchParam quickSearchParam = new(SearchText.NonNullValue(), MainDataGrid!.RowsPerPage, MainDataGrid!.CurrentPage);
			//await SessionStorage!.SetAsync(SearchParamName!, JsonConvert.SerializeObject(quickSearchParam, Formatting.Indented));
		}

		if (objId > 0)
			NavManager!.NavigateTo($"{UrlPrefix!}/cruc/update/{objId}");
		else
			NavManager!.NavigateTo($"{UrlPrefix!}/cruc/create/{objId}");
		await Task.CompletedTask;
	}

	public async Task LoadSavedSearchFilters()
	{
		await Task.CompletedTask;
		if (string.IsNullOrEmpty(SearchParamName))
			return;

		// *** LOAD SAVED SEARCH PARAM ***
		//=========================================================================
		//var result = await SessionStorage!.GetAsync<string>(SearchParamName);

		//if (result.Success && !string.IsNullOrEmpty(result.Value))
		//{
		//	QuickSearchParam? quickSearchParam = JsonConvert.DeserializeObject<QuickSearchParam>(result.Value!);

		//	if (quickSearchParam != null)
		//	{
		//		SearchText = quickSearchParam.SearchText.NonNullValue();
		//		//RtbSearchTextBox!.Value = SearchText;
		//		//DataTable!.SetRowsPerPage(quickSearchParam.PageSize);
		//		//DataTable!.CurrentPage = quickSearchParam.PageNo;

		//		//await SessionStorage.DeleteAsync(SearchParamName);
		//		// await _dataTable!.ReloadServerData();
		//	}
		//}
	}

	#region DISPOSE AREA
	public async ValueTask DisposeAsync() // 👈 Add "DisposeAsync" method.
	{
		if (CurrentHotKeyContext != null)
		{
			await CurrentHotKeyContext.DisposeAsync();
		}
	}
	#endregion
}
