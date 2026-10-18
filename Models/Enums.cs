using System.ComponentModel.DataAnnotations;

namespace Qadaya.Models
{
    public enum CaseType
    {
        [Display(Name = "منازعة")]
        Dispute = 1,

        [Display(Name = "استشارة قانونية")]
        LegalConsultation = 2,

        [Display(Name = "مخالفة")]
        Violation = 3,

        [Display(Name = "عقد")]
        Contract = 4
    }

    public enum CaseStatus
    {
        [Display(Name = "جديدة")]
        New = 1,

        [Display(Name = "قيد المراجعة")]
        UnderReview = 2,

        [Display(Name = "قرار اللجنة")]
        CommitteeDecision = 3,

        [Display(Name = "مغلقة")]
        Closed = 4
    }

    public static class EnumDisplayExtensions
    {
        public static string ToArabic(this CaseType type) => type switch
        {
            CaseType.Dispute => "منازعة",
            CaseType.LegalConsultation => "استشارة قانونية",
            CaseType.Violation => "مخالفة",
            CaseType.Contract => "عقد",
            _ => type.ToString()
        };

        public static string ToArabic(this CaseStatus status) => status switch
        {
            CaseStatus.New => "جديدة",
            CaseStatus.UnderReview => "قيد المراجعة",
            CaseStatus.CommitteeDecision => "قرار اللجنة",
            CaseStatus.Closed => "مغلقة",
            _ => status.ToString()
        };

        public static string BadgeClass(this CaseStatus status) => status switch
        {
            CaseStatus.New => "badge-status-new",
            CaseStatus.UnderReview => "badge-status-review",
            CaseStatus.CommitteeDecision => "badge-status-decision",
            CaseStatus.Closed => "badge-status-closed",
            _ => "badge-secondary"
        };
    }
}
