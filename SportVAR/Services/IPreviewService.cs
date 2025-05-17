using OpenCvSharp;
using SportVAR.Models;
using System.Windows.Controls;
using System.Windows.Threading;

namespace SportVAR.Services;

public interface IPreviewService
{
    void Initialize(AppState appState, Image image, Slider slider, Dispatcher dispatcher, List<Mat> frameBuffer);

    void Start();

    void Stop();
}