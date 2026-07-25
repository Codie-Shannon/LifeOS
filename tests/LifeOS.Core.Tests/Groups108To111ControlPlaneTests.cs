using LifeOS.Core.ControlPlane;
using Xunit;

namespace LifeOS.Core.Tests;

public sealed class Groups108To111ControlPlaneTests
{
    private readonly ControlPlaneService _service = new();
    private readonly DateTimeOffset _now = new(2026, 7, 27, 12, 0, 0, TimeSpan.FromHours(12));

    [Fact]
    public void Sensitive_categories_require_an_explicit_timestamped_grant()
    {
        PrivacyProfile profile = Profile();

        Assert.False(_service.CanAccess(profile, SensitiveCategory.Money));

        profile = _service.SetPermission(profile, SensitiveCategory.Money, true, _now);
        Assert.True(_service.CanAccess(profile, SensitiveCategory.Money));
    }

    [Fact]
    public void Backup_round_trip_validates_integrity_and_excludes_credentials()
    {
        Dictionary<string, int> state = new() { ["items"] = 42 };
        BackupEnvelope backup = _service.Export(state, _now);

        Dictionary<string, int> restored = _service.ValidateAndRestore<Dictionary<string, int>>(backup);

        Assert.Equal(42, restored["items"]);
        Assert.False(backup.ContainsCredentials);
    }

    [Fact]
    public void Tampered_backup_is_rejected()
    {
        BackupEnvelope backup = _service.Export(new { value = 1 }, _now) with { Payload = "{\"value\":2}" };

        Assert.Throws<InvalidOperationException>(() => _service.ValidateAndRestore<object>(backup));
    }

    [Fact]
    public void Backup_export_rejects_credential_like_fields()
    {
        Assert.Throws<InvalidOperationException>(() =>
            _service.Export(new { accessToken = "fictional-secret" }, _now));
    }

    [Fact]
    public void Unsupported_backup_schema_is_rejected()
    {
        BackupEnvelope backup = _service.Export(new { value = 1 }, _now) with { SchemaVersion = 2 };

        Assert.Throws<InvalidOperationException>(() =>
            _service.ValidateAndRestore<object>(backup));
    }

    [Fact]
    public void Emergency_stop_disconnects_every_provider_and_keeps_an_audit()
    {
        ControlPlaneState stopped = _service.EmergencyStop(new[] { "google", "microsoft", "xero" }, _now);
        ControlPlaneState cleared = _service.UndoEmergencyStop(stopped, _now.AddMinutes(1));

        Assert.True(stopped.EmergencyStop);
        Assert.Equal(3, stopped.DisconnectedProviders.Count);
        Assert.False(cleared.EmergencyStop);
        Assert.Equal(3, cleared.DisconnectedProviders.Count);
        Assert.Contains("remain disconnected", cleared.Audit.Last());
    }

    [Fact]
    public void Emergency_stop_normalizes_duplicate_provider_names()
    {
        ControlPlaneState stopped = _service.EmergencyStop(
            new[] { " Google ", "google", "", "Microsoft" },
            _now);

        Assert.Equal(2, stopped.DisconnectedProviders.Count);
    }

    [Fact]
    public void Desktop_settings_exposes_the_control_plane_route()
    {
        string repositoryRoot = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
        string shell = File.ReadAllText(
            Path.Combine(repositoryRoot, "LifeOS.Desktop", "V8ShellWindow.xaml"));

        Assert.Contains("Tag=\"control-plane\"", shell, StringComparison.Ordinal);
        Assert.Contains(
            "AutomationProperties.AutomationId=\"Settings.Diagnostics.ControlPlane\"",
            shell,
            StringComparison.Ordinal);
    }

    private static PrivacyProfile Profile() =>
        new(
            Enum.GetValues<SensitiveCategory>()
                .Select(category => new CategoryPermission(category, false, null, "Settings > Privacy"))
                .ToArray(),
            false,
            90,
            true);
}
