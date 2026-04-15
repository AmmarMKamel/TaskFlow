using TaskFlow.src.Domain.Enums;

namespace TaskFlow.src.Application.DTOs.Task
{
    public class UpdateTaskDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }

        public Domain.Enums.TaskStatus? Status { get; set; }
        public TaskPriority? Priority { get; set; }

        public DateTime? DueDate { get; set; }
        public Guid? AssignedToUserId { get; set; }
    }
}
