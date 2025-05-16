using OpenCvSharp;

namespace SportVAR.Services;

public interface ICameraService : IDisposable
{
    void Start();
    
    void Stop();

    void SetFrameCallback(Action<Mat> callback);
}