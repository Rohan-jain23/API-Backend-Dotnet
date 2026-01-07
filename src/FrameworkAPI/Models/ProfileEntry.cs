namespace FrameworkAPI.Models;

public class ProfileEntry
{
    public string Name { get; set; }
    public double Value { get; set; }

    public ProfileEntry()
    {
        Name = string.Empty;
        Value = 0;
    }
}