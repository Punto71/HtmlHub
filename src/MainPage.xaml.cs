namespace HtmlHub;

public partial class MainPage : ContentPage
{
    readonly HtmlLibrary _lib;

    public MainPage(HtmlLibrary lib)
    {
        InitializeComponent();
        _lib = lib;
        _lib.AppOpened += OnAppOpened;
    }
    void OnEdgeSwiped(object? sender, SwipedEventArgs e)
    {
        Shell.Current.FlyoutIsPresented = true;
    }

    async void OnAppOpened(HtmlApp app)
    {
        Placeholder.IsVisible = false;
        Web.Source = new UrlWebViewSource { Url = "file://" + _lib.GetPath(app) };
        Web.IsVisible = true;

    }

    void OnMenu(object? sender, EventArgs e) => Shell.Current.FlyoutIsPresented = true;

    protected override bool OnBackButtonPressed()
    {
        if (Shell.Current.FlyoutIsPresented)
        {
            Shell.Current.FlyoutIsPresented = false;
            return true;
        }
        if (Web.IsVisible && Web.CanGoBack)
        {
            Web.GoBack();
            return true;
        }
        return base.OnBackButtonPressed();
    }
}
