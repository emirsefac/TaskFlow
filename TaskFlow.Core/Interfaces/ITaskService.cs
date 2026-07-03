using TaskFlow.Core.DTOs;

namespace TaskFlow.Core.Interfaces;

public interface ITaskService
{
    Task<IEnumerable<TaskItemDto>> GetByProjectIdAsync(int projectId, int requestingUserId);
    Task<TaskItemDto> GetByIdAsync(int taskId, int requestingUserId);
    Task<TaskItemDto> CreateAsync(int projectId, CreateTaskDto dto, int requestingUserId);
    Task<TaskItemDto> UpdateAsync(int taskId, UpdateTaskDto dto, int requestingUserId);
    Task<TaskItemDto> UpdateStatusAsync(int taskId, UpdateTaskStatusDto dto, int requestingUserId);
    Task<TaskItemDto> AssignAsync(int taskId, AssignTaskDto dto, int requestingUserId);
    Task DeleteAsync(int taskId, int requestingUserId);
}
