namespace LifeOS.Core.ItemState;

public static class LifeOsItemStateCalculator
{
    public static LifeOsItemStateSummary Calculate(LifeOsItemStateProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        var items = profile.Items ?? [];
        var rules = profile.TransitionRules ?? [];

        var reasons = new List<string>
        {
            profile.MasterRule
        };

        if (profile.ItemStateEngineActive)
        {
            reasons.Add("v4.1 turns the v4.0 spine map into a working item/state engine.");
        }

        if (profile.ManualReviewRequired)
        {
            reasons.Add("Manual review is required before imported, OCR, or uncertain items become trusted LifeOS state.");
        }

        if (profile.EvidenceBeforeTrustedState)
        {
            reasons.Add("Evidence or source notes are required before money, invoice, receipt, or integration items become trusted.");
        }

        if (!profile.RealIntegrationsEnabled)
        {
            reasons.Add("Real integrations remain disabled; v4.1 models state before v5 connects external data.");
        }

        reasons.Add("Command Centre should receive pressure signals from item state, not from random raw records.");

        return new LifeOsItemStateSummary
        {
            Version = profile.Version,
            Mode = profile.Mode,
            MasterRule = profile.MasterRule,
            TotalItems = items.Count,
            OpenItems = items.Count(item => item.State == LifeOsItemState.Open),
            NeedsReviewItems = items.Count(item => item.State == LifeOsItemState.NeedsReview),
            DueSoonOrTodayItems = items.Count(item => item.State is LifeOsItemState.DueSoon or LifeOsItemState.DueToday),
            OverdueItems = items.Count(item => item.State == LifeOsItemState.Overdue),
            WaitingItems = items.Count(item => item.State is LifeOsItemState.Waiting or LifeOsItemState.DoNotChase or LifeOsItemState.Blocked),
            PaidOrClosedItems = items.Count(item => item.State is LifeOsItemState.Paid or LifeOsItemState.Closed or LifeOsItemState.Archived),
            UntrustedItems = items.Count(item => !item.Trusted),
            MoneyImpactItems = items.Count(item => item.ImpactAreas.Contains(LifeOsItemImpactArea.Money)),
            SafeToSpendImpactItems = items.Count(item => item.AffectsSafeToSpend),
            AgendaImpactItems = items.Count(item => item.AffectsAgenda),
            WeeklyCloseOutImpactItems = items.Count(item => item.AffectsWeeklyCloseOut),
            CommandCentreImpactItems = items.Count(item => item.AffectsCommandCentre),
            TransitionRules = rules.Count,
            EvidenceRequiredTransitions = rules.Count(rule => rule.RequiresEvidence),
            ManualReviewTransitions = rules.Count(rule => rule.RequiresManualReview),
            RealIntegrationsEnabled = profile.RealIntegrationsEnabled,
            ItemStateEngineActive = profile.ItemStateEngineActive,
            ManualReviewRequired = profile.ManualReviewRequired,
            EvidenceBeforeTrustedState = profile.EvidenceBeforeTrustedState,
            Reasons = reasons,
            ReviewQueue = items
                .Where(item => item.State == LifeOsItemState.NeedsReview || !item.Trusted)
                .OrderByDescending(item => item.RiskLevel)
                .ThenBy(item => item.DueDate ?? DateTime.MaxValue)
                .ThenBy(item => item.Title)
                .ToList(),
            PressureItems = items
                .Where(item => item.AffectsCommandCentre || item.RiskLevel is LifeOsItemRiskLevel.High or LifeOsItemRiskLevel.Critical)
                .OrderByDescending(item => item.RiskLevel)
                .ThenBy(item => item.DueDate ?? DateTime.MaxValue)
                .ThenBy(item => item.Title)
                .ToList(),
            MoneyItems = items
                .Where(item => item.ImpactAreas.Contains(LifeOsItemImpactArea.Money))
                .OrderBy(item => item.DueDate ?? DateTime.MaxValue)
                .ThenBy(item => item.Title)
                .ToList(),
            WorkItems = items
                .Where(item => item.ImpactAreas.Contains(LifeOsItemImpactArea.Work))
                .OrderByDescending(item => item.RiskLevel)
                .ThenBy(item => item.Title)
                .ToList(),
            Rules = rules
                .OrderBy(rule => rule.Type)
                .ThenBy(rule => rule.FromState)
                .ThenBy(rule => rule.ToState)
                .ToList()
        };
    }

    public static string FormatType(LifeOsItemType type)
    {
        return type switch
        {
            LifeOsItemType.Bill => "Bill",
            LifeOsItemType.UpcomingPayment => "Upcoming payment",
            LifeOsItemType.PayLater => "Pay Later / BNPL",
            LifeOsItemType.Receipt => "Receipt",
            LifeOsItemType.Invoice => "Invoice",
            LifeOsItemType.ExpectedIncome => "Expected income",
            LifeOsItemType.ConfirmedPayment => "Confirmed payment",
            LifeOsItemType.WorkSession => "Work session",
            LifeOsItemType.WorkPipeline => "Work pipeline",
            LifeOsItemType.FollowUp => "Follow-up",
            LifeOsItemType.RelationshipThread => "Relationship thread",
            LifeOsItemType.AgendaItem => "Agenda item",
            LifeOsItemType.ProofItem => "Proof item",
            LifeOsItemType.EvidenceItem => "Evidence item",
            LifeOsItemType.RepairItem => "Repair item",
            LifeOsItemType.TripEvent => "Trip/event",
            LifeOsItemType.IntegrationPreview => "Integration preview",
            LifeOsItemType.WeeklyCloseOutItem => "Weekly close-out",
            _ => type.ToString()
        };
    }

    public static string FormatState(LifeOsItemState state)
    {
        return state switch
        {
            LifeOsItemState.Open => "Open",
            LifeOsItemState.NeedsReview => "Needs review",
            LifeOsItemState.Planned => "Planned",
            LifeOsItemState.DueSoon => "Due soon",
            LifeOsItemState.DueToday => "Due today",
            LifeOsItemState.Overdue => "Overdue",
            LifeOsItemState.Waiting => "Waiting",
            LifeOsItemState.DoNotChase => "Do not chase",
            LifeOsItemState.Blocked => "Blocked",
            LifeOsItemState.Confirmed => "Confirmed",
            LifeOsItemState.Paid => "Paid",
            LifeOsItemState.PartPaid => "Part paid",
            LifeOsItemState.Invoiced => "Invoiced",
            LifeOsItemState.PaymentExpected => "Payment expected",
            LifeOsItemState.Closed => "Closed",
            LifeOsItemState.Archived => "Archived",
            LifeOsItemState.Ignored => "Ignored",
            _ => state.ToString()
        };
    }

    public static string FormatRisk(LifeOsItemRiskLevel riskLevel)
    {
        return riskLevel switch
        {
            LifeOsItemRiskLevel.None => "No risk",
            LifeOsItemRiskLevel.Low => "Low",
            LifeOsItemRiskLevel.Medium => "Medium",
            LifeOsItemRiskLevel.High => "High",
            LifeOsItemRiskLevel.Critical => "Critical",
            _ => riskLevel.ToString()
        };
    }

    public static string FormatSource(LifeOsItemSourceKind sourceKind)
    {
        return sourceKind switch
        {
            LifeOsItemSourceKind.Manual => "Manual",
            LifeOsItemSourceKind.ReceiptOcr => "Receipt OCR",
            LifeOsItemSourceKind.EmailProfile => "Email profile",
            LifeOsItemSourceKind.CalendarImport => "Calendar import",
            LifeOsItemSourceKind.FileImport => "File import",
            LifeOsItemSourceKind.AccountingImport => "Accounting import",
            LifeOsItemSourceKind.TimerAgent => "TimerAgent",
            LifeOsItemSourceKind.MoneyTimeline => "Money Timeline",
            LifeOsItemSourceKind.PayLater => "Pay Later",
            LifeOsItemSourceKind.WorkPipeline => "Work Pipeline",
            LifeOsItemSourceKind.FutureIntegration => "Future integration",
            _ => sourceKind.ToString()
        };
    }

    public static string FormatImpactAreas(IEnumerable<LifeOsItemImpactArea> areas)
    {
        return string.Join(", ", areas.Select(FormatImpactArea));
    }

    public static string FormatImpactArea(LifeOsItemImpactArea area)
    {
        return area switch
        {
            LifeOsItemImpactArea.Money => "Money",
            LifeOsItemImpactArea.Time => "Time",
            LifeOsItemImpactArea.Work => "Work",
            LifeOsItemImpactArea.People => "People",
            LifeOsItemImpactArea.Proof => "Proof",
            LifeOsItemImpactArea.Evidence => "Evidence",
            LifeOsItemImpactArea.WeeklyCloseOut => "Weekly Close-Out",
            LifeOsItemImpactArea.IntegrationInbox => "Integration Inbox",
            LifeOsItemImpactArea.CommandCentre => "Command Centre",
            _ => area.ToString()
        };
    }
}
