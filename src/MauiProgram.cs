using Microsoft.Extensions.Logging;
#if ANDROID
using HtmlHub.Platforms.Android;
#endif

namespace HtmlHub;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>()
               .ConfigureFonts(f => f.AddFont("OpenSans-Regular.ttf", "OpenSansRegular"));

        builder.Services.AddSingleton<HtmlLibrary>();
        builder.Services.AddSingleton<AppShell>();
        builder.Services.AddTransient<MainPage>();

        // Настройки Android WebView, нужные для «настоящих» веб-приложений
        Microsoft.Maui.Handlers.WebViewHandler.Mapper.AppendToMapping("HtmlHub", (handler, view) =>
        {
#if ANDROID
            var s = handler.PlatformView.Settings;
            var downloadBridge = new BlobDownloadBridge();
            handler.PlatformView.SetWebChromeClient(new MultiFileWebChromeClient());
            handler.PlatformView.AddJavascriptInterface(downloadBridge, "HtmlHubDownload");
            handler.PlatformView.SetWebViewClient(new BlobDownloadWebViewClient(downloadBridge));
            s.JavaScriptEnabled = true;
            s.DomStorageEnabled = true;
            s.DatabaseEnabled = true;
            s.AllowFileAccess = true;
            s.AllowContentAccess = true;

            // Страница открыта как file://, её origin = null.
            // Эти флаги разрешают ей запросы к любым серверам (fetch/XHR) без блокировки CORS.
            s.AllowFileAccessFromFileURLs = true;
            s.AllowUniversalAccessFromFileURLs = true;

            s.MediaPlaybackRequiresUserGesture = false;
            s.MixedContentMode = Android.Webkit.MixedContentHandling.AlwaysAllow;

#if DEBUG
            // Отладка: в Chrome на ПК откройте chrome://inspect и смотрите консоль/сеть страницы
            Android.Webkit.WebView.SetWebContentsDebuggingEnabled(true);
#endif
#endif
        });

#if DEBUG
        builder.Logging.AddDebug();
#endif
        return builder.Build();
    }
}
