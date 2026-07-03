using TaskFlow.Core.DTOs;

namespace TaskFlow.Core.Interfaces;

public interface ICommentService
{
    Task<IEnumerable<CommentDto>> GetByTaskIdAsync(int taskId, int requestingUserId);
    Task<CommentDto> AddAsync(int taskId, CreateCommentDto dto, int requestingUserId);
}
