using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using OpenCvSharp;
using SportVAR.Models;
using SportVAR.ViewModels;

namespace SportVAR;

public partial class MainWindow
{
    private readonly AppState _appState = new();

    //private readonly ICameraService _camera;
    //private readonly Func<IVideoRecorder> _recorderFactory;
    //private readonly IPreviewService _previewService;
    //private readonly ICameraListService _cameraListService;
    private readonly List<Mat> _frameBuffer = [];
    private readonly bool _isSeeking = false;
    private CameraDetail _camera1Detail = new();
    private CameraModel _camera1Model = new();
    private CameraDetail _camera2Detail = new();
    private CameraModel _camera2Model = new();

    private IVideoRecorder _recorder;


    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        //_camera = camera;
        //_recorderFactory = recorderFactory;
        //_previewService = previewService;
        //_previewService.Initialize(_appState, liveImage1, frameSlider, _frameBuffer);

        //_camera.SetFrameCallback(OnFrameReceived);
        //_cameraListService = cameraListService;
    }

    private MainViewModel Vm => (MainViewModel)DataContext;

    //private void frameSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    //{
    //    if (!_isSeeking) return;

    //    var index = (int)e.NewValue;

    //    if (index < 0 || index >= _frameBuffer.Count) return;

    //    var seekFrame = _frameBuffer[index];
    //    liveImage1.Source = seekFrame.ToBitmapSource();
    //}

    //private void Slider_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    //{
    //    _appState.IsReviewing = true;
    //    _appState.IsUserDraggingSlider = true;
    //}

    //private void Slider_PreviewMouseUp(object sender, MouseButtonEventArgs e)
    //{
    //    _appState.IsUserDraggingSlider = false;

    //    var index = (int)frameSlider.Value;

    //    if (index < 0 || index >= _frameBuffer.Count) return;

    //    _appState.CurrentFrameIndex = index;

    //    _previewService.Start();
    //}

    //private void btToLive_Click(object sender, RoutedEventArgs e)
    //{
    //    _previewService.Stop();

    //    if (_frameBuffer.Count <= 0) return;

    //    var latest = _frameBuffer.Last();
    //    Dispatcher.Invoke(() =>
    //                      {
    //                          liveImage1.Source = latest.ToBitmapSource();
    //                          frameSlider.Value = _frameBuffer.Count - 1;
    //                      });
    //}

    private void cmbCameraNames_Loaded(object sender, RoutedEventArgs e)
    {
        cmbCamera1Names.ItemsSource = Vm.Camera1Options;
        cmbCamera2Names.ItemsSource = Vm.Camera2Options;
    }

    private void cmbCamera1Names_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (cmbCamera1Names.SelectedItem is CameraModel model)
            Vm.Camera1Selected(model);

        //_camera1Model = (CameraModel)cmbCamera1Names.SelectedItem;
        //cmbCamera1Resolutions.ItemsSource = _cameraListService.CameraResolution(_camera1Model.Name);
        //cmbCamera1Resolutions.DisplayMemberPath = "FormattedString";
    }

    private void cmbCamera1Resolutions_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (cmbCamera1Resolutions.SelectedItem is CameraDetail detail)
            Vm.Camera1ResolutionSelected(detail);

        //_camera1Detail = (CameraDetail)cmbCamera1Resolutions.SelectedItem;
        //_camera1Detail.Name = _camera1Model.Name;
        //_camera1Detail.Index = _camera1Model.Index;

        //_camera.SetCameraDetails(_camera1Detail);
    }

    private void cmbCamera2Names_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (cmbCamera2Names.SelectedItem is CameraModel model)
            Vm.Camera2Selected(model);

        //_camera2Model = (CameraModel)cmbCamera2Names.SelectedItem;
        //cmbCamera2Resolutions.ItemsSource = _cameraListService.CameraResolution(_camera2Model.Name);
        //cmbCamera2Resolutions.DisplayMemberPath = "FormattedString";
    }

    private void cmbCamera2Resolutions_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (cmbCamera2Resolutions.SelectedItem is CameraDetail detail)
            Vm.Camera2ResolutionSelected(detail);

        //_camera2Detail = (CameraDetail)cmbCamera2Resolutions.SelectedItem;
        //_camera2Detail.Name = _camera2Model.Name;
        //_camera2Detail.Index = _camera2Model.Index;

        //_camera.SetCameraDetails(_camera2Detail);
    }

    private void Slider_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        Vm.IsUserDraggingSlider = true;
    }

    private void Slider_PreviewMouseUp(object sender, MouseButtonEventArgs e)
    {
        Vm.IsUserDraggingSlider = false;
        Vm.SeekFrameCommand.Execute(Vm.SliderValue); // trigger seeking manually
        //Vm.StartPreviewCommand.Execute(null); // optional, resume preview
    }
}