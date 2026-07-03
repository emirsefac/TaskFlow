using TaskFlow.Core.DTOs;
using TaskFlow.Core.Entities;
using TaskFlow.Core.Enums;
using TaskFlow.Core.Exceptions;
using TaskFlow.Core.Interfaces;

namespace TaskFlow.Infrastructure.Services;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IUserRepository _userRepository;

    public ProjectService(IProjectRepository projectRepository, IUserRepository userRepository)
    {
        _projectRepository = projectRepository;
        _userRepository = userRepository;
    }

    public async Task<IEnumerable<ProjectDto>> GetMyProjectsAsync(int userId)
    {
        var projects = await _projectRepository.GetProjectsForUserAsync(userId);
        return projects.Select(p => new ProjectDto(
            p.Id, p.Name, p.Description, p.CreatedAt,
            p.Owner.FullName, p.Members.Count, p.Tasks.Count));
    }

    public async Task<ProjectDetailDto> GetByIdAsync(int projectId, int requestingUserId)
    {
        var project = await _projectRepository.GetWithDetailsAsync(projectId)
            ?? throw new NotFoundException("Proje bulunamadı.");

        // Yetki kontrolü: sadece projenin sahibi veya üyesi görebilir
        var isMember = project.OwnerId == requestingUserId ||
                       project.Members.Any(m => m.UserId == requestingUserId);
        if (!isMember)
            throw new ForbiddenException("Bu projeye erişim yetkiniz yok.");

        return new ProjectDetailDto(
            project.Id, project.Name, project.Description, project.CreatedAt,
            project.Owner.FullName,
            project.Members.Select(m => new ProjectMemberDto(
                m.UserId, m.User.FullName, m.User.Email, m.Role.ToString())).ToList(),
            project.Tasks.Select(t => MapTask(t, project.Name)).ToList());
    }

    public async Task<ProjectDto> CreateAsync(CreateProjectDto dto, int ownerId)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new BadRequestException("Proje adı zorunludur.");

        var project = new Project
        {
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim(),
            OwnerId = ownerId
        };

        await _projectRepository.AddAsync(project);
        await _projectRepository.SaveChangesAsync();

        // Proje sahibini otomatik olarak Owner rolüyle üye yap
        await _projectRepository.AddMemberAsync(new ProjectMember
        {
            ProjectId = project.Id,
            UserId = ownerId,
            Role = ProjectRole.Owner
        });
        await _projectRepository.SaveChangesAsync();

        var owner = await _userRepository.GetByIdAsync(ownerId);
        return new ProjectDto(project.Id, project.Name, project.Description,
            project.CreatedAt, owner?.FullName ?? "", 1, 0);
    }

    public async Task AddMemberAsync(int projectId, AddMemberDto dto, int requestingUserId)
    {
        var project = await _projectRepository.GetByIdAsync(projectId)
            ?? throw new NotFoundException("Proje bulunamadı.");

        // İş kuralı: sadece proje sahibi üye ekleyebilir
        if (project.OwnerId != requestingUserId)
            throw new ForbiddenException("Sadece proje sahibi üye ekleyebilir.");

        var userToAdd = await _userRepository.GetByEmailAsync(dto.Email)
            ?? throw new NotFoundException("Bu email ile kayıtlı kullanıcı bulunamadı.");

        var existing = await _projectRepository.GetMembershipAsync(projectId, userToAdd.Id);
        if (existing is not null)
            throw new BadRequestException("Kullanıcı zaten projenin üyesi.");

        await _projectRepository.AddMemberAsync(new ProjectMember
        {
            ProjectId = projectId,
            UserId = userToAdd.Id,
            Role = ProjectRole.Member
        });
        await _projectRepository.SaveChangesAsync();
    }

    private static TaskItemDto MapTask(TaskItem t, string projectName) =>
        new(t.Id, t.Title, t.Description, t.Status.ToString(), t.Priority.ToString(),
            t.DueDate, t.CreatedAt, t.UpdatedAt, t.ProjectId, projectName,
            t.AssignedUser?.FullName, t.AssignedUserId, t.Comments.Count);
}
