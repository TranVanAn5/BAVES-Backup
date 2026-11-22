namespace BAVES.Api.Shared.Authorization
{
    public static class Policies
    {
        public const string AnyAuthenticated = "AnyAuthenticated";
        public const string LearnerOnly = "LearnerOnly";
        public const string InstructorOrAdmin = "InstructorOrAdmin";
        public const string ExpertOrInstructor = "ExpertOrInstructor";
    }
}
