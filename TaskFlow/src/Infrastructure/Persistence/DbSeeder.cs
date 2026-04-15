using TaskFlow.src.Domain.Entities;

namespace TaskFlow.src.Infrastructure.Persistence
{
    public class DbSeeder
    {
        public static async Task SeedRolesAsync(AppDbContext context)
        {
            if (!context.Roles.Any())
            {
                var roles = new List<Role>
                {
                    new Role { Id = Guid.NewGuid(), Name = "Admin" },
                    new Role { Id = Guid.NewGuid(), Name = "Manager" },
                    new Role { Id = Guid.NewGuid(), Name = "User" }
                };

                await context.Roles.AddRangeAsync(roles);
                await context.SaveChangesAsync();
            }
        }
    }
}
