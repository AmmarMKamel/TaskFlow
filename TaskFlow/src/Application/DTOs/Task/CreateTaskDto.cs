using TaskFlow.src.Domain.Enums;

namespace TaskFlow.src.Application.DTOs.Task
{
    public class CreateTaskDto
    {
        public string Title { get; set; } = null!;
        public string? Description { get; set; }

        public TaskPriority Priority { get; set; }

        public DateTime? DueDate { get; set; }

        public Guid? AssignedToUserId { get; set; }
    }
}
