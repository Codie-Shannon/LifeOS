namespace LifeOS.Core.RelationshipRadar;

public enum RelationshipRadarStatus
{
    Active = 0,
    WaitingOnThem = 10,
    WaitingOnMe = 20,
    FollowUpDue = 30,
    DoNotChaseYet = 40,
    Parked = 50,
    Closed = 999
}
