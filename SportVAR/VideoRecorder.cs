using OpenCvSharp;
using SportVAR.Models;
using SportVAR.Utilities;

namespace SportVAR;

public class VideoRecorder : IVideoRecorder
{
    private Size _frameSize;
    private string _outputPath;
    private CameraDetail _cameraDetail;
    private VideoWriter _writer;

    public VideoRecorder(int width, int height, int fps = 30)
    {
        
    }

    public void Start()
    {
        _writer = new VideoWriter(_outputPath, FourCC.XVID, _cameraDetail.Fps, _frameSize);
        if (!_writer.IsOpened())
            throw new Exception("Failed to open VideoWriter.");
    }

    public void Start(CameraDetail cameraDetail)
    {
        SetCameraDetails(cameraDetail);

        _writer = new VideoWriter(_outputPath, FourCC.XVID, _cameraDetail.Fps, _frameSize);
        if (!_writer.IsOpened())
            throw new Exception("Failed to open VideoWriter.");
    }

    private void SetCameraDetails(CameraDetail cameraDetail)
    {
        _cameraDetail = cameraDetail;
        _frameSize = new Size(cameraDetail.Width, cameraDetail.Height);
        _outputPath = $"{DateTime.Now.ToFileTimeUtc()}.avi";

        if (System.IO.File.Exists(_outputPath))
            System.IO.File.Delete(_outputPath);
    }

    public void Write(Mat frame)
    {
        _writer.Write(frame);
    }

    public void Stop()
    {
        _writer.Release();
        _writer.Dispose();
        _writer = null!;
    }

    public bool IsRecording()
    {
        return _writer.IsOpened();
    }

    public void Dispose()
    {
        if (_writer.IsNull()) return;
        
        Stop();
    }
}