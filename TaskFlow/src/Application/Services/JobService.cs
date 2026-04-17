using Microsoft.EntityFrameworkCore;
using TaskFlow.src.Application.Interfaces;
using TaskFlow.src.Infrastructure.Persistence;

namespace TaskFlow.src.Application.Services
{
    public class JobService : IJobService
    {
        private readonly AppDbContext _context;

        public JobService(AppDbContext context)
        {
            _context = context;
        }

        public async Task CleanupExpiredRefreshTokensAsync()
        {
            var now = DateTime.UtcNow;

            var expiredTokens = await _context.RefreshTokens
                .Where(rt => rt.ExpiresAt < now || rt.IsRevoked)
                .ToListAsync();

            if (expiredTokens.Count == 0)
            {
                return;
            }

            _context.RefreshTokens.RemoveRange(expiredTokens);

            await _context.SaveChangesAsync();
        }
    }
}
