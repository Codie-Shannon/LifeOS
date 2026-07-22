using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LifeOS.Core.CareerStudio;

namespace LifeOS.Desktop;

public sealed class CareerStudioView : UserControl
{
    private static readonly DateTimeOffset ProofNow = new(2026, 7, 22, 14, 0, 0, TimeSpan.FromHours(12));
    private readonly CareerStudioProof _proof = CareerProofData.Build(ProofNow);
    private readonly IReadOnlyList<CareerApplication> _applications;
    private readonly CareerMaterialsProof _materials = CareerMaterialsProofData.Build(ProofNow);
    private readonly CareerPreparationProof _preparation;
    private readonly CareerClosureProof _closure;
    private readonly ContentControl _content = new();
    private readonly TextBlock _title = new();
    private readonly TextBlock _subtitle = new();
    private readonly Dictionary<string, Button> _buttons = new(StringComparer.Ordinal);
    private string _activePage = "Pipeline";

    public CareerStudioView()
    {
        Background = Brush("#0B1020");
        Foreground = Brushes.White;
        FontFamily = new FontFamily("Segoe UI");
        _applications = CareerApplicationProofData.Build(_proof, ProofNow);
        _preparation = CareerPreparationProofData.Build(_materials, _proof, ProofNow);
        _closure = CareerClosureProofData.Build(ProofNow);
        Content = Build();
        Show("Pipeline");
    }

    private UIElement Build()
    {
        var root = new Grid();
        root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(260) });
        root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var nav = new StackPanel();
        nav.Children.Add(Label("LIFEOS v12", 12, "#9AA9C7", FontWeights.SemiBold));
        nav.Children.Add(Label("Career Studio", 29, "#FFFFFF", FontWeights.Bold, new Thickness(0, 5, 0, 2)));
        nav.Children.Add(Label("Opportunity and application pipeline", 13, "#9AA9C7", FontWeights.Normal, new Thickness(0, 0, 0, 20)));

        foreach (var page in new[] { "Pipeline", "Opportunity detail", "Candidate review", "Application checklist", "Application timeline", "Career profile", "CV variants", "Portfolio evidence", "Cover letter", "Application pack", "Interview preparation", "Follow-ups", "References", "Questions to ask", "Career review", "Coverage report" })
        {
            var button = new Button
            {
                Content = page,
                Tag = page,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Padding = new Thickness(14, 11, 14, 11),
                Margin = new Thickness(0, 0, 0, 7),
                Background = Brushes.Transparent,
                Foreground = Brush("#D9E2F3"),
                BorderBrush = Brushes.Transparent,
                BorderThickness = new Thickness(1),
                Cursor = System.Windows.Input.Cursors.Hand,
                Template = NavigationButtonTemplate()
            };
            button.MouseEnter += (_, _) => ApplyNavigationVisual(button, isHover: true);
            button.MouseLeave += (_, _) => ApplyNavigationVisual(button, isHover: false);
            button.Click += (_, _) => Show((string)button.Tag);
            _buttons[page] = button;
            nav.Children.Add(button);
        }

        nav.Children.Add(BoundaryCard());

        var rail = new Border
        {
            Background = Brush("#11182B"),
            BorderBrush = Brush("#27324A"),
            BorderThickness = new Thickness(0, 0, 1, 0),
            Padding = new Thickness(18, 22, 18, 18),
            Child = new ScrollViewer
            {
                Content = nav,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            }
        };
        root.Children.Add(rail);

        var main = new Grid { Margin = new Thickness(28, 22, 28, 24) };
        main.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        main.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

        _title.FontSize = 31;
        _title.FontWeight = FontWeights.Bold;
        _title.Foreground = Brushes.White;

        _subtitle.FontSize = 14;
        _subtitle.Foreground = Brush("#A9B8D5");
        _subtitle.Margin = new Thickness(0, 5, 0, 20);

        var heading = new StackPanel();
        heading.Children.Add(_title);
        heading.Children.Add(_subtitle);
        main.Children.Add(heading);

        Grid.SetRow(_content, 1);
        main.Children.Add(_content);
        Grid.SetColumn(main, 1);
        root.Children.Add(main);

        return root;
    }

    private static ControlTemplate NavigationButtonTemplate()
    {
        var border = new FrameworkElementFactory(typeof(Border));
        border.SetBinding(Border.BackgroundProperty, new System.Windows.Data.Binding(nameof(Button.Background))
        {
            RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent)
        });
        border.SetBinding(Border.BorderBrushProperty, new System.Windows.Data.Binding(nameof(Button.BorderBrush))
        {
            RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent)
        });
        border.SetBinding(Border.BorderThicknessProperty, new System.Windows.Data.Binding(nameof(Button.BorderThickness))
        {
            RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent)
        });
        border.SetValue(Border.CornerRadiusProperty, new CornerRadius(0));

        var presenter = new FrameworkElementFactory(typeof(ContentPresenter));
        presenter.SetBinding(ContentPresenter.ContentProperty, new System.Windows.Data.Binding(nameof(Button.Content))
        {
            RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent)
        });
        presenter.SetBinding(ContentPresenter.MarginProperty, new System.Windows.Data.Binding(nameof(Button.Padding))
        {
            RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent)
        });
        presenter.SetBinding(ContentPresenter.HorizontalAlignmentProperty, new System.Windows.Data.Binding(nameof(Button.HorizontalContentAlignment))
        {
            RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent)
        });
        presenter.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
        border.AppendChild(presenter);

        return new ControlTemplate(typeof(Button)) { VisualTree = border };
    }

    private void ApplyNavigationVisual(Button button, bool isHover)
    {
        var active = string.Equals(button.Tag as string, _activePage, StringComparison.Ordinal);

        if (active)
        {
            button.Background = Brush("#23335A");
            button.BorderBrush = Brush("#607DFF");
            button.Foreground = Brushes.White;
            return;
        }

        button.Background = isHover ? Brush("#19233B") : Brushes.Transparent;
        button.BorderBrush = isHover ? Brush("#31405F") : Brushes.Transparent;
        button.Foreground = isHover ? Brushes.White : Brush("#D9E2F3");
    }

    private void Show(string page)
    {
        _activePage = page;

        foreach (var pair in _buttons)
        {
            var active = pair.Key == page;
            pair.Value.Background = active ? Brush("#23335A") : Brushes.Transparent;
            pair.Value.BorderBrush = active ? Brush("#607DFF") : Brushes.Transparent;
            pair.Value.Foreground = active ? Brushes.White : Brush("#D9E2F3");
        }

        (_title.Text, _subtitle.Text, _content.Content) = page switch
        {
            "Opportunity detail" => ("Opportunity detail", "Requirements, fit, source and linked evidence remain explicit and editable.", OpportunityDetail()),
            "Candidate review" => ("Imported and duplicate candidates", "Candidates await review; no silent trust promotion or merge occurs.", CandidateReview()),
            "Application checklist" => ("Application preparation", "Checklist completion and readiness remain explicit.", ApplicationChecklist()),
            "Application timeline" => ("Application timeline", "Status, follow-up and interview context remain auditable.", ApplicationTimeline()),
            "Career profile" => ("Career profile", "Trusted facts remain separate from presentation wording and carry provenance.", CareerProfile()),
            "CV variants" => ("Role-specific CV variants", "Requirement relevance, missing proof and unsupported claims are checked before export.", CvVariants()),
            "Portfolio evidence" => ("Portfolio evidence", "Projects remain linked to trusted Work, Project and document proof.", PortfolioEvidence()),
            "Cover letter" => ("Opportunity-linked cover letter", "Generated, manual, accepted and stale sections stay visibly distinct.", CoverLetter()),
            "Application pack" => ("Application pack", "Current, missing and stale materials are validated for the intended opportunity.", ApplicationPackView()),
                        "Interview preparation" => ("Interview preparation", "STAR examples, questions, logistics and read-only calendar context.", InterviewPreparation()),
            "Follow-ups" => ("Career follow-ups", "Due, overdue, waiting and completed follow-ups stay review-first.", FollowUps()),
            "References" => ("Reference readiness", "Permission, privacy, freshness and application usage remain explicit.", References()),
                        "Questions to ask" => ("Questions to ask", "User-owned interview questions remain ordered, linked and reviewable.", QuestionsToAsk()),
            "Career review" => ("Career Review", "Bounded analytics describe trusted LifeOS records, not hiring probability.", CareerReview()),
            "Coverage report" => ("Evidence and materials coverage", "Coverage remains drillable to authoritative records.", CoverageReport()),
            _ => ("Career opportunity pipeline", "One authoritative pipeline with board filters, closing dates and review-first status.", Pipeline())
        };
    }

    private UIElement Pipeline()
    {
        var panel = Vertical();
        var filters = new WrapPanel { Margin = new Thickness(0, 0, 0, 14) };
        foreach (var text in new[] { "All stages", "All employers", "All sources", "Closing date", "All work modes", "All priorities" })
            filters.Children.Add(Filter(text));
        panel.Children.Add(filters);

        foreach (var opportunity in _proof.Opportunities)
        {
            panel.Children.Add(Card(
                opportunity.Title,
                $"{opportunity.Employer.Name}\n{Humanize(opportunity.Stage)} â€¢ {Humanize(opportunity.WorkMode)} â€¢ {opportunity.Location}\nClosing: {(opportunity.ClosingUtc?.ToString("dd MMM yyyy") ?? "Not supplied")}\nSource: {Humanize(opportunity.Source.Type)} â€¢ Fresh {opportunity.FreshnessUtc:dd MMM HH:mm}",
                $"{Humanize(opportunity.Priority)} â€¢ {Humanize(opportunity.Stage)}".ToUpperInvariant()));
        }

        return Scroll(panel);
    }

    private UIElement OpportunityDetail()
    {
        var opportunity = _proof.Opportunities[0];
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(18) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var left = Vertical();
        left.Children.Add(Card(
            opportunity.Title,
            $"{opportunity.Employer.Name}\n{opportunity.RoleSummary}\n{Humanize(opportunity.WorkMode)} â€¢ {Humanize(opportunity.EmploymentType)}\n{opportunity.SalaryOrRateContext}\nSource: Direct employer listing",
            "AUTHORITATIVE LOCAL"));

        foreach (var requirement in opportunity.Requirements)
            left.Children.Add(Card(Humanize(requirement.Type), requirement.Description, requirement.IsRequired ? "REQUIRED" : "OPTIONAL"));

        var right = Vertical();
        var fit = opportunity.Fit!;
        right.Children.Add(Card(
            "User-reviewed fit",
            $"Strengths: {string.Join("; ", fit.Strengths)}\nGaps: {string.Join("; ", fit.Gaps)}\nBlockers: {(fit.Blockers.Count == 0 ? "None recorded" : string.Join("; ", fit.Blockers))}\n{fit.Explanation}",
            "EDITABLE"));

        right.Children.Add(Card(
            "Linked evidence",
            "People: Recruiter contact\nProjects: LifeOS\nDocuments: Role-specific CV\nPortfolio: Application development proof",
            "PRESERVED LINKS"));

        right.Children.Add(Card(
            "History",
            string.Join("\n", opportunity.History.Select(x => $"{x.OccurredUtc:dd MMM HH:mm} â€¢ {x.Action} â€¢ {x.SafeSummary}")),
            "AUDITABLE"));

        grid.Children.Add(left);
        Grid.SetColumn(right, 2);
        grid.Children.Add(right);
        return Scroll(grid);
    }

    private UIElement CandidateReview()
    {
        var panel = Vertical();

        foreach (var imported in _proof.ImportedCandidates)
        {
            panel.Children.Add(Card(
                $"Imported: {imported.RoleTitle}",
                $"{imported.EmployerName}\nSource: Integration Inbox email candidate\nCaptured: {imported.CapturedUtc:dd MMM yyyy HH:mm}\n{imported.ReviewReason}",
                Humanize(imported.ReviewState).ToUpperInvariant()));
        }

        foreach (var duplicate in _proof.DuplicateCandidates)
        {
            panel.Children.Add(Card(
                "Duplicate opportunity candidate",
                $"Fictional Engineering Ltd â€¢ Software Application Developer\nSignals: {string.Join(", ", duplicate.Signals.Select(Humanize))}\nConfidence: {duplicate.Confidence:P0}\nNo automatic merge, apply, contact or rejection.",
                Humanize(duplicate.ReviewState).ToUpperInvariant()));
        }

        return Scroll(panel);
    }

    private UIElement ApplicationChecklist()
    {
        var application = _applications[0];
        var requiredComplete = application.Checklist.Where(x => x.IsRequired).All(x => x.IsComplete);
        var readiness = requiredComplete ? "Ready for user review" : "Preparation incomplete";

        var panel = Vertical();
        panel.Children.Add(Card(
            "Explicit readiness state",
            $"Preparation status: {readiness}\nApplication status: {Humanize(application.State)}\nRequired items must be completed explicitly.\nLifeOS does not submit applications.",
            requiredComplete ? "READY FOR REVIEW" : "NOT READY"));

        foreach (var item in application.Checklist)
        {
            panel.Children.Add(Card(
                item.Label,
                $"{Humanize(item.Type)} â€¢ Required: {(item.IsRequired ? "Yes" : "No")}\nEvidence: {FriendlyEvidence(item.EvidenceLinkId)}\nCompletion can be reopened and remains auditable.",
                item.IsComplete ? "COMPLETE" : "OPEN"));
        }

        return Scroll(panel);
    }

    private UIElement ApplicationTimeline()
    {
        var application = _applications[0];
        var panel = Vertical();

        panel.Children.Add(Card(
            "Application status",
            $"State: {Humanize(application.State)}\nSubmitted: {application.SubmittedUtc:dd MMM yyyy HH:mm}\nChannel: {application.SubmissionChannel}\nConfirmation: Redacted proof reference",
            "EXTERNAL SUBMISSION RECORDED"));

        foreach (var item in application.Timeline.OrderByDescending(x => x.OccurredUtc))
        {
            panel.Children.Add(Card(
                Humanize(item.ToState),
                $"{item.OccurredUtc:dd MMM yyyy HH:mm}\n{item.SafeSummary}",
                "AUDIT EVENT"));
        }

        foreach (var follow in application.FollowUps)
        {
            panel.Children.Add(Card(
                "Follow-up",
                $"{follow.Description}\nDue: {follow.DueUtc:dd MMM yyyy HH:mm}\nNo message is sent automatically.",
                Humanize(follow.State).ToUpperInvariant()));
        }

        foreach (var interview in application.Interviews)
        {
            panel.Children.Add(Card(
                "Interview context",
                $"{interview.StartsUtc:dd MMM yyyy HH:mm} â€¢ {Humanize(interview.Format)}\nAttendees: Fictional recruiter and hiring panel\nPreparation complete: {(interview.PreparationComplete ? "Yes" : "No")}\nCalendar context: Linked read-only event",
                "READ-ONLY CONTEXT"));
        }

        return Scroll(panel);
    }


    private UIElement CareerProfile()
    {
        var panel = Vertical();
        panel.Children.Add(Card("Evidence-backed profile", $"{_materials.Facts.Count} factual records â€¢ {_materials.Skills.Count} skill-evidence links\nFacts remain authoritative; wording remains a derivative.", "TRUSTED MATERIALS"));
        foreach (var fact in _materials.Facts)
            panel.Children.Add(Card(fact.Category, $"{fact.FactualValue}\nSource: {fact.SourceId}\nEvidence: {(fact.Evidence.Count == 0 ? "None linked" : string.Join(", ", fact.Evidence))}\nOwner review: {Humanize(fact.OwnerReviewState)}", Humanize(fact.TrustState).ToUpperInvariant()));
        return Scroll(panel);
    }

    private UIElement CvVariants()
    {
        var panel = Vertical();
        var variant = _materials.Variants[0];
        panel.Children.Add(Card(variant.Name, $"Focus: {variant.Focus}\nVersion history: {variant.Versions.Count}\nExport: {_materials.Export.Format} derivative v{_materials.Export.Version}\nSource facts remain immutable.", _materials.Review.CanExport ? "EXPORT CHECKED" : "REVIEW REQUIRED"));
        foreach (var match in _materials.Review.Matches)
            panel.Children.Add(Card(match.Requirement, $"Matched facts: {(match.MatchedFactIds.Count == 0 ? "None" : string.Join(", ", match.MatchedFactIds))}\nEvidence: {(match.EvidenceIds.Count == 0 ? "Missing" : string.Join(", ", match.EvidenceIds))}", match.IsSupported ? "SUPPORTED" : match.IsRequired ? "BLOCKED" : "UNSUPPORTED WARNING"));
        foreach (var bullet in variant.Bullets)
            panel.Children.Add(Card("Reusable evidence-backed bullet", bullet.Text + $"\nFact: {bullet.CareerFactId}", Humanize(bullet.TrustState).ToUpperInvariant()));
        return Scroll(panel);
    }

    private UIElement PortfolioEvidence()
    {
        var panel = Vertical();
        foreach (var item in _materials.Portfolio)
            panel.Children.Add(Card(item.Title, $"{item.Summary}\nRole: {item.Role}\nTechnologies: {string.Join(", ", item.Technologies)}\nDocuments: {string.Join(", ", item.DocumentIds)}\nOutcomes: {string.Join("; ", item.Outcomes)}", Humanize(item.PrivacyState).ToUpperInvariant()));
        return Scroll(panel);
    }


    private UIElement CoverLetter()
    {
        var panel = Vertical();
        panel.Children.Add(Card("Requirement-to-evidence mapping", $"Opportunity: {_preparation.CoverLetter.OpportunityId}\nCV variant: {_preparation.CoverLetter.CVVariantId}\nVersion: {_preparation.CoverLetter.Version}\nNo section is promoted to fact automatically.", "REVIEWABLE DRAFT"));
        foreach (var section in _preparation.CoverLetter.Sections)
            panel.Children.Add(Card(section.Heading, $"{section.Text}\nSource facts: {(section.SourceFactIds.Count == 0 ? "Manual wording" : string.Join(", ", section.SourceFactIds))}", Humanize(section.State).ToUpperInvariant()));
        return Scroll(panel);
    }

    private UIElement ApplicationPackView()
    {
        var panel = Vertical();
        panel.Children.Add(Card("Pack readiness", $"Opportunity owner: {_preparation.Pack.OpportunityId}\nVersion: {_preparation.Pack.Version}\nRequired materials must be present and current.", _preparation.Pack.IsReady ? "READY FOR USER REVIEW" : "BLOCKED"));
        foreach (var item in _preparation.Pack.Items)
            panel.Children.Add(Card(item.Label, $"Type: {Humanize(item.Type)}\nRequired: {(item.Required ? "Yes" : "No")}\nMaterial: {item.MaterialId ?? "Missing"}", Humanize(item.Freshness).ToUpperInvariant()));
        return Scroll(panel);
    }

    private UIElement InterviewPreparation()
    {
        var plan = _preparation.Interview;
        var panel = Vertical();
        panel.Children.Add(Card("Interview logistics", $"{plan.StartsUtc:dd MMM yyyy HH:mm} â€¢ {plan.Format}\n{plan.Logistics}\nCalendar mutation: disabled", "READ-ONLY CONTEXT"));
        foreach (var star in plan.StarExamples)
            panel.Children.Add(Card($"STAR â€¢ {star.Title}", $"Situation: {star.Situation}\nTask: {star.Task}\nAction: {star.Action}\nResult: {star.Result}\nEvidence: {string.Join(", ", star.EvidenceIds)}", "USER-AUTHORED"));
        foreach (var q in plan.Questions)
            panel.Children.Add(Card("Likely question", q.Prompt + $"\nSource: {q.Source}", "PREPARATION"));
        foreach (var check in plan.Checks)
            panel.Children.Add(Card(check.Label, "Offline-safe checklist action through the existing queue/conflict boundary.", Humanize(check.State).ToUpperInvariant()));
        return Scroll(panel);
    }


    private UIElement FollowUps()
    {
        var panel = Vertical();
        panel.Children.Add(Card("Follow-up queue", "No message is sent automatically. Draft wording remains optional and reviewable.", "NO-SEND BOUNDARY"));
        foreach (var item in _closure.FollowUps.OrderBy(x => x.DueUtc))
            panel.Children.Add(Card(item.Title, $"{Humanize(item.Type)} â€¢ Due {item.DueUtc:dd MMM yyyy HH:mm}\nOwner: {item.Owner}\nRelated: {item.RelatedRecordId}\nChannel: {item.Channel}\nDraft: {item.DraftNote}\nOutcome: {item.Outcome ?? "Not recorded"}\nHistory: {string.Join("; ", item.History.Select(h => h.Action))}", Humanize(item.Status).ToUpperInvariant()));
        return Scroll(panel);
    }

    private UIElement References()
    {
        var panel = Vertical();
        panel.Children.Add(Card("Application pack reference review", "References require explicit permission before inclusion. Private details stay redacted.", "CONSENT-AWARE"));
        foreach (var reference in _closure.References)
            panel.Children.Add(Card(reference.DisplayName, $"Relationship: {reference.Relationship}\nRelevant work: {reference.RelevantWork}\nPermission: {Humanize(reference.Permission.State)}\nPreferred contact: {reference.PreferredContactMethod}\nAvailability: {reference.Availability}\nPrivacy: {Humanize(reference.PrivacyState)}\nUsage: {string.Join(", ", reference.UsageHistory.Select(x => x.ApplicationPackId))}\nWarnings: {(reference.ReadinessWarnings.Count == 0 ? "None" : string.Join("; ", reference.ReadinessWarnings))}", reference.CanUseInApplicationPack ? "READY" : "REVIEW REQUIRED"));
        return Scroll(panel);
    }

    private UIElement QuestionsToAsk()
    {
        var panel = Vertical();
        var plan = _closure.QuestionsToAsk;
        panel.Children.Add(Card("Linked interview", $"{plan.OpportunityTitle}\nInterview: {plan.InterviewUtc:dd MMM yyyy HH:mm}\nNotes: {plan.Notes}", "USER-OWNED"));
        foreach (var question in plan.Questions.OrderBy(x => x.Order))
            panel.Children.Add(Card($"{question.Order}. {question.Prompt}", $"Category: {question.Category}\nNotes: {question.Notes}\nLinked interview: {question.InterviewId}", question.Answered ? "ANSWERED" : "TO ASK"));
        return Scroll(panel);
    }


    private UIElement CareerReview()
    {
        var panel = Vertical();
        var review = _closure.Review;
        panel.Children.Add(Card("Review period", $"{review.Period.StartUtc:dd MMM yyyy} to {review.Period.EndUtc:dd MMM yyyy}\nData state: {review.DataState}\nNo suitability or hiring probability score is produced.", "BOUNDED RECORDS"));
        panel.Children.Add(Card("Pipeline summary", $"Discovered: {review.Pipeline.OpportunitiesDiscovered}\nPrepared: {review.Pipeline.ApplicationsPrepared}\nSubmitted: {review.Pipeline.ApplicationsSubmitted}\nInterviews: {review.Pipeline.Interviews}\nOffers: {review.Pipeline.Offers}\nClosed outcomes: {review.Pipeline.ClosedOutcomes}", "COUNTS ONLY"));
        panel.Children.Add(Card("Breakdowns", $"Sources: {string.Join(", ", review.SourceBreakdown.Select(x => $"{x.Key} {x.Value}"))}\nRole families: {string.Join(", ", review.RoleFamilyBreakdown.Select(x => $"{x.Key} {x.Value}"))}\nWork modes: {string.Join(", ", review.WorkModeBreakdown.Select(x => $"{x.Key} {x.Value}"))}\nStages: {string.Join(", ", review.StageBreakdown.Select(x => $"{x.Key} {x.Value}"))}", "DRILLABLE"));
        panel.Children.Add(Card("Follow-up responsiveness", $"Completed on time: {review.FollowUps.CompletedOnTime}\nOverdue: {review.FollowUps.Overdue}\nWaiting: {review.FollowUps.Waiting}\nAverage response: {review.FollowUps.AverageResponseHours:0.#} hours", "LIFEOS RECORDS"));
        return Scroll(panel);
    }

    private UIElement CoverageReport()
    {
        var panel = Vertical();
        var coverage = _closure.Review.Coverage;
        panel.Children.Add(Card("Evidence coverage", $"Trusted CV facts: {coverage.TrustedFacts}/{coverage.TotalFacts}\nPortfolio proof: {coverage.PortfolioWithEvidence}/{coverage.TotalPortfolio}\nReferences ready: {coverage.ReadyReferences}/{coverage.TotalReferences}\nApplication packs current: {coverage.ReadyPacks}/{coverage.TotalPacks}", "PRIVACY-CONTROLLED"));
        foreach (var item in coverage.DrillDown)
            panel.Children.Add(Card(item.Label, $"Status: {item.Status}\nAuthoritative record: {item.RecordId}\nSafe detail: {item.SafeDetail}", "DRILL-DOWN"));
        return Scroll(panel);
    }

    private static Border BoundaryCard()
    {
        var panel = new StackPanel();
        panel.Children.Add(Label("Review-first boundary", 16, "#FFFFFF", FontWeights.SemiBold));
        panel.Children.Add(Label(
            "No autonomous applications\nNo recruiter messaging\nNo invented qualifications\nImported leads remain review-first",
            12,
            "#A9B8D5",
            FontWeights.Normal,
            new Thickness(0, 8, 0, 0)));

        return new Border
        {
            Background = Brush("#17213A"),
            BorderBrush = Brush("#31405F"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(12),
            Margin = new Thickness(0, 18, 0, 0),
            Child = panel
        };
    }

    private static string Humanize(object value)
    {
        var text = value.ToString() ?? string.Empty;
        return Regex.Replace(text, "(?<=[a-z0-9])([A-Z])", " $1");
    }

    private static string FriendlyEvidence(string? evidenceId) =>
        evidenceId switch
        {
            null or "" => "Not linked",
            "doc-redacted-proof" => "Preserved proof document",
            "doc-cv-redacted" => "Role-specific CV",
            "portfolio-proof-01" => "Portfolio evidence",
            _ => "Linked preserved evidence"
        };

    private static StackPanel Vertical(params UIElement[] children)
    {
        var panel = new StackPanel();
        foreach (var child in children)
            panel.Children.Add(child);
        return panel;
    }

    private static ScrollViewer Scroll(UIElement child) => new()
    {
        Content = child,
        VerticalScrollBarVisibility = ScrollBarVisibility.Auto
    };

    private static ComboBox Filter(string text) => new()
    {
        Width = 170,
        Margin = new Thickness(0, 0, 10, 8),
        Padding = new Thickness(8),
        ItemsSource = new[] { text },
        SelectedIndex = 0
    };

    private static TextBlock Label(string text, double size, string color, FontWeight weight, Thickness? margin = null) => new()
    {
        Text = text,
        FontSize = size,
        Foreground = Brush(color),
        FontWeight = weight,
        Margin = margin ?? new Thickness(0),
        TextWrapping = TextWrapping.Wrap
    };

    private static Border Card(string title, string body, string badge)
    {
        var panel = new StackPanel();
        var top = new Grid();
        top.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        top.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        top.Children.Add(Label(title, 18, "#FFFFFF", FontWeights.SemiBold));

        var tag = new Border
        {
            Background = Brush("#233558"),
            CornerRadius = new CornerRadius(9),
            Padding = new Thickness(10, 5, 10, 5),
            Margin = new Thickness(12, 0, 0, 0),
            Child = Label(badge, 11, "#D7E3FF", FontWeights.Bold)
        };
        Grid.SetColumn(tag, 1);
        top.Children.Add(tag);

        panel.Children.Add(top);
        panel.Children.Add(Label(body, 13, "#B8C5DD", FontWeights.Normal, new Thickness(0, 8, 0, 0)));

        return new Border
        {
            Background = Brush("#151F35"),
            BorderBrush = Brush("#2D3A56"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(13),
            Padding = new Thickness(18),
            Margin = new Thickness(0, 0, 0, 12),
            Child = panel
        };
    }

    private static Brush Brush(string value) =>
        new SolidColorBrush((Color)ColorConverter.ConvertFromString(value));
}
