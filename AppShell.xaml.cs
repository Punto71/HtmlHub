namespace HtmlHub;

public partial class AppShell : Shell
{
    readonly HtmlLibrary _lib;

    public AppShell(HtmlLibrary lib)
    {
        InitializeComponent();
        _lib = lib;
        List.ItemsSource = _lib.Items;
    }

    void OnSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is HtmlApp app)
        {
            FlyoutIsPresented = false;
            _lib.Open(app);
            List.SelectedItem = null;
        }
    }

    async void OnAdd(object? sender, EventArgs e)
    {
        var app = await _lib.AddAsync();
        if (app is not null)
        {
            FlyoutIsPresented = false;
            _lib.Open(app);
        }
    }

    async void OnDelete(object? sender, EventArgs e)
    {
        if (sender is Button { BindingContext: HtmlApp app } &&
            await DisplayAlert("Удалить", $"Удалить «{app.Name}»?", "Да", "Нет"))
        {
            _lib.Delete(app);
        }
    }
}
