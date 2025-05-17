using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using SportVAR.Models;
using SportVAR.Services;
using SportVAR.Utilities;
using Window = System.Windows.Window;

namespace SportVAR;

public partial class MainWindow : Window
{
    private readonly ICameraService _camera;
    private readonly Func<IVideoRecorder> _recorderFactory;
    private readonly IPreviewService _previewService;
    private readonly List<Mat> _frameBuffer = [];
    //private readonly Timer _playbackTimer;
    private readonly bool _isSeeking = false;
    private readonly AppState _appState = new();

    private IVideoRecorder _recorder;
    private int _tickCount;

    //private const int TicksPerFrame = 2; // Play every 2 timer ticks

    public MainWindow(ICameraService camera, Func<IVideoRecorder> recorderFactory, IPreviewService previewService)
    {
        InitializeComponent();

        //_playbackTimer = new Timer(33); // ~30fps
        //_playbackTimer.Elapsed += PlaybackTimer_Elapsed;
        //_playbackTimer.AutoReset = true;

        _camera = camera;
        _recorderFactory = recorderFactory;
        _previewService = previewService;
        _previewService.Initialize(_appState, liveImage, frameSlider, Dispatcher.CurrentDispatcher, _frameBuffer);

        _camera.SetFrameCallback(OnFrameReceived);
    }

    //private void PlaybackTimer_Elapsed(object? sender, ElapsedEventArgs e)
    //{
    //    if (!_appState.IsPlaying || _frameBuffer.Count == 0)
    //    {
    //        _playbackTimer.Stop();
    //        return;
    //    }

    //    _tickCount++;
    //    if (_tickCount < TicksPerFrame)
    //        return;
    //    _tickCount = 0;

    //    if (_appState.CurrentFrameIndex >= _frameBuffer.Count)
    //    {
    //        // Reached end of buffer -> stop playing and switch back to live
    //        _appState.IsPlaying = false;
    //        _appState.IsReviewing = false;
    //        _playbackTimer.Stop();

    //        Dispatcher.Invoke(() =>
    //                          {
    //                              if (_frameBuffer.Count > 0)
    //                              {
    //                                  var latest = _frameBuffer.Last();
    //                                  liveImage.Source = latest.ToBitmapSource();
    //                                  frameSlider.Value = _frameBuffer.Count - 1;
    //                              }
    //                          });
    //        return;
    //    }

    //    var frame = _frameBuffer[_appState.CurrentFrameIndex];
    //    _appState.CurrentFrameIndex++;

    //    Dispatcher.Invoke(() =>
    //                      {
    //                          liveImage.Source = frame.ToBitmapSource();
    //                          frameSlider.Value = _appState.CurrentFrameIndex;
    //                      });
    //}


    private void OnFrameReceived(Mat frame)
    {
        if (frame.IsNull() || frame.Empty())
            return;

        if (_appState.IsRecording)
            _frameBuffer.Add(frame.Clone());

        if (_appState.IsReviewing) return;

        var currentFrame = frame.Clone();
        Dispatcher.Invoke(() =>
                          {
                              liveImage.Source = currentFrame.ToBitmapSource();
                              frameSlider.Maximum = _frameBuffer.Count - 1;
                              frameSlider.Value = _frameBuffer.Count - 1;
                          });
    }

    private void btRecord_Click(object sender, RoutedEventArgs e)
    {
        _camera.Start();

        if (!_appState.IsRecording)
        {
            _recorder = _recorderFactory.Invoke();
            _recorder.Start();
        }
        else
        {
            _recorder.Stop();
            _recorder.Dispose();
            _recorder = null!;
        }

        _appState.IsRecording = !_appState.IsRecording;
    }

    private void btStop_Click(object sender, RoutedEventArgs e)
    {
        _recorder.Stop();
        _camera.Stop();
        _camera.Dispose();
    }

    private void frameSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!_isSeeking) return;

        var index = (int)e.NewValue;

        if (index < 0 || index >= _frameBuffer.Count) return;

        var seekFrame = _frameBuffer[index];
        liveImage.Source = seekFrame.ToBitmapSource();
    }

    private void Slider_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        _appState.IsReviewing = true;
    }

    private void Slider_PreviewMouseUp(object sender, MouseButtonEventArgs e)
    {
        var index = (int)frameSlider.Value;

        if (index < 0 || index >= _frameBuffer.Count) return;

        _appState.CurrentFrameIndex = index;

        _appState.IsReviewing = true;
        _appState.IsPlaying = true;
        _previewService.Start();
    }

    private void btToLive_Click(object sender, RoutedEventArgs e)
    {
        _appState.IsPlaying = false;
        _appState.IsReviewing = false;
        _previewService.Stop();

        if (_frameBuffer.Count <= 0) return;

        var latest = _frameBuffer.Last();
        Dispatcher.Invoke(() =>
                          {
                              liveImage.Source = latest.ToBitmapSource();
                              frameSlider.Value = _frameBuffer.Count - 1;
                          });
    }
}