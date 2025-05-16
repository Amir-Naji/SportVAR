using OpenCvSharp;

namespace SportVAR;

public interface IVideoRecorder : IDisposable
{
    void Start();
    
    void Write(Mat frame);

    void Stop();
    
    bool IsRecording();
}