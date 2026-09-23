namespace Maiven_Portal_Managment.Dtos.response;

public sealed record GradeComponentResponse(
    long Id,
    long SectionId,
    string Name,
    decimal Weight);