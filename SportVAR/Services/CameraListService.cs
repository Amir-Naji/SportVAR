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

    public async Task<List<CameraDetail>> CameraResolution(string monikerString)
    {
        return await Task.Run(() =>
                              {
                                  var videoDevice = GetCameras().Cast<FilterInfo>().FirstOrDefault(x => x.MonikerString == monikerString);
                                  if (videoDevice == null) return new List<CameraDetail>();

                                  var videoSource = new VideoCaptureDevice(videoDevice.MonikerString);

                                  // Defensive: Check null or empty
                                  var caps = videoSource.VideoCapabilities;
                                  if (caps == null || caps.Length == 0)
                                      return new List<CameraDetail>();

                                  return videoSource.VideoCapabilities.Select(x => new CameraDetail
                                                                                   {
                                                                                       Width = x.FrameSize.Width,
                                                                                       Height = x.FrameSize.Height,
                                                                                       Fps = x.AverageFrameRate
                                                                                   }).ToList();
                              });
    }

    private static FilterInfoCollection GetCameras()
    {
        var videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
        if (videoDevices == null) throw new ArgumentNullException(nameof(videoDevices));
        return videoDevices;
    }
}