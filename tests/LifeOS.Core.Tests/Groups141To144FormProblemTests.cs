using System.Text.Json;
using LifeOS.Core.Forms;
using LifeOS.Shared.V8;
using Xunit;

namespace LifeOS.Core.Tests;

public sealed class Groups141To144FormProblemTests
{
    [Fact]
    public void Settings_form_requires_both_display_fields()
    {
        FormValidationResult result = V8SettingsFormValidator.Validate(new(" ", null));

        Assert.False(result.IsValid);
        Assert.Equal("required", Assert.Single(result.ForField("profile-name")).Code);
        Assert.Equal("required", Assert.Single(result.ForField("active-context")).Code);
    }

    [Fact]
    public void Settings_form_rejects_overlong_or_multiline_display_text()
    {
        FormValidationResult result = V8SettingsFormValidator.Validate(new(
            new string('a', V8SettingsFormValidator.MaximumDisplayTextLength + 1),
            "Personal\nHidden"));

        Assert.Contains(result.ForField("profile-name"), issue => issue.Code == "maximum-length");
        Assert.Contains(result.ForField("active-context"), issue => issue.Code == "single-line");
    }

    [Fact]
    public void Settings_form_accepts_trimmed_bounded_display_text()
    {
        FormValidationResult result = V8SettingsFormValidator.Validate(new(
            " Codie Shannon ",
            " Personal "));

        Assert.True(result.IsValid);
        Assert.Empty(result.Issues);
    }

    [Theory]
    [InlineData(typeof(UnauthorizedAccessException), "access-denied", true)]
    [InlineData(typeof(IOException), "local-storage-unavailable", true)]
    [InlineData(typeof(JsonException), "local-data-unreadable", false)]
    [InlineData(typeof(InvalidOperationException), "unexpected-local-error", true)]
    public void Exceptions_map_to_stable_actionable_problems(
        Type exceptionType,
        string expectedCode,
        bool expectedRetry)
    {
        Exception exception = (Exception)Activator.CreateInstance(
            exceptionType,
            "C:\\private\\secret-token-value")!;

        UserFacingProblem problem = UserFacingProblemFactory.FromException(
            exception,
            "save settings");

        Assert.Equal(expectedCode, problem.Code);
        Assert.Equal(expectedRetry, problem.CanRetry);
        Assert.DoesNotContain("private", problem.Detail, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("secret-token-value", problem.Detail, StringComparison.OrdinalIgnoreCase);
        Assert.False(string.IsNullOrWhiteSpace(problem.RecoveryAction));
    }

    [Fact]
    public void Household_is_a_persistable_locked_workspace()
    {
        V8Preferences preferences = new() { LastWorkspace = "household" };

        preferences.Normalize();

        Assert.Equal("Household", preferences.LastWorkspace);
        Assert.Contains("Household", V8Preferences.WorkspaceNames);
    }
}
