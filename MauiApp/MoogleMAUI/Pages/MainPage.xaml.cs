using MoogleMAUI.Models;
using MoogleMAUI.PageModels;

namespace MoogleMAUI.Pages
{
	public partial class MainPage : ContentPage
	{
		public MainPage(MainPageModel model)
		{
			InitializeComponent();
			BindingContext = model;
		}
	}
}