using TaskFlow.Core.Enums;
// .NET'in kendi System.Threading.Tasks.TaskStatus tipi ile çakışmayı önlemek için alias
using TaskStatus = TaskFlow.Core.Enums.TaskStatus;

namespace TaskFlow.Core.Entities;

public class TaskItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskStatus Status { get; set; } = TaskStatus.Backlog;
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public DateTime? DueDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    // Görev atanmamış olabilir, bu yüzden nullable
    public int? AssignedUserId { get; set; }
    public User? AssignedUser { get; set; }

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
