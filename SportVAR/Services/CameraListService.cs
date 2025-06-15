using AForge.Video.DirectShow;
using SportVAR.Models;

namespace SportVAR.Services;

public class CameraListService : ICameraListService
{
    public List<CameraModel> CameraNames()
    {
        var videoDevices = GetCameras();
        var index = 0;
        
        return videoDevices
                     .Cast<FilterInfo>()
                     .OrderByDescending(x => x)
                     .Select(d => new CameraModel { Index = index++, Name = d.Name, MonikerString = d.MonikerString})
                     .ToList();
    }

    public List<CameraDetail> CameraResolution(string monikerString)
    {
        var videoDevice = GetCameras().Cast<FilterInfo>().FirstOrDefault(x => x.MonikerString == monikerString);
        var videoSource = new VideoCaptureDevice(videoDevice!.MonikerString);

        return videoSource.VideoCapabilities.Select(x => new CameraDetail()
                                                         {
                                                             Width = x.FrameSize.Width,
                                                             Height = x.FrameSize.Height,
                                                             Fps = x.AverageFrameRate
                                                         })
                          .ToList();
    }

    private static FilterInfoCollection GetCameras()
    {
        var videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
        if (videoDevices == null) throw new ArgumentNullException(nameof(videoDevices));
        return videoDevices;
    }
}