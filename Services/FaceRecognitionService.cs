using System.Diagnostics;
using log4net;
using Maiven_Portal_Managment.Common;
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

    public async Task<FaceEmbeddingResult> TryCreateEmbeddingAsync(
        IFormFile imageFile,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(imageFile);
        var totalStopwatch = Stopwatch.StartNew();


        if (imageFile.Length <= 0 || imageFile.Length > options.MaxImageBytes)
        {
            return Reject(
                FaceFrameStatus.InvalidImage,
                $"Detail=InvalidFileSize ImageBytes={imageFile.Length} " +
                $"MaxImageBytes={options.MaxImageBytes}");
        }

        var imageBytes = await ReadImageBytesAsync(imageFile, cancellationToken);
        if (imageBytes is null)
        {
            return Reject(
                FaceFrameStatus.InvalidImage,
                "Detail=ImageReadFailed");
        }

        using var image = TryDecodeImage(imageBytes);
        if (image is null || image.Empty())
        {
            return Reject(
                FaceFrameStatus.InvalidImage,
                "Detail=ImageDecodeFailed");
        }

        await EnsureInitializedAsync(cancellationToken);
        await inferenceGate.WaitAsync(cancellationToken);
        try
        {
            ThrowIfDisposed();
            var result = TryCreateEmbedding(
                image,
                faceDetector ?? throw new InvalidOperationException(
                    "The YuNet model is not initialized."),
                faceRecognizer ?? throw new InvalidOperationException(
                    "The SFace model is not initialized."));

            Logger.Info(
                $"Face frame processing Status={result.Status} " +
                $"TotalLatencyMs={totalStopwatch.Elapsed.TotalMilliseconds:F2}");
            return result;
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

    private FaceEmbeddingResult TryCreateEmbedding(
        Mat image,
        FaceDetectorYN detector,
        FaceRecognizerSF recognizer)
    {
        using var detectionImage = CreateDetectionImage(image);
        detector.SetInputSize(detectionImage.Size());

        using var faces = new Mat();
        var detectionStopwatch = Stopwatch.StartNew();
        var detected = detector.Detect(detectionImage, faces);
        detectionStopwatch.Stop();
        var detectionLatencyMs = detectionStopwatch.Elapsed.TotalMilliseconds;
        var faceCount = faces.Empty() ? 0 : faces.Rows;

        Logger.Info(
            $"YuNet detection completed OriginalWidth={image.Width} OriginalHeight={image.Height} " +
            $"DetectionWidth={detectionImage.Width} DetectionHeight={detectionImage.Height} " +
            $"DetectionScoreThreshold={options.DetectionScoreThreshold:F2} " +
            $"DetectedFaceCount={faceCount} " +
            $"DetectionLatencyMs={detectionLatencyMs:F2}");

        if (detected == 0 || faces.Empty())
        {
            return Reject(
                FaceFrameStatus.NoFaceDetected,
                $"ImageWidth={image.Width} ImageHeight={image.Height} " +
                $"DetectionScoreThreshold={options.DetectionScoreThreshold:F2} " +
                "DetectedFaceCount=0 FaceDetectionScore=N/A " +
                "FaceWidthRatio=N/A Brightness=N/A BlurVariance=N/A RollDegrees=N/A");
        }

        if (faceCount > 1)
        {
            return Reject(
                FaceFrameStatus.MultipleFacesDetected,
                $"ImageWidth={image.Width} ImageHeight={image.Height} " +
                $"DetectionScoreThreshold={options.DetectionScoreThreshold:F2} " +
                $"DetectedFaceCount={faceCount} " +
                $"FaceDetectionScore={(faces.Cols >= 15 ? faces.At<float>(0, 14).ToString("F4") : "N/A")} " +
                "FaceWidthRatio=N/A Brightness=N/A BlurVariance=N/A RollDegrees=N/A");
        }

        var detectionColumns = faces.Cols;
        if (faceCount != 1 || detectionColumns < 15)
        {
            return Reject(
                FaceFrameStatus.InvalidImage,
                $"Detail=InvalidDetectionOutput DetectionRows={faceCount} " +
                $"DetectionColumns={detectionColumns}",
                warning: true);
        }

        using var remappedFaces = RemapFaceCoordinates(
            faces,
            detectionImage.Size(),
            image.Size());

        FaceFrameStatus qualityStatus;
        try
        {
            LogFaceDetectionMetrics(image, remappedFaces, faceCount);
            qualityStatus = GetQualityStatus(image, remappedFaces);
        }
        catch (OpenCVException exception)
        {
            Logger.Warn(
                $"Face frame rejected Reason={FaceFrameStatus.InvalidImage} " +
                "Detail=QualityCheckFailed",
                exception);
            return FaceEmbeddingResult.Rejected(FaceFrameStatus.InvalidImage);
        }

        if (qualityStatus != FaceFrameStatus.Success)
        {
            return FaceEmbeddingResult.Rejected(qualityStatus);
        }

        using var detectedFace = remappedFaces.Row(0);
        using var alignedFace = new Mat();

        try
        {
            recognizer.AlignCrop(image, detectedFace, alignedFace);
        }
        catch (OpenCVException exception)
        {
            Logger.Warn(
                $"Face frame rejected Reason={FaceFrameStatus.AlignmentFailed}",
                exception);
            return FaceEmbeddingResult.Rejected(FaceFrameStatus.AlignmentFailed);
        }

        if (alignedFace.Empty())
        {
            return Reject(
                FaceFrameStatus.AlignmentFailed,
                warning: true);
        }

        using var feature = new Mat();

        try
        {
            recognizer.Feature(alignedFace, feature);

            if (feature.Empty())
            {
                return Reject(
                    FaceFrameStatus.EmbeddingCreationFailed,
                    warning: true);
            }

            feature.GetArray(out float[] embedding);
            return FaceEmbeddingResult.Succeeded(Normalize(embedding));
        }
        catch (Exception exception)
            when (exception is OpenCVException or BadRequestException)
        {
            Logger.Warn(
                $"Face frame rejected Reason={FaceFrameStatus.EmbeddingCreationFailed}",
                exception);
            return FaceEmbeddingResult.Rejected(
                FaceFrameStatus.EmbeddingCreationFailed);
        }
    }

    private Mat CreateDetectionImage(Mat image)
    {
        var originalLongEdge = Math.Max(image.Width, image.Height);
        if (originalLongEdge <= options.DetectionMaxLongEdge)
        {
            return image.Clone();
        }

        var scale = options.DetectionMaxLongEdge / (double)originalLongEdge;
        var detectionWidth = Math.Max(1, (int)Math.Round(image.Width * scale));
        var detectionHeight = Math.Max(1, (int)Math.Round(image.Height * scale));
        var resizedImage = new Mat();
        Cv2.Resize(
            image,
            resizedImage,
            new Size(detectionWidth, detectionHeight),
            0,
            0,
            InterpolationFlags.Area);
        return resizedImage;
    }

    private static Mat RemapFaceCoordinates(
        Mat faces,
        Size detectionSize,
        Size originalSize)
    {
        if (faces.Cols < 15)
        {
            throw new OpenCVException("YuNet detection output must contain 15 columns.");
        }

        var scaleX = originalSize.Width / (double)detectionSize.Width;
        var scaleY = originalSize.Height / (double)detectionSize.Height;
        var remappedFaces = faces.Clone();
        var faceRows = faces.Rows;

        for (var row = 0; row < faceRows; row++)
        {
            remappedFaces.Set(row, 0, (float)(faces.At<float>(row, 0) * scaleX));
            remappedFaces.Set(row, 1, (float)(faces.At<float>(row, 1) * scaleY));
            remappedFaces.Set(row, 2, (float)(faces.At<float>(row, 2) * scaleX));
            remappedFaces.Set(row, 3, (float)(faces.At<float>(row, 3) * scaleY));

            for (var column = 4; column <= 12; column += 2)
            {
                remappedFaces.Set(
                    row,
                    column,
                    (float)(faces.At<float>(row, column) * scaleX));
            }

            for (var column = 5; column <= 13; column += 2)
            {
                remappedFaces.Set(
                    row,
                    column,
                    (float)(faces.At<float>(row, column) * scaleY));
            }
        }

        return remappedFaces;
    }

    private void LogFaceDetectionMetrics(Mat image, Mat faces, int faceCount)
    {
        var faceWidth = faces.At<float>(0, 2);
        var faceWidthRatio = faceWidth / image.Width;
        var faceDetectionScore = faces.At<float>(0, 14);

        var faceRectangle = CreateClippedFaceRectangle(image, faces);
        if (faceRectangle.Width <= 0 || faceRectangle.Height <= 0)
        {
            Logger.Info(
                $"YuNet face detection ImageWidth={image.Width} ImageHeight={image.Height} " +
                $"DetectionScoreThreshold={options.DetectionScoreThreshold:F2} " +
                $"DetectedFaceCount={faceCount} " +
                $"FaceDetectionScore={faceDetectionScore:F4} " +
                $"FaceWidthRatio={faceWidthRatio:F4} Brightness=N/A BlurVariance=N/A RollDegrees=N/A");
            return;
        }

        using var faceRegion = new Mat(image, faceRectangle);
        using var grayFace = new Mat();
        Cv2.CvtColor(faceRegion, grayFace, ColorConversionCodes.BGR2GRAY);

        var brightness = Cv2.Mean(grayFace).Val0;

        using var laplacian = new Mat();
        Cv2.Laplacian(grayFace, laplacian, MatType.CV_64F);
        Cv2.MeanStdDev(laplacian, out _, out var standardDeviation);
        var blurVariance = standardDeviation.Val0 * standardDeviation.Val0;

        var rightEyeX = faces.At<float>(0, 4);
        var rightEyeY = faces.At<float>(0, 5);
        var leftEyeX = faces.At<float>(0, 6);
        var leftEyeY = faces.At<float>(0, 7);
        var rollDegrees = Math.Abs(
            Math.Atan2(leftEyeY - rightEyeY, leftEyeX - rightEyeX) *
            180.0 /
            Math.PI);

        Logger.Info(
            $"YuNet face detection ImageWidth={image.Width} ImageHeight={image.Height} " +
            $"DetectionScoreThreshold={options.DetectionScoreThreshold:F2} " +
            $"DetectedFaceCount={faceCount} " +
            $"FaceDetectionScore={faceDetectionScore:F4} " +
            $"FaceWidthRatio={faceWidthRatio:F4} " +
            $"Brightness={brightness:F2} " +
            $"BlurVariance={blurVariance:F2} " +
            $"RollDegrees={rollDegrees:F2}");
    }

    private FaceFrameStatus GetQualityStatus(Mat image, Mat faces)
    {
        var faceWidth = faces.At<float>(0, 2);
        var faceWidthRatio = faceWidth / image.Width;
        if (faceWidth <= 0 || faceWidthRatio < options.MinimumFaceWidthRatio)
        {
            LogQualityRejection(
                FaceFrameStatus.FaceTooSmall,
                $"FaceWidthRatio={faceWidthRatio:F4} " +
                $"MinimumFaceWidthRatio={options.MinimumFaceWidthRatio:F4}");
            return FaceFrameStatus.FaceTooSmall;
        }

        var faceRectangle = CreateClippedFaceRectangle(image, faces);
        if (faceRectangle.Width <= 0 || faceRectangle.Height <= 0)
        {
            LogQualityRejection(
                FaceFrameStatus.InvalidImage,
                "Detail=InvalidFaceRectangle",
                warning: true);
            return FaceFrameStatus.InvalidImage;
        }

        using var faceRegion = new Mat(image, faceRectangle);
        using var grayFace = new Mat();
        Cv2.CvtColor(faceRegion, grayFace, ColorConversionCodes.BGR2GRAY);

        var brightness = Cv2.Mean(grayFace).Val0;
        if (brightness < options.MinimumBrightness)
        {
            LogQualityRejection(
                FaceFrameStatus.TooDark,
                $"Brightness={brightness:F2} " +
                $"MinimumBrightness={options.MinimumBrightness:F2}");
            return FaceFrameStatus.TooDark;
        }

        if (brightness > options.MaximumBrightness)
        {
            LogQualityRejection(
                FaceFrameStatus.TooBright,
                $"Brightness={brightness:F2} " +
                $"MaximumBrightness={options.MaximumBrightness:F2}");
            return FaceFrameStatus.TooBright;
        }

        using var laplacian = new Mat();
        Cv2.Laplacian(grayFace, laplacian, MatType.CV_64F);
        Cv2.MeanStdDev(laplacian, out _, out var standardDeviation);
        var blurVariance = standardDeviation.Val0 * standardDeviation.Val0;
        if (blurVariance < options.MinimumBlurVariance)
        {
            LogQualityRejection(
                FaceFrameStatus.TooBlurry,
                $"BlurVariance={blurVariance:F2} " +
                $"MinimumBlurVariance={options.MinimumBlurVariance:F2}");
            return FaceFrameStatus.TooBlurry;
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
            LogQualityRejection(
                FaceFrameStatus.FaceTooTilted,
                $"RollDegrees={rollDegrees:F2} " +
                $"MaximumRollDegrees={options.MaximumRollDegrees:F2}");
            return FaceFrameStatus.FaceTooTilted;
        }

        return FaceFrameStatus.Success;
    }

    private static FaceEmbeddingResult Reject(
        FaceFrameStatus status,
        string? details = null,
        bool warning = false)
    {
        var message = $"Face frame rejected Reason={status}";
        if (!string.IsNullOrWhiteSpace(details))
        {
            message += $" {details}";
        }

        if (warning)
        {
            Logger.Warn(message);
        }
        else
        {
            Logger.Info(message);
        }

        return FaceEmbeddingResult.Rejected(status);
    }

    private static void LogQualityRejection(
        FaceFrameStatus status,
        string details,
        bool warning = false)
    {
        var message = $"Face frame rejected Reason={status} {details}";

        if (warning)
        {
            Logger.Warn(message);
        }
        else
        {
            Logger.Info(message);
        }
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
