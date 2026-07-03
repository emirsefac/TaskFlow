using TaskFlow.Core.DTOs;
using TaskFlow.Core.Entities;
using TaskFlow.Core.Exceptions;
using TaskFlow.Core.Interfaces;

namespace TaskFlow.Infrastructure.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly ITaskNotifier _notifier;

    public TaskService(ITaskRepository taskRepository,
                       IProjectRepository projectRepository,
                       ITaskNotifier notifier)
    {
        _taskRepository = taskRepository;
        _projectRepository = projectRepository;
        _notifier = notifier;
    }

    public async Task<IEnumerable<TaskItemDto>> GetByProjectIdAsync(int projectId, int requestingUserId)
    {
        await EnsureMemberAsync(projectId, requestingUserId);
        var tasks = await _taskRepository.GetByProjectIdAsync(projectId);
        return tasks.Select(Map);
    }

    public async Task<TaskItemDto> GetByIdAsync(int taskId, int requestingUserId)
    {
        var task = await GetTaskOrThrowAsync(taskId);
        await EnsureMemberAsync(task.ProjectId, requestingUserId);
        return Map(task);
    }

    public async Task<TaskItemDto> CreateAsync(int projectId, CreateTaskDto dto, int requestingUserId)
    {
        await EnsureMemberAsync(projectId, requestingUserId);

        if (string.IsNullOrWhiteSpace(dto.Title))
            throw new BadRequestException("Görev başlığı zorunludur.");

        // Görev birine atanacaksa, o kişinin de proje üyesi olması gerekir
        if (dto.AssignedUserId.HasValue)
            await EnsureMemberAsync(projectId, dto.AssignedUserId.Value,
                "Görev atanacak kullanıcı projenin üyesi değil.");

        var task = new TaskItem
        {
            Title = dto.Title.Trim(),
            Description = dto.Description?.Trim(),
            Priority = dto.Priority,
            DueDate = dto.DueDate,
            ProjectId = projectId,
            AssignedUserId = dto.AssignedUserId
        };

        await _taskRepository.AddAsync(task);
        await _taskRepository.SaveChangesAsync();

        var created = Map((await _taskRepository.GetWithDetailsAsync(task.Id))!);

        // SignalR üzerinden projedeki herkese "yeni görev" bildirimi
        await _notifier.NotifyTaskCreatedAsync(projectId, created);
        return created;
    }

    public async Task<TaskItemDto> UpdateAsync(int taskId, UpdateTaskDto dto, int requestingUserId)
    {
        var task = await GetTaskOrThrowAsync(taskId);
        await EnsureMemberAsync(task.ProjectId, requestingUserId);

        if (string.IsNullOrWhiteSpace(dto.Title))
            throw new BadRequestException("Görev başlığı zorunludur.");

        task.Title = dto.Title.Trim();
        task.Description = dto.Description?.Trim();
        task.Priority = dto.Priority;
        task.DueDate = dto.DueDate;
        task.UpdatedAt = DateTime.UtcNow;

        _taskRepository.Update(task);
        await _taskRepository.SaveChangesAsync();
        return Map(task);
    }

    public async Task<TaskItemDto> UpdateStatusAsync(int taskId, UpdateTaskStatusDto dto, int requestingUserId)
    {
        var task = await GetTaskOrThrowAsync(taskId);
        await EnsureMemberAsync(task.ProjectId, requestingUserId);

        task.Status = dto.Status;
        task.UpdatedAt = DateTime.UtcNow;

        _taskRepository.Update(task);
        await _taskRepository.SaveChangesAsync();

        var updated = Map(task);

        // Kanban board'daki herkese durum değişikliğini anlık bildir
        await _notifier.NotifyTaskStatusChangedAsync(task.ProjectId, updated);
        return updated;
    }

    public async Task<TaskItemDto> AssignAsync(int taskId, AssignTaskDto dto, int requestingUserId)
    {
        var task = await GetTaskOrThrowAsync(taskId);
        await EnsureMemberAsync(task.ProjectId, requestingUserId);

        if (dto.UserId.HasValue)
            await EnsureMemberAsync(task.ProjectId, dto.UserId.Value,
                "Görev atanacak kullanıcı projenin üyesi değil.");

        task.AssignedUserId = dto.UserId;
        task.UpdatedAt = DateTime.UtcNow;

        _taskRepository.Update(task);
        await _taskRepository.SaveChangesAsync();

        return Map((await _taskRepository.GetWithDetailsAsync(taskId))!);
    }

    public async Task DeleteAsync(int taskId, int requestingUserId)
    {
        var task = await GetTaskOrThrowAsync(taskId);

        // İş kuralı: görevi sadece proje sahibi silebilir
        var project = await _projectRepository.GetByIdAsync(task.ProjectId);
        if (project!.OwnerId != requestingUserId)
            throw new ForbiddenException("Görevi sadece proje sahibi silebilir.");

        _taskRepository.Delete(task);
        await _taskRepository.SaveChangesAsync();
    }

    // ---- Yardımcı metotlar ----

    private async Task<TaskItem> GetTaskOrThrowAsync(int taskId) =>
        await _taskRepository.GetWithDetailsAsync(taskId)
            ?? throw new NotFoundException("Görev bulunamadı.");

    private async Task EnsureMemberAsync(int projectId, int userId,
        string message = "Bu projeye erişim yetkiniz yok.")
    {
        if (!await _projectRepository.IsUserMemberAsync(projectId, userId))
            throw new ForbiddenException(message);
    }

    private static TaskItemDto Map(TaskItem t) =>
        new(t.Id, t.Title, t.Description, t.Status.ToString(), t.Priority.ToString(),
            t.DueDate, t.CreatedAt, t.UpdatedAt, t.ProjectId, t.Project?.Name ?? "",
            t.AssignedUser?.FullName, t.AssignedUserId, t.Comments?.Count ?? 0);
}
