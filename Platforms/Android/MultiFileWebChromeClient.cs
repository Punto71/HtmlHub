using Android.Webkit;

namespace HtmlHub.Platforms.Android;

public sealed class MultiFileWebChromeClient : WebChromeClient
{
    public override bool OnShowFileChooser(global::Android.Webkit.WebView? webView, IValueCallback? filePathCallback, FileChooserParams? fileChooserParams)
    {
        if (filePathCallback is null || fileChooserParams is null)
            return false;

        if (Platform.CurrentActivity is MainActivity activity)
            return activity.OpenFileChooser(filePathCallback, fileChooserParams);

        filePathCallback.OnReceiveValue(null);
        return true;
    }
}
