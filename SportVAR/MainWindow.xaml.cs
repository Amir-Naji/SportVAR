using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using SportVAR.Services;
using Window = System.Windows.Window;

namespace SportVAR;

public partial class MainWindow : Window
{
    private readonly ICameraService _camera;
    private readonly Func<IVideoRecorder> _recorderFactory;

    private bool _isRecording;
    private IVideoRecorder _recorder;
    private List<Mat> _frameBuffer = new();
    private bool _isSeeking = false;
    private bool _isReviewing = false; // stays true until user says "jump to live"
    private bool _isPlaying = false;
    private int _playbackIndex = 0;
    private System.Timers.Timer _playbackTimer;
    private int _tickCount = 0;
    private int _ticksPerFrame = 2; // Play every 2 timer ticks



    public MainWindow(ICameraService camera, Func<IVideoRecorder> recorderFactory)
    {
        InitializeComponent();

        _playbackTimer = new System.Timers.Timer(50); // ~30fps
        _playbackTimer.Elapsed += PlaybackTimer_Elapsed;
        _playbackTimer.AutoReset = true;

        _camera = camera;
        _recorderFactory = recorderFactory;

        _camera.SetFrameCallback(OnFrameReceived);
    }

    private void PlaybackTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
        if (!_isPlaying || _frameBuffer.Count == 0)
        {
            _playbackTimer.Stop();
            return;
        }

        _tickCount++;
        if (_tickCount < _ticksPerFrame)
            return;
        _tickCount = 0;

        if (_playbackIndex >= _frameBuffer.Count)
        {
            // Reached end of buffer -> stop playing and switch back to live
            _isPlaying = false;
            _isReviewing = false;
            _playbackTimer.Stop();

            Dispatcher.Invoke(() =>
                              {
                                  if (_frameBuffer.Count > 0)
                                  {
                                      var latest = _frameBuffer.Last();
                                      liveImage.Source = latest.ToBitmapSource();
                                      frameSlider.Value = _frameBuffer.Count - 1;
                                  }
                              });
            return;
        }

        var frame = _frameBuffer[_playbackIndex];
        _playbackIndex++;

        Dispatcher.Invoke(() =>
                          {
                              liveImage.Source = frame.ToBitmapSource();
                              frameSlider.Value = _playbackIndex;
                          });
    }


    private void OnFrameReceived(Mat frame)
    {
        if (frame == null || frame.Empty())
            return;

        if (_isRecording)
        {
            _frameBuffer.Add(frame.Clone());
        }

        if (!_isReviewing)
        {
            var currentFrame = frame.Clone();
            Dispatcher.Invoke(() =>
                              {
                                  liveImage.Source = currentFrame.ToBitmapSource();
                                  frameSlider.Maximum = _frameBuffer.Count - 1;
                                  frameSlider.Value = _frameBuffer.Count - 1;
                              });
        }
    }

    private void ToggleRecord_Click(object sender, RoutedEventArgs e)
    {
        if (!_isRecording)
        {
            _recorder = _recorderFactory.Invoke();
            _recorder.Start();
        }
        else
        {
            _recorder.Stop();
            _recorder.Dispose();
            _recorder = null;
        }

        _isRecording = !_isRecording;
    }

    private void btRecord_Click(object sender, RoutedEventArgs e)
    {
        _camera.Start();

        if (!_isRecording)
        {
            _recorder = _recorderFactory.Invoke();
            _recorder.Start();
        }
        else
        {
            _recorder.Stop();
            _recorder.Dispose();
            _recorder = null;
        }

        _isRecording = !_isRecording;
    }

    private void btStop_Click(object sender, RoutedEventArgs e)
    {
        _recorder?.Stop();
        _recorder?.Dispose();
        _camera.Stop();
        _camera.Dispose();
    }

    private void frameSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        //if (_isSeeking && _frameBuffer.Count > 0)
        //{
        //    int index = (int)frameSlider.Value;
        //    if (index >= 0 && index < _frameBuffer.Count)
        //    {
        //        var frame = _frameBuffer[index];
        //        liveImage.Source = frame.ToBitmapSource();
        //    }
        //}

        if (_isSeeking)
        {
            int index = (int)e.NewValue;
            if (index >= 0 && index < _frameBuffer.Count)
            {
                var seekFrame = _frameBuffer[index];
                liveImage.Source = seekFrame.ToBitmapSource();
            }
        }
    }

    private void Slider_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        _isReviewing = true;
    }

    private void Slider_PreviewMouseUp(object sender, MouseButtonEventArgs e)
    {
        int index = (int)frameSlider.Value;
        if (index >= 0 && index < _frameBuffer.Count)
        {
            _isReviewing = true;
            _isPlaying = true;
            _playbackIndex = index;

            _playbackTimer.Start();
        }
    }

    private void JumpToLive()
    {
        _isPlaying = false;
        _isReviewing = false;
        _playbackTimer.Stop();

        if (_frameBuffer.Count > 0)
        {
            var latest = _frameBuffer.Last();
            Dispatcher.Invoke(() =>
                              {
                                  liveImage.Source = latest.ToBitmapSource();
                                  frameSlider.Value = _frameBuffer.Count - 1;
                              });
        }
    }


    private void btToLive_Click(object sender, RoutedEventArgs e)
    {
        JumpToLive();
    }
}