using SportVAR.Models;
using System.Collections.ObjectModel;

namespace SportVAR.Services;

public interface ICameraConfigurator
{
    //List<CameraDetail> CameraSelected(CameraModel model);

    //void CameraResolutionSelected(CameraDetail detail);

    Task<ObservableCollection<CameraDetail>> GetCameraResolutions(CameraModel model);

    CameraDetail SetSelectedResolution(CameraModel model, CameraDetail detail);
}