using Pathify.Models;

public class ProjectProposal
{
    public int ProposalId { get; set; }

    public int TeamId { get; set; }
    public virtual Team Team { get; set; } = null!;

    public string ProjectName { get; set; } = null!;
    public string ProjectDescription { get; set; } = null!;

    public string Status { get; set; } = "PendingProfessors";

    public string InternalApproval { get; set; } = "Pending";
    public string ExternalApproval { get; set; } = "Pending";

    public string? RejectionReason { get; set; }          // تفضل موجودة لحالة رفض السوبر أدمن
    public string? InternalRejectionReason { get; set; }  // ✅ جديد
    public string? ExternalRejectionReason { get; set; }  // ✅ جديد
    public string? RejectedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? ReviewedAt { get; set; }
}