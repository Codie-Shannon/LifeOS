using LifeOS.Core.LifeOsSpine;

namespace LifeOS.Shared.LifeOsSpine;

public static class LifeOsSpineDemoData
{
    public static LifeOsSpineProfile CreateDefaultProfile()
    {
        return new LifeOsSpineProfile
        {
            Version = "v4.0",
            Mode = "LifeOS spine recovery map",
            MasterRule = "Everything important becomes an item. Every item has state. Every state affects pressure. Every pressure feeds the Command Centre.",
            IntegrationsDeferredToV5 = true,
            CompanionAppDeferredToV65 = true,
            MajorUiReshapeDeferred = true,
            ItemStateModelRequired = true,
            WeeklyPressureEngineRequired = true,
            Modules =
            [
                Module("Command Centre", LifeOsSpineArea.CommandCentre, LifeOsSpineStatus.Active, 1, true, "Shows the current pressure state and safe next actions.", "Pressure Engine, money, work, agenda, follow-ups, proof, weekly close-out.", "What matters now?"),
                Module("Pressure Engine", LifeOsSpineArea.PressureEngine, LifeOsSpineStatus.NeedsModel, 1, true, "Turns item states into weekly/today pressure.", "Money, time, work, people, proof, hidden deductions, due dates.", "Pressure level and reason."),
                Module("Money Pressure", LifeOsSpineArea.MoneySpine, LifeOsSpineStatus.Active, 1, true, "Separates safe money from expected money.", "Safe-to-Spend, Money Timeline, bills, pay later, expected income.", "Money pressure."),
                Module("Safe-to-Spend", LifeOsSpineArea.MoneySpine, LifeOsSpineStatus.NeedsModel, 1, true, "Calculates actually safe money after commitments.", "Bills, pay later, hidden deductions, confirmed income, minimums.", "Safe money, not expected money."),
                Module("Money Timeline", LifeOsSpineArea.MoneySpine, LifeOsSpineStatus.Canon, 1, true, "Digitises real paper cashflow-by-date logic.", "Incoming/outgoing money by date/time and lowest balance point.", "Lowest balance and danger dates."),
                Module("Bills / Subscriptions", LifeOsSpineArea.MoneySpine, LifeOsSpineStatus.PlannedForV4, 1, true, "Tracks recurring and one-off payment obligations as items.", "Upcoming Payments, Agenda, Safe-to-Spend, Weekly Close-Out.", "Bills due / overdue."),
                Module("Upcoming Payments", LifeOsSpineArea.MoneySpine, LifeOsSpineStatus.PlannedForV4, 1, true, "Tracks future money leaving before it becomes a problem.", "Bills, pay later, Agenda, Money Timeline.", "Upcoming payment pressure."),
                Module("Pay Later / Zip / Afterpay", LifeOsSpineArea.MoneySpine, LifeOsSpineStatus.PlannedForV4, 1, true, "Tracks BNPL totals, instalments, due dates, and danger weeks.", "Safe-to-Spend, Upcoming Payments, Agenda, Weekly Close-Out.", "Pay-later load."),
                Module("Money Profile / Hidden Deductions", LifeOsSpineArea.MoneySpine, LifeOsSpineStatus.PlannedForV4, 1, true, "Tracks tax, GST, ACC, student loan, KiwiSaver, debts, and custom deductions.", "Safe-to-Spend and Money Pressure.", "Hidden deduction pressure."),
                Module("Agenda", LifeOsSpineArea.TimeSpine, LifeOsSpineStatus.Active, 1, true, "Date-first, calm event/due-date list.", "Calendar events, bills due, follow-ups, school/family commitments, work sessions.", "Today / this week load."),
                Module("Payment Calendar", LifeOsSpineArea.TimeSpine, LifeOsSpineStatus.PlannedForV4, 2, true, "Shows money due by date without becoming a cluttered calendar clone.", "Upcoming Payments, Bills, Pay Later, Money Timeline.", "Payment due dates."),
                Module("Follow-Ups / Waiting-On", LifeOsSpineArea.PeopleRelationshipSpine, LifeOsSpineStatus.Active, 1, true, "Tracks who needs action, who is waiting, and who should not be chased yet.", "Work Pipeline, Relationship Radar, Email Radar, Agenda.", "Follow-up due / do-not-chase."),
                Module("Relationship / Email Radar", LifeOsSpineArea.PeopleRelationshipSpine, LifeOsSpineStatus.Canon, 2, true, "Structured profile-based email/thread state, not magic inbox reading.", "Follow-Ups, Work Pipeline, timeline, proof, payment mentions.", "Waiting-on state."),
                Module("Work Pipeline / Opportunity Tracker", LifeOsSpineArea.WorkSpine, LifeOsSpineStatus.PlannedForV4, 1, true, "Tracks paid work, warm leads, proof projects, portfolio projects, blocked work, and parked ideas.", "Follow-Ups, Work Sessions, Proof Tracker, Paid Work Centre, Money Timeline.", "Active work / blocked work."),
                Module("Work Sessions", LifeOsSpineArea.WorkSpine, LifeOsSpineStatus.Active, 1, true, "Tracks focused work blocks.", "Timesheet Evidence, Paid Work Centre, Proof Tracker, Weekly Close-Out.", "Invoice-ready work."),
                Module("Paid Work Centre", LifeOsSpineArea.WorkSpine, LifeOsSpineStatus.Active, 1, true, "Reviews billable work and turns it into invoice/work summary state.", "Work Sessions, invoices, expected income, proof, client summaries.", "Invoice needed / payment expected."),
                Module("Proof Tracker", LifeOsSpineArea.EvidenceSpine, LifeOsSpineStatus.Active, 1, true, "Turns work evidence into proof/case-study material.", "Work Sessions, Paid Work Centre, Evidence Vault, portfolio proof.", "Proof needs finishing."),
                Module("Evidence Vault", LifeOsSpineArea.EvidenceSpine, LifeOsSpineStatus.Active, 1, true, "Stores evidence/photos/screenshots/files as local proof links.", "Receipt OCR, proof items, client work, repairs, trips.", "Evidence exists / missing."),
                Module("Receipt OCR / Evidence-to-Item", LifeOsSpineArea.EvidenceSpine, LifeOsSpineStatus.PlannedForV4, 1, true, "Turns receipts/documents into reviewed stateful money/proof items.", "Receipt, bill, payment, pay-later, proof, Money Timeline.", "Needs review / money item created."),
                Module("Weekly Close-Out", LifeOsSpineArea.WeeklyCloseOut, LifeOsSpineStatus.PlannedForV4, 1, true, "The weekly reset loop that reviews money, work, proof, follow-ups, open loops, and next week pressure.", "All spine areas.", "Week complete / still open."),
                Module("Integration Inbox / Sync Preview Queue", LifeOsSpineArea.IntegrationSpine, LifeOsSpineStatus.PlannedForV4, 1, true, "Staging area before external/OCR/imported data becomes trusted.", "Every integration and reviewable item.", "Data waiting for review."),
                Module("Universal Spine", LifeOsSpineArea.TechnicalSpine, LifeOsSpineStatus.Active, 1, true, "Technical linking layer across modules.", "Every important item, evidence, person, project, and timeline link.", "Linked context."),
                Module("Search / Knowledge", LifeOsSpineArea.TechnicalSpine, LifeOsSpineStatus.Active, 2, true, "Local knowledge/search structure before connected knowledge sources.", "Docs, notes, email profiles, proof, local search profiles.", "Knowledge needs review."),
                Module("Settings / Safety", LifeOsSpineArea.TechnicalSpine, LifeOsSpineStatus.Active, 1, true, "Guardrails for money, evidence, screenshots, destructive actions, privacy, and integration safety.", "All risky state transitions and future integrations.", "Safety gate active.")
            ],
            ItemRules =
            [
                ItemRule(LifeOsItemType.Bill, "Bill Item", "manual entry, statement, email, receipt, import", true, "Bill evidence should link to invoice/notice/receipt when available.", "Due/overdue bills reduce safe-to-spend and increase weekly pressure.", "Money Spine → Agenda → Weekly Close-Out"),
                ItemRule(LifeOsItemType.UpcomingPayment, "Upcoming Payment Item", "bill, subscription, BNPL instalment, fine, rego, school/family cost", true, "Payment evidence or source note required for trusted state.", "Due soon/today/overdue items affect Money Timeline and Command Centre.", "Money Spine → Time Spine"),
                ItemRule(LifeOsItemType.PayLater, "Pay Later / BNPL Item", "Zip, Afterpay, Laybuy, manual screenshot, email receipt", true, "Keep source evidence and instalment schedule notes.", "Remaining balance and weekly load reduce safe-to-spend.", "Pay Later → Upcoming Payments → Weekly Close-Out"),
                ItemRule(LifeOsItemType.Receipt, "Receipt Item", "photo upload, file upload, email receipt, manual entry", true, "Original file is evidence; OCR result is untrusted until reviewed.", "Reviewed receipts create/link payment, bill, pay-later, proof, or expense items.", "Evidence Vault → Integration Inbox → Money Spine"),
                ItemRule(LifeOsItemType.Invoice, "Invoice Item", "paid work, accounting export, manual invoice, client summary", true, "Invoice should link to work sessions and proof.", "Invoiced/payment expected is not safe money until paid.", "Paid Work Centre → Expected Income → Money Timeline"),
                ItemRule(LifeOsItemType.ExpectedIncome, "Expected Income Item", "invoice, job, timesheet, client promise, accounting status", true, "Source/evidence required before it affects expected-income totals.", "Expected income is not safe money until confirmed paid.", "Money Pressure → Money Timeline"),
                ItemRule(LifeOsItemType.WorkPipeline, "Work Pipeline Item", "lead, client, project, proof job, portfolio work", true, "Link emails, notes, files, proof, and payment state where possible.", "Waiting/blocked/invoice/payment states affect work pressure.", "Work Spine → Follow-Ups → Command Centre"),
                ItemRule(LifeOsItemType.FollowUp, "Follow-Up Item", "manual follow-up, email profile, client state, waiting-on", true, "Thread/profile evidence improves confidence.", "Due/overdue follow-ups increase people/work pressure; do-not-chase reduces noise.", "People Spine → Agenda"),
                ItemRule(LifeOsItemType.AgendaItem, "Agenda Item", "manual event, calendar import later, bill due, payment due, work session", true, "Imported events need review before trust.", "Fixed/flexible events affect daily and weekly load.", "Time Spine → Pressure Engine"),
                ItemRule(LifeOsItemType.WorkSession, "Work Session Item", "TimerAgent, manual work log, project session", true, "Work sessions should link to proof and paid work state.", "Invoice-needed sessions create work/money pressure.", "Work Sessions → Paid Work Centre"),
                ItemRule(LifeOsItemType.ProofItem, "Proof Item", "screenshots, notes, docs, before/after, client result", true, "Evidence links are required for proof confidence.", "Missing proof creates close-out and portfolio pressure.", "Evidence Spine → Work Spine"),
                ItemRule(LifeOsItemType.IntegrationPreview, "Integration Preview Item", "email, calendar, files, accounting, bank/open banking later, OCR", true, "External data must be reviewed before becoming trusted LifeOS state.", "Unreviewed imported data creates review pressure, not final truth.", "Integration Inbox → Item State Engine")
            ],
            PressureSources =
            [
                Pressure("Expected money is not paid", LifeOsSpineArea.MoneySpine, PressureImpactLevel.Critical, "Expected income, invoices, payment expected items", "What money is expected but not safe?", "Expected money warning", "Wait, follow-up, or mark paid only with evidence."),
                Pressure("Bills / upcoming payments due", LifeOsSpineArea.MoneySpine, PressureImpactLevel.Critical, "Bills, subscriptions, pay-later instalments, upcoming payments", "What money leaves this week?", "Money danger week", "Pay, defer, or budget."),
                Pressure("Hidden deductions missing", LifeOsSpineArea.MoneySpine, PressureImpactLevel.High, "Tax, GST, ACC, student loan, KiwiSaver, custom deductions", "What deductions are not visible in the balance?", "Safe-to-spend risk", "Add/confirm deductions before spending."),
                Pressure("Follow-up due", LifeOsSpineArea.PeopleRelationshipSpine, PressureImpactLevel.High, "Follow-up items, relationship threads, waiting-on status", "Who needs a message and who should not be chased?", "Follow-up pressure", "Send, wait, or mark do-not-chase."),
                Pressure("Work stuck or invoice-ready", LifeOsSpineArea.WorkSpine, PressureImpactLevel.High, "Work pipeline, work sessions, paid work centre", "What active work needs movement, proof, invoice, or payment follow-up?", "Work pressure", "Move one item to next state."),
                Pressure("Proof missing", LifeOsSpineArea.EvidenceSpine, PressureImpactLevel.Medium, "Proof items, work sessions, screenshots, receipts", "What evidence is needed to close or prove the work?", "Proof pressure", "Attach or create evidence."),
                Pressure("Unreviewed imported/OCR data", LifeOsSpineArea.IntegrationSpine, PressureImpactLevel.High, "Integration preview items, OCR outputs, manual imports", "What data is waiting before it becomes trusted state?", "Review queue pressure", "Accept, reject, link, or archive."),
                Pressure("Weekly close-out open", LifeOsSpineArea.WeeklyCloseOut, PressureImpactLevel.High, "Open money/work/follow-up/proof items", "What has to be closed before the week resets?", "Close-out pressure", "Review week and set next week pressure.")
            ]
        };
    }

    private static LifeOsSpineModule Module(
        string name,
        LifeOsSpineArea area,
        LifeOsSpineStatus status,
        int priority,
        bool requiredForV4,
        string purpose,
        string connectsTo,
        string commandCentreSignal)
    {
        return new LifeOsSpineModule
        {
            Name = name,
            Area = area,
            Status = status,
            Priority = priority,
            RequiredForV4 = requiredForV4,
            Purpose = purpose,
            ConnectsTo = connectsTo,
            CommandCentreSignal = commandCentreSignal,
            Boundary = "v4 completes the LifeOS operating spine. Real external integrations are v5."
        };
    }

    private static LifeOsStatefulItemRule ItemRule(
        LifeOsItemType itemType,
        string name,
        string sourceExamples,
        bool requiredForV4,
        string evidenceRule,
        string pressureRule,
        string landingRule)
    {
        return new LifeOsStatefulItemRule
        {
            ItemType = itemType,
            Name = name,
            SourceExamples = sourceExamples,
            RequiredForV4 = requiredForV4,
            EvidenceRule = evidenceRule,
            PressureRule = pressureRule,
            LandingRule = landingRule,
            ReviewGate = "Create/review item before it affects trusted state.",
            AllowedStates =
            [
                LifeOsItemState.Open,
                LifeOsItemState.NeedsReview,
                LifeOsItemState.Planned,
                LifeOsItemState.DueSoon,
                LifeOsItemState.DueToday,
                LifeOsItemState.Overdue,
                LifeOsItemState.Waiting,
                LifeOsItemState.Blocked,
                LifeOsItemState.Confirmed,
                LifeOsItemState.Paid,
                LifeOsItemState.PartPaid,
                LifeOsItemState.Invoiced,
                LifeOsItemState.PaymentExpected,
                LifeOsItemState.Closed,
                LifeOsItemState.Archived,
                LifeOsItemState.Ignored
            ]
        };
    }

    private static LifeOsPressureSource Pressure(
        string name,
        LifeOsSpineArea area,
        PressureImpactLevel impactLevel,
        string sourceItems,
        string pressureQuestion,
        string commandCentreSignal,
        string safeNextAction)
    {
        return new LifeOsPressureSource
        {
            Name = name,
            Area = area,
            ImpactLevel = impactLevel,
            SourceItems = sourceItems,
            PressureQuestion = pressureQuestion,
            CommandCentreSignal = commandCentreSignal,
            SafeNextAction = safeNextAction
        };
    }
}
