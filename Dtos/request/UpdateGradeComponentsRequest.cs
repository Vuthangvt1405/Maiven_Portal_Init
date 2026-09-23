namespace Maiven_Portal_Managment.Dtos.request;

public sealed record UpdateGradeComponentsRequest(
    IReadOnlyList<GradeComponentWeightRequest> Components);

public sealed record GradeComponentWeightRequest(
    long GradeComponentId,
    decimal Weight);