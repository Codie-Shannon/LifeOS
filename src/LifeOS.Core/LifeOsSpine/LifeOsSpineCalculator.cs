namespace LifeOS.Core.LifeOsSpine;

public static class LifeOsSpineCalculator
{
    public static LifeOsSpineSummary Calculate(LifeOsSpineProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        var modules = profile.Modules ?? [];
        var itemRules = profile.ItemRules ?? [];
        var pressureSources = profile.PressureSources ?? [];

        var reasons = new List<string>
        {
            profile.MasterRule
        };

        if (profile.ItemStateModelRequired)
        {
            reasons.Add("v4 must finish the item/state model before real integrations land.");
        }

        if (profile.WeeklyPressureEngineRequired)
        {
            reasons.Add("The Weekly Pressure Engine is the product core; modules exist to reduce pressure.");
        }

        if (profile.IntegrationsDeferredToV5)
        {
            reasons.Add("Integrations are deferred to v5 so external data has a real LifeOS spine to land in.");
        }

        if (profile.CompanionAppDeferredToV65)
        {
            reasons.Add("The companion app is deferred to v6.5 after the spine, integrations, review gates, and automation boundaries exist.");
        }

        if (profile.MajorUiReshapeDeferred)
        {
            reasons.Add("Major UI/workspace reshape remains deferred until real workflows are proven.");
        }

        reasons.Add("v4.0 maps the canon spine; later v4.x versions build the missing spine modules and pressure rules.");

        return new LifeOsSpineSummary
        {
            Version = profile.Version,
            Mode = profile.Mode,
            MasterRule = profile.MasterRule,
            ModuleCount = modules.Count,
            CanonModules = modules.Count(module => module.Status == LifeOsSpineStatus.Canon),
            ActiveModules = modules.Count(module => module.Status == LifeOsSpineStatus.Active),
            NeedsModelModules = modules.Count(module => module.Status == LifeOsSpineStatus.NeedsModel),
            RequiredForV4Modules = modules.Count(module => module.RequiredForV4),
            ItemRuleCount = itemRules.Count,
            ItemTypesCovered = itemRules.Select(rule => rule.ItemType).Distinct().Count(),
            StateCount = itemRules
                .SelectMany(rule => rule.AllowedStates ?? [])
                .Distinct()
                .Count(),
            PressureSourceCount = pressureSources.Count,
            CriticalPressureSources = pressureSources.Count(source => source.ImpactLevel == PressureImpactLevel.Critical),
            IntegrationsDeferredToV5 = profile.IntegrationsDeferredToV5,
            CompanionAppDeferredToV65 = profile.CompanionAppDeferredToV65,
            MajorUiReshapeDeferred = profile.MajorUiReshapeDeferred,
            ItemStateModelRequired = profile.ItemStateModelRequired,
            WeeklyPressureEngineRequired = profile.WeeklyPressureEngineRequired,
            Reasons = reasons,
            CoreModules = modules
                .OrderBy(module => module.Area)
                .ThenBy(module => module.Priority)
                .ThenBy(module => module.Name)
                .ToList(),
            ItemRules = itemRules
                .OrderByDescending(rule => rule.RequiredForV4)
                .ThenBy(rule => rule.ItemType)
                .ToList(),
            PressureSources = pressureSources
                .OrderByDescending(source => source.ImpactLevel)
                .ThenBy(source => source.Name)
                .ToList()
        };
    }

    public static string FormatArea(LifeOsSpineArea area)
    {
        return area switch
        {
            LifeOsSpineArea.CommandCentre => "Command Centre",
            LifeOsSpineArea.PressureEngine => "Pressure Engine",
            LifeOsSpineArea.MoneySpine => "Money Spine",
            LifeOsSpineArea.WorkSpine => "Work Spine",
            LifeOsSpineArea.PeopleRelationshipSpine => "People / Relationship Spine",
            LifeOsSpineArea.TimeSpine => "Time Spine",
            LifeOsSpineArea.EvidenceSpine => "Evidence Spine",
            LifeOsSpineArea.IntegrationSpine => "Integration Spine",
            LifeOsSpineArea.TechnicalSpine => "Technical Spine",
            LifeOsSpineArea.WeeklyCloseOut => "Weekly Close-Out",
            _ => area.ToString()
        };
    }

    public static string FormatStatus(LifeOsSpineStatus status)
    {
        return status switch
        {
            LifeOsSpineStatus.Canon => "Canon",
            LifeOsSpineStatus.Active => "Active",
            LifeOsSpineStatus.NeedsModel => "Needs model",
            LifeOsSpineStatus.PlannedForV4 => "Planned for v4",
            LifeOsSpineStatus.PlannedForV5 => "Planned for v5",
            LifeOsSpineStatus.Parked => "Parked",
            _ => status.ToString()
        };
    }

    public static string FormatItemType(LifeOsItemType itemType)
    {
        return itemType switch
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
            _ => itemType.ToString()
        };
    }

    public static string FormatImpact(PressureImpactLevel impactLevel)
    {
        return impactLevel switch
        {
            PressureImpactLevel.None => "None",
            PressureImpactLevel.Low => "Low",
            PressureImpactLevel.Medium => "Medium",
            PressureImpactLevel.High => "High",
            PressureImpactLevel.Critical => "Critical",
            _ => impactLevel.ToString()
        };
    }

    public static string FormatStates(IEnumerable<LifeOsItemState> states)
    {
        return string.Join(", ", states.Select(FormatState));
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
}
