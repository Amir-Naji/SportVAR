using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using SportVAR.Models;
using SportVAR.Services;
using SportVAR.Utilities;

namespace SportVAR.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly ICameraService _camera;
    private readonly ICameraListService _cameraListService;
    private readonly List<Mat> _frameBuffer = [];
    private readonly IPreviewService _previewService;
    private readonly Func<IVideoRecorder> _recorderFactory;

    private CameraDetail _camera1Detail = new();
    private CameraModel _camera1Model = new();
    private CameraDetail _camera2Detail = new();
    private CameraModel _camera2Model = new();

    private IVideoRecorder? _recorder;

    [ObservableProperty] private BitmapSource? _currentFrame;
    [ObservableProperty] private bool _isRecording;
    [ObservableProperty] private bool _isReviewing;
    [ObservableProperty] private bool _isUserDraggingSlider;
    [ObservableProperty] private BitmapSource? _liveImage1Source;
    [ObservableProperty] private BitmapSource? _liveImage2Source;
    [ObservableProperty] private int _sliderMaximum;
    [ObservableProperty] private int _sliderValue;

    public MainViewModel(ICameraService camera,
                         Func<IVideoRecorder> recorderFactory,
                         IPreviewService previewService,
                         ICameraListService cameraListService)
    {
        _camera = camera;
        _recorderFactory = recorderFactory;
        _previewService = previewService;
        _cameraListService = cameraListService;

        _previewService.Initialize(
                                   _frameBuffer,
                                   () => IsUserDraggingSlider,
                                   frame => LiveImage1Source = frame.ToBitmapSource(),
                                   index => SliderValue = index,
                                   () => SliderValue
                                  );

        _camera.SetFrameCallback(OnFrameReceived);
        LoadCameras();
    }

    public ObservableCollection<CameraModel> Camera1Options { get; } = [];
    public ObservableCollection<CameraModel> Camera2Options { get; } = [];
    public ObservableCollection<CameraDetail> Camera1Resolutions { get; } = [];
    public ObservableCollection<CameraDetail> Camera2Resolutions { get; } = [];

    private void OnFrameReceived(Mat frame)
    {
        if (frame.Empty() || frame.IsNull()) return;

        if (IsRecording)
            _frameBuffer.Add(frame.Clone());

        if (IsReviewing) return;

        var current = frame.Clone();
        CurrentFrame = current.ToBitmapSource();
        SliderMaximum = _frameBuffer.Count - 1;
        SliderValue = _frameBuffer.Count - 1;
    }

    [RelayCommand]
    private void ToggleRecording()
    {
        _camera.Start();

        if (!IsRecording)
        {
            _recorder = _recorderFactory();
            _recorder.Start();
        }
        else
        {
            _recorder?.Stop();
            _recorder?.Dispose();
            _recorder = null;
        }

        IsRecording = !IsRecording;
    }

    [RelayCommand]
    private void StopRecording()
    {
        _recorder?.Stop();
        _camera.Stop();
        _camera.Dispose();
        IsRecording = false;
    }

    [RelayCommand]
    private void SeekFrame(int index)
    {
        if (!IsReviewing || index < 0 || index >= _frameBuffer.Count) return;

        var frame = _frameBuffer[index];
        CurrentFrame = frame.ToBitmapSource();
    }

    [RelayCommand]
    private void ToLive()
    {
        _previewService.Stop();

        if (_frameBuffer.Count <= 0) return;
        var latest = _frameBuffer.Last();
        CurrentFrame = latest.ToBitmapSource();
        SliderValue = _frameBuffer.Count - 1;
    }

    private void LoadCameras()
    {
        var list = _cameraListService.CameraNames();
        foreach (var cam in list)
        {
            Camera1Options.Add(cam);
            Camera2Options.Add(cam);
        }
    }

    public void Camera1Selected(CameraModel model)
    {
        _camera1Model = model;
        Camera1Resolutions.Clear();
        foreach (var res in _cameraListService.CameraResolution(model.Name))
            Camera1Resolutions.Add(res);
    }

    public void Camera1ResolutionSelected(CameraDetail detail)
    {
        _camera1Detail = detail;
        _camera1Detail.Name = _camera1Model.Name;
        _camera1Detail.Index = _camera1Model.Index;
        _camera.SetCameraDetails(_camera1Detail);
    }

    public void Camera2Selected(CameraModel model)
    {
        _camera2Model = model;
        Camera2Resolutions.Clear();
        foreach (var res in _cameraListService.CameraResolution(model.Name))
            Camera2Resolutions.Add(res);
    }

    public void Camera2ResolutionSelected(CameraDetail detail)
    {
        _camera2Detail = detail;
        _camera2Detail.Name = _camera2Model.Name;
        _camera2Detail.Index = _camera2Model.Index;
        _camera.SetCameraDetails(_camera2Detail);
    }
}