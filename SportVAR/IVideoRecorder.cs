using OpenCvSharp;
using SportVAR.Models;

namespace SportVAR;

public interface IVideoRecorder : IDisposable
{
    void Start();

    void Start(CameraDetail cameraDetail);

    void Write(Mat frame);

    void Stop();

    bool IsRecording();
}