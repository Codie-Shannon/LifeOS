using LifeOS.Core.WorkTime;
using Xunit;

namespace LifeOS.Core.Tests;

public sealed class Group67And68WorkTimeTests
{
    private readonly WorkTimeService _service = new();
    private readonly DateTimeOffset _start = new(2026, 7, 27, 9, 0, 0, TimeSpan.FromHours(12));

    [Fact]
    public void Timer_pause_and_resume_exclude_paused_time()
    {
        WorkTimeEntry running = _service.Start("Harbour Office", "Website", "Release preparation", true, 85m, _start);
        WorkTimeEntry paused = _service.Pause(running, _start.AddHours(1));
        WorkTimeEntry resumed = _service.Resume(paused, _start.AddHours(1.5));
        WorkTimeEntry completed = _service.Stop(resumed, _start.AddHours(2));

        Assert.Equal(TimeSpan.FromHours(1.5), completed.Accumulated);
        Assert.Equal(WorkTimerState.Completed, completed.State);
        Assert.Equal(4, completed.Evidence.Count);
    }

    [Fact]
    public void Manual_entry_requires_a_reason_and_positive_duration()
    {
        Assert.Throws<ArgumentException>(() => _service.AddManual(
            "Harbour Office",
            "Website",
            "Correction",
            _start,
            _start.AddHours(1),
            false,
            0m,
            "",
            _start.AddHours(2)));

        Assert.Throws<ArgumentException>(() => _service.AddManual(
            "Harbour Office",
            "Website",
            "Correction",
            _start,
            _start,
            false,
            0m,
            "Forgot to start the timer.",
            _start.AddHours(2)));
    }

    [Fact]
    public void Daily_summary_separates_billable_and_non_billable_time()
    {
        WorkTimeEntry billable = _service.AddManual(
            "Harbour Office", "Website", "Build", _start, _start.AddHours(2), true, 85m, "Imported from notes.", _start.AddHours(3));
        WorkTimeEntry admin = _service.AddManual(
            "", "LifeOS", "Administration", _start.AddHours(3), _start.AddHours(3.5), false, 0m, "Local admin record.", _start.AddHours(4));

        WorkDaySummary summary = _service.Summarize(new[] { billable, admin }, new DateOnly(2026, 7, 27), _start.AddHours(5));

        Assert.Equal(TimeSpan.FromHours(2.5), summary.Total);
        Assert.Equal(TimeSpan.FromHours(2), summary.Billable);
        Assert.Equal(TimeSpan.FromMinutes(30), summary.NonBillable);
        Assert.Equal(170m, summary.BillableValue);
    }

    [Fact]
    public void Timesheet_export_preserves_classification_and_evidence_count()
    {
        WorkTimeEntry entry = _service.AddManual(
            "Harbour Office", "Website", "Build", _start, _start.AddHours(2), true, 85m, "Imported from notes.", _start.AddHours(3));
        entry = _service.AttachEvidence(entry, "document", "brief-42", "Approved brief.", _start.AddHours(3));

        string csv = _service.ExportTimesheet(new[] { entry }, _start.AddHours(4));

        Assert.Contains("\"Harbour Office\"", csv);
        Assert.Contains(",True,85.00,170.00,2", csv);
    }
}
