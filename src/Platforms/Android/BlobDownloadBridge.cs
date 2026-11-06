using Android.Content;
using Android.OS;
using Android.Provider;
using Android.Util;
using Android.Webkit;
using Java.Interop;
using Microsoft.Maui.ApplicationModel;

namespace HtmlHub.Platforms.Android;

public sealed class BlobDownloadBridge : Java.Lang.Object
{
    const string Tag = "HtmlHubDownload";
    readonly Context _context = global::Android.App.Application.Context;

    [JavascriptInterface]
    [Export("SaveFile")]
    public void SaveFile(string fileName, string mimeType, string base64)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            try
            {
                var safeName = Path.GetFileName(fileName.Replace('\\', '/'));
                if (string.IsNullOrWhiteSpace(safeName))
                    safeName = "download";

                var bytes = Convert.FromBase64String(base64);
                if (Build.VERSION.SdkInt >= BuildVersionCodes.Q)
                    SaveToMediaStore(safeName, mimeType, bytes);
                else
                    await SaveToLegacyDownloadsAsync(safeName, bytes);

                global::Android.Widget.Toast.MakeText(_context, "Файл сохранен в Downloads", global::Android.Widget.ToastLength.Short)?.Show();
            }
            catch (Exception ex)
            {
                Log.Error(Tag, ex.ToString());
                global::Android.Widget.Toast.MakeText(_context, "Не удалось сохранить файл", global::Android.Widget.ToastLength.Short)?.Show();
            }
        });
    }

    void SaveToMediaStore(string fileName, string mimeType, byte[] bytes)
    {
        var resolver = _context.ContentResolver!;
        var values = new ContentValues();
        values.Put(MediaStore.IMediaColumns.DisplayName, fileName);
        values.Put(MediaStore.IMediaColumns.MimeType, string.IsNullOrWhiteSpace(mimeType) ? "application/octet-stream" : mimeType);
        values.Put(MediaStore.IMediaColumns.RelativePath, global::Android.OS.Environment.DirectoryDownloads);
        values.Put(MediaStore.IMediaColumns.IsPending, 1);

        var uri = resolver.Insert(MediaStore.Downloads.ExternalContentUri, values)
            ?? throw new IOException("Could not create a Downloads entry.");

        try
        {
            using (var stream = resolver.OpenOutputStream(uri) ?? throw new IOException("Could not open the Downloads entry."))
                stream.Write(bytes, 0, bytes.Length);

            values.Clear();
            values.Put(MediaStore.IMediaColumns.IsPending, 0);
            resolver.Update(uri, values, null, null);
        }
        catch
        {
            resolver.Delete(uri, null, null);
            throw;
        }
    }

    static async Task SaveToLegacyDownloadsAsync(string fileName, byte[] bytes)
    {
        var permission = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();
        if (permission != PermissionStatus.Granted)
            permission = await Permissions.RequestAsync<Permissions.StorageWrite>();
        if (permission != PermissionStatus.Granted)
            throw new UnauthorizedAccessException("Storage permission was not granted.");

#pragma warning disable CA1422
        var downloads = global::Android.OS.Environment.GetExternalStoragePublicDirectory(global::Android.OS.Environment.DirectoryDownloads)!.AbsolutePath;
#pragma warning restore CA1422
        Directory.CreateDirectory(downloads);

        var path = Path.Combine(downloads, fileName);
        if (File.Exists(path))
        {
            var name = Path.GetFileNameWithoutExtension(fileName);
            var extension = Path.GetExtension(fileName);
            var index = 1;
            do
            {
                path = Path.Combine(downloads, $"{name} ({index++}){extension}");
            } while (File.Exists(path));
        }

        await File.WriteAllBytesAsync(path, bytes);
    }
}
