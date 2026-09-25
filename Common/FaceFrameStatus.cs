namespace Maiven_Portal_Managment.Common;

public enum FaceFrameStatus
{
    Success,
    NoFaceDetected,
    MultipleFacesDetected,
    FaceTooSmall,
    TooDark,
    TooBright,
    TooBlurry,
    FaceTooTilted,
    AlignmentFailed,
    EmbeddingCreationFailed,
    InvalidImage,
    NoCompatibleCredential,
    BelowMatchThreshold,
    InsufficientMargin,
    DifferentIdentity,
    InsufficientConsensus,
    MatchedStudentUnavailable
}
