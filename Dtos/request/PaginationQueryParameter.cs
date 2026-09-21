namespace Maiven_Portal_Managment.Dtos.request;
public record PaginationQueryParameters(
    int PageNumber = 1,
    int PageSize = 10
);