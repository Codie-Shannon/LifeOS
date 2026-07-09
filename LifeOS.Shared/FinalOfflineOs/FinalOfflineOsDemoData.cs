using LifeOS.Core.FinalOfflineOs;

namespace LifeOS.Shared.FinalOfflineOs;

public static class FinalOfflineOsDemoData
{
    public static FinalOfflineOsProfile CreateDefaultProfile()
    {
        return new FinalOfflineOsProfile
        {
            Version = "v3.9",
            Mode = "Final offline OS foundation",
            LocalFirstComplete = true,
            ReadyForV4Integrations = true,
            ExternalIntegrationsEnabled = false,
            AiAssistantEnabled = false,
            MajorUiReshapeDeferred = true,
            ScreenshotDocsCurrent = true,
            DemoSafeDataRequired = true,
            Notes = "Final offline OS checkpoint before v4 integrations.",
            Checkpoints =
            [
                Checkpoint("Command Centre absorbs module signals", OfflineOsArea.Command, OfflineReadinessStatus.Ready, 1, true, "Command Centre shows pressure, daily flow, release, spine, navigation, and knowledge signals.", "Use as the first stop before integrations."),
                Checkpoint("Money and paid work stay separated", OfflineOsArea.Money, OfflineReadinessStatus.Ready, 1, true, "Expected money, pending manual income, invoice-ready work, and safe-to-spend are separated.", "Integrations must not mark expected money as safe."),
                Checkpoint("Proof and evidence are local-first", OfflineOsArea.Proof, OfflineReadinessStatus.Ready, 1, true, "Evidence Vault, Proof Tracker, Timesheet Evidence, docs, and screenshots prove local proof handling.", "Files integrations should feed proof/evidence state."),
                Checkpoint("Relationship state is manual-review", OfflineOsArea.Relationship, OfflineReadinessStatus.Ready, 1, true, "Relationship Radar and Follow-Ups track waiting-on/do-not-chase without reading real inboxes.", "Email integration must require confirmation before actions."),
                Checkpoint("Daily flow controls next safe action", OfflineOsArea.Daily, OfflineReadinessStatus.Ready, 2, true, "Daily Operating Flow, Agenda, and Daily State are distinct.", "Calendar integrations should feed daily pressure, not take control."),
                Checkpoint("Settings and safety guardrails exist", OfflineOsArea.Safety, OfflineReadinessStatus.Ready, 1, true, "Local-only, manual review, expected-money safety, screenshot privacy, and destructive-action rules exist.", "Keep guardrails active before v4."),
                Checkpoint("Universal Spine links module context", OfflineOsArea.Spine, OfflineReadinessStatus.Ready, 1, true, "Universal Spine connects work, money, proof, relationships, daily flow, release, safety, and knowledge.", "Use as the integration context spine."),
                Checkpoint("OS Navigation protects core modules", OfflineOsArea.Navigation, OfflineReadinessStatus.Ready, 2, true, "Scrollable sidebar, protected modules, workspace groups, planned modules, and review modules are mapped.", "Keep current page model until workflows prove redesign needs."),
                Checkpoint("Search / Knowledge has local shape", OfflineOsArea.Knowledge, OfflineReadinessStatus.Ready, 2, true, "Search profiles, knowledge items, source boundaries, and review/planned states exist.", "Integrations should feed this layer later."),
                Checkpoint("Release docs and screenshots are current", OfflineOsArea.Release, OfflineReadinessStatus.ReviewNeeded, 1, true, "v3.9 code checkpoint requires final Group 09 screenshots and docs before tag.", "Capture final screenshots, package docs, tag v3.9."),
                Checkpoint("External integrations are deliberately off", OfflineOsArea.Integration, OfflineReadinessStatus.PlannedForV4, 1, true, "v3.9 defines landing zones but does not connect real services.", "Start v4 integrations after v3.9 tag."),
                Checkpoint("Major UI reshape is deliberately deferred", OfflineOsArea.Navigation, OfflineReadinessStatus.PlannedForV4, 3, false, "Large workspace redesign waits until after integrations and AI assistant usage prove real workflows.", "Do not reshape UI before v7/v8 boundary.")
            ],
            LandingZones =
            [
                LandingZone("Outlook / Gmail", IntegrationLandingZoneType.Email, "Relationship Radar / Follow-Ups", "Universal Spine links email threads to work, relationships, proof, and follow-up state.", "Manual review before send; no automatic chasing."),
                LandingZone("Calendar", IntegrationLandingZoneType.Calendar, "Daily Operating Flow / Agenda", "Calendar pressure feeds daily flow and command signals.", "Calendar entries inform the day; they do not auto-control it."),
                LandingZone("SharePoint / Drive / local files", IntegrationLandingZoneType.Files, "Evidence Vault / Proof Tracker", "File metadata feeds proof, evidence, docs, and release screenshots.", "No private URLs or secrets in screenshots/docs."),
                LandingZone("Xero / accounting exports", IntegrationLandingZoneType.Accounting, "Paid Work Centre / Money Pressure", "Accounting state feeds paid/unpaid/pending money without overriding safe-money rules.", "Expected money is not safe until confirmed paid."),
                LandingZone("TimerAgent / work logs", IntegrationLandingZoneType.Timer, "Work Sessions / Timesheet Evidence", "Timer/work logs feed work-session proof and timesheet evidence.", "Manual review before client/admin wording."),
                LandingZone("Docs / notes / knowledge sources", IntegrationLandingZoneType.Knowledge, "Search / Knowledge Centre", "Docs and notes feed local search profiles and knowledge items.", "Search hits require review before becoming final truth."),
                LandingZone("Manual imports", IntegrationLandingZoneType.ManualImport, "Universal Spine / Evidence Vault", "CSV/JSON/manual files can be imported into local modules before real connectors.", "Validate source and privacy before importing."),
                LandingZone("Future APIs", IntegrationLandingZoneType.FutureApi, "OS Navigation / Universal Spine", "Future APIs should land through defined modules and spine links.", "No bypassing guardrails or direct automation.")
            ]
        };
    }

    private static OfflineOsCheckpoint Checkpoint(
        string title,
        OfflineOsArea area,
        OfflineReadinessStatus status,
        int priority,
        bool requiredForV4,
        string evidence,
        string nextAction)
    {
        return new OfflineOsCheckpoint
        {
            Title = title,
            Area = area,
            Status = status,
            Priority = priority,
            RequiredForV4 = requiredForV4,
            Evidence = evidence,
            NextAction = nextAction,
            Boundary = "Offline/local-first checkpoint. Real integrations start in v4.",
            UpdatedAt = DateTime.Now
        };
    }

    private static IntegrationLandingZone LandingZone(
        string sourceName,
        IntegrationLandingZoneType zoneType,
        string targetModule,
        string spineConnection,
        string safetyRule)
    {
        return new IntegrationLandingZone
        {
            SourceName = sourceName,
            ZoneType = zoneType,
            TargetModule = targetModule,
            SpineConnection = spineConnection,
            Status = OfflineReadinessStatus.PlannedForV4,
            SafetyRule = safetyRule,
            Notes = "Landing zone only. No real connector is active in v3.9.",
            UpdatedAt = DateTime.Now
        };
    }
}
