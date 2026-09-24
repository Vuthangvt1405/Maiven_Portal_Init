using log4net;
using Maiven_Portal_Managment.Configuration;
using Maiven_Portal_Managment.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using OpenCvSharp;
using OpenCvSharp.Dnn;

namespace Maiven_Portal_Managment.Services;

public sealed class FaceRecognitionService : IDisposable
{
    private static readonly ILog Logger =
        LogManager.GetLogger(typeof(FaceRecognitionService));
    private readonly FaceRecognitionOptions options;
    private readonly string yuNetModelPath;
    private readonly string sFaceModelPath;
    private readonly SemaphoreSlim initializationGate = new(1, 1);
    private readonly SemaphoreSlim inferenceGate = new(1, 1);
    private FaceDetectorYN? faceDetector;
    private FaceRecognizerSF? faceRecognizer;
    private bool initialized;
    private bool disposed;

    public FaceRecognitionService(
        IOptions<FaceRecognitionOptions> optionsAccessor,
        IWebHostEnvironment environment)
    {
        options = optionsAccessor.Value;

        yuNetModelPath = ResolveModelPath(
            environment.ContentRootPath,
            options.YuNetModelPath);
        sFaceModelPath = ResolveModelPath(
            environment.ContentRootPath,
            options.SFaceModelPath);
    }

    public string ModelName => options.ModelName;

    public string ModelVersion => options.ModelVersion;

    public async Task<float[]?> TryCreateEmbeddingAsync(
        IFormFile imageFile,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(imageFile);

        if (imageFile.Length <= 0 || imageFile.Length > options.MaxImageBytes)
        {
            Logger.Info(
                $"Face frame rejected Reason=InvalidFileSize " +
                $"ImageBytes={imageFile.Length} MaxImageBytes={options.MaxImageBytes}");
            return null;
        }

        var imageBytes = await ReadImageBytesAsync(imageFile, cancellationToken);
        if (imageBytes is null)
        {
            Logger.Info("Face frame rejected Reason=ImageReadFailed");
            return null;
        }

        using var image = TryDecodeImage(imageBytes);
        if (image is null || image.Empty())
        {
            Logger.Info("Face frame rejected Reason=ImageDecodeFailed");
            return null;
        }

        await EnsureInitializedAsync(cancellationToken);
        await inferenceGate.WaitAsync(cancellationToken);
        try
        {
            ThrowIfDisposed();
            return TryCreateEmbedding(
                image,
                faceDetector ?? throw new InvalidOperationException(
                    "The YuNet model is not initialized."),
                faceRecognizer ?? throw new InvalidOperationException(
                    "The SFace model is not initialized."));
        }
        finally
        {
            inferenceGate.Release();
        }
    }

    public float[] CreateRepresentativeEmbedding(
        IReadOnlyCollection<float[]> embeddings)
    {
        ArgumentNullException.ThrowIfNull(embeddings);

        if (embeddings.Count == 0)
        {
            throw new BadRequestException("At least one face embedding is required.");
        }

        var dimension = embeddings.First().Length;
        if (dimension == 0 || embeddings.Any(item => item.Length != dimension))
        {
            throw new BadRequestException(
                "All face embeddings must have the same non-zero dimension.");
        }

        var sum = new double[dimension];

        foreach (var embedding in embeddings)
        {
            var normalized = Normalize(embedding);
            for (var index = 0; index < dimension; index++)
            {
                sum[index] += normalized[index];
            }
        }

        var average = new float[dimension];
        for (var index = 0; index < dimension; index++)
        {
            average[index] = (float)(sum[index] / embeddings.Count);
        }

        return Normalize(average);
    }

    public static float[] Normalize(IReadOnlyList<float> embedding)
    {
        ArgumentNullException.ThrowIfNull(embedding);

        if (embedding.Count == 0)
        {
            throw new BadRequestException("The face embedding cannot be empty.");
        }

        double squaredNorm = 0;
        for (var index = 0; index < embedding.Count; index++)
        {
            var value = embedding[index];
            if (!float.IsFinite(value))
            {
                throw new BadRequestException("The face embedding contains an invalid value.");
            }

            squaredNorm += value * value;
        }

        var norm = Math.Sqrt(squaredNorm);
        if (norm <= double.Epsilon)
        {
            throw new BadRequestException("The face embedding has no usable values.");
        }

        var normalized = new float[embedding.Count];
        for (var index = 0; index < embedding.Count; index++)
        {
            normalized[index] = (float)(embedding[index] / norm);
        }

        return normalized;
    }

    public static double CosineSimilarity(
        IReadOnlyList<float> first,
        IReadOnlyList<float> second)
    {
        ArgumentNullException.ThrowIfNull(first);
        ArgumentNullException.ThrowIfNull(second);

        if (first.Count == 0 || first.Count != second.Count)
        {
            throw new BadRequestException(
                "Face embeddings must have the same non-zero dimension.");
        }

        double dotProduct = 0;
        double firstSquaredNorm = 0;
        double secondSquaredNorm = 0;

        for (var index = 0; index < first.Count; index++)
        {
            var firstValue = first[index];
            var secondValue = second[index];

            if (!float.IsFinite(firstValue) || !float.IsFinite(secondValue))
            {
                throw new BadRequestException(
                    "A face embedding contains an invalid value.");
            }

            dotProduct += firstValue * secondValue;
            firstSquaredNorm += firstValue * firstValue;
            secondSquaredNorm += secondValue * secondValue;
        }

        var denominator = Math.Sqrt(firstSquaredNorm * secondSquaredNorm);
        if (denominator <= double.Epsilon)
        {
            throw new BadRequestException("A face embedding has no usable values.");
        }

        return Math.Clamp(dotProduct / denominator, -1.0, 1.0);
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        disposed = true;
        faceRecognizer?.Dispose();
        faceDetector?.Dispose();
        initializationGate.Dispose();
        inferenceGate.Dispose();
    }

    private float[]? TryCreateEmbedding(
        Mat image,
        FaceDetectorYN detector,
        FaceRecognizerSF recognizer)
    {
        detector.SetInputSize(image.Size());

        using var faces = new Mat();
        var detected = detector.Detect(image, faces);

        if (detected == 0 || faces.Empty())
        {
            Logger.Info("Face frame rejected Reason=NoFaceDetected");
            return null;
        }
        if (faces.Rows != 1)
        {
            Logger.Info(
                $"Face frame rejected Reason=FaceCountNotOne " +
                $"DetectedFaceCount={faces.Rows}");
            return null;
        }
        if (faces.Cols < 15)
        {
            Logger.Warn(
                $"Face frame rejected Reason=InvalidDetectionOutput " +
                $"DetectionColumns={faces.Cols}");
            return null;
        }

        if (!PassesQualityChecks(image, faces))
        {
            return null;
        }

        using var detectedFace = faces.Row(0);
        using var alignedFace = new Mat();
        recognizer.AlignCrop(image, detectedFace, alignedFace);

        if (alignedFace.Empty())
        {
            Logger.Warn("Face frame rejected Reason=AlignmentFailed");
            return null;
        }

        using var feature = new Mat();
        recognizer.Feature(alignedFace, feature);

        if (feature.Empty())
        {
            Logger.Warn("Face frame rejected Reason=EmbeddingCreationFailed");
            return null;
        }

        feature.GetArray(out float[] embedding);
        return Normalize(embedding);
    }

    private bool PassesQualityChecks(Mat image, Mat faces)
    {
        var faceWidth = faces.At<float>(0, 2);
        var faceWidthRatio = faceWidth / image.Width;
        if (faceWidth <= 0 || faceWidthRatio < options.MinimumFaceWidthRatio)
        {
            Logger.Info(
                $"Face frame rejected Reason=FaceTooSmall " +
                $"FaceWidthRatio={faceWidthRatio:F4} " +
                $"MinimumFaceWidthRatio={options.MinimumFaceWidthRatio:F4}");
            return false;
        }

        var faceRectangle = CreateClippedFaceRectangle(image, faces);
        if (faceRectangle.Width <= 0 || faceRectangle.Height <= 0)
        {
            Logger.Warn("Face frame rejected Reason=InvalidFaceRectangle");
            return false;
        }

        using var faceRegion = new Mat(image, faceRectangle);
        using var grayFace = new Mat();
        Cv2.CvtColor(faceRegion, grayFace, ColorConversionCodes.BGR2GRAY);

        var brightness = Cv2.Mean(grayFace).Val0;
        if (brightness < options.MinimumBrightness ||
            brightness > options.MaximumBrightness)
        {
            Logger.Info(
                $"Face frame rejected Reason=BrightnessOutOfRange " +
                $"Brightness={brightness:F2} " +
                $"AllowedRange={options.MinimumBrightness:F2}-{options.MaximumBrightness:F2}");
            return false;
        }

        using var laplacian = new Mat();
        Cv2.Laplacian(grayFace, laplacian, MatType.CV_64F);
        Cv2.MeanStdDev(laplacian, out _, out var standardDeviation);
        var blurVariance = standardDeviation.Val0 * standardDeviation.Val0;
        if (blurVariance < options.MinimumBlurVariance)
        {
            Logger.Info(
                $"Face frame rejected Reason=ImageTooBlurry " +
                $"BlurVariance={blurVariance:F2} " +
                $"MinimumBlurVariance={options.MinimumBlurVariance:F2}");
            return false;
        }

        var rightEyeX = faces.At<float>(0, 4);
        var rightEyeY = faces.At<float>(0, 5);
        var leftEyeX = faces.At<float>(0, 6);
        var leftEyeY = faces.At<float>(0, 7);
        var rollDegrees = Math.Abs(
            Math.Atan2(leftEyeY - rightEyeY, leftEyeX - rightEyeX) *
            180.0 /
            Math.PI);

        if (rollDegrees > options.MaximumRollDegrees)
        {
            Logger.Info(
                $"Face frame rejected Reason=FaceTooTilted " +
                $"RollDegrees={rollDegrees:F2} " +
                $"MaximumRollDegrees={options.MaximumRollDegrees:F2}");
            return false;
        }
        return true;
    }

    private static Rect CreateClippedFaceRectangle(Mat image, Mat faces)
    {
        var x = Math.Max(0, (int)Math.Floor(faces.At<float>(0, 0)));
        var y = Math.Max(0, (int)Math.Floor(faces.At<float>(0, 1)));
        var right = Math.Min(
            image.Width,
            (int)Math.Ceiling(faces.At<float>(0, 0) + faces.At<float>(0, 2)));
        var bottom = Math.Min(
            image.Height,
            (int)Math.Ceiling(faces.At<float>(0, 1) + faces.At<float>(0, 3)));

        return new Rect(x, y, Math.Max(0, right - x), Math.Max(0, bottom - y));
    }

    private async Task<byte[]?> ReadImageBytesAsync(
        IFormFile imageFile,
        CancellationToken cancellationToken)
    {
        await using var input = imageFile.OpenReadStream();
        await using var output = new MemoryStream((int)imageFile.Length);

        var buffer = new byte[81920];
        long totalBytesRead = 0;

        while (true)
        {
            var bytesRead = await input.ReadAsync(buffer, cancellationToken);
            if (bytesRead == 0)
            {
                break;
            }

            totalBytesRead += bytesRead;
            if (totalBytesRead > options.MaxImageBytes)
            {
                return null;
            }

            await output.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
        }

        return output.ToArray();
    }

    private async Task EnsureInitializedAsync(CancellationToken cancellationToken)
    {
        ThrowIfDisposed();

        if (Volatile.Read(ref initialized))
        {
            return;
        }

        await initializationGate.WaitAsync(cancellationToken);
        try
        {
            ThrowIfDisposed();

            if (initialized)
            {
                return;
            }

            EnsureModelExists(yuNetModelPath, "YuNet");
            EnsureModelExists(sFaceModelPath, "SFace");

            FaceDetectorYN? detector = null;
            FaceRecognizerSF? recognizer = null;

            try
            {
                detector = FaceDetectorYN.Create(
                    yuNetModelPath,
                    string.Empty,
                    new Size(320, 320),
                    options.DetectionScoreThreshold,
                    options.NmsThreshold,
                    options.DetectionTopK,
                    (Backend)0,
                    (Target)0);

                recognizer = FaceRecognizerSF.Create(
                    sFaceModelPath,
                    string.Empty,
                    (Backend)0,
                    (Target)0);

                faceDetector = detector;
                faceRecognizer = recognizer;
                Volatile.Write(ref initialized, true);
            }
            catch (Exception exception)
            {
                recognizer?.Dispose();
                detector?.Dispose();
                throw new InvalidOperationException(
                    "The local face recognition models could not be loaded.",
                    exception);
            }
        }
        finally
        {
            initializationGate.Release();
        }
    }

    private static string ResolveModelPath(string contentRootPath, string configuredPath)
    {
        var path = Path.IsPathRooted(configuredPath)
            ? configuredPath
            : Path.Combine(contentRootPath, configuredPath);

        return Path.GetFullPath(path);
    }

    private static Mat? TryDecodeImage(byte[] imageBytes)
    {
        try
        {
            return Cv2.ImDecode(imageBytes, ImreadModes.Color);
        }
        catch (OpenCVException)
        {
            return null;
        }
    }

    private static void EnsureModelExists(string modelPath, string modelName)
    {
        if (!File.Exists(modelPath))
        {
            throw new InvalidOperationException(
                $"The {modelName} model file was not found at '{modelPath}'.");
        }
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
    }
}
