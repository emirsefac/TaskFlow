using TaskFlow.Core.DTOs;

namespace TaskFlow.Core.Interfaces;

public interface IProjectService
{
    Task<IEnumerable<ProjectDto>> GetMyProjectsAsync(int userId);
    Task<ProjectDetailDto> GetByIdAsync(int projectId, int requestingUserId);
    Task<ProjectDto> CreateAsync(CreateProjectDto dto, int ownerId);
    Task AddMemberAsync(int projectId, AddMemberDto dto, int requestingUserId);
}
