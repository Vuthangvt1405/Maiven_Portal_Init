using Maiven_Portal_Managment.Models.Enums;

namespace Maiven_Portal_Managment.Dtos.request;

public sealed class UserQueryParameters
{
    public string? Name { get; set; }

    public string? Email {get; set; }

    public UserRoleFilter? Role { get; set; }

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 10;
}
