//using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components.Forms;
//using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using DataLayer.AuxComponents.Extensions;
using CurrieTechnologies.Razor.SweetAlert2;
using MudBlazor;
using Toolbelt.Blazor.HotKeys2;
using DataLayer.Repos;
using DataLayer.Models.SysCore.NonPersistent;
using PruIT_CMDB_ITSM.Client.Components;

namespace PruIT_CMDB_ITSM.Client.Pages;

public class CRUCPageBase<T> : ComponentBase, IAsyncDisposable
{
    [Inject]
	public required IJSRuntime JsRuntime { get; set; }

	//[Inject]
	//public required ProtectedSessionStorage SessionStorage { get; set; }

	[Inject]
	public required NavigationManager NavManager { get; set; }

	[Inject]
	public required ISnackbar Snackbar { get; set; }

	//[Inject]
	//public required NotificationService RadzenNotifSvc { get; set; }

	//[Inject]
	//public required DialogService RadzenDiagSvc { get; set; }

	[Inject]
	public required SweetAlertService Swal { get; set; }

	[Inject]
	public required IUowPruIT Uow { get; set; }

	[Inject]
	public required HotKeys HotKeys { get; set; }

	[CascadingParameter(Name = "AuthUser")]
	public UserSessionInfo? LoggedInUser { get; set; }

	[CascadingParameter(Name = "SystemModulePermissions")]
	public List<SysModPerm> SystemModulePermissions { get; set; }

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

	protected MudForm? CrucForm { get; set; }
	protected bool IsValidationPassed { get; set; }
	protected string[] MudValidationErrors { get; set; }

	public string ObjectDisplayName { get; set; }

	public string? HeaderTitle { get; set; }

	protected AppModulePermission ModulePermission { get; set; }
	protected HotKeysContext? CurrentHotKeyContext { get; set; }
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
		MudValidationErrors = [];
	}

    public virtual async ValueTask SaveHotKeyPressed() => await Save(false);

    public virtual async ValueTask SaveAndCloseHotKeyPressed() => await Save(true);

    public void SaveDummy()
	{
		return;
	}

	protected virtual async Task Save(bool closeAfterSave = false)
	{
		//!IMPORTANT : This needs to be manually implemented Save & Close Button
		if (!CurrentEditContext!.Validate() || IsSaving)
			return;

		if (CrucForm != null)
		{
			//await CrucForm!.ResetAsync();
			await CrucForm!.Validate();

			if (!IsValidationPassed)
				return;
		}
		
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
					NavManager.NavigateTo($"{UrlPrefix}/main");
				}
				else
				{
					NavManager.NavigateTo($"{UrlPrefix}/cruc/{CRUCModes.UPDATE}/{Id}", forceLoad: true);
					//StateHasChanged();
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

    public void OnEditFormKeyPressed(KeyboardEventArgs args)
    {
        return;
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
        NavManager.NavigateTo($"{UrlPrefix}/main");
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
