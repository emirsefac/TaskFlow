using TaskFlow.Core.DTOs;

namespace TaskFlow.Core.Interfaces;

// Core katmanı SignalR'ı doğrudan bilmemeli (katman bağımlılığını tersine çevirmek için).
// Bu arayüzü Infrastructure/API katmanında SignalR Hub kullanan bir sınıf implemente edecek.
public interface ITaskNotifier
{
    Task NotifyTaskStatusChangedAsync(int projectId, TaskItemDto task);
    Task NotifyTaskCreatedAsync(int projectId, TaskItemDto task);
}
