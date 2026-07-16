using LifeOS.Mobile.Core.Services;
using LifeOS.Mobile.Views;

namespace LifeOS.Mobile;

public sealed class MobileBootstrapper
{
    private readonly MobileFoundationService _foundation;
    public MobileBootstrapper(MobileFoundationService foundation) => _foundation = foundation;
    public async Task<Page> CreatePageAsync()
    {
        try { await _foundation.InitializeDemoAsync(); return new MobileShell(_foundation); }
        catch (Exception ex) { return new RecoveryPage($"{ex.GetType().Name}: {ex.Message}"); }
    }
}
