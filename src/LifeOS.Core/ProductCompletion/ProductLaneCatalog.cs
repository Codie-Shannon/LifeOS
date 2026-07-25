namespace LifeOS.Core.ProductCompletion;

public sealed record ProductLaneMetric(string Label, string Value, string Detail);

public sealed record ProductLaneAction(
    string Title,
    string State,
    string Detail,
    bool RequiresReview);

public sealed record ProductLaneDefinition(
    string Route,
    string ScreenshotGroup,
    string Title,
    string Subtitle,
    string Boundary,
    IReadOnlyList<ProductLaneMetric> Metrics,
    IReadOnlyList<ProductLaneAction> Actions);

public static class ProductLaneCatalog
{
    private static readonly Dictionary<string, ProductLaneDefinition> Lanes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["work-time"] = new(
                "work-time",
                "SG-67",
                "Work Time & Billable Records",
                "Groups 67-68 - timers, manual entries, daily totals and exportable source proof",
                "Local timers and manual records change only when the user acts. Evidence remains attached and reviewable.",
                new[]
                {
                    new ProductLaneMetric("Today", "6h 30m", "Local tracked time"),
                    new ProductLaneMetric("Billable", "5h 45m", "NZD 488.75"),
                    new ProductLaneMetric("Non-billable", "45m", "Administration"),
                    new ProductLaneMetric("Evidence", "7", "Source references retained")
                },
                new[]
                {
                    new ProductLaneAction("Website release preparation", "Running", "1h 12m - BayForge Systems - billable", false),
                    new ProductLaneAction("Client discovery call", "Completed", "1h 30m - evidence attached - ready for timesheet", false),
                    new ProductLaneAction("Correct manual entry", "Needs review", "45m manual adjustment with a required reason", true),
                    new ProductLaneAction("Export weekly timesheet", "Ready", "CSV preview contains billable classification and proof count", true)
                }),
            ["operating-day"] = new(
                "operating-day",
                "SG-68",
                "Operating Day & Work Proof",
                "Groups 69-72 - calendar-linked planning, pressure-aware reminders and v14 closure",
                "Calendar blocks and suggested next actions remain proposals until accepted. Work, Career and Household boundaries stay visible.",
                new[]
                {
                    new ProductLaneMetric("Protected blocks", "3", "Accepted today"),
                    new ProductLaneMetric("Review queue", "2", "Calendar proposals"),
                    new ProductLaneMetric("Stop points", "2", "Boundary changes"),
                    new ProductLaneMetric("Proof linked", "5/6", "One link required")
                },
                new[]
                {
                    new ProductLaneAction("Client delivery block", "Protected", "09:00-11:00 - linked work session and proof", false),
                    new ProductLaneAction("Imported supplier call", "Needs review", "Calendar source - accept, defer or reject", true),
                    new ProductLaneAction("Household appointment", "Accepted", "11:30 - Work to Household boundary shown", false),
                    new ProductLaneAction("Close v14 operating lane", "Blocked", "Link the remaining Career administration proof first", true)
                }),
            ["guarded-providers"] = new(
                "guarded-providers",
                "SG-69",
                "Guarded Provider Contracts",
                "Groups 73-76 - connector registry, Integration Inbox v2, permission gates and v15 closure",
                "Provider data remains an untrusted preview. Writes are disabled by default, decisions are audited, and Emergency Stop wins.",
                new[]
                {
                    new ProductLaneMetric("Providers", "6", "Manual setup available"),
                    new ProductLaneMetric("Needs review", "4", "Source-backed candidates"),
                    new ProductLaneMetric("Duplicates", "1", "Never silently merged"),
                    new ProductLaneMetric("External writes", "0", "Disabled by default")
                },
                new[]
                {
                    new ProductLaneAction("Google Calendar event", "New", "Read-only source - accept, defer, reject or link", true),
                    new ProductLaneAction("Outlook message follow-up", "Conflict", "Field-level difference requires a decision", true),
                    new ProductLaneAction("Xero import contract", "Setup later", "Read-only placeholder; no credentials stored", false),
                    new ProductLaneAction("Emergency Stop", "Ready", "Stops provider intake and all external writes", false)
                }),
            ["documentation-hub"] = new(
                "documentation-hub",
                "SG-70",
                "Documentation & Packaging Hub",
                "Groups 77-79 - concise in-app help, website documentation and release-asset links",
                "Public, beta and internal audiences are separated. Private paths, account details and handoff notes never enter public copy.",
                new[]
                {
                    new ProductLaneMetric("Public guides", "12", "Website-owned"),
                    new ProductLaneMetric("In-app help", "8", "Concise entries"),
                    new ProductLaneMetric("Release links", "5", "Version-aligned"),
                    new ProductLaneMetric("Boundary checks", "3", "All passing")
                },
                new[]
                {
                    new ProductLaneAction("Move long-form connector guide", "Ready", "Website destination selected; app retains a short setup link", false),
                    new ProductLaneAction("Release screenshot index", "Linked", "Stable repo evidence path without local machine details", false),
                    new ProductLaneAction("Public copy scan", "Passed", "No private handoff notes, credentials or absolute paths", false)
                }),
            ["beta-readiness"] = new(
                "beta-readiness",
                "SG-71",
                "Private Beta Readiness",
                "Groups 80-82 - setup choices, portal distribution and closed-beta validation",
                "Core local value never requires an external provider. Optional setup supports now, later or decline and can be changed in Settings.",
                new[]
                {
                    new ProductLaneMetric("Core modules", "8/8", "Locally ready"),
                    new ProductLaneMetric("Optional providers", "2", "Configured later"),
                    new ProductLaneMetric("Platforms", "3", "Desktop, mobile, web"),
                    new ProductLaneMetric("Release checks", "7/7", "Closed-beta gate")
                },
                new[]
                {
                    new ProductLaneAction("Core local setup", "Ready", "Review queues, work, household and money operate offline", false),
                    new ProductLaneAction("External AI", "Declined", "Native intelligence remains available; change anytime", false),
                    new ProductLaneAction("Private beta distribution", "Ready", "Dedicated test identities and signed Android build", false),
                    new ProductLaneAction("Close baseline", "Review", "Confirm release notes and screenshot evidence before closure", true)
                }),
            ["intelligence"] = new(
                "intelligence",
                "SG-72",
                "Native Intelligence & Optional AI",
                "Groups 83-86 - deterministic ranking, provider controls and review-first suggestions",
                "Native rules always work offline. External AI is optional, category-scoped, cost-controlled and cannot mutate LifeOS directly.",
                new[]
                {
                    new ProductLaneMetric("Native suggestions", "6", "No paid AI required"),
                    new ProductLaneMetric("AI mode", "Ask", "Confirm every request"),
                    new ProductLaneMetric("Monthly cap", "NZD 5", "NZD 0.42 used"),
                    new ProductLaneMetric("Needs review", "4", "No automatic mutation")
                },
                new[]
                {
                    new ProductLaneAction("Power bill follow-up", "High confidence", "Native rule: due tomorrow from confirmed source", true),
                    new ProductLaneAction("Supplier summary", "Ask first", "Optional OpenAI enrichment; Work category permitted", true),
                    new ProductLaneAction("Health data suggestion", "Blocked", "Sensitive category permission is off", false),
                    new ProductLaneAction("External AI setup", "Configured", "OpenAI primary; switch off, ask or capped anytime", false)
                }),
            ["communications"] = new(
                "communications",
                "SG-73",
                "Scheduled Communications",
                "Groups 87-90 - SMS and email scheduling, unified review, quiet hours and safety closure",
                "Every scheduled SMS or email requires explicit approval. Editing or rescheduling revokes approval; quiet hours and Emergency Stop always apply.",
                new[]
                {
                    new ProductLaneMetric("Drafts", "4", "SMS, Gmail and Outlook"),
                    new ProductLaneMetric("Approved", "1", "Due after quiet hours"),
                    new ProductLaneMetric("Quiet hours", "21:00-07:00", "Local time"),
                    new ProductLaneMetric("Sent automatically", "0", "Approval required")
                },
                new[]
                {
                    new ProductLaneAction("SMS: appointment reminder", "Draft", "Phone bridge - approve, edit, cancel or reschedule", true),
                    new ProductLaneAction("Email: supplier follow-up", "Approved", "Outlook - scheduled 08:30 with source proof", false),
                    new ProductLaneAction("Gmail: weekly summary", "Needs review", "Draft changed; previous approval was revoked", true),
                    new ProductLaneAction("Communication Emergency Stop", "Ready", "Immediately blocks every channel", false)
                }),
            ["social-publishing"] = new(
                "social-publishing",
                "SG-74",
                "Social & Messenger Review",
                "Groups 91-94 - Meta capability mapping, scheduled drafts and transparent fallback",
                "Official APIs are preferred. Unsupported actions show their limitation and use an explicit browser-assisted or manual handoff; nothing posts invisibly.",
                new[]
                {
                    new ProductLaneMetric("Drafts", "3", "Page, group and Messenger"),
                    new ProductLaneMetric("API ready", "1", "Page publishing"),
                    new ProductLaneMetric("Browser handoff", "2", "Visible fallback"),
                    new ProductLaneMetric("Published", "0", "Provider confirmation required")
                },
                new[]
                {
                    new ProductLaneAction("Facebook Page release note", "Draft", "Official API capability; approval still required", true),
                    new ProductLaneAction("Community group update", "Browser handoff", "API limitation shown before opening the browser", true),
                    new ProductLaneAction("Messenger reply", "Needs review", "Source-backed draft with recipient and consent status", true),
                    new ProductLaneAction("Provider limitation log", "Visible", "No fake claims for unsupported profile posting", false)
                }),
            ["pay-later-insights"] = new(
                "pay-later-insights",
                "SG-75",
                "Pay-Later & Money Integration Review",
                "Groups 95-98 - Afterpay/Zip parsing, safe-money exclusion and read-only contracts",
                "Email evidence creates candidates only. Safe money changes after confirmation; banking/accounting contracts cannot reconcile or initiate payments.",
                new[]
                {
                    new ProductLaneMetric("Remaining", "NZD 360", "Confirmed Afterpay + Zip"),
                    new ProductLaneMetric("Next deductions", "NZD 90", "Excluded from safe money"),
                    new ProductLaneMetric("Needs review", "2", "Source-backed candidates"),
                    new ProductLaneMetric("Payments initiated", "0", "Hard boundary")
                },
                new[]
                {
                    new ProductLaneAction("Afterpay statement", "Candidate", "NZD 240 remaining - next NZD 60 - source retained", true),
                    new ProductLaneAction("Zip receipt", "Confirmed", "NZD 120 remaining - duplicate scan passed", false),
                    new ProductLaneAction("Xero export contract", "Read-only", "CSV/Xero export available; provider write disabled", false),
                    new ProductLaneAction("Bank payment action", "Blocked", "Payment initiation is outside product-complete scope", false)
                }),
            ["grocery-lookup"] = new(
                "grocery-lookup",
                "SG-76",
                "NZ Grocery Price Lookup",
                "Groups 99-103 - consent, visible worker, retailer adapters and price intelligence",
                "Lookup runs only after retailer and location consent. Every price shows source, timestamp and confidence; carts, orders and payments never change.",
                new[]
                {
                    new ProductLaneMetric("Location", "Whanganui", "Nearest town enabled"),
                    new ProductLaneMetric("Retailers", "3", "New World, PaknSave, Woolworths"),
                    new ProductLaneMetric("Fresh prices", "9", "Within 24 hours"),
                    new ProductLaneMetric("Cart mutations", "0", "Hard boundary")
                },
                new[]
                {
                    new ProductLaneAction("Standard milk 2L", "NZD 3.90", "PaknSave - captured 12:00 - 88% confidence", true),
                    new ProductLaneAction("White bread loaf", "Compare 3", "Similar products across permitted nearby stores", true),
                    new ProductLaneAction("Woolworths adapter", "Manual fallback", "Provider path unavailable; visible manual entry offered", false),
                    new ProductLaneAction("Background refresh", "Off", "Can be enabled explicitly from grocery settings", false)
                }),
            ["evidence-automation"] = new(
                "evidence-automation",
                "SG-77",
                "Evidence Automation",
                "Groups 104-107 - screenshot intake, PDF generation and repository completion gates",
                "Evidence tools copy and rename proof without altering originals. A group cannot close until screenshots, builds, tests, notes and repository checks are present.",
                new[]
                {
                    new ProductLaneMetric("New screenshots", "8", "Old folder excluded"),
                    new ProductLaneMetric("Stable names", "8/8", "Group-prefixed"),
                    new ProductLaneMetric("Gate checks", "8", "All required"),
                    new ProductLaneMetric("Repo mutations", "0", "Export is copy-only")
                },
                new[]
                {
                    new ProductLaneAction("Screenshot intake", "Ready", "Scans selected folder and excludes old evidence", false),
                    new ProductLaneAction("Evidence PDF", "Ready", "Builds a visual pack from approved PNG files", false),
                    new ProductLaneAction("Completion gate", "Blocked", "Pack 2 screenshots are intentionally pending", true),
                    new ProductLaneAction("Repo-safe export", "Verified", "Original files and canonical repository remain unchanged", false)
                })
        };

    public static IReadOnlyCollection<string> Routes => Lanes.Keys;

    public static ProductLaneDefinition Get(string route) =>
        Lanes.TryGetValue(route, out ProductLaneDefinition? lane)
            ? lane
            : throw new ArgumentOutOfRangeException(nameof(route), route, "Unknown product lane.");
}
