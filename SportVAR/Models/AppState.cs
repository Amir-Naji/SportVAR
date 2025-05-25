using System.Security.AccessControl;

namespace SportVAR.Models;

public record AppState()
{
    public bool IsRecording { get; set; }
    
    public bool IsPlaying { get; set; }

    public bool IsReviewing { get; set; }

    public bool IsPreviewing => !IsRecording && !IsPlaying;
    
    public int CurrentFrameIndex { get; set; }

    public bool IsUserDraggingSlider { get; set; }


    //public int PlaybackIndex { get; set; }
}