using System.Drawing;
using System.Windows.Forms;

namespace LifeOS.TimerAgent.Services;

public sealed class TrayIconService : IDisposable
{
    private readonly NotifyIcon _notifyIcon;

    public event EventHandler? ShowRequested;
    public event EventHandler? HideRequested;
    public event EventHandler? ExitRequested;

    public TrayIconService()
    {
        var menu = new ContextMenuStrip();

        var showItem = new ToolStripMenuItem("Show Timer");
        showItem.Click += (_, _) => ShowRequested?.Invoke(this, EventArgs.Empty);

        var hideItem = new ToolStripMenuItem("Hide Timer");
        hideItem.Click += (_, _) => HideRequested?.Invoke(this, EventArgs.Empty);

        var exitItem = new ToolStripMenuItem("Exit TimerAgent");
        exitItem.Click += (_, _) => ExitRequested?.Invoke(this, EventArgs.Empty);

        menu.Items.Add(showItem);
        menu.Items.Add(hideItem);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(exitItem);

        _notifyIcon = new NotifyIcon
        {
            Text = "Life OS TimerAgent",
            Icon = SystemIcons.Application,
            ContextMenuStrip = menu,
            Visible = true
        };

        _notifyIcon.DoubleClick += (_, _) => ShowRequested?.Invoke(this, EventArgs.Empty);
    }

    public void UpdateTooltip(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            _notifyIcon.Text = "Life OS TimerAgent";
            return;
        }

        _notifyIcon.Text = text.Length > 63
            ? text[..63]
            : text;
    }

    public void Dispose()
    {
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
    }
}