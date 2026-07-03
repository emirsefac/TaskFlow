using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Core.DTOs;
using TaskFlow.Core.Interfaces;

namespace TaskFlow.API.Controllers;

[Authorize]
[Route("api")]
public class TasksController : BaseApiController
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    /// <summary>Bir projedeki tüm görevleri listeler (Kanban board verisi).</summary>
    [HttpGet("projects/{projectId:int}/tasks")]
    public async Task<ActionResult<IEnumerable<TaskItemDto>>> GetByProject(int projectId)
        => Ok(await _taskService.GetByProjectIdAsync(projectId, CurrentUserId));

    /// <summary>Tek bir görevin detayını getirir.</summary>
    [HttpGet("tasks/{id:int}")]
    public async Task<ActionResult<TaskItemDto>> GetById(int id)
        => Ok(await _taskService.GetByIdAsync(id, CurrentUserId));

    /// <summary>Projeye yeni görev ekler; SignalR ile üyelere anlık bildirim gider.</summary>
    [HttpPost("projects/{projectId:int}/tasks")]
    public async Task<ActionResult<TaskItemDto>> Create(int projectId, CreateTaskDto dto)
    {
        var task = await _taskService.CreateAsync(projectId, dto, CurrentUserId);
        return CreatedAtAction(nameof(GetById), new { id = task.Id }, task);
    }

    /// <summary>Görevin başlık/açıklama/öncelik/tarih bilgilerini günceller.</summary>
    [HttpPut("tasks/{id:int}")]
    public async Task<ActionResult<TaskItemDto>> Update(int id, UpdateTaskDto dto)
        => Ok(await _taskService.UpdateAsync(id, dto, CurrentUserId));

    /// <summary>Görevin durumunu değiştirir (Kanban sütun taşıma); SignalR bildirimi tetikler.</summary>
    [HttpPatch("tasks/{id:int}/status")]
    public async Task<ActionResult<TaskItemDto>> UpdateStatus(int id, UpdateTaskStatusDto dto)
        => Ok(await _taskService.UpdateStatusAsync(id, dto, CurrentUserId));

    /// <summary>Görevi bir üyeye atar veya atamayı kaldırır (UserId = null).</summary>
    [HttpPatch("tasks/{id:int}/assign")]
    public async Task<ActionResult<TaskItemDto>> Assign(int id, AssignTaskDto dto)
        => Ok(await _taskService.AssignAsync(id, dto, CurrentUserId));

    /// <summary>Görevi siler (sadece proje sahibi).</summary>
    [HttpDelete("tasks/{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _taskService.DeleteAsync(id, CurrentUserId);
        return NoContent();
    }
}
