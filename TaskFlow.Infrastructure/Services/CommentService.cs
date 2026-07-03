using TaskFlow.Core.DTOs;
using TaskFlow.Core.Entities;
using TaskFlow.Core.Exceptions;
using TaskFlow.Core.Interfaces;

namespace TaskFlow.Infrastructure.Services;

public class CommentService : ICommentService
{
    private readonly ICommentRepository _commentRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IUserRepository _userRepository;

    public CommentService(ICommentRepository commentRepository,
                          ITaskRepository taskRepository,
                          IProjectRepository projectRepository,
                          IUserRepository userRepository)
    {
        _commentRepository = commentRepository;
        _taskRepository = taskRepository;
        _projectRepository = projectRepository;
        _userRepository = userRepository;
    }

    public async Task<IEnumerable<CommentDto>> GetByTaskIdAsync(int taskId, int requestingUserId)
    {
        var task = await _taskRepository.GetByIdAsync(taskId)
            ?? throw new NotFoundException("Görev bulunamadı.");

        if (!await _projectRepository.IsUserMemberAsync(task.ProjectId, requestingUserId))
            throw new ForbiddenException("Bu göreve erişim yetkiniz yok.");

        var comments = await _commentRepository.GetByTaskItemIdAsync(taskId);
        return comments.Select(c => new CommentDto(
            c.Id, c.Content, c.CreatedAt, c.User.FullName, c.UserId));
    }

    public async Task<CommentDto> AddAsync(int taskId, CreateCommentDto dto, int requestingUserId)
    {
        if (string.IsNullOrWhiteSpace(dto.Content))
            throw new BadRequestException("Yorum içeriği boş olamaz.");

        var task = await _taskRepository.GetByIdAsync(taskId)
            ?? throw new NotFoundException("Görev bulunamadı.");

        if (!await _projectRepository.IsUserMemberAsync(task.ProjectId, requestingUserId))
            throw new ForbiddenException("Bu göreve yorum yapma yetkiniz yok.");

        var comment = new Comment
        {
            Content = dto.Content.Trim(),
            TaskItemId = taskId,
            UserId = requestingUserId
        };

        await _commentRepository.AddAsync(comment);
        await _commentRepository.SaveChangesAsync();

        var user = await _userRepository.GetByIdAsync(requestingUserId);
        return new CommentDto(comment.Id, comment.Content, comment.CreatedAt,
            user?.FullName ?? "", requestingUserId);
    }
}
