using System.ComponentModel;
using System.Reflection;

namespace Maiven_Portal_Managment.Data.Entities.Enums;

public enum DefaultGradeComponent
{
    [Description("Attendance")]
    Attendance = 1,

    [Description("Midterm")]
    MidtermExam = 2,

    [Description("Lab")]
    Assignment = 3,

    [Description("Final")]
    FinalExam = 4
}

public static class DefaultGradeComponentExtensions
{
    public static string GetDescription(this Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        var attribute = field?.GetCustomAttribute<DescriptionAttribute>();
        return attribute?.Description ?? value.ToString();
    }
}
