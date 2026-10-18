namespace Qadaya.Models
{
    public class CaseHistory
    {
        public int Id { get; set; }

        public int CaseId { get; set; }
        public LegalCase? Case { get; set; }

        public CaseStatus OldStatus { get; set; }
        public CaseStatus NewStatus { get; set; }

        public string ChangedByUserId { get; set; } = string.Empty;
        public string ChangedByName { get; set; } = string.Empty;

        public DateTime ChangedAt { get; set; } = DateTime.Now;

        public string? Note { get; set; }
    }
}
