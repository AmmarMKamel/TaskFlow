using TaskFlow.src.Application.DTOs.Task;

namespace TaskFlow.src.Application.Interfaces
{
    public interface ITaskService
    {
        Task<List<TaskResponseDto>> GetTasksAsync(Guid userId);
        Task<TaskResponseDto> CreateTaskAsync(Guid userId, CreateTaskDto dto);
        Task<TaskResponseDto> UpdateTaskAsync(Guid taskId, UpdateTaskDto dto);
    }
}
