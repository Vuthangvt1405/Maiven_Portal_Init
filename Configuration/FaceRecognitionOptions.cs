using System.ComponentModel.DataAnnotations;

namespace Maiven_Portal_Managment.Configuration;

public sealed class FaceRecognitionOptions : IValidatableObject
{
    public const string SectionName = "FaceRecognition";

    [Required]
    public string YuNetModelPath { get; set; } = string.Empty;

    [Required]
    public string SFaceModelPath { get; set; } = string.Empty;

    [Required]
    public string ModelName { get; set; } = string.Empty;

    [Required]
    public string ModelVersion { get; set; } = string.Empty;

    [Range(1, 20 * 1024 * 1024)]
    public long MaxImageBytes { get; set; }

    [Range(0.01, 1.0)]
    public float DetectionScoreThreshold { get; set; }

    [Range(0.01, 1.0)]
    public float NmsThreshold { get; set; }

    [Range(1, 5000)]
    public int DetectionTopK { get; set; }

    [Range(0.01, 1.0)]
    public double MinimumFaceWidthRatio { get; set; }

    [Range(0.0, 255.0)]
    public double MinimumBrightness { get; set; }

    [Range(0.0, 255.0)]
    public double MaximumBrightness { get; set; }

    [Range(0.0, double.MaxValue)]
    public double MinimumBlurVariance { get; set; }

    [Range(0.0, 90.0)]
    public double MaximumRollDegrees { get; set; }

    [Range(0.0, 1.0)]
    public double MatchThreshold { get; set; }

    [Range(0.0, 1.0)]
    public double MinMargin { get; set; }

    [Range(0.0, 1.0)]
    public double DuplicateThreshold { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (MinimumBrightness >= MaximumBrightness)
        {
            yield return new ValidationResult(
                "MinimumBrightness must be lower than MaximumBrightness.",
                [nameof(MinimumBrightness), nameof(MaximumBrightness)]);
        }
    }
}
