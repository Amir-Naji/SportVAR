using SportVAR.Models;

namespace SportVAR.Services;

public interface ICameraListService
{
    List<CameraModel> CameraNames();

    List<CameraDetail> CameraResolution(string cameraName);
}