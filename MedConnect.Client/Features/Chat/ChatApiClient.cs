using MedConnect.Client.Services;

namespace MedConnect.Client.Features.Chat;

public class ChatApiClient(HttpClient http)
{
    public Task<List<ConversationDto>> GetConversationsAsync() =>
        SafeApi.GetListAsync<ConversationDto>(http, "api/v1/chats/conversations");

    public Task<List<ChatMessageDto>> GetHistoryAsync(string otherUserId) =>
        SafeApi.GetListAsync<ChatMessageDto>(http, $"api/v1/chats/history/{otherUserId}");

    public Task<List<ContactDto>> GetContactsAsync() =>
        SafeApi.GetListAsync<ContactDto>(http, "api/v1/chats/contacts");

    public Task<ApiResult<ChatMessageDto>> SendMessageAsync(SendMessageDto dto) =>
        SafeApi.PostAsync<SendMessageDto, ChatMessageDto>(http, "api/v1/chats/send", dto);
}

public record SendMessageDto(string ReceiverId, string Message, bool IsUrgent = false, string? AttachmentType = null, int? AttachmentId = null);
public record ChatMessageDto(int ChatMessageId, string SenderId, string SenderName, string ReceiverId, string ReceiverName, string Message, DateTime Timestamp, bool IsUrgent, string? AttachmentType, int? AttachmentId);
public record ConversationDto(string OtherUserId, string OtherUserName, string OtherUserRole, ChatMessageDto LastMessage);
public record ContactDto(string UserId, string FullName, string Role);
