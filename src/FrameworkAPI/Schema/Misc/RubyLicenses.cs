using System;
using System.Collections.Generic;
using WuH.Ruby.LicenceManager.Client;
using Applications = FrameworkAPI.Constants.LicensesApplications;

namespace FrameworkAPI.Schema.Misc;

/// <summary>
/// Status of a machines licenses for RUBY extensions and connection modules.
/// </summary>
public class RubyLicenses(Dictionary<string, LicenceValidationInfo> licenceValidationInfos)
{
    /// <summary>
    /// True, if the machine has a valid license for RUBY Anilox.
    /// </summary>
    public bool HasValidAniloxLicense
        => licenceValidationInfos.GetValueOrDefault(Applications.Anilox)?.IsValid ?? false;

    /// <summary>
    /// Timestamp on which the RUBY Anilox license expires.
    /// </summary>
    public DateTime? ExpiryDateOfAniloxLicense
        => licenceValidationInfos.GetValueOrDefault(Applications.Anilox)?.ExpiryDate;

    /// <summary>
    /// True, if the machine has a valid license for RUBY Check.
    /// </summary>
    public bool HasValidCheckLicense
        => licenceValidationInfos.GetValueOrDefault(Applications.Check)?.IsValid ?? false;

    /// <summary>
    /// Timestamp on which the RUBY Check license expires.
    /// </summary>
    public DateTime? ExpiryDateOfCheckLicense
        => licenceValidationInfos.GetValueOrDefault(Applications.Check)?.ExpiryDate;

    /// <summary>
    /// True, if the machine has a valid license for Connect 4 Flow.
    /// </summary>
    public bool HasValidConnect4FlowLicense
        => licenceValidationInfos.GetValueOrDefault(Applications.Connect4Flow)?.IsValid ?? false;

    /// <summary>
    /// Timestamp on which the Connect 4 Flow license expires.
    /// </summary>
    public DateTime? ExpiryDateOfConnect4FlowLicense
        => licenceValidationInfos.GetValueOrDefault(Applications.Connect4Flow)?.ExpiryDate;

    /// <summary>
    /// True, if the RUBY instance has a valid license for RUBY Go.
    /// </summary>
    public bool HasValidGoLicense
        => licenceValidationInfos.GetValueOrDefault(Applications.Go)?.IsValid ?? false;

    /// <summary>
    /// Timestamp on which the RUBY Go license expires.
    /// </summary>
    public DateTime? ExpiryDateOfGoLicense
        => licenceValidationInfos.GetValueOrDefault(Applications.Go)?.ExpiryDate;

    /// <summary>
    /// True, if the machine has a valid license for RUBY Track.
    /// </summary>
    public bool HasValidTrackLicense
        => licenceValidationInfos.GetValueOrDefault(Applications.Track)?.IsValid ?? false;

    /// <summary>
    /// Timestamp on which the RUBY Track license expires.
    /// </summary>
    public DateTime? ExpiryDateOfTrackLicense
        => licenceValidationInfos.GetValueOrDefault(Applications.Track)?.ExpiryDate;
}