using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Media.Imaging;

namespace SportVAR.ViewModels;

public partial class ViewModel3 : ObservableObject, IDisposable
{
    [ObservableProperty] private BitmapSource? _cameraImage;

    private int _frameCount = 0;
    private CancellationTokenSource _recordingTokenSource;

    public ViewModel3()
    {
        _recordingTokenSource = new CancellationTokenSource();
        StartFakeCamera(_recordingTokenSource.Token);
    }

    private async void StartFakeCamera(CancellationToken token)
    {
        await Task.Run(() =>
                       {
                           while (!token.IsCancellationRequested)
                           {
                               _frameCount++;
                               var bmp = CreateTestImage($"Frame {_frameCount}");
                               bmp.Freeze();

                               App.Current.Dispatcher.Invoke(() => CameraImage = bmp);

                               Thread.Sleep(500);
                           }
                       });
    }

    private BitmapSource CreateTestImage(string text)
    {
        var bmp = new System.Drawing.Bitmap(320, 240);
        using var g = System.Drawing.Graphics.FromImage(bmp);
        g.Clear(System.Drawing.Color.DarkBlue);
        g.DrawString(text, new System.Drawing.Font("Arial", 24), System.Drawing.Brushes.White, new System.Drawing.PointF(10, 100));
        return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                                                                            bmp.GetHbitmap(), IntPtr.Zero, System.Windows.Int32Rect.Empty,
                                                                            BitmapSizeOptions.FromEmptyOptions());
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}