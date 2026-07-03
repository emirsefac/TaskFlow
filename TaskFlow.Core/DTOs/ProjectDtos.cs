namespace TaskFlow.Core.DTOs;

public record CreateProjectDto(string Name, string? Description);

public record ProjectDto(int Id, string Name, string? Description, DateTime CreatedAt,
    string OwnerName, int MemberCount, int TaskCount);

public record ProjectDetailDto(int Id, string Name, string? Description, DateTime CreatedAt,
    string OwnerName, List<ProjectMemberDto> Members, List<TaskItemDto> Tasks);

public record ProjectMemberDto(int UserId, string FullName, string Email, string Role);

public record AddMemberDto(string Email);
