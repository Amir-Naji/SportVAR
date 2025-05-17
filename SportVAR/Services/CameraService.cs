using OpenCvSharp;
using SportVAR.Utilities;

namespace SportVAR.Services;

public class CameraService : ICameraService
{
    private VideoCapture _capture;
    private CancellationTokenSource _cts;
    private Action<Mat> _frameCallback;

    private int _width = 1280;
    public int _height { get; set; } = 720;
    public int _fps { get; set; } = 30;

    public void Start()
    {
        _capture = new VideoCapture(0, VideoCaptureAPIs.DSHOW)
                   {
                       FrameWidth = _width,
                       FrameHeight = _height,
                       Fps = _fps
                   };

        if (!_capture.IsOpened())
            throw new Exception("Cannot open camera.");

        _cts = new CancellationTokenSource();
        Task.Run(() => Loop(_cts.Token));
    }

    private void Loop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            using var frame = new Mat();
            _capture.Read(frame);
            if (!frame.Empty())
                _frameCallback?.Invoke(frame.Clone());
            Thread.Sleep(1000 / _fps);
        }
    }

    public void Stop()
    {
        if (_cts.IsNotNull())
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null!;
        }

        if (_capture.IsNull()) return;

        _capture.Release();
        _capture.Dispose();
        _capture = null!;

    }

    public void Dispose()
    {
        Stop();
        _cts?.Dispose();
    }

    public void SetFrameCallback(Action<Mat> callback)
    {
        _frameCallback = callback;
    }
}