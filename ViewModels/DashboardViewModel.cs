using Qadaya.Models;

namespace Qadaya.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalCases { get; set; }
        public int OpenCases { get; set; }
        public int ClosedThisMonth { get; set; }
        public int PendingCommitteeDecisions { get; set; }
        public int TotalCommittees { get; set; }
        public int TotalDisciplinaryRecords { get; set; }

        public int NewCount { get; set; }
        public int UnderReviewCount { get; set; }
        public int CommitteeDecisionCount { get; set; }
        public int ClosedCount { get; set; }

        public int DisputeCount { get; set; }
        public int ConsultationCount { get; set; }
        public int ViolationCount { get; set; }
        public int ContractCount { get; set; }

        public List<LegalCase> RecentCases { get; set; } = new();
    }
}
