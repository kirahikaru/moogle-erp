using MoogleMAUI.Models;

namespace MoogleMAUI.Pages
{
	public partial class ProjectDetailPage : ContentPage
	{
		public ProjectDetailPage(ProjectDetailPageModel model)
		{
			InitializeComponent();

			BindingContext = model;
		}
	}
}
