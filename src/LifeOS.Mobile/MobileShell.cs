using LifeOS.Mobile.Core.Services;
using LifeOS.Mobile.Views;

namespace LifeOS.Mobile;

public sealed class MobileShell : Shell
{
    public MobileShell(MobileFoundationService foundation)
    {
        Title = "LifeOS Full Mobile";
        FlyoutBehavior = FlyoutBehavior.Flyout;
        var tabs = new TabBar
        {
            Items =
            {
                new ShellContent { Title = "Home", Route = "home", Content = new HomePage(foundation) },
                new ShellContent { Title = "Work", Route = "work", Content = new WorkPage(foundation) },
                new ShellContent { Title = "Career", Route = "career", Content = new CareerPage() },
                new ShellContent { Title = "Money", Route = "money", Content = new MoneyPage() },
                new ShellContent { Title = "Projects", Route = "projects", Content = new ProjectsPage() },
                new ShellContent { Title = "More", Route = "more", Content = new MorePage(foundation) }
            }
        };
        Items.Add(tabs);
    }
}
