namespace DomainSearch;

public partial class MainPage
{
	public MainPage()
	{
		BindingContext = new ViewModel();
		InitializeComponent();
	}
}

