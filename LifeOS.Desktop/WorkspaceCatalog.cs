using System;
using System.Collections.Generic;
using System.Linq;

namespace LifeOS.Desktop;

internal sealed record WorkspaceMetricDefinition(
    string Label,
    string MetricKey,
    string FallbackValue,
    string Detail);

internal sealed record WorkspaceModuleDefinition(
    string Id,
    string Title,
    string Status,
    string Description,
    string? RouteId,
    string ActionLabel)
{
    public bool CanOpen => !string.IsNullOrWhiteSpace(RouteId);

    public string AutomationId => $"Workspace.Module.{Id}";
}

internal sealed record WorkspaceSectionDefinition(
    string Title,
    string Description,
    IReadOnlyList<WorkspaceModuleDefinition> Modules);

internal sealed record WorkspaceDefinition(
    string Name,
    string DisplayTitle,
    string Subtitle,
    string Eyebrow,
    string Description,
    string ContextSummary,
    IReadOnlyList<WorkspaceMetricDefinition> Metrics,
    IReadOnlyList<WorkspaceSectionDefinition> Sections);

internal sealed record WorkspaceMetricView(
    string Label,
    string Value,
    string Detail);

internal static class WorkspaceCatalog
{
    private static WorkspaceMetricDefinition Metric(
        string label,
        string key,
        string fallback,
        string detail) =>
        new(label, key, fallback, detail);

    private static WorkspaceModuleDefinition Route(
        string routeId,
        string title,
        string status,
        string description,
        string actionLabel = "Open module") =>
        new(routeId, title, status, description, routeId, actionLabel);

    private static WorkspaceModuleDefinition Native(
        string id,
        string title,
        string status,
        string description) =>
        new(id, title, status, description, null, "Native workspace surface");

    private static WorkspaceModuleDefinition Navigate(
        string workspace,
        string title,
        string status,
        string description) =>
        new(
            $"home-{workspace.ToLowerInvariant()}",
            title,
            status,
            description,
            $"workspace:{workspace}",
            $"Open {workspace}");

    public static IReadOnlyList<WorkspaceDefinition> All { get; } =
        new WorkspaceDefinition[]
        {
            new(
                "Home",
                "Good afternoon, Codie.",
                "Balanced operating overview",
                "TODAY / PRESSURE / WORK / MONEY",
                "One calm view of immediate pressure, review highlights, paid work and upcoming money.",
                "Home stays summary-first. Editing remains inside the owning workspace or preserved module so there is only one authoritative state.",
                new WorkspaceMetricDefinition[]
                {
                    Metric("Agenda", "agenda", "0", "Trusted local records"),
                    Metric("Follow-ups", "follow-ups", "0", "Review attention"),
                    Metric("Work pipeline", "work-pipeline", "0", "Active records"),
                    Metric("Pay later", "pay-later", "0", "Upcoming obligations")
                },
                new WorkspaceSectionDefinition[]
                {
                    new(
                        "Immediate operating view",
                        "Home links into the final workspaces without creating duplicate editors.",
                        new WorkspaceModuleDefinition[]
                        {
                            Route("command-centre", "Command Centre", "Preserved behaviour", "Open the existing pressure, safe-to-spend, agenda, follow-up and pipeline checkpoint."),
                            Navigate("Work", "Work review", "Workspace", "Clients, waiting-on, follow-ups, sessions, timesheets and paid-work records."),
                            Navigate("Money", "Money upcoming", "Workspace", "Weekly money, bills, payment timing and expected-income review."),
                            Navigate("Life", "Today and routines", "Workspace", "Agenda, daily state, routines and personal operating flow.")
                        })
                }),
            new(
                "Work",
                "Work",
                "Clients, follow-ups and paid work",
                "CLIENT DELIVERY",
                "Clients, waiting-on items, follow-ups, work sessions, timesheets, invoices and work records now share one permanent workspace.",
                "Work owns client-delivery records. Career activity remains separate, and Money owns personal financial control.",
                new WorkspaceMetricDefinition[]
                {
                    Metric("Pipeline", "work-pipeline", "0", "Canonical records"),
                    Metric("Follow-ups", "follow-ups", "0", "Waiting and next action"),
                    Metric("Sessions", "work-sessions", "0", "Time evidence"),
                    Metric("Timesheets", "timesheets", "0", "Preserved behaviour")
                },
                new WorkspaceSectionDefinition[]
                {
                    new(
                        "Clients and delivery",
                        "Open the same persisted v7 records through focused workspace routes.",
                        new WorkspaceModuleDefinition[]
                        {
                            Route("work-pipeline", "Work Pipeline", "Canonical records", "Active work, opportunities, stages, waiting-on state, invoice readiness and expected value."),
                            Route("follow-ups", "Follow-Ups", "Canonical records", "People, contexts, due dates, priority, money links and next actions."),
                            Route("paid-work-centre", "Paid Work Centre", "Preserved behaviour", "Billable work, invoice readiness, payment state and proof-led delivery."),
                            Route("work-sessions", "Work Sessions", "Canonical records", "Hours, rates, billable state, descriptions and session evidence."),
                            Route("timesheet-evidence", "Timesheet Evidence", "Preserved behaviour", "Review timesheet-ready work without copying or rewriting source records.")
                        })
                }),
            new(
                "Career",
                "Career",
                "Career stays separate from Work",
                "PROFILE / APPLICATIONS",
                "Profile, CVs, applications, interviews and career follow-ups are organised without mixing employment activity into client delivery.",
                "Career has its own information architecture. Native surfaces do not invent records; existing relationship follow-ups remain available through their canonical store.",
                new WorkspaceMetricDefinition[]
                {
                    Metric("Relationships", "relationships", "0", "Career follow-up source"),
                    Metric("CVs", "", "Ready", "Native surface"),
                    Metric("Applications", "", "Ready", "Native surface"),
                    Metric("Interviews", "", "Ready", "Native surface")
                },
                new WorkspaceSectionDefinition[]
                {
                    new(
                        "Career studio foundation",
                        "The permanent separation is established now; richer Career Studio editing remains later roadmap work.",
                        new WorkspaceModuleDefinition[]
                        {
                            Native("career-profile", "Profile", "Native workspace surface", "A dedicated home for role direction, strengths and career positioning without client-work fields."),
                            Native("career-cvs", "CVs", "Native workspace surface", "A distinct collection surface for CV versions and role-specific evidence."),
                            Native("career-applications", "Applications", "Native workspace surface", "Application status and next-action structure, separate from Work Pipeline."),
                            Native("career-interviews", "Interviews", "Native workspace surface", "Interview preparation and follow-up structure without inventing stored records."),
                            Route("relationship-radar", "Relationship Radar", "Canonical records", "Use existing relationship and follow-up records where they support career activity.")
                        })
                }),
            new(
                "Money",
                "Money",
                "Income, bills and weekly control",
                "WEEKLY MONEY CONTROL",
                "Weekly money, expected income, bills, payment timing, evidence and pressure signals now live together.",
                "Money owns personal financial control. Expected income remains unsafe until confirmed through trusted LifeOS state.",
                new WorkspaceMetricDefinition[]
                {
                    Metric("Pay later", "pay-later", "0", "Upcoming obligations"),
                    Metric("Receipts", "receipts", "0", "Evidence records"),
                    Metric("Payment calendar", "", "Open", "Timing view"),
                    Metric("Safe money", "", "Review", "Trust boundary")
                },
                new WorkspaceSectionDefinition[]
                {
                    new(
                        "Money control",
                        "The workspace groups financial views while preserving their existing persisted models and safety rules.",
                        new WorkspaceModuleDefinition[]
                        {
                            Route("money-pressure", "Money Pressure", "Canonical state", "Current balance, paid and pending income, obligations, deductions and safe-to-spend pressure."),
                            Route("money-timeline", "Money Timeline", "Preserved behaviour", "Chronological money events and expected-income timing."),
                            Route("bills-payments", "Bills / Payments", "Canonical state", "Bills, obligations, status and review windows."),
                            Route("money-profile", "Money Profile", "Canonical state", "Buffers, hidden deductions and safe-to-spend planning."),
                            Route("payment-calendar", "Payment Calendar", "Preserved behaviour", "Due dates, expected income and payment attention in one calendar view."),
                            Route("pay-later", "Pay Later", "Canonical records", "BNPL obligations, due dates, pressure and notes."),
                            Route("receipt-evidence", "Receipt Evidence", "Canonical records", "Receipts and payment proof retained as evidence.")
                        })
                }),
            new(
                "Life",
                "Life",
                "Personal operating systems",
                "ROUTINES / HOUSEHOLD / ADMIN",
                "Goals, routines, family, household, road trips and personal administration share one life workspace.",
                "Life owns personal operating state. Client delivery, career activity and money control remain in their dedicated workspaces.",
                new WorkspaceMetricDefinition[]
                {
                    Metric("Agenda", "agenda", "0", "Immediate commitments"),
                    Metric("Weekly reviews", "weekly-close-out", "0", "Close-out records"),
                    Metric("Daily state", "", "Visible", "Personal context"),
                    Metric("Flow", "", "Manual", "No background action")
                },
                new WorkspaceSectionDefinition[]
                {
                    new(
                        "Personal operations",
                        "Existing modules are grouped into a clearer personal operating loop.",
                        new WorkspaceModuleDefinition[]
                        {
                            Route("agenda", "Agenda", "Canonical records", "Commitments, dates, time, pressure, status and notes."),
                            Route("daily-state", "Daily State", "Preserved behaviour", "Current personal state and pressure context."),
                            Route("daily-operating-flow", "Daily Operating Flow", "Preserved behaviour", "A manual, visible operating sequence for the day."),
                            Route("weekly-close-out", "Weekly Close-Out", "Canonical records", "Done, moved, waiting and next-week review."),
                            Route("timer-agent", "TimerAgent", "Preserved utility", "Desktop-only timing utility retained inside the Life workspace.")
                        })
                }),
            new(
                "Projects",
                "Projects",
                "Milestones, evidence and delivery",
                "ACTIVE PROJECTS / PROOF",
                "Active projects, milestones, proof and delivery evidence now share one workspace.",
                "Projects owns project delivery and proof. Future DevOps views may join here without becoming another core page.",
                new WorkspaceMetricDefinition[]
                {
                    Metric("Proof items", "proof", "0", "Canonical records"),
                    Metric("Projects", "", "Active", "Core module"),
                    Metric("Evidence vault", "", "Linked", "Preserved behaviour"),
                    Metric("DevOps", "", "Later", "No early scope")
                },
                new WorkspaceSectionDefinition[]
                {
                    new(
                        "Delivery and evidence",
                        "Milestones and evidence are grouped without changing the underlying record stores.",
                        new WorkspaceModuleDefinition[]
                        {
                            Route("projects", "Projects", "Canonical module", "Active project records and delivery structure."),
                            Route("proof-tracker", "Proof Tracker", "Canonical records", "Project proof, evidence status, links and notes."),
                            Route("evidence-vault", "Evidence Vault", "Preserved behaviour", "Cross-module evidence and provenance review.")
                        })
                }),
            new(
                "Assistant",
                "Assistant",
                "Source-backed, review-first help",
                "ANSWERS / PLANS / MEMORY / REVIEW",
                "Source-backed answers, review-only plans, explicit memory and intake review remain bounded and visible.",
                "Assistant behaviour is preserved: no execution, approval, confirmation, connector write or hidden background work.",
                new WorkspaceMetricDefinition[]
                {
                    Metric("Inbox", "integration-inbox", "0", "Untrusted review records"),
                    Metric("Sources", "", "Bounded", "Permission controlled"),
                    Metric("Plans", "", "Review-only", "Executable: No"),
                    Metric("Memory", "", "Explicit", "Revocable and auditable")
                },
                new WorkspaceSectionDefinition[]
                {
                    new(
                        "Assistant and review",
                        "The existing v7 capability remains the authoritative implementation beneath the new workspace.",
                        new WorkspaceModuleDefinition[]
                        {
                            Route("assistant", "Assistant", "Preserved safety boundary", "Source-backed answers, evidence disclosure, uncertainty and review-only planning."),
                            Route("memory", "Memory", "Explicit and revocable", "User-confirmed memory with scope, audit, expiry, revocation and deletion."),
                            Route("search-knowledge", "Search / Knowledge", "Preserved behaviour", "Local search and knowledge surfaces."),
                            Route("integration-inbox", "Integration Inbox", "Review-first", "Untrusted imported records remain separate from trusted operational state."),
                            Route("email-radar", "Email Radar", "Read-only review", "Local email-radar evidence and review without sending or external mutation.")
                        })
                }),
            new(
                "Settings",
                "Settings",
                "System behaviour only",
                "SYSTEM CONTROL",
                "Settings contains system behaviour, safety, diagnostics and release surfaces—not operational records.",
                "Group 44 organises existing system capability only. Final preference persistence, full themes, accessibility and legacy removal remain Group 45.",
                new WorkspaceMetricDefinition[]
                {
                    Metric("System scope", "", "Only", "No operational dumping ground"),
                    Metric("Emergency Stop", "", "Preserved", "Authoritative boundary"),
                    Metric("Legacy routes", "", "Internal", "Removed in Group 45"),
                    Metric("v8 release", "", "Group 45", "Not closed early")
                },
                new WorkspaceSectionDefinition[]
                {
                    new(
                        "System behaviour and diagnostics",
                        "Existing platform and safety modules are grouped while final Settings work remains explicitly deferred.",
                        new WorkspaceModuleDefinition[]
                        {
                            Route("settings-safety", "Settings / Safety", "Canonical settings", "Current safety and theme profile retained until final Group 45 Settings migration."),
                            Route("automation-centre", "Automation Centre", "Emergency Stop preserved", "Guarded, manual, foreground-only automation and recovery controls."),
                            Route("desktop-release", "Desktop Release", "Diagnostics", "Release readiness and platform checkpoint evidence."),
                            Route("item-state-engine", "Item State Engine", "Platform diagnostic", "Shared item-state rules and platform behaviour."),
                            Route("lifeos-spine", "LifeOS Spine", "Platform diagnostic", "Cross-module spine and state relationships."),
                            Route("final-offline-os", "Offline OS Checkpoint", "Platform diagnostic", "Offline operating-system readiness."),
                            Route("os-navigation", "OS Navigation", "Platform diagnostic", "Navigation and route-state evidence."),
                            Route("universal-spine", "Universal Spine", "Platform diagnostic", "Shared platform structure and integrity.")
                        })
                })
        };

    public static WorkspaceDefinition Get(string name) =>
        All.Single(definition =>
            string.Equals(definition.Name, name, StringComparison.OrdinalIgnoreCase));

    public static bool IsRouteAllowed(string workspace, string routeId)
    {
        if (routeId.StartsWith("workspace:", StringComparison.OrdinalIgnoreCase))
        {
            string target = routeId["workspace:".Length..];
            return All.Any(definition =>
                string.Equals(definition.Name, target, StringComparison.OrdinalIgnoreCase));
        }

        return Get(workspace)
            .Sections
            .SelectMany(section => section.Modules)
            .Any(module => string.Equals(module.RouteId, routeId, StringComparison.Ordinal));
    }

    public static IReadOnlyCollection<string> PreservedRouteIds =>
        All
            .SelectMany(definition => definition.Sections)
            .SelectMany(section => section.Modules)
            .Select(module => module.RouteId)
            .Where(routeId =>
                !string.IsNullOrWhiteSpace(routeId) &&
                !routeId.StartsWith("workspace:", StringComparison.OrdinalIgnoreCase))
            .Cast<string>()
            .OrderBy(routeId => routeId, StringComparer.Ordinal)
            .ToArray();

    public static void Validate(IReadOnlyCollection<string> bridgeRouteIds)
    {
        string[] expectedWorkspaceNames =
        {
            "Home", "Work", "Career", "Money", "Life", "Projects", "Assistant", "Settings"
        };

        if (All.Count != expectedWorkspaceNames.Length ||
            !All.Select(definition => definition.Name)
                .SequenceEqual(expectedWorkspaceNames, StringComparer.Ordinal))
        {
            throw new InvalidOperationException("The v8 workspace catalog must contain the eight locked workspaces in order.");
        }

        string[] duplicateModuleIds = All
            .SelectMany(definition => definition.Sections)
            .SelectMany(section => section.Modules)
            .GroupBy(module => module.Id, StringComparer.Ordinal)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToArray();

        if (duplicateModuleIds.Length > 0)
        {
            throw new InvalidOperationException(
                $"Duplicate workspace module IDs: {string.Join(", ", duplicateModuleIds)}");
        }

        HashSet<string> catalogRoutes = new(PreservedRouteIds, StringComparer.Ordinal);
        HashSet<string> bridgeRoutes = new(bridgeRouteIds, StringComparer.Ordinal);

        if (!catalogRoutes.SetEquals(bridgeRoutes))
        {
            string missingFromBridge = string.Join(", ", catalogRoutes.Except(bridgeRoutes));
            string missingFromCatalog = string.Join(", ", bridgeRoutes.Except(catalogRoutes));

            throw new InvalidOperationException(
                $"Workspace route mismatch. Missing from bridge: {missingFromBridge}. Missing from catalog: {missingFromCatalog}.");
        }
    }
}
