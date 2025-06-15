using System.Timers;
using OpenCvSharp;
using Timer = System.Timers.Timer;

namespace SportVAR.Services;

public class PreviewService : IPreviewService
{
    private readonly Timer _playbackTimer;
    private List<Mat> _frameBuffer = [];
    private int _tickCount;
    private const int TicksPerFrame = 2;

    private Func<bool>? _isUserDraggingSlider;
    private Action<Mat>? _displayFrame;
    private Action<int>? _updateSlider;
    private Func<int>? _getCurrentSliderIndex;

    private bool _isPlaying;

    public PreviewService()
    {
        _playbackTimer = new Timer(33); // ~30fps
        _playbackTimer.Elapsed += PlaybackTimer_Elapsed;
        _playbackTimer.AutoReset = true;
    }

    public void Initialize(List<Mat> frameBuffer,
                           Func<bool> isUserDraggingSlider,
                           Action<Mat> displayFrame,
                           Action<int> updateSlider,
                           Func<int> getCurrentSliderIndex)
    {
        _frameBuffer = frameBuffer;
        _isUserDraggingSlider = isUserDraggingSlider;
        _displayFrame = displayFrame;
        _updateSlider = updateSlider;
        _getCurrentSliderIndex = getCurrentSliderIndex;
    }

    public void Start()
    {
        _isPlaying = true;
        _tickCount = 0;
        _playbackTimer.Start();
    }

    public void Stop()
    {
        _isPlaying = false;
        _playbackTimer.Stop();
    }

    private void PlaybackTimer_Elapsed(object? sender, ElapsedEventArgs e)
    {
        if (!_isPlaying || _frameBuffer.Count == 0) return;

        if (++_tickCount < TicksPerFrame) return;
        _tickCount = 0;

        var currentIndex = _getCurrentSliderIndex?.Invoke() ?? 0;

        if (currentIndex >= _frameBuffer.Count)
        {
            Stop();
            return;
        }

        var frame = _frameBuffer[currentIndex];
        _displayFrame?.Invoke(frame);
        if (!(_isUserDraggingSlider?.Invoke() ?? false))
            _updateSlider?.Invoke(currentIndex + 1);
    }
}
