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

        public AuthService(AppDbContext context, PasswordHasher passwordHasher)
        {
            _context = context ?? throw new ArgumentNullException(nameof(AppDbContext));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(PasswordHasher));
        }

        public async Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request)
        {
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (existingUser != null)
            {
                throw new Exception("Email already exists.");
            }

            var (hash, salt) = _passwordHasher.HashPassword(request.Password);

            var user = new User
            {
                Id = Guid.NewGuid(),
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PasswordHash = hash,
                PasswordSalt = salt
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new RegisterResponseDto
            {
                Message = "User registered successfully."
            };
        }
    }
}
