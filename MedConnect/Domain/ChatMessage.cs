using System;

namespace MedConnect.Domain;

public class ChatMessage
{
    public int ChatMessageId { get; set; }
    
    public required string SenderId { get; set; }
    public ApplicationUser? Sender { get; set; }

    public required string ReceiverId { get; set; }
    public ApplicationUser? Receiver { get; set; }

    public required string Message { get; set; }
    public DateTime Timestamp { get; set; }
    public bool IsUrgent { get; set; }

    public string? AttachmentType { get; set; } // e.g. "Prescription", "Referral"
    public int? AttachmentId { get; set; }
}
