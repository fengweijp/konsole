namespace Konsole
{
    /// <summary>
    /// get the host window size and current cursor Y position. X is always going to default to 0 if you've just executed a command to run
    /// a new console app.
    /// </summary>
    public interface IHostSizer
    {
        int Width { get; }
        int Height { get; }
        int CursorTop { get; }
    }
}
