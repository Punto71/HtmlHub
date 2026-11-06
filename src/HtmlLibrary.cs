using System.Collections.ObjectModel;
using System.Text.Json;

namespace HtmlHub;

public class HtmlLibrary
{
    readonly string _dir = Path.Combine(FileSystem.AppDataDirectory, "apps");
    string IndexPath => Path.Combine(_dir, "index.json");

    public ObservableCollection<HtmlApp> Items { get; } = new();

    // Вызывается, когда пользователь выбрал приложение в меню
    public event Action<HtmlApp>? AppOpened;

    public HtmlLibrary()
    {
        Directory.CreateDirectory(_dir);
        if (File.Exists(IndexPath))
        {
            var list = JsonSerializer.Deserialize<List<HtmlApp>>(File.ReadAllText(IndexPath));
            foreach (var a in list ?? new()) Items.Add(a);
        }
    }

    public string GetPath(HtmlApp app) => Path.Combine(_dir, app.Id + ".html");

    public void Open(HtmlApp app) => AppOpened?.Invoke(app);

    public async Task<HtmlApp?> AddAsync()
    {
        var htmlType = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
        {
            { DevicePlatform.Android, new[] { "text/html" } }
        });

        var result = await FilePicker.Default.PickAsync(new PickOptions
        {
            PickerTitle = "Выберите HTML-файл",
            FileTypes = htmlType
        });
        if (result is null) return null;

        var app = new HtmlApp { Name = Path.GetFileNameWithoutExtension(result.FileName) };

        await using (var src = await result.OpenReadAsync())
        await using (var dst = File.Create(GetPath(app)))
            await src.CopyToAsync(dst);

        Items.Add(app);
        Save();
        return app;
    }

    public void Delete(HtmlApp app)
    {
        var path = GetPath(app);
        if (File.Exists(path)) File.Delete(path);
        Items.Remove(app);
        Save();
    }

    void Save() => File.WriteAllText(IndexPath, JsonSerializer.Serialize(Items.ToList()));
}
