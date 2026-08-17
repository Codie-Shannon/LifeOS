using LifeOS.Mobile.Core.Foundation;
using LifeOS.Mobile.Core.Services;
using LifeOS.Mobile.Views;

namespace LifeOS.Mobile;

public sealed class MobileBootstrapper
{
    private readonly MobileFoundationService _foundation;
    public MobileBootstrapper(MobileFoundationService foundation) => _foundation = foundation;
    public async Task<Page> CreatePageAsync()
    {
        try
        {
            MobilePreferences preferences = await _foundation.InitializeAsync();
            return new MobileShell(_foundation, preferences.ExperienceMode);
        }
        catch (Exception ex) { return new RecoveryPage($"{ex.GetType().Name}: {ex.Message}"); }
    }
}
