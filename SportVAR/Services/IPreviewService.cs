using OpenCvSharp;

namespace SportVAR.Services;

public interface IPreviewService
{
    //void Initialize(AppState appState, Image image, Slider slider, List<Mat> frameBuffer);

    void Initialize(List<Mat> frameBuffer,
                    Func<bool> isUserDraggingSlider,
                    Action<Mat> displayFrame,
                    Action<int> updateSlider,
                    Func<int> getCurrentSliderIndex);

    void Start();

    void Stop();
}