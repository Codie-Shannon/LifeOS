namespace LifeOS.Core.FinalOfflineOs;

public static class FinalOfflineOsCalculator
{
    public static FinalOfflineOsSummary Calculate(FinalOfflineOsProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        var checkpoints = profile.Checkpoints ?? [];
        var landingZones = profile.LandingZones ?? [];

        var ready = checkpoints.Count(checkpoint => checkpoint.Status == OfflineReadinessStatus.Ready);
        var review = checkpoints.Count(checkpoint => checkpoint.Status == OfflineReadinessStatus.ReviewNeeded);
        var required = checkpoints.Count(checkpoint => checkpoint.RequiredForV4);
        var readyZones = landingZones.Count(zone => zone.Status is OfflineReadinessStatus.Ready or OfflineReadinessStatus.PlannedForV4);
        var planned = landingZones.Count(zone => zone.Status == OfflineReadinessStatus.PlannedForV4);

        var reasons = new List<string>();

        if (profile.LocalFirstComplete)
        {
            reasons.Add("The offline/local-first foundation is complete enough to become the v4 integration base.");
        }

        if (profile.ReadyForV4Integrations)
        {
            reasons.Add("Integration landing zones are defined so external data has somewhere controlled to land.");
        }

        if (!profile.ExternalIntegrationsEnabled)
        {
            reasons.Add("External integrations remain off in v3.9; v4 starts the real connection work.");
        }

        if (!profile.AiAssistantEnabled)
        {
            reasons.Add("AI assistant execution remains off; assistant workflows come after integrations and hardening.");
        }

        if (profile.MajorUiReshapeDeferred)
        {
            reasons.Add("Major UI/workspace reshape remains deferred until integrations and AI assistant usage prove the real workflows.");
        }

        if (profile.ScreenshotDocsCurrent)
        {
            reasons.Add("Screenshot groups, release notes, current status, README, and version history should be current before tagging v3.9.");
        }

        if (profile.DemoSafeDataRequired)
        {
            reasons.Add("Demo-safe data remains required: no real names, emails, IDs, URLs, payment details, or secrets.");
        }

        if (review > 0)
        {
            reasons.Add($"{review} checkpoint(s) still need review before treating the offline OS as fully closed.");
        }

        if (landingZones.Count > 0)
        {
            reasons.Add($"{landingZones.Count} v4 integration landing zone(s) are mapped.");
        }

        return new FinalOfflineOsSummary
        {
            Version = profile.Version,
            Mode = profile.Mode,
            TotalCheckpoints = checkpoints.Count,
            ReadyCheckpoints = ready,
            ReviewNeededCheckpoints = review,
            RequiredForV4 = required,
            LandingZones = landingZones.Count,
            LandingZonesReadyForV4 = readyZones,
            PlannedForV4 = planned,
            AreaCount = checkpoints.Select(checkpoint => checkpoint.Area).Distinct().Count(),
            LocalFirstComplete = profile.LocalFirstComplete,
            ReadyForV4Integrations = profile.ReadyForV4Integrations,
            ExternalIntegrationsEnabled = profile.ExternalIntegrationsEnabled,
            AiAssistantEnabled = profile.AiAssistantEnabled,
            MajorUiReshapeDeferred = profile.MajorUiReshapeDeferred,
            ScreenshotDocsCurrent = profile.ScreenshotDocsCurrent,
            Reasons = reasons,
            RequiredCheckpoints = checkpoints
                .Where(checkpoint => checkpoint.RequiredForV4)
                .OrderBy(checkpoint => checkpoint.Priority)
                .ThenBy(checkpoint => checkpoint.Area)
                .ToList(),
            ReviewItems = checkpoints
                .Where(checkpoint => checkpoint.Status != OfflineReadinessStatus.Ready)
                .OrderBy(checkpoint => checkpoint.Priority)
                .ThenBy(checkpoint => checkpoint.Area)
                .ToList(),
            IntegrationLandingZones = landingZones
                .OrderBy(zone => zone.ZoneType)
                .ThenBy(zone => zone.SourceName)
                .ToList()
        };
    }

    public static string FormatArea(OfflineOsArea area)
    {
        return area switch
        {
            OfflineOsArea.Command => "Command",
            OfflineOsArea.Money => "Money",
            OfflineOsArea.Work => "Work",
            OfflineOsArea.Proof => "Proof",
            OfflineOsArea.Relationship => "Relationship",
            OfflineOsArea.Daily => "Daily",
            OfflineOsArea.Safety => "Safety",
            OfflineOsArea.Spine => "Spine",
            OfflineOsArea.Navigation => "Navigation",
            OfflineOsArea.Knowledge => "Knowledge",
            OfflineOsArea.Release => "Release",
            OfflineOsArea.Integration => "Integration",
            _ => area.ToString()
        };
    }

    public static string FormatStatus(OfflineReadinessStatus status)
    {
        return status switch
        {
            OfflineReadinessStatus.Ready => "Ready",
            OfflineReadinessStatus.ReviewNeeded => "Review needed",
            OfflineReadinessStatus.PlannedForV4 => "Planned for v4",
            OfflineReadinessStatus.Blocked => "Blocked",
            OfflineReadinessStatus.NotIncluded => "Not included",
            _ => status.ToString()
        };
    }

    public static string FormatLandingZoneType(IntegrationLandingZoneType zoneType)
    {
        return zoneType switch
        {
            IntegrationLandingZoneType.Email => "Email",
            IntegrationLandingZoneType.Calendar => "Calendar",
            IntegrationLandingZoneType.Files => "Files",
            IntegrationLandingZoneType.Accounting => "Accounting",
            IntegrationLandingZoneType.Timer => "Timer",
            IntegrationLandingZoneType.Knowledge => "Knowledge",
            IntegrationLandingZoneType.ManualImport => "Manual import",
            IntegrationLandingZoneType.FutureApi => "Future API",
            _ => zoneType.ToString()
        };
    }
}
