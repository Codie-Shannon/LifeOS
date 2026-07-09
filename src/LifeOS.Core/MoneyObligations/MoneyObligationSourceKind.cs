namespace LifeOS.Core.MoneyObligations;

public enum MoneyObligationSourceKind
{
    Manual,
    Receipt,
    Statement,
    Email,
    Calendar,
    PayLaterProvider,
    BankStatementLater,
    AccountingExportLater,
    OcrImportLater
}
