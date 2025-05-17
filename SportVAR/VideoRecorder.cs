using OpenCvSharp;
using SportVAR.Utilities;

namespace SportVAR;

public class VideoRecorder : IVideoRecorder
{
    private VideoWriter _writer;
    private readonly int _fps;
    private readonly Size _frameSize;
    private readonly string _outputPath;

    public VideoRecorder(int width, int height, int fps = 30)
    {
        _fps = fps;
        _frameSize = new Size(width, height);
        _outputPath = $"{DateTime.Now.ToFileTimeUtc()}.avi";

        if (System.IO.File.Exists(_outputPath))
            System.IO.File.Delete(_outputPath);
    }

    public void Start()
    {
        _writer = new VideoWriter(_outputPath, FourCC.XVID, _fps, _frameSize);
        if (!_writer.IsOpened())
            throw new Exception("Failed to open VideoWriter.");
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