using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Webkit;
using AndroidX.Core.View;
using Microsoft.Maui.Controls.PlatformConfiguration;

namespace HtmlHub;

[Activity(
    Theme = "@style/Maui.SplashTheme",
    MainLauncher = true,
    LaunchMode = LaunchMode.SingleTop,
    ConfigurationChanges =
        ConfigChanges.ScreenSize |
        ConfigChanges.Orientation |
        ConfigChanges.UiMode |
        ConfigChanges.ScreenLayout |
        ConfigChanges.SmallestScreenSize |
        ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    const int FileChooserRequestCode = 46217;
    IValueCallback? _fileChooserCallback;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        GoFullScreen();
    }

    public override void OnWindowFocusChanged(bool hasFocus)
    {
        base.OnWindowFocusChanged(hasFocus);
        if (hasFocus) GoFullScreen();
    }

    public bool OpenFileChooser(IValueCallback callback, WebChromeClient.FileChooserParams fileChooserParams)
    {
        _fileChooserCallback?.OnReceiveValue(null);
        _fileChooserCallback = callback;

        try
        {
            var intent = fileChooserParams.CreateIntent();
            intent.PutExtra(Intent.ExtraAllowMultiple, true);

            StartActivityForResult(intent, FileChooserRequestCode);
            return true;
        }
        catch
        {
            _fileChooserCallback = null;
            callback.OnReceiveValue(null);
            return true;
        }
    }

    protected override void OnActivityResult(int requestCode, Result resultCode, Intent? data)
    {
        if (requestCode == FileChooserRequestCode)
        {
            var callback = _fileChooserCallback;
            _fileChooserCallback = null;
            var uris = resultCode == Result.Ok && data is not null
                ? GetSelectedUris(data)
                : null;
            callback?.OnReceiveValue(uris is { Length: > 0 }
                ? Java.Lang.Object.FromArray(uris)
                : null);
            return;
        }

        base.OnActivityResult(requestCode, resultCode, data);
    }

    static Android.Net.Uri[]? GetSelectedUris(Intent data)
    {
        if (data.ClipData is { ItemCount: > 0 } clipData)
        {
            var uris = new List<Android.Net.Uri>(clipData.ItemCount);
            for (var i = 0; i < clipData.ItemCount; i++)
            {
                if (clipData.GetItemAt(i)?.Uri is { } uri)
                    uris.Add(uri);
            }

            if (uris.Count > 0)
                return uris.ToArray();
        }

        return data.Data is { } singleUri ? new[] { singleUri } : null;
    }

    void GoFullScreen()
    {
        if (Window is null) return;
        WindowCompat.SetDecorFitsSystemWindows(Window, false);
        var c = WindowCompat.GetInsetsController(Window, Window.DecorView);
        c?.SystemBarsBehavior = WindowInsetsControllerCompat.BehaviorShowTransientBarsBySwipe;
        c?.Hide(WindowInsetsCompat.Type.NavigationBars());
    }
}