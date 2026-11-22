namespace BAVES.Api.Modules.Scenario.Services;

internal static class ScenarioReviewRoles
{
    public static bool IsExpert(IReadOnlyCollection<string> roles) =>
        Has(roles, "EXPERT", "BAExpert", "BAEXPERT");

    public static bool IsAdmin(IReadOnlyCollection<string> roles) =>
        Has(roles, "ADMIN", "Admin");

    public static bool IsInstructor(IReadOnlyCollection<string> roles) =>
        Has(roles, "INSTRUCTOR", "Instructor");

    public static bool IsInstructorOrAdmin(IReadOnlyCollection<string> roles) =>
        IsInstructor(roles) || IsAdmin(roles);

    public static bool IsLearner(IReadOnlyCollection<string> roles) =>
        Has(roles, "LEARNER", "Learner", "STUDENT", "Student");

    private static bool Has(IReadOnlyCollection<string> roles, params string[] names) =>
        roles.Any(role => names.Any(name => string.Equals(role, name, StringComparison.OrdinalIgnoreCase)));
}
