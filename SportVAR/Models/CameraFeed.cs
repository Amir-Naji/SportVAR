using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;
using AForge.Video;
using AForge.Video.DirectShow;

namespace SportVAR.Models;

public class CameraFeed
{
    private readonly VideoCaptureDevice _device;

    public CameraFeed(string monikerString)
    {
        _device = new VideoCaptureDevice(monikerString);
        _device.NewFrame += OnNewFrame;
    }

    public event Action<BitmapImage> FrameReady;

    private void OnNewFrame(object sender, NewFrameEventArgs e)
    {
        using var bitmap = (Bitmap)e.Frame.Clone();
        FrameReady?.Invoke(ConvertToBitmapImage(bitmap));
    }

    public void Start()
    {
        _device.Start();
    }

    public void Stop()
    {
        if (!_device.IsRunning) return;

        _device.SignalToStop();
        _device.WaitForStop();
    }

    private BitmapImage ConvertToBitmapImage(Bitmap bitmap)
    {
        using var memory = new MemoryStream();
        bitmap.Save(memory, ImageFormat.Bmp);
        memory.Position = 0;
        var image = new BitmapImage();
        image.BeginInit();
        image.StreamSource = memory;
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.EndInit();
        image.Freeze();
        return image;
    }
}