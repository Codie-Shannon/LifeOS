namespace LifeOS.Modules.Timer.Models;

public enum TimedTaskMode
{
    Exclusive = 0,
    Parallel = 1,
    Linked = 2,
    Countdown = 3,
    Waiting = 4
}