using OpenCvSharp;
using SportVAR.Models;

namespace SportVAR.Services;

public interface ICameraService : IDisposable
{
    void Start();
    
    void Stop();

    void SetFrameCallback(Action<Mat> callback);

    void SetCameraDetails(CameraDetail cameraDetail);
}