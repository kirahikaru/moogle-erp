using CurrieTechnologies.Razor.SweetAlert2;
using DataLayer.AuxComponents.Extensions;
using DataLayer.GlobalConstant;
using DataLayer.Models;
using DataLayer.Models.SystemCore.NonPersistent;
using DataLayer.Repos;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using MudBlazor;

namespace MoogleKhErp;

public class MainPageBase<T> : ComponentBase where T : class
{
	[Inject]
	public required SweetAlertService SwalSvc { get; set; }

	[Inject]
	public required IJSRuntime JsRuntime { get; set; }

	[Inject]
	public required NavigationManager NavMngr { get; set; }

	[Inject]
	public required IUowMoogleKhErp Uow { get; set; }

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
	public IEnumerable<T> MainDataList { get; set; }

	[CascadingParameter(Name = "AuthUser")]
	public UserSessionInfo? LoggedInUser { get; set; }

	public readonly string DataGridHdrStyle = "font-weight:700; background-color:var(--ssv-pharma-blue); color:#FFFFFF; border-right:1px solid #FFFFFF; padding-inline-start: 10px; padding-inline-end:5px; line-height:100%; padding-top:4px; padding-bottom:4px";
	public string AuditTrailUser => LoggedInUser != null ? LoggedInUser.UserNameAndUserID : "Public";

	public MainPageBase()
	{
		PageSize = 50;
		ObjectDisplayName = typeof(T).GetDisplayName();
		HeaderTitle = ObjectDisplayName;
		NavPathItems = [];
		InvalidMsgList = [];
		MainDataGrid = new MudDataGrid<T>();
		MainDataList = [];
	}

	public async Task OnSearchTextBoxKeyDown(KeyboardEventArgs e)
	{
		if (e == null || string.IsNullOrEmpty(e.Key))
			return;

		switch (e.Key.ToUpper())
		{
			case KeyboardKeys.ENTER:
				{
					await MainDataGrid!.ReloadServerData();
				}
				break;
			case KeyboardKeys.ESCAPE:
				{
					SearchText = "";
					await MainDataGrid!.ReloadServerData();
				}
				break;
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

	public void PerformCRUC(string crucMode, int objId = 0)
	{
		if (!CRUDCModes.IsValid(crucMode))
			throw new Exception("Invalid CRUC mode");

		if (objId < 0)
			throw new Exception("Invalid Object.Id provided");

		NavMngr.NavigateTo($"{UrlPrefix!}/{crucMode}/{objId}");
	}

	protected async Task DeleteRecord(T objToDelete)
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
					BaseRepos<T> baseRepos = new(Uow.Connection);
					int delCount = await baseRepos.DeleteAsync((objToDelete as AuditObject)!.Id, AuditTrailUser);

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
}
