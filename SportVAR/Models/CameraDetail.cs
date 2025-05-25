namespace SportVAR.Models;

public record CameraDetail
{
    public int Index { get; set; }

    public string Name { get; set; }

    public int Width { get; set; }

    public int Height { get; set; }

    public int Fps { get; set; }

    public string FormattedString => $"{Width} x {Height} @ {Fps}fps";
}