using LifeOS.Core.PaidWorkMoneyProof;
using LifeOS.Core.ProofTracker;
using LifeOS.Core.WorkPipeline;
using LifeOS.Shared.Money;
using LifeOS.Shared.ProofTracker;
using LifeOS.Shared.WorkPipeline;
using LifeOS.Shared.WorkSessions;

namespace LifeOS.Shared.PaidWorkMoneyProof;

public static class PaidWorkMoneyProofSnapshotService
{
    public static PaidWorkMoneyProofSummary Create()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var money = MoneyPressureStorage.Load().Calculate();
        var workSessions = WorkSessionStorage.Load();
        var proofItems = ProofStorage.Load();
        var pipeline = WorkPipelineCalculator.Calculate(WorkPipelineStorage.Load(), today);

        return PaidWorkMoneyProofCalculator.Calculate(workSessions, proofItems, money, pipeline, today);
    }
}
