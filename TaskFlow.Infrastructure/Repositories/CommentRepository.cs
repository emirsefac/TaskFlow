using Microsoft.EntityFrameworkCore;
using TaskFlow.Core.Entities;
using TaskFlow.Core.Interfaces;
using TaskFlow.Infrastructure.Data;

namespace TaskFlow.Infrastructure.Repositories;

public class CommentRepository : Repository<Comment>, ICommentRepository
{
    public CommentRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Comment>> GetByTaskItemIdAsync(int taskItemId) =>
        await _dbSet
            .Include(c => c.User)
            .Where(c => c.TaskItemId == taskItemId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();
}
