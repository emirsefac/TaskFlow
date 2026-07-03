namespace TaskFlow.Core.DTOs;

public record CreateCommentDto(string Content);

public record CommentDto(int Id, string Content, DateTime CreatedAt, string UserName, int UserId);
