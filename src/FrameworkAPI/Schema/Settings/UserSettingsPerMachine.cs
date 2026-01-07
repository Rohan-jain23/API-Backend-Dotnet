namespace FrameworkAPI.Schema.Settings;

/// <summary>
/// User settings which are set individually for each machine.
/// </summary>
public class UserSettingsPerMachine
{
#pragma warning disable
    private readonly string _machineId;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserSettingsPerMachine"/> class.
    /// </summary>
    public UserSettingsPerMachine(string machineId)
    {
        _machineId = machineId;
    }
}