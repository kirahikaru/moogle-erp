using CurrieTechnologies.Razor.SweetAlert2;
using DataLayer.AuxComponents.Extensions;
using DataLayer.Models;
using DataLayer.Models.SysCore.NonPersistent;
using DataLayer.Repos;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using MudBlazor;
using Toolbelt.Blazor.HotKeys2;

namespace MoogleKhErp.Client.Pages;

public class MainPageBase<T> : ComponentBase, IAsyncDisposable where T : class
{
	[Inject]
	public required SweetAlertService SwalSvc { get; set; }

	[Inject]
	public required IJSRuntime JsRuntime { get; set; }

	[Inject]
	public required NavigationManager NavMngr { get; set; }

	[Inject]
	public required IUowMoogleKhErp Uow { get; set; }

	[Inject]
	public required IUowMoogleKhErpPg PostgresUow { get; set; }

	[Inject]
	public required HotKeys HotKeys { get; set; }

	public string? SearchText { get; set; }
	public string? SearchParamName { get; set; }
	public bool IsSearching { get; set; }
	public required string HeaderTitle { get; set; }
	public required string ObjectDisplayName { get; set; }

	public string? UrlPrefix { get; set; }
	public List<BreadcrumbItem> NavPathItems { get; set; }
	public Dictionary<string, string> InvalidMsgList { get; set; }

	public int PageSize { get; set; }
	public MudDataGrid<T> MainDataGrid { get; set; }
	public T? SelectedObject { get; set; }
	public IList<T> SelectedObjects { get; set; }
	public IEnumerable<T> MainDataList { get; set; }
	public MudTextField<string>? UITextBoxSearch { get; set; }
	protected HotKeysContext? CurrentHotKeyContext { get; set; }

	[CascadingParameter(Name = "AuthUser")]
	public UserSessionInfo? LoggedInUser { get; set; }

	public int SelectedRowNo { get; set; }

	public string AuditTrailUser => LoggedInUser != null ? LoggedInUser.UserNameAndUserID : "Public";

	public MainPageBase()
	{
		PageSize = 50;
		SelectedRowNo = -1;
		ObjectDisplayName = typeof(T).GetDisplayName();
		HeaderTitle = ObjectDisplayName;
		NavPathItems = [];
		InvalidMsgList = [];
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

	public virtual async Task OnSearchClicked()
	{
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
					await UITextBoxSearch!.FocusAsync();
					await MainDataGrid!.ReloadServerData();
				}
				break;
			case KeyboardKeys.ESCAPE:
				{
					//SearchText = "";
					await UITextBoxSearch!.Clear();
					//StateHasChanged();
					await UITextBoxSearch!.FocusAsync();
					await MainDataGrid!.ReloadServerData();
				}
				break;
		}
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
		if (IsSearching) return new GridData<T>
		{
			Items = [],
			TotalItems = 0
		};

		try
		{
			return new GridData<T>
			{
				Items = [],
				TotalItems = 0
			};
		}
		catch (Exception ex)
		{
			await JsRuntime.InvokeVoidAsync("console.error", ex.GetFullMessage());

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

	public async ValueTask CreateRecord()
	{
		await Task.Delay(100); // Give some time for UI to settle before navigation
		PerformCRUC(CRUDCModes.CREATE);
	}

	public virtual async ValueTask EditHotKeyPressed()
	{
		await Task.Delay(100); // Give some time for UI to settle before navigation
		if (SelectedObject != null && SelectedObject is AuditObject && (SelectedObject as AuditObject)!.Id > 0)
		{
			PerformCRUC(CRUDCModes.UPDATE, (SelectedObject as AuditObject)!.Id);
		}
	}

	public void PerformCRUC(string crucMode, int objId = 0)
	{
		if (!CRUDCModes.IsValid(crucMode))
			throw new Exception("Invalid CRUC mode");

		if (objId < 0)
			throw new Exception("Invalid Object.Id provided");

		NavMngr.NavigateTo($"{UrlPrefix!}/{crucMode}/{objId}");
	}

	protected async Task DeleteRecord(AuditObject objToDelete)
	{
		if (objToDelete == null)
		{
			await SwalSvc.FireAsync(new SweetAlertOptions
			{
				Title = "Object not selected",
				Text = "No object selected to be deleted. Please try again",
				Icon = SweetAlertIcon.Warning,
				ShowConfirmButton = false,
				Timer = 1500
			});
		}
		else
		{
			SweetAlertResult result = await SwalSvc.FireAsync(new SweetAlertOptions
			{
				Title = "Delete Confiramtion",
				Text = $"Are you sure you want to delete the following '{(objToDelete as AuditObject)!.ObjectName} ({(objToDelete as AuditObject)!.ObjectCode})' record?",

				ShowCancelButton = true,
				ShowConfirmButton = true,
				ConfirmButtonText = "Yes, Confirmed",
				CancelButtonText = "Cancel"
			});

			if (!string.IsNullOrEmpty(result.Value))
			{
				try
				{
					throw new NotImplementedException();
					//BaseRepos<AuditObject> baseRepos = new(Uow.Connection, new DatabaseObj((T as AuditObject)));

					//int delCount = await baseRepos.DeleteAsync(objToDelete.Id, AuditTrailUser);
					int delCount = 0;
					if (delCount > 0)
					{
						await MainDataGrid!.ReloadServerData();
						StateHasChanged();
						await SwalSvc.FireAsync(new SweetAlertOptions
						{
							Title = "Success",
							Text = "Record successfully deleted",
							Icon = SweetAlertIcon.Success,
							ShowConfirmButton = false,
							Timer = 1500
						});
					}
					else
					{
						await SwalSvc.FireAsync(new SweetAlertOptions
						{
							Title = "Failure",
							Text = "Application failed to form deletion of the record. The record may have been no longer available in system database.",
							Icon = SweetAlertIcon.Warning,
							ShowConfirmButton = true,
							ConfirmButtonText = "OK"
						});
					}
				}
				catch
				{
					await SwalSvc.FireAsync(new SweetAlertOptions
					{
						Title = "Error",
						Text = "Application encountered error while trying to process the operation. Please try again. If the problem still persists, please contact your technical support.",
						Icon = SweetAlertIcon.Error,
						ShowConfirmButton = true,
						ConfirmButtonText = "OK"
					});
				}
			}
		}
	}

	public async ValueTask DisposeAsync() // 👈 Add "DisposeAsync" method.
	{
		if (CurrentHotKeyContext != null)
		{
			await CurrentHotKeyContext.DisposeAsync();
		}
	}
}
