namespace LifeOS.Core.Documents;
public enum DocumentType { Receipt, Invoice, Statement, Contract, Quote, Timesheet, ProofOfPayment, GeneralEvidence }
public enum DocumentIntakeState { Draft, ReviewRequired, Accepted, Rejected, Deferred, Cancelled, MissingOriginal, CorruptFile, UnsupportedType, ExtractionFailed }
public enum DocumentReviewAction { Accept, Correct, Reject, Defer, Link }
public enum DocumentLinkArea { Money, Work, Project, Career, Life, Person }
