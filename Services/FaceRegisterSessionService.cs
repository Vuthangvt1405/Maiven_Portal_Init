using Maiven_Portal_Managment.Dtos.Response;
using Maiven_Portal_Managment.Exceptions;

namespace Maiven_Portal_Managment.Services;

public sealed class FaceRegisterSessionService
{
    private const int RequiredFrameCount = 3;
    private static readonly TimeSpan SessionLifetime = TimeSpan.FromMinutes(10);

    private readonly object syncRoot = new();
    private readonly Dictionary<Guid, FaceRegisterSession> sessions = [];
    private readonly Dictionary<long, Guid> activeSessionIdsByUser = [];

    public FaceRegisterResponse StartSession(long userId)
    {
        lock (syncRoot)
        {
            RemoveExpiredSessions(DateTime.UtcNow);

            if (activeSessionIdsByUser.TryGetValue(userId, out var existingSessionId))
            {
                RemoveSession(existingSessionId);
            }

            var session = new FaceRegisterSession
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ExpiresAt = DateTime.UtcNow.Add(SessionLifetime)
            };

            sessions.Add(session.Id, session);
            activeSessionIdsByUser[userId] = session.Id;

            return new FaceRegisterResponse
            {
                SessionId = session.Id,
                RequiredFrames = RequiredFrameCount,
                ExpiresAt = session.ExpiresAt
            };
        }
    }

    public FaceFrameResponse AddEmbedding(
        Guid sessionId,
        long userId,
        float[] embedding)
    {
        ArgumentNullException.ThrowIfNull(embedding);

        if (embedding.Length == 0 || embedding.Any(value => !float.IsFinite(value)))
        {
            throw new BadRequestException("The face embedding is invalid.");
        }

        lock (syncRoot)
        {
            var session = GetActiveSession(sessionId, userId, DateTime.UtcNow);

            if (session.Embeddings.Count >= RequiredFrameCount)
            {
                throw new BadRequestException(
                    "The registration session already has enough accepted frames.");
            }

            if (session.Embeddings.Count > 0 &&
                session.Embeddings[0].Length != embedding.Length)
            {
                throw new BadRequestException(
                    "All face embeddings in a registration session must have the same dimension.");
            }

            session.Embeddings.Add((float[])embedding.Clone());

            return new FaceFrameResponse
            {
                Accepted = true,
                AcceptedCount = session.Embeddings.Count,
                RequiredCount = RequiredFrameCount
            };
        }
    }

    public FaceFrameResponse GetFrameStatus(Guid sessionId, long userId)
    {
        lock (syncRoot)
        {
            var session = GetActiveSession(sessionId, userId, DateTime.UtcNow);

            return new FaceFrameResponse
            {
                Accepted = false,
                AcceptedCount = session.Embeddings.Count,
                RequiredCount = RequiredFrameCount
            };
        }
    }

    public IReadOnlyList<float[]> GetCompletedEmbeddings(Guid sessionId, long userId)
    {
        lock (syncRoot)
        {
            var session = GetActiveSession(sessionId, userId, DateTime.UtcNow);

            if (session.Embeddings.Count != RequiredFrameCount)
            {
                throw new BadRequestException(
                    $"The registration session requires {RequiredFrameCount} accepted frames before confirmation.");
            }

            return session.Embeddings
                .Select(embedding => (float[])embedding.Clone())
                .ToArray();
        }
    }

    public void CompleteSession(Guid sessionId, long userId)
    {
        lock (syncRoot)
        {
            _ = GetActiveSession(sessionId, userId, DateTime.UtcNow);
            RemoveSession(sessionId);
        }
    }

    public void CancelSession(Guid sessionId, long userId)
    {
        lock (syncRoot)
        {
            _ = GetActiveSession(sessionId, userId, DateTime.UtcNow);
            RemoveSession(sessionId);
        }
    }

    private FaceRegisterSession GetActiveSession(
        Guid sessionId,
        long userId,
        DateTime utcNow)
    {
        RemoveExpiredSessions(utcNow);

        if (!sessions.TryGetValue(sessionId, out var session) ||
            session.UserId != userId)
        {
            throw new NotFoundException(
                "The face registration session was not found or has expired.");
        }

        return session;
    }

    private void RemoveExpiredSessions(DateTime utcNow)
    {
        var expiredSessionIds = sessions.Values
            .Where(session => session.ExpiresAt <= utcNow)
            .Select(session => session.Id)
            .ToArray();

        foreach (var sessionId in expiredSessionIds)
        {
            RemoveSession(sessionId);
        }
    }

    private void RemoveSession(Guid sessionId)
    {
        if (!sessions.Remove(sessionId, out var session))
        {
            return;
        }

        if (activeSessionIdsByUser.TryGetValue(session.UserId, out var activeSessionId) &&
            activeSessionId == sessionId)
        {
            activeSessionIdsByUser.Remove(session.UserId);
        }
    }

    private sealed class FaceRegisterSession
    {
        public Guid Id { get; init; }

        public long UserId { get; init; }

        public DateTime ExpiresAt { get; init; }

        public List<float[]> Embeddings { get; } = [];
    }
}
