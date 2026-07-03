using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Core.DTOs;
using TaskFlow.Core.Interfaces;

namespace TaskFlow.API.Controllers;

[Authorize]
[Route("api/tasks/{taskId:int}/comments")]
public class CommentsController : BaseApiController
{
    private readonly ICommentService _commentService;

    public CommentsController(ICommentService commentService)
    {
        _commentService = commentService;
    }

    /// <summary>Görevin tüm yorumlarını listeler.</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CommentDto>>> GetByTask(int taskId)
        => Ok(await _commentService.GetByTaskIdAsync(taskId, CurrentUserId));

    /// <summary>Göreve yorum ekler.</summary>
    [HttpPost]
    public async Task<ActionResult<CommentDto>> Add(int taskId, CreateCommentDto dto)
    {
        var comment = await _commentService.AddAsync(taskId, dto, CurrentUserId);
        return Ok(comment);
    }
}
