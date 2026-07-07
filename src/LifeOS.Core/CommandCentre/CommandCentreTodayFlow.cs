namespace LifeOS.Core.CommandCentre;

public static class CommandCentreTodayFlow
{
    public static IReadOnlyList<CommandCentreTodayAction> Build(IEnumerable<CommandCentreSignal> todaySignals)
    {
        ArgumentNullException.ThrowIfNull(todaySignals);

        return todaySignals
            .Where(signal => signal.RequiresAction || signal.Priority is CommandCentreSignalPriority.Critical or CommandCentreSignalPriority.High)
            .OrderBy(signal => signal.SortWeight)
            .ThenByDescending(signal => signal.Priority)
            .Take(7)
            .Select(signal => new CommandCentreTodayAction
            {
                Title = signal.Title,
                Reason = signal.Detail,
                Action = string.IsNullOrWhiteSpace(signal.NextAction) ? "Review and decide the next safe action." : signal.NextAction,
                Priority = signal.Priority,
                Source = signal.Source
            })
            .ToList();
    }
}
