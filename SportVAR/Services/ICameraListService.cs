using SportVAR.Models;

namespace SportVAR.Services;

public interface ICameraListService
{
    List<CameraModel> CameraNames();

    Task<List<CameraDetail>> CameraResolution(string monikerString);
}