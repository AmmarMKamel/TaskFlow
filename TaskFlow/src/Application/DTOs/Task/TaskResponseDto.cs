namespace TaskFlow.src.Application.DTOs.Task
{
    public class TaskResponseDto
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = null!;
        public string? Description { get; set; }

        public string Status { get; set; } = null!;
        public string Priority { get; set; } = null!;

        public DateTime? DueDate { get; set; }

        public string CreatedBy { get; set; } = null!;
        public string? AssignedTo { get; set; }
    }
}
