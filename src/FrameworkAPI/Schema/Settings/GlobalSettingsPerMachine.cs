namespace FrameworkAPI.Schema.Settings;

/// <summary>
/// Global settings which are set individually for each machine.
/// </summary>
public class GlobalSettingsPerMachine
{
#pragma warning disable
    private readonly string _machineId;

    /// <summary>
    /// Initializes a new instance of the <see cref="GlobalSettingsPerMachine"/> class.
    /// </summary>
    public GlobalSettingsPerMachine(string machineId)
    {
        _machineId = machineId;
    }
}