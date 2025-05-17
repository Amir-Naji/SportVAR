using OpenCvSharp;
using SportVAR.Models;
using System.Timers;
using System.Windows.Controls;
using System.Windows.Threading;
using OpenCvSharp.WpfExtensions;
using Timer = System.Timers.Timer;

namespace SportVAR.Services;

public class PreviewService : IPreviewService
{
    private readonly Timer _playbackTimer;

    private List<Mat> _frameBuffer = [];
    private Dispatcher _dispatcher;
    private Image _liveImage;
    private Slider _frameSlider;
    private int _tickCount;
    private AppState _appState;

    private const int TicksPerFrame = 2; // Play every 2 timer ticks


    public PreviewService()
    {
        _playbackTimer = new Timer(33); // ~30fps
        _playbackTimer.Elapsed += PlaybackTimer_Elapsed;
        _playbackTimer.AutoReset = true;
    }

    public void Initialize(AppState appState, Image image, Slider slider, Dispatcher dispatcher, List<Mat> frameBuffer)
    {
        _appState = appState;
        _liveImage = image;
        _frameSlider = slider;
        _dispatcher = dispatcher;
        _frameBuffer = frameBuffer;
    }

    public void Start()
    {
        _playbackTimer.Start();
    }

    public void Stop()
    {
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

        if (_appState.CurrentFrameIndex >= _frameBuffer.Count)
        {
            // Reached end of buffer -> stop playing and switch back to live
            StopPlayback();

            _dispatcher.Invoke(() =>
                               {
                                   if (_frameBuffer.Count <= 0) return;

                                   var latest = _frameBuffer.Last();
                                   _liveImage.Source = latest.ToBitmapSource();
                                   _frameSlider.Value = _frameBuffer.Count - 1;
                               });
            return;
        }

        var frame = _frameBuffer[_appState.CurrentFrameIndex++];

        _dispatcher.Invoke(() =>
                          {
                              _liveImage.Source = frame.ToBitmapSource();
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