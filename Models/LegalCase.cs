using System.ComponentModel.DataAnnotations;

namespace Qadaya.Models
{
    public class LegalCase
    {
        public int Id { get; set; }

        [Display(Name = "رقم القضية")]
        public string CaseNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "عنوان القضية مطلوب")]
        [Display(Name = "عنوان القضية")]
        [StringLength(250)]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "نوع القضية")]
        public CaseType CaseType { get; set; }

        [Display(Name = "الحالة")]
        public CaseStatus Status { get; set; } = CaseStatus.New;

        [Required(ErrorMessage = "اسم الموظف المرتبط مطلوب")]
        [Display(Name = "الموظف المرتبط")]
        [StringLength(150)]
        public string LinkedEmployeeName { get; set; } = string.Empty;

        [Display(Name = "الإدارة / القسم")]
        [StringLength(150)]
        public string Department { get; set; } = string.Empty;

        [Display(Name = "تاريخ الفتح")]
        [DataType(DataType.Date)]
        public DateTime OpenedDate { get; set; } = DateTime.Now;

        [Display(Name = "اللجنة المكلفة")]
        public int? CommitteeId { get; set; }
        public Committee? Committee { get; set; }

        [Display(Name = "وصف القضية")]
        [DataType(DataType.MultilineText)]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? ClosedAt { get; set; }

        public ICollection<CaseHistory> HistoryEntries { get; set; } = new List<CaseHistory>();
        public ICollection<DisciplinaryRecord> DisciplinaryRecords { get; set; } = new List<DisciplinaryRecord>();
    }
}
