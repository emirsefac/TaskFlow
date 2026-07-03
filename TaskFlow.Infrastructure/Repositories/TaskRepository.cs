using Microsoft.EntityFrameworkCore;
using TaskFlow.Core.Entities;
using TaskFlow.Core.Interfaces;
using TaskFlow.Infrastructure.Data;

namespace TaskFlow.Infrastructure.Repositories;

public class TaskRepository : Repository<TaskItem>, ITaskRepository
{
    public TaskRepository(AppDbContext context) : base(context) { }

    public async Task<TaskItem?> GetWithDetailsAsync(int id) =>
        await _dbSet
            .Include(t => t.Project)
            .Include(t => t.AssignedUser)
            .Include(t => t.Comments)
            .FirstOrDefaultAsync(t => t.Id == id);

    public async Task<IEnumerable<TaskItem>> GetByProjectIdAsync(int projectId) =>
        await _dbSet
            .Include(t => t.Project)
            .Include(t => t.AssignedUser)
            .Include(t => t.Comments)
            .Where(t => t.ProjectId == projectId)
            .OrderBy(t => t.Status)
            .ThenByDescending(t => t.Priority)
            .ToListAsync();

    public async Task<IEnumerable<TaskItem>> GetByAssignedUserAsync(int userId) =>
        await _dbSet
            .Include(t => t.Project)
            .Include(t => t.AssignedUser)
            .Include(t => t.Comments)
            .Where(t => t.AssignedUserId == userId)
            .ToListAsync();
}
