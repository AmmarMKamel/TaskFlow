namespace TaskFlow.src.Application.Interfaces
{
    public interface IJobService
    {
        Task CleanupExpiredRefreshTokensAsync();
    }
}
