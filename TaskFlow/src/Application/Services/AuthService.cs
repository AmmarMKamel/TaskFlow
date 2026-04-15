using Microsoft.EntityFrameworkCore;
using TaskFlow.src.Application.DTOs.Auth;
using TaskFlow.src.Application.Interfaces;
using TaskFlow.src.Domain.Entities;
using TaskFlow.src.Infrastructure.Persistence;
using TaskFlow.src.Infrastructure.Services;

namespace TaskFlow.src.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly PasswordHasher _passwordHasher;
        private readonly JwtService _jwtService;
        private readonly RefreshTokenService _refreshTokenService;

        public AuthService(
            AppDbContext context,
            PasswordHasher passwordHasher,
            JwtService jwtService,
            RefreshTokenService refreshTokenService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(AppDbContext));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(PasswordHasher));
            _jwtService = jwtService ?? throw new ArgumentNullException(nameof(JwtService));
            _refreshTokenService = refreshTokenService ?? throw new ArgumentNullException(nameof(RefreshTokenService));
        }

        public async Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request)
        {
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (existingUser != null)
            {
                throw new Exception("Email already exists.");
            }

            var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User");
            if (userRole == null)
            {
                throw new Exception("Default role not found");
            }

            var (hash, salt) = _passwordHasher.HashPassword(request.Password);

            var user = new User
            {
                Id = Guid.NewGuid(),
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PasswordHash = hash,
                PasswordSalt = salt,
                UserRoles = new List<UserRole>
                {
                    new UserRole
                    {
                        RoleId = userRole.Id
                    }
                }
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new RegisterResponseDto
            {
                Message = "User registered successfully."
            };
        }

        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
            {
                throw new Exception("Invalid email or password");
            }

            var isValid = _passwordHasher.VerifyPassword(
                request.Password,
                user.PasswordHash,
                user.PasswordSalt);
            if (!isValid)
            {
                throw new Exception("Invalid email or password");
            }

            var accessToken = _jwtService.GenerateToken(user);
            var refreshTokenValue = _refreshTokenService.GenerateRefreshToken();

            var refreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = refreshTokenValue,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };

            //user.RefreshTokens.Add(refreshToken);

            await _context.RefreshTokens.AddAsync(refreshToken);

            var entries = _context.ChangeTracker.Entries()
    .Where(e => e.State == EntityState.Modified || e.State == EntityState.Added || e.State == EntityState.Deleted)
    .ToList();

            await _context.SaveChangesAsync();

            return new LoginResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshTokenValue
            };
        }

        public async Task<RefreshResponseDto> RefreshTokenAsync(RefreshRequestDto request)
        {
            var existingToken = await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);
            if (existingToken == null)
            {
                throw new Exception("Invalid refresh token");
            }

            if (existingToken.IsRevoked || existingToken.ExpiresAt < DateTime.UtcNow)
            {
                throw new Exception("Refresh token expired or revoked");
            }

            existingToken.IsRevoked = true;
            existingToken.RevokedAt = DateTime.UtcNow;

            var newRefreshTokenValue = _refreshTokenService.GenerateRefreshToken();

            var newRefreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = newRefreshTokenValue,
                UserId = existingToken.UserId,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };

            existingToken.ReplacedByToken = newRefreshTokenValue;

            _context.RefreshTokens.Add(newRefreshToken);

            var newAccessToken = _jwtService.GenerateToken(existingToken.User);

            await _context.SaveChangesAsync();

            return new RefreshResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshTokenValue
            };
        }

        public async Task RevokeTokenAsync(string refreshToken)
        {
            var token = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken);
            if (token == null)
            {
                throw new Exception("Token not found");
            }

            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }
    }
}
