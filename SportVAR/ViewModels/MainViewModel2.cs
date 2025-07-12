using CommunityToolkit.Mvvm.ComponentModel;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using SportVAR.Models;
using SportVAR.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Imaging;
using Size = OpenCvSharp.Size;

namespace SportVAR.ViewModels;

public partial class MainViewModel2 : ObservableObject, IDisposable
{
    private readonly ICameraConfigurator _cameraConfigurator;
    private readonly ICameraListService _cameraListService;
    private readonly ICameraService _cameraService;
    private readonly List<Mat> _frameBuffer = [];
    private readonly IPreviewService _previewService;
    private readonly IVideoRecorder _videoRecorder;

    private CameraDetail _camera1Detail = new();

    [ObservableProperty] private BitmapSource? _camera1Image;
    private CameraModel _camera1Model = new();
    private CameraDetail _camera2Detail = new();
    [ObservableProperty] private BitmapSource? _camera2Image;
    private CameraModel _camera2Model = new();
    private VideoCapture _capture1;
    private VideoCapture _capture2;
    private bool _isRecording;
    [ObservableProperty] private int _maxSliderValue;
    private CancellationTokenSource _tokenSource;
    private VideoWriter _writer1;
    private VideoWriter _writer2;

    public MainViewModel2(ICameraService cameraService, ICameraListService cameraListService,
                          ICameraConfigurator cameraConfigurator, IPreviewService previewService,
                          IVideoRecorder videoRecorder)
    {
        ToggleRecordingCommand = new RelayCommand(ToggleRecording);

        _cameraService = cameraService;
        _cameraListService = cameraListService;
        _cameraConfigurator = cameraConfigurator;
        _previewService = previewService;
        _videoRecorder = videoRecorder;

        LoadCameras();
    }

    public RelayCommand ToggleRecordingCommand { get; }
    public ObservableCollection<CameraModel> Camera1Options { get; } = [];
    public ObservableCollection<CameraModel> Camera2Options { get; } = [];
    public ObservableCollection<CameraDetail> Camera1Resolutions { get; } = [];
    public ObservableCollection<CameraDetail> Camera2Resolutions { get; } = [];

    public void Dispose()
    {
        TryCleanup();
    }

    public event PropertyChangedEventHandler PropertyChanged;


    private void LoadCameras()
    {
        var list = _cameraListService.CameraNames();
        foreach (var cam in list)
        {
            Camera1Options.Add(cam);
            Camera2Options.Add(cam);
        }
    }

    public async Task Camera1Selected(CameraModel model)
    {
        _camera1Model = model;
        Camera1Resolutions.Clear();
        foreach (var res in await _cameraListService.CameraResolution(model.MonikerString))
            Camera1Resolutions.Add(res);
    }

    public void Camera1ResolutionSelected(CameraDetail detail)
    {
        _camera1Detail = _cameraConfigurator.SetSelectedResolution(_camera1Model, detail);
    }

    public async Task Camera2Selected(CameraModel model)
    {
        _camera2Model = model;
        Camera2Resolutions.Clear();
        foreach (var res in await _cameraListService.CameraResolution(model.MonikerString))
            Camera2Resolutions.Add(res);
    }

    public void Camera2ResolutionSelected(CameraDetail detail)
    {
        _camera2Detail = _cameraConfigurator.SetSelectedResolution(_camera2Model, detail);
    }

    private void ToggleRecording()
    {
        if (_isRecording)
        {
            _isRecording = false;
            _tokenSource?.Cancel();
            _writer1?.Release();
            _writer2?.Release();
        }
        else
        {
            _isRecording = true;
            _tokenSource = new CancellationTokenSource();

            _capture1 = new VideoCapture(_camera1Detail.Index, VideoCaptureAPIs.DSHOW)
                        {
                            FrameWidth = _camera1Detail.Width,
                            FrameHeight = _camera1Detail.Height,
                            Fps = _camera1Detail.Fps
                        };

            _capture2 = new VideoCapture(_camera2Detail.Index, VideoCaptureAPIs.DSHOW)
                        {
                            FrameWidth = _camera2Detail.Width,
                            FrameHeight = _camera2Detail.Height,
                            Fps = _camera2Detail.Fps
                        };

            Record(_camera1Detail);
            Record(_camera2Detail);

            var frameSize = new Size(_camera1Detail.Width, _camera1Detail.Height);
            var outputPath = $"{DateTime.Now.ToFileTimeUtc()}.avi";
            _writer1 = new VideoWriter(outputPath, FourCC.XVID, _camera1Detail.Fps, frameSize);

            outputPath = $"{DateTime.Now.ToFileTimeUtc()}.avi";
            frameSize = new Size(_camera2Detail.Width, _camera2Detail.Height);
            _writer2 = new VideoWriter(outputPath, FourCC.XVID, _camera2Detail.Fps, frameSize);

            Task.Run(() => CaptureLoop(_capture1, frame => Camera1Image = frame, _writer1, _tokenSource.Token));
            Task.Run(() => CaptureLoop(_capture2, frame => Camera2Image = frame, _writer2, _tokenSource.Token));
        }
    }

    private void LiveShow()
    {
    }

    private void Record(CameraDetail cameraDetail)
    {
        _videoRecorder.Start(cameraDetail);
    }

    private static void CaptureLoop(VideoCapture capture,
                                    Action<BitmapSource> updateImage,
                                    VideoWriter writer,
                                    CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            using var mat = new Mat();
            if (!capture.Read(mat) || mat.Empty()) continue;

            writer.Write(mat);

            var bitmap = mat.ToBitmapSource();
            bitmap.Freeze();
            Application.Current.Dispatcher.Invoke(() => updateImage(bitmap));
        }

        capture.Release();
    }

    //private void OnPropertyChanged(string name)
    //{
    //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    //}

    public void TryCleanup()
    {
        try
        {
            CleanUp();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error during cleanup: {ex.Message}");
        }
    }

    private void CleanUp()
    {
        //_camera1?.Stop();
        //_camera2?.Stop();

        _capture1?.Release();
        _capture1?.Dispose();

        _capture2?.Release();
        _capture2?.Dispose();

        _writer1?.Release();
        _writer1?.Dispose();

        _writer2?.Release();
        _writer2?.Dispose();

        _tokenSource?.Cancel();
        _tokenSource?.Dispose();
    }
}