using AForge.Video.DirectShow;
using DirectShowLib;
using SportVAR.Models;
using FilterCategory = AForge.Video.DirectShow.FilterCategory;
using FilterInfo = AForge.Video.DirectShow.FilterInfo;

namespace SportVAR.Services;

public class CameraListService : ICameraListService
{
    public List<CameraModel> CameraNames()
    {
        DsDevice[] systemCameras = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
        var index = 0;

        return systemCameras.Select(camera =>
                                            new CameraModel
                                            {
                                                Name = camera.Name,
                                                MonikerString = camera.DevicePath,
                                                Index = index++
                                            }).ToList();
    }

    public async Task<List<CameraDetail>> CameraResolution(string monikerString)
    {
        return await Task.Run(() =>
                              {
                                  var videoDevice = GetCameras().Cast<FilterInfo>()
                                                                .FirstOrDefault(x => x.MonikerString == monikerString);
                                  if (videoDevice == null) return [];

                                  var videoSource = new VideoCaptureDevice(videoDevice.MonikerString);

                                  // Defensive: Check null or empty
                                  var caps = videoSource.VideoCapabilities;
                                  if (caps == null || caps.Length == 0)
                                      return [];

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