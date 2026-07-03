using TaskFlow.Core.Enums;
using TaskStatus = TaskFlow.Core.Enums.TaskStatus;

namespace TaskFlow.Core.DTOs;

public record CreateTaskDto(string Title, string? Description, TaskPriority Priority,
    DateTime? DueDate, int? AssignedUserId);

public record UpdateTaskDto(string Title, string? Description, TaskPriority Priority, DateTime? DueDate);

public record UpdateTaskStatusDto(TaskStatus Status);

public record AssignTaskDto(int? UserId);

public record TaskItemDto(int Id, string Title, string? Description, string Status,
    string Priority, DateTime? DueDate, DateTime CreatedAt, DateTime? UpdatedAt,
    int ProjectId, string ProjectName, string? AssignedUserName, int? AssignedUserId,
    int CommentCount);
