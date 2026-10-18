using System.ComponentModel.DataAnnotations;

namespace Qadaya.Models
{
    public class DisciplinaryRecord
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم الموظف مطلوب")]
        [Display(Name = "اسم الموظف")]
        [StringLength(150)]
        public string EmployeeName { get; set; } = string.Empty;

        [Required(ErrorMessage = "نوع المخالفة مطلوب")]
        [Display(Name = "نوع المخالفة")]
        [StringLength(200)]
        public string ViolationType { get; set; } = string.Empty;

        [Required(ErrorMessage = "القرار الصادر مطلوب")]
        [Display(Name = "القرار الصادر")]
        [StringLength(300)]
        public string Decision { get; set; } = string.Empty;

        [Display(Name = "تاريخ القرار")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; } = DateTime.Now;

        [Display(Name = "القضية المرتبطة")]
        public int? CaseId { get; set; }
        public LegalCase? Case { get; set; }

        [Display(Name = "اللجنة المرتبطة")]
        public int? CommitteeId { get; set; }
        public Committee? Committee { get; set; }

        [Display(Name = "ملاحظات")]
        public string? Notes { get; set; }
    }
}
