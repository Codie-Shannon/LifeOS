using LifeOS.Core.Models;
using LifeOS.Modules.Timer.Models;

namespace LifeOS.TimerAgent.Storage;

public sealed class TimerAgentAppState
{
    public List<ContactProfile> Contacts { get; set; } = new();
    public List<TimedTask> TimedTasks { get; set; } = new();

    public Guid? SelectedContactId { get; set; }
    public Guid? SelectedTaskId { get; set; }

    public DateTimeOffset SavedAt { get; set; } = DateTimeOffset.Now;
}