using Microsoft.AspNetCore.SignalR;
using TaskFlow.API.Hubs;
using TaskFlow.Core.DTOs;
using TaskFlow.Core.Interfaces;

namespace TaskFlow.API.Services;

// Core'daki ITaskNotifier soyutlamasını SignalR ile hayata geçiren sınıf.
// Böylece iş mantığı katmanı SignalR'ı hiç bilmeden bildirim gönderebiliyor.
public class SignalRTaskNotifier : ITaskNotifier
{
    private readonly IHubContext<TaskHub> _hubContext;

    public SignalRTaskNotifier(IHubContext<TaskHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task NotifyTaskStatusChangedAsync(int projectId, TaskItemDto task) =>
        _hubContext.Clients.Group(TaskHub.GroupName(projectId))
            .SendAsync("TaskStatusChanged", task);

    public Task NotifyTaskCreatedAsync(int projectId, TaskItemDto task) =>
        _hubContext.Clients.Group(TaskHub.GroupName(projectId))
            .SendAsync("TaskCreated", task);
}
