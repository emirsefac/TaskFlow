using Microsoft.EntityFrameworkCore;
using TaskFlow.Core.Entities;
using TaskFlow.Core.Interfaces;
using TaskFlow.Infrastructure.Data;

namespace TaskFlow.Infrastructure.Repositories;

public class ProjectRepository : Repository<Project>, IProjectRepository
{
    public ProjectRepository(AppDbContext context) : base(context) { }

    public async Task<Project?> GetWithDetailsAsync(int id) =>
        await _dbSet
            .Include(p => p.Owner)
            .Include(p => p.Members).ThenInclude(m => m.User)
            .Include(p => p.Tasks).ThenInclude(t => t.AssignedUser)
            .Include(p => p.Tasks).ThenInclude(t => t.Comments)
            .FirstOrDefaultAsync(p => p.Id == id);

    public async Task<IEnumerable<Project>> GetProjectsForUserAsync(int userId) =>
        await _dbSet
            .Include(p => p.Owner)
            .Include(p => p.Members)
            .Include(p => p.Tasks)
            .Where(p => p.OwnerId == userId || p.Members.Any(m => m.UserId == userId))
            .ToListAsync();

    public async Task<bool> IsUserMemberAsync(int projectId, int userId) =>
        await _dbSet.AnyAsync(p => p.Id == projectId &&
            (p.OwnerId == userId || p.Members.Any(m => m.UserId == userId)));

    public async Task<ProjectMember?> GetMembershipAsync(int projectId, int userId) =>
        await _context.ProjectMembers
            .FirstOrDefaultAsync(m => m.ProjectId == projectId && m.UserId == userId);

    public async Task AddMemberAsync(ProjectMember member) =>
        await _context.ProjectMembers.AddAsync(member);
}
