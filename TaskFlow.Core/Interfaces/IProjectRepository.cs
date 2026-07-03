using TaskFlow.Core.Entities;

namespace TaskFlow.Core.Interfaces;

public interface IProjectRepository : IRepository<Project>
{
    Task<Project?> GetWithDetailsAsync(int id);
    Task<IEnumerable<Project>> GetProjectsForUserAsync(int userId);
    Task<bool> IsUserMemberAsync(int projectId, int userId);
    Task<ProjectMember?> GetMembershipAsync(int projectId, int userId);
    Task AddMemberAsync(ProjectMember member);
}
