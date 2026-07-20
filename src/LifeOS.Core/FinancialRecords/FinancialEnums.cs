namespace LifeOS.Core.FinancialRecords;

public enum FinancialAccountType { Cash, Bank, Card, Receivable, Payable, Manual }
public enum TransactionDirection { Income, Expense, Transfer }
public enum FinancialReviewState { Draft, PendingReview, Confirmed, Rejected }
public enum InvoiceState { Draft, Issued, Overdue, PartiallyPaid, Paid, Cancelled, Disputed }
public enum PaymentState { Expected, Received, Allocated, PartiallyAllocated, Reversed, ReviewRequired }
public enum FinancialPartyType { Person, Client, Supplier, Employer, Other }
public enum FinancialLinkType { Work, Project, Person, Evidence }
