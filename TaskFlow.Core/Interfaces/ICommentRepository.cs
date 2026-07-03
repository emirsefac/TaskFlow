using TaskFlow.Core.Entities;

namespace TaskFlow.Core.Interfaces;

public interface ICommentRepository : IRepository<Comment>
{
    Task<IEnumerable<Comment>> GetByTaskItemIdAsync(int taskItemId);
}
