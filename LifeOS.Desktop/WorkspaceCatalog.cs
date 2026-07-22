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

internal sealed record WorkspaceMetricView(string Label, string Value, string Detail);

internal static class WorkspaceCatalog
{
    private static WorkspaceMetricDefinition Metric(
        string label,
        string key,
        string fallback,
        string detail) => new(label, key, fallback, detail);

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
                "Home is summary-first. Editing remains inside the owning workspace or canonical module.",
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
                        "Home links into the permanent workspaces without creating duplicate editors.",
                        new WorkspaceModuleDefinition[]
                        {
                            Route("command-centre", "Command Centre", "Canonical module", "Pressure, safe-to-spend, agenda, follow-up and pipeline checkpoint."),
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
                "Clients, waiting-on items, follow-ups, work sessions, timesheets, invoices and work records share one permanent workspace.",
                "Work owns client-delivery records. Career and personal money remain separate.",
                new WorkspaceMetricDefinition[]
                {
                    Metric("Pipeline", "work-pipeline", "0", "Canonical records"),
                    Metric("Follow-ups", "follow-ups", "0", "Waiting and next action"),
                    Metric("Sessions", "work-sessions", "0", "Time evidence"),
                    Metric("Timesheets", "timesheets", "0", "Canonical evidence")
                },
                new WorkspaceSectionDefinition[]
                {
                    new(
                        "Clients and delivery",
                        "Focused routes open the authoritative records and behaviour.",
                        new WorkspaceModuleDefinition[]
                        {
                            Route("work-pipeline", "Work Pipeline", "Canonical records", "Active work, opportunities, stages, waiting-on state, invoice readiness and expected value."),
                            Route("follow-ups", "Follow-Ups", "Canonical records", "People, contexts, due dates, priority, money links and next actions."),
                            Route("paid-work-centre", "Paid Work Centre", "Canonical behaviour", "Billable work, invoice readiness, payment state and proof-led delivery."),
                            Route("work-sessions", "Work Sessions", "Canonical records", "Hours, rates, billable state, descriptions and session evidence."),
                            Route("timesheet-evidence", "Timesheet Evidence", "Canonical behaviour", "Review timesheet-ready work without copying or rewriting source records.")
                        })
                }),
            new(
                "Career",
                "Career",
                "Career stays separate from Work",
                "PROFILE / APPLICATIONS",
                "Profile, CVs, applications, interviews and career follow-ups are organised without mixing employment activity into client delivery.",
                "Career has its own information architecture and does not reuse client-work state.",
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
                        "The permanent separation is established without inventing employment records.",
                        new WorkspaceModuleDefinition[]
                        {
                            Native("career-profile", "Profile", "Native workspace surface", "Role direction, strengths and career positioning without client-work fields."),
                            Native("career-cvs", "CVs", "Native workspace surface", "CV versions and role-specific evidence."),
                            Route("v12-career-studio", "Career Studio", "Canonical records", "Authoritative opportunity and application pipeline with review-first imported candidates."),
                            Native("career-interviews", "Interviews", "Native workspace surface", "Interview preparation and follow-up structure."),
                            Route("relationship-radar", "Relationship Radar", "Canonical records", "Relationship and follow-up records where they support career activity.")
                        })
                }),
            new(
                "Money",
                "Money",
                "Income, bills and weekly control",
                "WEEKLY MONEY CONTROL",
                "Weekly money, expected income, bills, payment timing, evidence and pressure signals live together.",
                "Money owns personal financial control. Expected income remains unsafe until confirmed.",
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
                        "Financial views retain their persisted models and safety rules.",
                        new WorkspaceModuleDefinition[]
                        {
                            Route("v11-document-intake", "Document & Evidence Intake", "Original-preserving intake", "Capture, classify, review and link receipts, invoices, statements, contracts and general evidence."),
                            Route("v11-money-foundation", "Money v11 Foundation", "Authoritative v11 surface", "Accounts, transactions, invoices, payments, linked evidence, manual confirmation and audit history across the trusted financial model."),
                            Route("money-pressure", "Money Pressure", "Canonical state", "Current balance, paid and pending income, obligations, deductions and safe-to-spend pressure."),
                            Route("money-timeline", "Money Timeline", "Canonical behaviour", "Chronological money events and expected-income timing."),
                            Route("bills-payments", "Bills / Payments", "Canonical state", "Bills, obligations, status and review windows."),
                            Route("money-profile", "Money Profile", "Canonical state", "Buffers, hidden deductions and safe-to-spend planning."),
                            Route("payment-calendar", "Payment Calendar", "Canonical behaviour", "Due dates, expected income and payment attention."),
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
                "Life owns personal operating state; Work, Career and Money remain dedicated.",
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
                        "Existing modules form one clear personal operating loop.",
                        new WorkspaceModuleDefinition[]
                        {
                            Route("agenda", "Agenda", "Canonical records", "Commitments, dates, time, pressure, status and notes."),
                            Route("daily-state", "Daily State", "Canonical behaviour", "Current personal state and pressure context."),
                            Route("daily-operating-flow", "Daily Operating Flow", "Canonical behaviour", "A manual, visible operating sequence for the day."),
                            Route("weekly-close-out", "Weekly Close-Out", "Canonical records", "Done, moved, waiting and next-week review."),
                            Route("timer-agent", "TimerAgent", "Canonical utility", "Desktop-only timing utility inside Life.")
                        })
                }),
            new(
                "Household",
                "Household",
                "Grocery planning and recurring essentials",
                "GROCERY / ESSENTIALS / SHOPPING",
                "Grocery lists, recurring essentials, quantities, priorities and review-first household planning share one workspace.",
                "Household keeps planning explicit. Recurring essentials remain reviewable candidates and no purchase, payment or external cart mutation occurs automatically.",
                new WorkspaceMetricDefinition[]
                {
                    Metric("Active lists", "", "1", "Shopping or ready"),
                    Metric("Due essentials", "", "2", "Review required"),
                    Metric("Unresolved items", "", "2", "Require attention"),
                    Metric("Estimated spend", "", "NZD 68.40", "Manual estimate")
                },
                new WorkspaceSectionDefinition[]
                {
                    new(
                        "Grocery and household",
                        "Plan recurring essentials and execute shopping without autonomous ordering.",
                        new WorkspaceModuleDefinition[]
                        {
                            Route(
                                "v13-grocery-planning",
                                "Grocery Planning",
                                "Canonical records",
                                "Lists, recurring essentials, quantities, priorities and review-first household planning.")
                        })
                }),
            new(
                "Projects",
                "Projects",
                "Milestones, evidence and delivery",
                "ACTIVE PROJECTS / PROOF",
                "Active projects, milestones, proof and delivery evidence share one workspace.",
                "Projects owns delivery and proof without creating another core page.",
                new WorkspaceMetricDefinition[]
                {
                    Metric("Proof items", "proof", "0", "Canonical records"),
                    Metric("Projects", "", "Active", "Core module"),
                    Metric("Evidence vault", "", "Linked", "Canonical behaviour"),
                    Metric("DevOps", "", "Deferred", "Outside v8 scope")
                },
                new WorkspaceSectionDefinition[]
                {
                    new(
                        "Delivery and evidence",
                        "Milestones and evidence remain linked to their authoritative stores.",
                        new WorkspaceModuleDefinition[]
                        {
                            Route("projects", "Projects", "Canonical module", "Active project records and delivery structure."),
                            Route("proof-tracker", "Proof Tracker", "Canonical records", "Project proof, evidence status, links and notes."),
                            Route("evidence-vault", "Evidence Vault", "Canonical behaviour", "Cross-module evidence and provenance review.")
                        })
                }),
            new(
                "Assistant",
                "Assistant",
                "Source-backed, review-first help",
                "ANSWERS / PLANS / MEMORY / REVIEW",
                "Source-backed answers, review-only plans, explicit memory and intake review remain bounded and visible.",
                "Assistant cannot execute, approve, confirm, write externally or hide background work.",
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
                        "The v7 capability remains authoritative beneath the final workspace.",
                        new WorkspaceModuleDefinition[]
                        {
                            Route("assistant", "Assistant", "Preserved safety boundary", "Source-backed answers, evidence disclosure, uncertainty and review-only planning."),
                            Route("memory", "Memory", "Explicit and revocable", "User-confirmed memory with scope, audit, expiry, revocation and deletion."),
                            Route("search-knowledge", "Search / Knowledge", "Canonical behaviour", "Local search and knowledge surfaces."),
                            Route("integration-inbox", "Integration Inbox", "Review-first", "Untrusted imported records remain separate from trusted operational state."),
                            Route("email-radar", "Email Radar", "Read-only review", "Local email evidence without sending or external mutation.")
                        })
                }),
            new(
                "Settings",
                "Settings",
                "System behaviour only",
                "SYSTEM CONTROL",
                "Appearance, startup, accessibility, profiles, privacy, sync, integrations, safety and diagnostics live here—not operational records.",
                "Settings controls the shell and safety boundaries. It does not become an operational dumping ground.",
                new WorkspaceMetricDefinition[]
                {
                    Metric("Core pages", "", "8", "Permanent workspaces"),
                    Metric("Obsolete shell routes", "", "0", "Removed"),
                    Metric("Emergency Stop", "", "Available", "Textual state"),
                    Metric("Desktop release", "", "v9.0.0-alpha.2", "Group 47 normalized review pipeline")
                },
                new WorkspaceSectionDefinition[]
                {
                    new(
                        "Diagnostics and retained system capability",
                        "System diagnostics remain reachable through final allowlisted module routes.",
                        new WorkspaceModuleDefinition[]
                        {
                            Route("settings-safety", "Settings / Safety", "Canonical settings", "Underlying safety profile and local settings evidence."),
                            Route("automation-centre", "Automation Centre", "Emergency Stop protected", "Guarded, manual, foreground-only automation and recovery controls."),
                            Route("desktop-release", "Desktop Release", "Diagnostics", "Release readiness and platform checkpoint evidence."),
                            Route("item-state-engine", "Item State Engine", "Diagnostics", "Shared item-state rules and platform behaviour."),
                            Route("lifeos-spine", "LifeOS Spine", "Diagnostics", "Cross-module spine and state relationships."),
                            Route("final-offline-os", "Offline OS Checkpoint", "Diagnostics", "Offline operating-system readiness."),
                            Route("os-navigation", "OS Navigation", "Diagnostics", "Navigation and route-state evidence."),
                            Route("universal-spine", "Universal Spine", "Diagnostics", "Shared platform structure and integrity.")
                        })
                })
        };

    public static WorkspaceDefinition Get(string name) =>
        All.Single(definition =>
            string.Equals(definition.Name, name, StringComparison.OrdinalIgnoreCase));

    public static bool TryGet(string? name, out WorkspaceDefinition definition)
    {
        definition = All.First();

        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        WorkspaceDefinition? match = All.FirstOrDefault(candidate =>
            string.Equals(candidate.Name, name, StringComparison.OrdinalIgnoreCase));

        if (match is null)
        {
            return false;
        }

        definition = match;
        return true;
    }

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

    public static IReadOnlyCollection<string> ModuleRouteIds => All
        .SelectMany(definition => definition.Sections)
        .SelectMany(section => section.Modules)
        .Select(module => module.RouteId)
        .Where(routeId =>
            !string.IsNullOrWhiteSpace(routeId) &&
            !routeId.StartsWith("workspace:", StringComparison.OrdinalIgnoreCase))
        .Cast<string>()
        .OrderBy(routeId => routeId, StringComparer.Ordinal)
        .ToArray();

    public static void Validate(IReadOnlyCollection<string> routeIds)
    {
        string[] expectedWorkspaceNames =
        {
            "Home",
            "Work",
            "Career",
            "Money",
            "Life",
            "Household",
            "Projects",
            "Assistant",
            "Settings"
        };

        if (All.Count != expectedWorkspaceNames.Length ||
            !All.Select(definition => definition.Name)
                .SequenceEqual(expectedWorkspaceNames, StringComparer.Ordinal))
        {
            throw new InvalidOperationException(
                "The workspace catalog must contain the nine locked workspaces in order.");
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

        HashSet<string> catalogRoutes = new(ModuleRouteIds, StringComparer.Ordinal);
        HashSet<string> finalRoutes = new(routeIds, StringComparer.Ordinal);

        if (!catalogRoutes.SetEquals(finalRoutes))
        {
            string missingFromFinalRoutes = string.Join(", ", catalogRoutes.Except(finalRoutes));
            string missingFromCatalog = string.Join(", ", finalRoutes.Except(catalogRoutes));

            throw new InvalidOperationException(
                $"Final v8 route mismatch. Missing from route map: {missingFromFinalRoutes}. " +
                $"Missing from catalog: {missingFromCatalog}.");
        }
    }
}
