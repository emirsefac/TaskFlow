using TaskFlow.Core.Enums;

namespace TaskFlow.Core.Entities;

// User ile Project arasındaki many-to-many ilişkiyi kuran ara tablo (join entity)
public class ProjectMember
{
    public int Id { get; set; }

    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public ProjectRole Role { get; set; } = ProjectRole.Member;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}
