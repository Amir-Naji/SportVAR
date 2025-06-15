using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using AForge.Video.DirectShow;
using SportVAR.Models;
using SportVAR.Services;
using SportVAR.Utilities;

namespace SportVAR.ViewModels;

public class MainViewModel2 : INotifyPropertyChanged
{
    private readonly ICameraService _cameraService;
    private readonly ICameraListService _cameraListService;

    private CameraFeed _camera1, _camera2;
    private CameraDetail _camera1Detail = new();
    private CameraModel _camera1Model = new();
    private CameraDetail _camera2Detail = new();
    private CameraModel _camera2Model = new();
    private FilterInfo _selectedCamera1;
    private FilterInfo _selectedCamera2;


    public MainViewModel2(ICameraService cameraService, ICameraListService cameraListService)
    {
        //LoadAvailableCameras();

        StartCommand = new RelayCommand(StartCameras);
        StopCommand = new RelayCommand(StopCameras);
        _cameraService = cameraService;
        _cameraListService = cameraListService;

        LoadCameras();
    }

    public ObservableCollection<FilterInfo> AvailableCameras { get; } = new();
    public BitmapImage Camera1Image { get; private set; }
    public BitmapImage Camera2Image { get; private set; }
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

    public ICommand StartCommand { get; }

    public ICommand StopCommand { get; }

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

    private void OnPropertyChanged(string name)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
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
        foreach (var res in _cameraListService.CameraResolution(model.MonikerString))
            Camera1Resolutions.Add(res);
    }

    public void Camera1ResolutionSelected(CameraDetail detail)
    {
        _camera1Detail = detail;
        _camera1Detail.Name = _camera1Model.Name;
        _camera1Detail.Index = _camera1Model.Index;
        _camera1Detail.MonikerString = _camera1Model.MonikerString;
        _cameraService.SetCameraDetails(_camera1Detail);
    }

    public void Camera2Selected(CameraModel model)
    {
        _camera2Model = model;
        Camera2Resolutions.Clear();
        foreach (var res in _cameraListService.CameraResolution(model.MonikerString))
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
}