using SportVAR.Models;
using System.Collections.ObjectModel;

namespace SportVAR.Services;

public class CameraConfigurator(ICameraListService cameraListService, ICameraService cameraService) : ICameraConfigurator
{
    private readonly List<CameraDetail> _camera1Resolutions = new();
    
    private CameraDetail _camera1Detail = new();
    private CameraModel _camera1Model = new();

    //public List<CameraDetail> CameraSelected(CameraModel model)
    //{
    //    _camera1Model = model;
    //    _camera1Resolutions.Clear();
    //    foreach (var res in cameraListService.CameraResolution(model.MonikerString))
    //        _camera1Resolutions.Add(res);

    //    return _camera1Resolutions;
    //}

    //public void CameraResolutionSelected(CameraDetail detail)
    //{
    //    _camera1Detail = detail;
    //    _camera1Detail.Name = _camera1Model.Name;
    //    _camera1Detail.Index = _camera1Model.Index;
    //    _camera1Detail.MonikerString = _camera1Model.MonikerString;
    //    cameraService.SetCameraDetails(_camera1Detail);
    //}

    private void LoadCameras()
    {
        //var list = cameraListService.CameraNames();
        //foreach (var cam in list)
        //{
        //    Camera1Options.Add(cam);
        //    Camera2Options.Add(cam);
        //}
    }

    public async  Task<ObservableCollection<CameraDetail>> GetCameraResolutions(CameraModel model)
    {
        var details = cameraListService.CameraResolution(model.MonikerString);
        return new ObservableCollection<CameraDetail>(await details);
    }

    public CameraDetail SetSelectedResolution(CameraModel model, CameraDetail detail)
    {
        detail.Name = model.Name;
        detail.Index = model.Index;
        detail.MonikerString = model.MonikerString;

        cameraService.SetCameraDetails(detail);
        return detail;
    }
}