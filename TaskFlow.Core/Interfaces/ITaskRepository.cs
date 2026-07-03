using TaskFlow.Core.Entities;

namespace TaskFlow.Core.Interfaces;

public interface ITaskRepository : IRepository<TaskItem>
{
    Task<TaskItem?> GetWithDetailsAsync(int id);
    Task<IEnumerable<TaskItem>> GetByProjectIdAsync(int projectId);
    Task<IEnumerable<TaskItem>> GetByAssignedUserAsync(int userId);
}
