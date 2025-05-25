using System.Timers;
using System.Windows.Controls;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using SportVAR.Models;
using Timer = System.Timers.Timer;

namespace SportVAR.Services;

public class PreviewService : IPreviewService
{
    private readonly IDispatcher _dispatcher;

    private readonly Timer _playbackTimer;
    private AppState _appState;
    private List<Mat> _frameBuffer = [];
    private Slider _frameSlider;
    private Image _liveImage;
    private int _tickCount;

    private const int TicksPerFrame = 2; // Play every 2 timer ticks

    public PreviewService(IDispatcher dispatcher)
    {
        _playbackTimer = new Timer(33); // ~30fps
        _playbackTimer.Elapsed += PlaybackTimer_Elapsed;
        _playbackTimer.AutoReset = true;
        _dispatcher = dispatcher;
    }

    public void Initialize(AppState appState, Image image, Slider slider, List<Mat> frameBuffer)
    {
        _appState = appState;
        _liveImage = image;
        _frameSlider = slider;
        _frameBuffer = frameBuffer;
    }

    public void Start()
    {
        _appState.IsReviewing = true;
        _appState.IsPlaying = true;
        _playbackTimer.Start();
    }

    public void Stop()
    {
        _appState.IsPlaying = false;
        _appState.IsReviewing = false;
        _playbackTimer.Stop();
    }

    private void PlaybackTimer_Elapsed(object? sender, ElapsedEventArgs e)
    {
        if (!_appState.IsPlaying || _frameBuffer.Count == 0)
        {
            _playbackTimer.Stop();
            return;
        }

        if (SlowdownSliderForPreview()) return;

        UpdateUIAtPlaybackEnd();
        UpdateUIWithFrame(_frameBuffer[_appState.CurrentFrameIndex++]);
    }

    // Safeguard for the last frame to make sure the last frame is displayed.
    private void UpdateUIAtPlaybackEnd()
    {
        if (_appState.CurrentFrameIndex < _frameBuffer.Count) return;

        // Reached end of buffer -> stop playing and switch back to live
        StopPlayback();
        _dispatcher.Invoke(() =>
                           {
                               if (_frameBuffer.Count <= 0) return;

                               var latest = _frameBuffer.Last();
                               _liveImage.Source = latest.ToBitmapSource();
                               if (!_appState.IsUserDraggingSlider)
                                   _frameSlider.Value = _frameBuffer.Count - 1;
                           });
    }

    private void UpdateUIWithFrame(Mat frame)
    {
        _dispatcher.Invoke(() =>
                           {
                               _liveImage.Source = frame.ToBitmapSource();
                               if (!_appState.IsUserDraggingSlider)
                                   _frameSlider.Value = _appState.CurrentFrameIndex;
                           });
    }

    private bool SlowdownSliderForPreview()
    {
        _tickCount++;

        if (_tickCount < TicksPerFrame)
            return true;

        _tickCount = 0;
        return false;
    }

    private void StopPlayback()
    {
        _appState.IsPlaying = false;
        _appState.IsReviewing = false;
        _playbackTimer.Stop();
    }
}