using CommunityToolkit.Mvvm.Input;
using MoogleMAUI.Models;

namespace MoogleMAUI.PageModels
{
	public interface IProjectTaskPageModel
	{
		IAsyncRelayCommand<ProjectTask> NavigateToTaskCommand { get; }
		bool IsBusy { get; }
	}
}