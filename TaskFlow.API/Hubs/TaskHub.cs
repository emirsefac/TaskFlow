using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace TaskFlow.API.Hubs;

// İstemciler (frontend) bu hub'a bağlanıp proje bazlı gruplara katılır.
// Bir görev güncellendiğinde sadece o projenin üyelerine bildirim gider.
[Authorize]
public class TaskHub : Hub
{
    public async Task JoinProject(int projectId) =>
        await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(projectId));

    public async Task LeaveProject(int projectId) =>
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(projectId));

    public static string GroupName(int projectId) => $"project-{projectId}";
}
