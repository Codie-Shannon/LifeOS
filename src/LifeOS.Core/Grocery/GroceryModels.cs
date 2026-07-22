namespace LifeOS.Core.Grocery;

public enum GroceryCategory { Produce, Bakery, Dairy, Meat, Frozen, Pantry, Drinks, Household, Cleaning, PersonalCare, Custom }
public enum GroceryListState { Draft, Ready, Shopping, Paused, Completed, Cancelled, Archived }
public enum GroceryPriority { Low, Normal, High, Essential }
public enum EssentialReviewState { Due, DueSoon, Deferred, SkippedOnce, Accepted, Current }
public enum ShoppingItemState { Pending, Checked, Unavailable, Substituted, Skipped }
public enum GroceryActionKind { Check, Undo, AdjustQuantity, MarkUnavailable, Substitute, Skip, AddNote, StartList, PauseList, ResumeList, CompleteList }

public sealed record GroceryItem(
    string Id,
    string Name,
    GroceryCategory Category,
    string? PreferredBrand,
    IReadOnlyList<string> AcceptableAlternatives,
    string? Notes);

public sealed record QuantityRequirement(decimal Quantity, string Unit, string? PackSize);

public sealed record GroceryListItem(
    string Id,
    string GroceryItemId,
    string RequestedName,
    QuantityRequirement Quantity,
    GroceryPriority Priority,
    DateOnly? RequiredBy,
    ShoppingItemState State,
    string? PurchasedName,
    string? Note,
    bool Required);

public sealed record GroceryList(
    string Id,
    string Name,
    GroceryListState State,
    string Currency,
    decimal EstimatedTotal,
    DateTimeOffset EstimatedAt,
    string EstimateSource,
    IReadOnlyList<GroceryListItem> Items);

public sealed record RecurringEssential(
    string Id,
    string GroceryItemId,
    int CadenceDays,
    DateOnly NextDue,
    QuantityRequirement UsualQuantity,
    string? PreferredStore,
    EssentialReviewState ReviewState);

public sealed record GroceryTemplate(string Id, string Name, IReadOnlyList<GroceryListItem> Items);
public sealed record StorePreference(string StoreName, string? SectionOrder);
public sealed record HouseholdMemberAssignment(string GroceryListId, string MemberName);
public sealed record GroceryHistory(DateTimeOffset At, string Action, string EntityId, string Detail);
public sealed record DuplicateGroceryCandidate(string ExistingItemId, string CandidateItemId, string Reason, bool RequiresReview);
public sealed record GroceryDashboard(
    GroceryList? ActiveList,
    IReadOnlyList<RecurringEssential> DueEssentials,
    IReadOnlyList<GroceryList> RecentlyCompleted,
    int UnresolvedRequired,
    string Boundary);
public sealed record OfflineGroceryAction(
    string Id,
    GroceryActionKind Kind,
    string ListId,
    string? ItemId,
    string? Value,
    DateTimeOffset QueuedAt,
    bool RequiresConflictReview);
public sealed record GroceryConflict(
    string ListId,
    string ItemId,
    string DesktopValue,
    string MobileValue,
    string ResolutionState);
