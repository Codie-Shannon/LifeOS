using LifeOS.Mobile.Core.Foundation;
using LifeOS.Mobile.Core.Services;
using LifeOS.Mobile.Views;

namespace LifeOS.Mobile;

public sealed class MobileShell : Shell
{
    public MobileShell(
        MobileFoundationService foundation,
        MobileExperienceMode experienceMode)
    {
        Title = "LifeOS Full Mobile";
        FlyoutBehavior = FlyoutBehavior.Flyout;
        var tabs = new TabBar
        {
            Items =
            {
                new ShellContent { Title = "Home", Route = "home", Content = new HomePage(foundation, experienceMode) },
                new ShellContent { Title = "Work", Route = "work", Content = ProofPage("Work", experienceMode, () => new WorkPage(foundation)) },
                new ShellContent { Title = "Career", Route = "career", Content = ProofPage("Career", experienceMode, () => new CareerPage()) },
                new ShellContent { Title = "Grocery", Route = "grocery", Content = ProofPage("Grocery", experienceMode, () => new GroceryPage()) },
                new ShellContent { Title = "Money", Route = "money", Content = ProofPage("Money", experienceMode, () => new MoneyPage()) },
                new ShellContent { Title = "Projects", Route = "projects", Content = ProofPage("Projects", experienceMode, () => new ProjectsPage()) },
                new ShellContent { Title = "More", Route = "more", Content = new MorePage(foundation, experienceMode) }
            }
        };
        Items.Add(tabs);
    }

    private static Page ProofPage(
        string workspace,
        MobileExperienceMode experienceMode,
        Func<Page> createPortfolioDemoPage) =>
        experienceMode == MobileExperienceMode.PortfolioDemo
            ? createPortfolioDemoPage()
            : new PortfolioDemoBoundaryPage(workspace);
}
