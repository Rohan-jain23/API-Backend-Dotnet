using System;
using WuH.Ruby.AlarmDataHandler.Client;
using WuH.Ruby.Common.Core;

namespace FrameworkAPI.Schema.Misc;

/// <summary>
///
/// </summary>
public class MachineAlarm(Alarm alarmDataHandlerItem, string languageTag)
{

    /// <summary>
    /// The internal ObjectId of this alarm.
    /// </summary>
    public string Id { get; set; } = alarmDataHandlerItem.Id;

    /// <summary>
    /// Unique identifier (usually WuH equipment number, like: "EQ12345") of the machine this alarm occurred on.
    /// [Source: Machine]
    /// </summary>
    public string MachineId { get; set; } = alarmDataHandlerItem.MachineId;

    /// <summary>
    /// The timestamp this alarm started on the machine.
    /// </summary>
    public DateTime Start { get; set; } = alarmDataHandlerItem.StartTimestamp;

    /// <summary>
    /// The timestamp this alarm ended on the machine.
    /// If null, this alarm is still active.
    /// </summary>
    public DateTime? End { get; set; } = alarmDataHandlerItem.EndTimestamp;

    /// <summary>
    /// The combined code of this alarm (Format: ModuleId-AlarmNumber).
    /// </summary>
    public string AlarmCode { get; set; } = alarmDataHandlerItem.AlarmCode;

    /// <summary>
    /// The severity of this alarm as a localized text.
    /// </summary>
    public string AlarmLevel { get; set; } = DisplayNameHelper.GetTextInLanguageOrFallBackToEnglish(alarmDataHandlerItem.AlarmLevel, languageTag);

    /// <summary>
    /// The localized description of this alarm number.
    /// </summary>
    public string AlarmText { get; set; } = DisplayNameHelper.GetTextInLanguageOrFallBackToEnglish(alarmDataHandlerItem.AlarmText, languageTag);

    /// <summary>
    /// The localized name of the machine module this alarm is related to.
    /// </summary>
    public string ModuleName { get; set; } = DisplayNameHelper.GetTextInLanguageOrFallBackToEnglish(alarmDataHandlerItem.ModuleName, languageTag);
}