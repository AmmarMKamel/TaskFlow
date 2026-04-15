using Microsoft.EntityFrameworkCore;
using TaskFlow.src.Application.DTOs.Task;
using TaskFlow.src.Application.Interfaces;
using TaskFlow.src.Domain.Entities;
using TaskFlow.src.Infrastructure.Persistence;

namespace TaskFlow.src.Application.Services
{
    public class TaskService : ITaskService
    {
        private readonly AppDbContext _context;

        public TaskService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<TaskResponseDto> CreateTaskAsync(Guid userId, CreateTaskDto dto)
        {
            var task = new TaskItem
            {
                Id = Guid.NewGuid(),
                Title = dto.Title,
                Description = dto.Description,
                Priority = dto.Priority,
                DueDate = dto.DueDate,
                CreatedByUserId = userId,
                AssignedToUserId = dto.AssignedToUserId
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            return new TaskResponseDto
            {
                Id = task.Id,
                Title = dto.Title,
                Description = dto.Description,
                Status = task.Status.ToString(),
                Priority = task.Priority.ToString()
            };
        }

        public async Task<List<TaskResponseDto>> GetTasksAsync(Guid userId)
        {
            var tasks = await _context.Tasks
                .Include(t => t.CreatedByUser)
                .Include(t => t.AssignedToUser)
                .Where(t => t.CreatedByUserId == userId || t.AssignedToUserId == userId)
                .ToListAsync();

            return tasks.Select(t => new TaskResponseDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                Status = t.Status.ToString(),
                Priority = t.Priority.ToString(),
                CreatedBy = t.CreatedByUser.Email,
                AssignedTo = t.AssignedToUser != null ? t.AssignedToUser.Email : null
            }).ToList();
        }

        public async Task<TaskResponseDto> UpdateTaskAsync(Guid taskId, UpdateTaskDto dto)
        {
            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null)
            {
                throw new Exception("Task not found");
            }

            if (dto.Title != null) task.Title = dto.Title;
            if (dto.Description != null) task.Description = dto.Description;
            if (dto.Status.HasValue) task.Status = dto.Status.Value;
            if (dto.Priority.HasValue) task.Priority = dto.Priority.Value;
            if (dto.DueDate.HasValue) task.DueDate = dto.DueDate.Value;
            if (dto.AssignedToUserId.HasValue) task.AssignedToUserId = dto.AssignedToUserId.Value;

            await _context.SaveChangesAsync();

            return new TaskResponseDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Priority = task.Priority.ToString()
            };
        }
    }
}
