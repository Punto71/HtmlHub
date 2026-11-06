using Android.Graphics;
using Android.Webkit;

namespace HtmlHub.Platforms.Android;

public sealed class BlobDownloadWebViewClient(BlobDownloadBridge bridge) : WebViewClient
{
    const string BridgeName = "HtmlHubDownload";
    const string Script = """
        (() => {
            if (window.__htmlHubBlobDownloadInstalled) return;
            window.__htmlHubBlobDownloadInstalled = true;
            document.addEventListener('click', event => {
                let target = event.target;
                if (target && target.nodeType !== 1) target = target.parentElement;
                const link = target && target.closest ? target.closest('a[download]') : null;
                if (!link || !link.href.startsWith('blob:')) return;
                event.preventDefault();
                event.stopImmediatePropagation();
                fetch(link.href)
                    .then(response => response.blob())
                    .then(blob => {
                        const reader = new FileReader();
                        reader.onloadend = () => {
                            const base64 = String(reader.result).split(',', 2)[1];
                            window.HtmlHubDownload.SaveFile(
                                link.download || 'download',
                                blob.type || 'application/octet-stream',
                                base64);
                        };
                        reader.readAsDataURL(blob);
                    })
                    .catch(error => console.error('Download failed', error));
            }, true);
        })();
        """;

    public override void OnPageStarted(global::Android.Webkit.WebView? view, string? url, Bitmap? favicon)
    {
        if (view is not null)
        {
            if (IsLocalFile(url))
                view.AddJavascriptInterface(bridge, BridgeName);
            else
                view.RemoveJavascriptInterface(BridgeName);
        }

        base.OnPageStarted(view, url, favicon);
    }

    public override void OnPageFinished(global::Android.Webkit.WebView? view, string? url)
    {
        base.OnPageFinished(view, url);

        if (view is not null && IsLocalFile(url))
            view.EvaluateJavascript(Script, null);
    }

    static bool IsLocalFile(string? url) =>
        Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
        uri.Scheme.Equals("file", StringComparison.OrdinalIgnoreCase);
}
