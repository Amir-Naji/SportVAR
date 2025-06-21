using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Imaging;
using AForge.Video.DirectShow;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using SportVAR.Models;
using SportVAR.Services;
using SportVAR.Utilities;
using Size = OpenCvSharp.Size;

namespace SportVAR.ViewModels;

public class MainViewModel2 : INotifyPropertyChanged
{
    private readonly ICameraListService _cameraListService;
    private readonly ICameraService _cameraService;
    private readonly ICameraConfigurator _cameraConfigurator;

    private CameraFeed _camera1, _camera2;

    private CameraDetail _camera1Detail = new();
    //public BitmapImage Camera1Image { get; private set; }

    private BitmapSource _camera1Image;
    private CameraModel _camera1Model = new();
    private CameraDetail _camera2Detail = new();
    private BitmapSource _camera2Image;
    private CameraModel _camera2Model = new();

    private VideoCapture _capture1;
    private VideoCapture _capture2;
    private bool _isRecording;
    private FilterInfo _selectedCamera1;
    private FilterInfo _selectedCamera2;
    private CancellationTokenSource _tokenSource;
    private VideoWriter _writer1;
    private VideoWriter _writer2;

    public MainViewModel2(ICameraService cameraService, ICameraListService cameraListService, ICameraConfigurator cameraConfigurator)
    {
        ToggleRecordingCommand = new RelayCommand(ToggleRecording);

        //StartCommand = new RelayCommand(StartCameras);
        //StopCommand = new RelayCommand(StopCameras);
        _cameraService = cameraService;
        _cameraListService = cameraListService;
        _cameraConfigurator = cameraConfigurator;

        LoadCameras();
    }

    public RelayCommand ToggleRecordingCommand { get; }
    public ObservableCollection<FilterInfo> AvailableCameras { get; } = new();

    //public BitmapImage Camera1Image
    //{
    //    get => _camera1Image;
    //    set
    //    {
    //        _camera1Image = value;
    //        OnPropertyChanged(nameof(Camera1Image));
    //    }
    //}

    public BitmapSource Camera1Image
    {
        get => _camera1Image;
        set
        {
            _camera1Image = value;
            OnPropertyChanged(nameof(Camera1Image));
        }
    }

    public BitmapSource Camera2Image
    {
        get => _camera2Image;
        set
        {
            _camera2Image = value;
            OnPropertyChanged(nameof(Camera2Image));
        }
    }

    public ObservableCollection<CameraModel> Camera1Options { get; } = [];
    public ObservableCollection<CameraModel> Camera2Options { get; } = [];
    public ObservableCollection<CameraDetail> Camera1Resolutions { get; } = [];
    public ObservableCollection<CameraDetail> Camera2Resolutions { get; } = [];

    public FilterInfo SelectedCamera1
    {
        get => _selectedCamera1;
        set
        {
            _selectedCamera1 = value;
            OnPropertyChanged(nameof(SelectedCamera1));
        }
    }

    public FilterInfo SelectedCamera2
    {
        get => _selectedCamera2;
        set
        {
            _selectedCamera2 = value;
            OnPropertyChanged(nameof(SelectedCamera2));
        }
    }

    //public ICommand StartCommand { get; }

    //public ICommand StopCommand { get; }

    public event PropertyChangedEventHandler PropertyChanged;

    private bool CanStart()
    {
        return SelectedCamera1.IsNotNull() && SelectedCamera2.IsNotNull();
    }

    private void StartCameras()
    {
        _camera1 = new CameraFeed(_camera1Detail.MonikerString);
        _camera2 = new CameraFeed(_camera2Detail.MonikerString);

        _camera1.FrameReady += img => Camera1Image = RaiseImageChange(img, nameof(Camera1Image));
        _camera2.FrameReady += img => Camera2Image = RaiseImageChange(img, nameof(Camera2Image));

        _camera1.Start();
        _camera2.Start();
    }

    private void StopCameras()
    {
        _camera1?.Stop();
        _camera2?.Stop();
    }

    private BitmapImage RaiseImageChange(BitmapImage image, string prop)
    {
        OnPropertyChanged(prop);
        return image;
    }

    //private void OnPropertyChanged(string name)
    //{
    //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    //}

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
        //_camera1Model = model;
        //Camera1Resolutions = _cameraConfigurator.GetCameraResolutions(model);
        //OnPropertyChanged(nameof(Camera1Resolutions));

        _camera1Model = model;
        Camera1Resolutions.Clear();
        foreach (var res in await _cameraListService.CameraResolution(model.MonikerString))
            Camera1Resolutions.Add(res);
    }

    public void Camera1ResolutionSelected(CameraDetail detail)
    {
        //_camera1Detail = detail;
        //_camera1Detail.Name = _camera1Model.Name;
        //_camera1Detail.Index = _camera1Model.Index;
        //_camera1Detail.MonikerString = _camera1Model.MonikerString;
        //_cameraService.SetCameraDetails(_camera1Detail);
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
        _camera2Detail = detail;
        _camera2Detail.Name = _camera2Model.Name;
        _camera2Detail.Index = _camera2Model.Index;
        _camera2Detail.MonikerString = _camera2Model.MonikerString;
        _cameraService.SetCameraDetails(_camera2Detail);
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

            int width = 640, height = 480, fps = 25;

            _writer1 = new VideoWriter("camera1.avi", FourCC.XVID, fps, new Size(width, height));
            _writer2 = new VideoWriter("camera2.avi", FourCC.XVID, fps, new Size(width, height));

            Task.Run(() => CaptureLoop(_capture1, frame => Camera1Image = frame, _writer1, _tokenSource.Token));
            Task.Run(() => CaptureLoop(_capture2, frame => Camera2Image = frame, _writer2, _tokenSource.Token));
        }
    }

    private static void CaptureLoop(VideoCapture capture, Action<BitmapSource> updateImage, VideoWriter writer,
                                    CancellationToken token)
    {
        using var mat = new Mat();

        while (!token.IsCancellationRequested)
        {
            if (!capture.Read(mat) || mat.Empty()) continue;

            writer.Write(mat);

            var bitmap = mat.ToBitmapSource();
            bitmap.Freeze();
            Application.Current.Dispatcher.Invoke(() => updateImage(bitmap));
        }

        capture.Release();
    }

    private void OnPropertyChanged(string name)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}