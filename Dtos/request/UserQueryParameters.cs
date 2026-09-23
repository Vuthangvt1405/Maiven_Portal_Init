namespace Maiven_Portal_Managment.Dtos.request
{
public sealed class UserQueryParameters
{
    public string? Name { get; set; }

    public string? Email { get; set; }

    public int? Status { get; set; }

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 10;
}}