using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using MedConnect.Domain;
using MedConnect.Data;

namespace MedConnect.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly ApplicationDbContext _dbContext;

    public ChatHub(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, UserGroup(userId));
        }
        await base.OnConnectedAsync();
    }

    public async Task SendMessage(string receiverId, string message, bool isUrgent, string? attachmentType, int? attachmentId)
    {
        var senderId = Context.UserIdentifier;
        if (string.IsNullOrEmpty(senderId)) return;

        // Save to database
        var chatMessage = new ChatMessage
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            Message = message,
            Timestamp = DateTime.UtcNow,
            IsUrgent = isUrgent,
            AttachmentType = attachmentType,
            AttachmentId = attachmentId
        };

        _dbContext.ChatMessages.Add(chatMessage);
        await _dbContext.SaveChangesAsync();

        // Broadcast to receiver group and sender group (for multi-device sync)
        await Clients.Group(UserGroup(receiverId)).SendAsync("ReceiveMessage", senderId, message, chatMessage.Timestamp, isUrgent, attachmentType, attachmentId);
        await Clients.Group(UserGroup(senderId)).SendAsync("ReceiveMessage", senderId, message, chatMessage.Timestamp, isUrgent, attachmentType, attachmentId);
    }

    public static string UserGroup(string userId) => $"user-{userId}";
}
