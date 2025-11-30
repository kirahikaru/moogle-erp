using CurrieTechnologies.Razor.SweetAlert2;
using DataLayer.AuxComponents.Extensions;
using DataLayer.Models.SysCore.NonPersistent;
using DataLayer.Repos;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using MudBlazor;

namespace MoogleERP.Components;

public class CRUCPageBase<T> : ComponentBase
{
	[Inject]
	public required IJSRuntime JsRuntime { get; set; }

	[Inject]
	public required NavigationManager NavMngr { get; set; }

	[Inject]
	public required IUowMoogleKhErp Uow { get; set; }

	[Inject]
	public required IUowMoogleKhErpPg PostgresUow { get; set; }

	[Inject]
	public required SweetAlertService Swal { get; set; }

	[Inject]
	public required ISnackbar Snackbar { get; set; }

	[CascadingParameter(Name = "AuthUser")]
	public UserSessionInfo? LoggedInUser { get; set; }

	[CascadingParameter(Name = "SystemModulePermissions")]
	public List<SysModPerm> SystemModulePermissions { get; set; }

	protected AppModulePermission ModulePermission { get; set; }

	[Parameter]
	public int Id { get; set; }

	/// <summary>
	/// Create/Read/Update/Clone Mode
	/// </summary>
	[Parameter]
	public required string CRUCMode { get; set; }

	[Parameter]
	public bool IsViewMode { get; set; }

	protected T? CurrentObject { get; set; }
	protected EditContext? CurrentEditContext { get; set; }

	public string? ObjectDisplayName { get; set; }

	public string? HeaderTitle { get; set; }

	public string? UrlPrefix { get; set; }
	protected bool IsSaving { get; set; }
	public Dictionary<string, string> InvalidMsgList { get; set; }

	public string AuditTrailUser => LoggedInUser != null ? LoggedInUser.UserNameAndUserID : "Public";

	public CRUCPageBase()
	{
		SystemModulePermissions = [];
		InvalidMsgList = [];
		ModulePermission = new();
		ObjectDisplayName = typeof(T).GetDisplayName();
		HeaderTitle = ObjectDisplayName;
	}

	public virtual async ValueTask SaveHotKeyPressed() => await Save(false);

	public virtual async ValueTask SaveAndCloseHotKeyPressed() => await Save(true);

	public void SaveDummy()
	{
		return;
	}

	public void OnEditFormKeyPressed(KeyboardEventArgs args)
	{
		return;
	}

	protected virtual async Task Save(bool closeAfterSave = false)
	{
		//!IMPORTANT : This needs to be manually implemented Save & Close Button
		if (!CurrentEditContext!.Validate() || IsSaving)
			return;

		IsSaving = true;

		// run back-end validation function
		await ValidatePreSave();

		if (CheckAndDisplayPreSaveValidation())
		{
			IsSaving = false;
			return;
		}

		try
		{
			await PreSaveProcessing();
			bool isSaveSuccess = await SaveAndCommitProcessing();

			if (isSaveSuccess)
			{
				await PostSaveProcessing();
				Snackbar.Add(
					Id > 0 ? "Record has been successfully updated." : "Record has been successfully added",
					Severity.Success);

				if (closeAfterSave)
				{
					NavMngr.NavigateTo($"{UrlPrefix}/main");
				}
				else
				{
					StateHasChanged();
				}
			}
			else
				throw new Exception("Save And Commit processing failed");
		}
		catch (Exception ex)
		{
			await JsRuntime!.InvokeVoidAsync("console.error", ex.GetFullMessage());

			await Swal.FireAsync(new SweetAlertOptions
			{
				Title = "Error Encountered",
				Text = "System encountered error while trying to save please try again.",
				Icon = SweetAlertIcon.Error,
				ShowConfirmButton = true//,
										//Timer = 1500
			});
		}
		finally
		{
			IsSaving = false;
		}
	}

	protected virtual async Task ValidatePreSave()
	{
		InvalidMsgList.Clear();
		await Task.CompletedTask;
	}

	protected virtual async Task PreSaveProcessing()
	{
		await Task.CompletedTask;
	}

	protected virtual async Task<bool> SaveAndCommitProcessing()
	{
		await Task.CompletedTask;
		throw new NotImplementedException();
	}

	protected virtual async Task PostSaveProcessing()
	{
		await Task.CompletedTask;
	}

	protected virtual bool CheckAndDisplayPreSaveValidation()
	{
		// if there is error don't proceed and show message popup
		if (InvalidMsgList != null && InvalidMsgList.Count > 0)
		{
			Snackbar.Add(
					"Please refer to detailed validation to clear",
					Severity.Error);

			return true;
		}

		return false;
	}

	public async ValueTask CancelHotkeyPressed()
	{
		SweetAlertResult result = await Swal.FireAsync(new SweetAlertOptions
		{
			Title = "Exit Confirmation",
			Text = $"Are you sure you want to cancel current operation and return to main page? Cancellation mean that all your edition will not be saved.",
			Icon = SweetAlertIcon.Question,
			ShowCancelButton = true,
			ShowConfirmButton = true,
			ConfirmButtonText = "Yes, Confirmed",
			CancelButtonText = "Cancel"
		});

		if (!string.IsNullOrEmpty(result.Value))
		{
			Cancel();
		}
	}

	public virtual void Cancel()
	{
		NavMngr.NavigateTo($"{UrlPrefix}/main");
	}
}
