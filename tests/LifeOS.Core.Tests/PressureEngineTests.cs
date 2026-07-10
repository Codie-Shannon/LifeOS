using LifeOS.Core.CommandCentrePressure;
using Xunit;

namespace LifeOS.Core.Tests;

public sealed class PressureEngineTests
{
    [Fact]
    public void SuppressWaitingOnOthers()
    {
        var summary = CommandCentrePressureCalculator.Calculate(
        [
            new PressureSignal
            {
                Key = "waiting-client",
                Module = "Work Pipeline",
                Title = "Waiting on client",
                Severity = PressureSeverity.High,
                Lane = PressureLane.Waiting,
                IsWaitingOnOthers = true,
                IsDueNow = false,
                NextAction = "Do not chase yet."
            }
        ],
        DefaultPolicy());

        Assert.Empty(summary.TopSignals);
        Assert.Equal(1, summary.SuppressedSignals);
        Assert.Equal(1, summary.ProtectedSignals);
    }

    [Fact]
    public void UntrustedSignalsRequireReview()
    {
        var summary = CommandCentrePressureCalculator.Calculate(
        [
            new PressureSignal
            {
                Key = "ocr-receipt",
                Module = "Receipt Evidence",
                Title = "OCR candidate",
                Severity = PressureSeverity.High,
                Lane = PressureLane.ActNow,
                IsTrusted = false,
                NextAction = "Review source evidence."
            }
        ],
        DefaultPolicy());

        Assert.Equal(1, summary.ReviewSignals);
        Assert.Equal(0, summary.ActNowSignals);
        Assert.Equal("Review source evidence.", summary.NextSafestAction);
    }

    [Fact]
    public void ActNowWinsNextSafestAction()
    {
        var summary = CommandCentrePressureCalculator.Calculate(
        [
            new PressureSignal
            {
                Key = "review",
                Module = "Integration Inbox",
                Title = "Preview needs review",
                Severity = PressureSeverity.High,
                Lane = PressureLane.Review,
                NextAction = "Review import."
            },
            new PressureSignal
            {
                Key = "act",
                Module = "Money",
                Title = "Bill due today",
                Severity = PressureSeverity.Critical,
                Lane = PressureLane.ActNow,
                IsDueNow = true,
                NextAction = "Pay or defer the bill deliberately."
            }
        ],
        DefaultPolicy());

        Assert.Equal("Critical", summary.PressureLabel);
        Assert.Equal("Pay or defer the bill deliberately.", summary.NextSafestAction);
    }

    [Fact]
    public void TopSignalsRespectPolicyLimit()
    {
        var policy = DefaultPolicy();
        policy.MaximumTopSignals = 2;

        var summary = CommandCentrePressureCalculator.Calculate(
        [
            Signal("a", PressureSeverity.Normal),
            Signal("b", PressureSeverity.Critical),
            Signal("c", PressureSeverity.High)
        ],
        policy);

        Assert.Equal(2, summary.TopSignals.Count);
        Assert.Equal("b", summary.TopSignals[0].Key);
        Assert.Equal("c", summary.TopSignals[1].Key);
    }

    private static PressureSignal Signal(string key, PressureSeverity severity)
    {
        return new PressureSignal
        {
            Key = key,
            Module = "Test",
            Title = key,
            Severity = severity,
            Lane = PressureLane.ActNow,
            NextAction = $"Handle {key}."
        };
    }

    private static CommandCentrePressurePolicy DefaultPolicy()
    {
        return new CommandCentrePressurePolicy
        {
            LowWeight = 1,
            NormalWeight = 3,
            HighWeight = 7,
            CriticalWeight = 12,
            NormalScore = 5,
            HighScore = 16,
            CriticalScore = 30,
            MaximumTopSignals = 8,
            SuppressWaitingOnOthers = true,
            RequireReviewForUntrusted = true
        };
    }
}
