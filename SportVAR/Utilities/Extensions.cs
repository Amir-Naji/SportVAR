namespace SportVAR.Utilities;

public static class Extensions
{
    public static bool IsNull(this object? input)
    {
        return input == null;
    }

    public static bool IsNotNull(this object? input)
    {
        return input != null;
    }
}