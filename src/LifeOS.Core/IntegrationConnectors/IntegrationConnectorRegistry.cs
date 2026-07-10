using LifeOS.Core.IntegrationInbox;

namespace LifeOS.Core.IntegrationConnectors;

public static class IntegrationConnectorRegistry
{
    private static readonly IntegrationConnectorDefinition[] ConnectorDefinitions =
    [
        Core("manual-csv", "Manual CSV import", IntegrationConnectorFirstMode.LocalFile, IntegrationConnectorRiskLevel.Low, "First safe connector for deterministic preview rows."),
        Core("manual-json", "Manual JSON import", IntegrationConnectorFirstMode.LocalFile, IntegrationConnectorRiskLevel.Low, "Structured local import without live authentication."),
        Core("drag-drop-file", "Drag-and-drop file intake", IntegrationConnectorFirstMode.LocalFile, IntegrationConnectorRiskLevel.Low, "Fast local capture into Integration Inbox."),
        Core("local-folder-watcher", "Local folder watcher", IntegrationConnectorFirstMode.LocalReadOnly, IntegrationConnectorRiskLevel.Medium, "Read-only documents, receipts, exports, and evidence."),

        Calendar("google-calendar", "Google Calendar", IntegrationConnectorFirstMode.ReadOnlyApi, "Commitments, deadlines, meeting prep, and recurring pressure."),
        Calendar("outlook-calendar", "Outlook Calendar / Microsoft 365 Calendar", IntegrationConnectorFirstMode.ReadOnlyApi, "Work and personal calendar pressure."),
        Calendar("ics-import", "Local ICS import", IntegrationConnectorFirstMode.LocalFile, "Safe calendar fixture before OAuth."),

        Email("gmail", "Gmail", "Follow-ups, receipts, invoices, admin threads, and attachment evidence."),
        Email("outlook-email", "Outlook Email / Microsoft 365 Mail", "Work follow-ups, receipts, attachments, and admin threads."),

        File("google-drive", "Google Drive", IntegrationConnectorFirstMode.ReadOnlyApi, "Documents, receipts, project evidence, and source files."),
        File("onedrive", "OneDrive", IntegrationConnectorFirstMode.ReadOnlyApi, "Documents, receipts, project evidence, and source files."),
        File("sharepoint", "SharePoint", IntegrationConnectorFirstMode.ReadOnlyApi, "Work/client documents and evidence."),
        File("pdf-intake", "PDF intake", IntegrationConnectorFirstMode.LocalFile, "Receipts, invoices, contracts, and evidence."),

        Money("bank-csv-export", "CSV bank exports", IntegrationConnectorFirstMode.LocalFile, IntegrationConnectorRoadmapPhase.V5Foundation, "Transaction and payment history previews only."),
        Money("xero", "Xero", IntegrationConnectorFirstMode.DeferredApi, IntegrationConnectorRoadmapPhase.V5Plus, "Accounting, invoices, paid/unpaid evidence."),
        Money("stripe", "Stripe", IntegrationConnectorFirstMode.DeferredApi, IntegrationConnectorRoadmapPhase.V5Plus, "Business and client payment status."),
        Money("paypal", "PayPal", IntegrationConnectorFirstMode.DeferredApi, IntegrationConnectorRoadmapPhase.V5Plus, "Payments, subscriptions, and client receipts."),
        Money("wise", "Wise", IntegrationConnectorFirstMode.DeferredApi, IntegrationConnectorRoadmapPhase.V5Plus, "International payments and balances."),
        Money("open-banking", "Bank feed / open banking", IntegrationConnectorFirstMode.DeferredApi, IntegrationConnectorRoadmapPhase.DeferredHighRisk, "Full money brain and cashflow truth after safety hardening."),

        Bnpl("afterpay", "Afterpay", "Repayment schedules, money pressure, and safe-to-spend adjustment previews."),
        Bnpl("zip", "Zip", "Repayment schedules, money pressure, and safe-to-spend adjustment previews."),
        Bnpl("klarna", "Klarna", "Optional BNPL repayment schedule previews."),
        Bnpl("paypal-pay-in-4", "PayPal Pay in 4", "Optional BNPL repayment schedule previews."),

        Ocr("local-ocr", "Local OCR engine", IntegrationConnectorFirstMode.LocalReadOnly, IntegrationConnectorRiskLevel.Medium, "Receipt and invoice text extraction."),
        Ocr("cloud-ocr", "Cloud OCR provider", IntegrationConnectorFirstMode.DeferredApi, IntegrationConnectorRiskLevel.High, "Higher-quality extraction if local OCR is insufficient."),

        Work("github", "GitHub", IntegrationConnectorFirstMode.ReadOnlyApi, IntegrationConnectorRiskLevel.Medium, "Dev work evidence and project trail."),
        Work("gitlab", "GitLab", IntegrationConnectorFirstMode.OptionalLater, IntegrationConnectorRiskLevel.Medium, "Dev work evidence."),
        Work("jira", "Jira / Atlassian", IntegrationConnectorFirstMode.OptionalLater, IntegrationConnectorRiskLevel.High, "Tickets, deadlines, and work status."),
        Work("linear", "Linear", IntegrationConnectorFirstMode.OptionalLater, IntegrationConnectorRiskLevel.Medium, "Tasks and project status."),

        TaskNote("todoist", "Todoist", "External task migration or mirror."),
        TaskNote("microsoft-to-do", "Microsoft To Do", "Work/personal task bridge."),
        TaskNote("notion", "Notion", "Notes, tasks, and project context."),
        TaskNote("obsidian-markdown", "Obsidian / local markdown folder", "Knowledge, notes, and project evidence."),

        Ai("generative-follow-ups", "Generative follow-up suggestions", IntegrationConnectorRiskLevel.High, "Suggested replies, reminders, and next actions with provenance."),
        Ai("draft-replies", "Draft replies", IntegrationConnectorRiskLevel.VeryHigh, "Prepare messages without sending."),
        Ai("weekly-close-out-narrative", "Weekly close-out narrative", IntegrationConnectorRiskLevel.Medium, "Review week, pressure, evidence, and misses."),

        Communication("sms", "SMS", IntegrationConnectorFirstMode.MobileDeferred, IntegrationConnectorRoadmapPhase.MobileCompanion, "Mobile capture and explicit user-send only."),
        Communication("whatsapp", "WhatsApp", IntegrationConnectorFirstMode.MobileDeferred, IntegrationConnectorRoadmapPhase.MobileCompanion, "Personal/work follow-up signals if API access is sane."),
        Communication("messenger", "Messenger / Facebook messages", IntegrationConnectorFirstMode.MobileDeferred, IntegrationConnectorRoadmapPhase.V8V9CommunicationContent, "Deferred private communication with explicit approval only."),
        Meta("facebook-pages", "Facebook Pages", "Owned page posts, comments, scheduled content, and business/admin follow-ups."),
        Meta("facebook-groups", "Facebook Groups", "Owned/admin group posts and comments when permitted."),
        Meta("facebook-profile", "Facebook public profile archive", "Optional read-only personal timeline memory and evidence."),
        Meta("instagram", "Instagram posts/messages", "Meta-family content and communication, deferred behind strict scopes."),

        Context("browser-bookmarks", "Browser bookmarks", IntegrationConnectorFirstMode.LocalFile, IntegrationConnectorRiskLevel.Medium, "Research trails and project evidence."),
        Context("browser-history", "Browser history", IntegrationConnectorFirstMode.OptionalLater, IntegrationConnectorRiskLevel.VeryHigh, "Optional local import only with strong consent."),
        Context("google-contacts", "Google Contacts", IntegrationConnectorFirstMode.ReadOnlyApi, IntegrationConnectorRiskLevel.High, "People context and client/family/admin links."),
        Context("outlook-people", "Outlook People", IntegrationConnectorFirstMode.ReadOnlyApi, IntegrationConnectorRiskLevel.High, "Work and admin contact context."),
        Context("location-context", "Location context", IntegrationConnectorFirstMode.MobileDeferred, IntegrationConnectorRiskLevel.VeryHigh, "Travel, appointments, mileage, and errands."),

        Admin("order-emails", "Order and delivery emails", "Deliveries, receipts, returns, and household/admin pressure."),
        Admin("utilities-accounts", "Utilities and account portals", "Bills, renewals, obligations, and evidence."),
        Admin("government-tax-portals", "Government/tax portals", "Manual export and document upload only at first."),

        Automation("zapier", "Zapier", "Deferred external automation ingress."),
        Automation("make", "Make", "Deferred external automation ingress."),
        Automation("ifttt", "IFTTT", "Deferred personal automation ingress."),
        Automation("webhooks", "Webhooks", "Controlled generic integration bridge."),

        WebsiteMobile("lifeos-docs-hub", "LifeOS website docs hub", IntegrationConnectorFirstMode.LocalFile, IntegrationConnectorRiskLevel.Low, "Move in-app docs outward over time."),
        WebsiteMobile("mobile-companion-app", "Mobile companion app", IntegrationConnectorFirstMode.MobileDeferred, IntegrationConnectorRiskLevel.High, "Capture, notifications, mobile-first flows."),
        WebsiteMobile("push-notifications", "Push notifications", IntegrationConnectorFirstMode.MobileDeferred, IntegrationConnectorRiskLevel.High, "Reminders and pressure signals.")
    ];

    public static IReadOnlyList<IntegrationConnectorDefinition> All => ConnectorDefinitions;

    public static IntegrationConnectorDefinition GetRequired(string key)
    {
        return ConnectorDefinitions.FirstOrDefault(connector => connector.Key == key)
            ?? throw new ArgumentException($"Unknown integration connector '{key}'.", nameof(key));
    }

    public static IReadOnlyList<IntegrationConnectorDefinition> ForPhase(IntegrationConnectorRoadmapPhase phase)
    {
        return ConnectorDefinitions.Where(connector => connector.RoadmapPhase == phase).ToArray();
    }

    private static IntegrationConnectorDefinition Core(string key, string name, IntegrationConnectorFirstMode firstMode, IntegrationConnectorRiskLevel riskLevel, string notes)
    {
        return new(key, name, IntegrationConnectorCategory.CoreIntake, firstMode, riskLevel, IntegrationConnectorRoadmapPhase.V5Foundation, IntegrationSourceKind.ManualImport, IntegrationConnectorActionPolicy.PreviewOnly, BasicReadScopes(firstMode), [IntegrationTargetKind.EvidenceVault, IntegrationTargetKind.SearchKnowledge], notes);
    }

    private static IntegrationConnectorDefinition Calendar(string key, string name, IntegrationConnectorFirstMode firstMode, string notes)
    {
        return new(key, name, IntegrationConnectorCategory.Calendar, firstMode, IntegrationConnectorRiskLevel.Medium, IntegrationConnectorRoadmapPhase.V5Foundation, IntegrationSourceKind.Calendar, IntegrationConnectorActionPolicy.PreviewOnly, BasicReadScopes(firstMode), [IntegrationTargetKind.ItemState, IntegrationTargetKind.FollowUps], notes);
    }

    private static IntegrationConnectorDefinition Email(string key, string name, string notes)
    {
        return new(key, name, IntegrationConnectorCategory.Email, IntegrationConnectorFirstMode.MetadataReadOnlyApi, IntegrationConnectorRiskLevel.High, IntegrationConnectorRoadmapPhase.V5Plus, IntegrationSourceKind.Email, IntegrationConnectorActionPolicy.PreviewOnly, [IntegrationConnectorScope.ReadMetadata, IntegrationConnectorScope.ReadContent, IntegrationConnectorScope.ReadAttachments], [IntegrationTargetKind.FollowUps, IntegrationTargetKind.BillsPayments, IntegrationTargetKind.EvidenceVault], notes);
    }

    private static IntegrationConnectorDefinition File(string key, string name, IntegrationConnectorFirstMode firstMode, string notes)
    {
        return new(key, name, IntegrationConnectorCategory.FilesAndDocuments, firstMode, IntegrationConnectorRiskLevel.High, IntegrationConnectorRoadmapPhase.V5Plus, IntegrationSourceKind.CloudFile, IntegrationConnectorActionPolicy.PreviewOnly, BasicReadScopes(firstMode), [IntegrationTargetKind.EvidenceVault, IntegrationTargetKind.SearchKnowledge], notes);
    }

    private static IntegrationConnectorDefinition Money(string key, string name, IntegrationConnectorFirstMode firstMode, IntegrationConnectorRoadmapPhase phase, string notes)
    {
        return new(key, name, IntegrationConnectorCategory.Money, firstMode, IntegrationConnectorRiskLevel.High, phase, IntegrationSourceKind.BankingPreview, IntegrationConnectorActionPolicy.PreviewOnly, BasicReadScopes(firstMode), [IntegrationTargetKind.PaymentCalendar, IntegrationTargetKind.BillsPayments, IntegrationTargetKind.EvidenceVault], notes);
    }

    private static IntegrationConnectorDefinition Bnpl(string key, string name, string notes)
    {
        return new(key, name, IntegrationConnectorCategory.BuyNowPayLater, IntegrationConnectorFirstMode.EmailDerivedPreview, IntegrationConnectorRiskLevel.High, IntegrationConnectorRoadmapPhase.V5Plus, IntegrationSourceKind.Email, IntegrationConnectorActionPolicy.PreviewOnly, [IntegrationConnectorScope.ReadMetadata, IntegrationConnectorScope.ReadContent], [IntegrationTargetKind.PaymentCalendar, IntegrationTargetKind.BillsPayments], notes);
    }

    private static IntegrationConnectorDefinition Ocr(string key, string name, IntegrationConnectorFirstMode firstMode, IntegrationConnectorRiskLevel riskLevel, string notes)
    {
        return new(key, name, IntegrationConnectorCategory.ReceiptsInvoicesOcr, firstMode, riskLevel, IntegrationConnectorRoadmapPhase.V5Foundation, IntegrationSourceKind.ReceiptOcr, IntegrationConnectorActionPolicy.PreviewOnly, BasicReadScopes(firstMode), [IntegrationTargetKind.EvidenceVault, IntegrationTargetKind.BillsPayments], notes);
    }

    private static IntegrationConnectorDefinition Work(string key, string name, IntegrationConnectorFirstMode firstMode, IntegrationConnectorRiskLevel riskLevel, string notes)
    {
        return new(key, name, IntegrationConnectorCategory.WorkAndTime, firstMode, riskLevel, IntegrationConnectorRoadmapPhase.V5Plus, IntegrationSourceKind.CloudFile, IntegrationConnectorActionPolicy.PreviewOnly, BasicReadScopes(firstMode), [IntegrationTargetKind.WorkPipeline, IntegrationTargetKind.EvidenceVault], notes);
    }

    private static IntegrationConnectorDefinition TaskNote(string key, string name, string notes)
    {
        return new(key, name, IntegrationConnectorCategory.TasksAndNotes, IntegrationConnectorFirstMode.OptionalLater, IntegrationConnectorRiskLevel.Medium, IntegrationConnectorRoadmapPhase.V5Plus, IntegrationSourceKind.CloudFile, IntegrationConnectorActionPolicy.PreviewOnly, [IntegrationConnectorScope.ReadMetadata, IntegrationConnectorScope.ReadContent], [IntegrationTargetKind.ItemState, IntegrationTargetKind.SearchKnowledge], notes);
    }

    private static IntegrationConnectorDefinition Ai(string key, string name, IntegrationConnectorRiskLevel riskLevel, string notes)
    {
        return new(key, name, IntegrationConnectorCategory.AiAssistance, IntegrationConnectorFirstMode.AiAssistedPreview, riskLevel, IntegrationConnectorRoadmapPhase.V7Assistant, IntegrationSourceKind.ManualImport, key == "draft-replies" ? IntegrationConnectorActionPolicy.DraftWithExplicitApproval : IntegrationConnectorActionPolicy.PreviewOnly, [IntegrationConnectorScope.GenerateSuggestion], [IntegrationTargetKind.FollowUps, IntegrationTargetKind.ItemState], notes);
    }

    private static IntegrationConnectorDefinition Communication(string key, string name, IntegrationConnectorFirstMode firstMode, IntegrationConnectorRoadmapPhase phase, string notes)
    {
        return new(key, name, IntegrationConnectorCategory.Communication, firstMode, IntegrationConnectorRiskLevel.VeryHigh, phase, IntegrationSourceKind.Email, IntegrationConnectorActionPolicy.DraftWithExplicitApproval, [IntegrationConnectorScope.ReadMetadata, IntegrationConnectorScope.GenerateSuggestion], [IntegrationTargetKind.FollowUps], notes);
    }

    private static IntegrationConnectorDefinition Meta(string key, string name, string notes)
    {
        return new(key, name, IntegrationConnectorCategory.Communication, IntegrationConnectorFirstMode.DeferredApi, IntegrationConnectorRiskLevel.VeryHigh, IntegrationConnectorRoadmapPhase.V8V9CommunicationContent, IntegrationSourceKind.Email, IntegrationConnectorActionPolicy.DraftWithExplicitApproval, [IntegrationConnectorScope.ReadMetadata, IntegrationConnectorScope.ReadContent, IntegrationConnectorScope.GenerateSuggestion], [IntegrationTargetKind.FollowUps, IntegrationTargetKind.SearchKnowledge], notes);
    }

    private static IntegrationConnectorDefinition Context(string key, string name, IntegrationConnectorFirstMode firstMode, IntegrationConnectorRiskLevel riskLevel, string notes)
    {
        return new(key, name, IntegrationConnectorCategory.BrowserContactsPhotosContext, firstMode, riskLevel, IntegrationConnectorRoadmapPhase.V8V9CommunicationContent, IntegrationSourceKind.CloudFile, IntegrationConnectorActionPolicy.PreviewOnly, BasicReadScopes(firstMode), [IntegrationTargetKind.SearchKnowledge, IntegrationTargetKind.EvidenceVault], notes);
    }

    private static IntegrationConnectorDefinition Admin(string key, string name, string notes)
    {
        return new(key, name, IntegrationConnectorCategory.OrdersAccountsLifeAdmin, IntegrationConnectorFirstMode.EmailDerivedPreview, IntegrationConnectorRiskLevel.High, IntegrationConnectorRoadmapPhase.V5Plus, IntegrationSourceKind.Email, IntegrationConnectorActionPolicy.PreviewOnly, [IntegrationConnectorScope.ReadMetadata, IntegrationConnectorScope.ReadContent], [IntegrationTargetKind.BillsPayments, IntegrationTargetKind.PaymentCalendar, IntegrationTargetKind.EvidenceVault], notes);
    }

    private static IntegrationConnectorDefinition Automation(string key, string name, string notes)
    {
        return new(key, name, IntegrationConnectorCategory.LearningCrmSupportAutomation, IntegrationConnectorFirstMode.DeferredApi, IntegrationConnectorRiskLevel.High, IntegrationConnectorRoadmapPhase.DeferredHighRisk, IntegrationSourceKind.ManualImport, IntegrationConnectorActionPolicy.PreviewOnly, [IntegrationConnectorScope.ReadMetadata], [IntegrationTargetKind.ItemState, IntegrationTargetKind.FollowUps], notes);
    }

    private static IntegrationConnectorDefinition WebsiteMobile(string key, string name, IntegrationConnectorFirstMode firstMode, IntegrationConnectorRiskLevel riskLevel, string notes)
    {
        return new(key, name, IntegrationConnectorCategory.WebsiteMobile, firstMode, riskLevel, IntegrationConnectorRoadmapPhase.MobileCompanion, IntegrationSourceKind.ManualImport, IntegrationConnectorActionPolicy.PreviewOnly, BasicReadScopes(firstMode), [IntegrationTargetKind.EvidenceVault, IntegrationTargetKind.FollowUps], notes);
    }

    private static IntegrationConnectorScope[] BasicReadScopes(IntegrationConnectorFirstMode firstMode)
    {
        return firstMode is IntegrationConnectorFirstMode.LocalFile or IntegrationConnectorFirstMode.LocalReadOnly or IntegrationConnectorFirstMode.InAppEntry
            ? [IntegrationConnectorScope.ReadLocalFile]
            : [IntegrationConnectorScope.ReadMetadata, IntegrationConnectorScope.ReadContent];
    }
}
