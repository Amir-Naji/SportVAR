using System.Windows;
using System.Windows.Input;
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
    private readonly ICameraListService _cameraListService;
    private readonly List<Mat> _frameBuffer = [];
    private readonly bool _isSeeking = false;
    private readonly AppState _appState = new();
    
    private IVideoRecorder _recorder;
    private CameraDetail _cameraDetail = new();
    private CameraModel _cameraModel = new();

    public MainWindow(ICameraService camera, Func<IVideoRecorder> recorderFactory, IPreviewService previewService, ICameraListService cameraListService)
    {
        InitializeComponent();

        _camera = camera;
        _recorderFactory = recorderFactory;
        _previewService = previewService;
        _previewService.Initialize(_appState, liveImage, frameSlider, _frameBuffer);

        _camera.SetFrameCallback(OnFrameReceived);
        _cameraListService = cameraListService;
    }

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
                              liveImage.Source = currentFrame.ToBitmapSource(); ;
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
        _appState.IsRecording = !_appState.IsRecording;
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
        _appState.IsUserDraggingSlider = true;
    }

    private void Slider_PreviewMouseUp(object sender, MouseButtonEventArgs e)
    {
        _appState.IsUserDraggingSlider = false;

        var index = (int)frameSlider.Value;

        if (index < 0 || index >= _frameBuffer.Count) return;

        _appState.CurrentFrameIndex = index;

        _previewService.Start();
    }

    private void btToLive_Click(object sender, RoutedEventArgs e)
    {
        _previewService.Stop();

        if (_frameBuffer.Count <= 0) return;

        var latest = _frameBuffer.Last();
        Dispatcher.Invoke(() =>
                          {
                              liveImage.Source = latest.ToBitmapSource();
                              frameSlider.Value = _frameBuffer.Count - 1;
                          });
    }

    private void cmbCameraNames_Loaded(object sender, RoutedEventArgs e)
    {
        cmbCameraNames.ItemsSource = _cameraListService.CameraNames();
        cmbCameraNames.DisplayMemberPath = "Name";
    }

    private void cmbCameraNames_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        _cameraModel = (CameraModel)cmbCameraNames.SelectedItem;
        cmbCameraResolutions.ItemsSource = _cameraListService.CameraResolution(_cameraModel.Name);
        cmbCameraResolutions.DisplayMemberPath = "FormattedString";
    }

    private void cmbCameraResolutions_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        _cameraDetail = (CameraDetail)cmbCameraResolutions.SelectedItem;
        _cameraDetail.Name = _cameraModel.Name;
        _cameraDetail.Index = _cameraModel.Index;

        _camera.SetCameraDetails(_cameraDetail);
    }
}