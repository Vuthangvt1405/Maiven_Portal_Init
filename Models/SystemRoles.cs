namespace Maiven_Portal_Managment.Models;

public static class SystemRoles
{
    public static class Student
    {
        public const long Id = 1;
        public const string Code = "STUDENT";
        public const string Name = "Student";
        public const string Description = "Student self-registration role.";
    }

    public static class Teacher
    {
        public const long Id = 2;
        public const string Code = "TEACHER";
        public const string Name = "Teacher";
        public const string Description = "Teacher role.";
    }

    public static class Admin
    {
        public const long Id = 3;
        public const string Code = "ADMIN";
        public const string Name = "Admin";
        public const string Description = "System administrator role.";
    }
}
