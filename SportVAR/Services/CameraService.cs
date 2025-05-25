using OpenCvSharp;
using SportVAR.Models;
using SportVAR.Utilities;

namespace SportVAR.Services;

public class CameraService: ICameraService
{
    private VideoCapture _capture;
    private CancellationTokenSource _cts;
    private Action<Mat> _frameCallback;
    private CameraDetail _cameraDetail;

    public void Start()
    {
        _capture = new VideoCapture(_cameraDetail.Index, VideoCaptureAPIs.DSHOW)
                   {
                       FrameWidth = _cameraDetail.Width,
                       FrameHeight = _cameraDetail.Height,
                       Fps = _cameraDetail.Fps
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
            Thread.Sleep(1000 / _cameraDetail.Fps);
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

    public void SetCameraDetails(CameraDetail cameraDetail)
    {
        _cameraDetail = cameraDetail;
    }
}