namespace SportVAR.Models;

public record CameraDetail
{
    public int Index { get; set; }

    public string Name { get; set; }

    public string MonikerString { get; set; }

    public int Width { get; set; }

    public int Height { get; set; }

    public int Fps { get; set; }

    public bool IsSelected => !string.IsNullOrEmpty(Name) && Width > 0 && Height > 0 && Fps > 0;

    public string FormattedString => $"{Width} x {Height} @ {Fps}fps";
}