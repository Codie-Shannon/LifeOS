using LifeOS.Core.Forms;

namespace LifeOS.Shared.V8;

public sealed record V8SettingsFormInput(string? ProfileName, string? ActiveContext);

public static class V8SettingsFormValidator
{
    public const int MaximumDisplayTextLength = 80;

    public static FormValidationResult Validate(V8SettingsFormInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        return FormValidation.Combine(
            FormValidation.Required("profile-name", input.ProfileName, "Profile name"),
            FormValidation.MaximumLength("profile-name", input.ProfileName, "Profile name", MaximumDisplayTextLength),
            FormValidation.SingleLine("profile-name", input.ProfileName, "Profile name"),
            FormValidation.Required("active-context", input.ActiveContext, "Active context"),
            FormValidation.MaximumLength("active-context", input.ActiveContext, "Active context", MaximumDisplayTextLength),
            FormValidation.SingleLine("active-context", input.ActiveContext, "Active context"));
    }
}
