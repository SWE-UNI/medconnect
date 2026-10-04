using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using MedConnect.Data;
using MedConnect.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace MedConnect.Features.Chat;

[ApiController]
[Route("api/v1/chats")]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ChatController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet("conversations")]
    public async Task<IActionResult> GetConversations()
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(currentUserId)) return Unauthorized();

        // Retrieve messages matching current user
        var messages = await _context.ChatMessages
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .Where(m => m.SenderId == currentUserId || m.ReceiverId == currentUserId)
            .OrderByDescending(m => m.Timestamp)
            .ToListAsync();

        // Group by the other user ID
        var groups = messages.GroupBy(m => m.SenderId == currentUserId ? m.ReceiverId : m.SenderId);

        var conversations = new List<ConversationDto>();

        foreach (var g in groups)
        {
            var otherUserId = g.Key;
            var lastMessage = g.First();
            
            // Get other user details
            var otherUser = lastMessage.SenderId == currentUserId ? lastMessage.Receiver : lastMessage.Sender;
            if (otherUser == null) continue;

            var role = (await _userManager.GetRolesAsync(otherUser)).FirstOrDefault() ?? "User";

            conversations.Add(new ConversationDto(
                otherUserId,
                otherUser.FullName,
                role,
                new ChatMessageDto(
                    lastMessage.ChatMessageId,
                    lastMessage.SenderId,
                    lastMessage.Sender?.FullName ?? "Unknown",
                    lastMessage.ReceiverId,
                    lastMessage.Receiver?.FullName ?? "Unknown",
                    lastMessage.Message,
                    lastMessage.Timestamp,
                    lastMessage.IsUrgent,
                    lastMessage.AttachmentType,
                    lastMessage.AttachmentId
                )
            ));
        }

        return Ok(conversations);
    }

    [HttpGet("history/{otherUserId}")]
    public async Task<IActionResult> GetHistory(string otherUserId)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(currentUserId)) return Unauthorized();

        var messages = await _context.ChatMessages
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .Where(m => (m.SenderId == currentUserId && m.ReceiverId == otherUserId) || 
                        (m.SenderId == otherUserId && m.ReceiverId == currentUserId))
            .OrderBy(m => m.Timestamp)
            .Select(m => new ChatMessageDto(
                m.ChatMessageId,
                m.SenderId,
                m.Sender!.FullName,
                m.ReceiverId,
                m.Receiver!.FullName,
                m.Message,
                m.Timestamp,
                m.IsUrgent,
                m.AttachmentType,
                m.AttachmentId
            ))
            .ToListAsync();

        return Ok(messages);
    }

    [HttpGet("contacts")]
    public async Task<IActionResult> GetContacts()
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(currentUserId)) return Unauthorized();

        // Every other user is a valid contact — messaging isn't role-restricted.
        // Patients can reach staff, and staff can reach each other directly.
        var users = await _context.Users
            .Where(u => u.Id != currentUserId)
            .Select(u => new ContactDto(u.Id, u.FullName, ""))
            .ToListAsync();

        var result = new List<ContactDto>();
        foreach (var u in users)
        {
            var userObj = await _userManager.FindByIdAsync(u.UserId);
            if (userObj != null)
            {
                var role = (await _userManager.GetRolesAsync(userObj)).FirstOrDefault() ?? "User";
                result.Add(u with { Role = role });
            }
        }

        return Ok(result);
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendMessage(SendMessageDto dto)
    {
        var senderId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(senderId)) return Unauthorized();

        var chatMessage = new ChatMessage
        {
            SenderId = senderId,
            ReceiverId = dto.ReceiverId,
            Message = dto.Message,
            Timestamp = DateTime.UtcNow,
            IsUrgent = dto.IsUrgent,
            AttachmentType = dto.AttachmentType,
            AttachmentId = dto.AttachmentId
        };

        _context.ChatMessages.Add(chatMessage);
        await _context.SaveChangesAsync();

        var sender = await _context.Users.FindAsync(senderId);
        var receiver = await _context.Users.FindAsync(dto.ReceiverId);

        var msgDto = new ChatMessageDto(
            chatMessage.ChatMessageId,
            senderId,
            sender?.FullName ?? "Unknown",
            dto.ReceiverId,
            receiver?.FullName ?? "Unknown",
            chatMessage.Message,
            chatMessage.Timestamp,
            chatMessage.IsUrgent,
            chatMessage.AttachmentType,
            chatMessage.AttachmentId
        );

        var hubContext = HttpContext.RequestServices.GetService<Microsoft.AspNetCore.SignalR.IHubContext<MedConnect.Hubs.ChatHub>>();
        if (hubContext != null)
        {
            await hubContext.Clients.Group(MedConnect.Hubs.ChatHub.UserGroup(dto.ReceiverId))
                .SendAsync("ReceiveMessage", senderId, dto.Message, chatMessage.Timestamp, dto.IsUrgent, dto.AttachmentType, dto.AttachmentId);
            await hubContext.Clients.Group(MedConnect.Hubs.ChatHub.UserGroup(senderId))
                .SendAsync("ReceiveMessage", senderId, dto.Message, chatMessage.Timestamp, dto.IsUrgent, dto.AttachmentType, dto.AttachmentId);
        }

        return Ok(msgDto);
    }
}

public record SendMessageDto(string ReceiverId, string Message, bool IsUrgent = false, string? AttachmentType = null, int? AttachmentId = null);
public record ChatMessageDto(int ChatMessageId, string SenderId, string SenderName, string ReceiverId, string ReceiverName, string Message, DateTime Timestamp, bool IsUrgent, string? AttachmentType, int? AttachmentId);
public record ConversationDto(string OtherUserId, string OtherUserName, string OtherUserRole, ChatMessageDto LastMessage);
public record ContactDto(string UserId, string FullName, string Role);
