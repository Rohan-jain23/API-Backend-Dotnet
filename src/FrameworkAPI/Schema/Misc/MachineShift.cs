using System;

namespace FrameworkAPI.Schema.Misc;

/// <summary>
/// An actual shift, derived from the shift and time zone settings.
/// Usually, the machine is operated by different operators during the day.
/// The period of time one operator is working on the machine is called 'shift'.
/// </summary>
public class MachineShift(string shiftName, DateTime startTime, DateTime endTime, string? mainOperatorName)
{
    /// <summary>
    /// Friendly name for the shift that is entered by the user.
    /// This name does not need to be localized as the user will enter the name in it's language.
    /// </summary>
    public string ShiftName { get; set; } = shiftName;

    /// <summary>
    /// Start timestamp of the shift in UTC.
    /// </summary>
    public DateTime StartTime { get; set; } = startTime;

    /// <summary>
    /// End timestamp of the shift in UTC.
    /// If this shift is currently active, this will be the machine time.
    /// </summary>
    public DateTime EndTime { get; set; } = endTime;

    /// <summary>
    /// Name of the user that was logged-in during the shift at the Operator UI.
    /// Users that login for the first time in the 30 minutes before the shift change,
    /// will be mapped to the following shift.
    /// </summary>
    public string? MainOperatorName { get; set; } = mainOperatorName;
}
