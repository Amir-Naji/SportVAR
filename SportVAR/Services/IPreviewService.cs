using System.Windows.Controls;
using OpenCvSharp;
using SportVAR.Models;

namespace SportVAR.Services;

public interface IPreviewService
{
    void Initialize(AppState appState, Image image, Slider slider, List<Mat> frameBuffer);

    void Start();

    void Stop();
}