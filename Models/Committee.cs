using System.ComponentModel.DataAnnotations;

namespace Qadaya.Models
{
    public class Committee
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم اللجنة مطلوب")]
        [Display(Name = "اسم اللجنة")]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "نوع اللجنة مطلوب")]
        [Display(Name = "نوع اللجنة")]
        [StringLength(150)]
        public string Type { get; set; } = string.Empty;

        [Display(Name = "تاريخ التأسيس")]
        [DataType(DataType.Date)]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Display(Name = "وصف مختصر")]
        public string? Description { get; set; }

        public ICollection<CommitteeMember> Members { get; set; } = new List<CommitteeMember>();
        public ICollection<LegalCase> Cases { get; set; } = new List<LegalCase>();
    }

    public class CommitteeMember
    {
        public int Id { get; set; }
        public int CommitteeId { get; set; }
        public Committee? Committee { get; set; }

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        [Display(Name = "الدور في اللجنة")]
        [StringLength(100)]
        public string RoleInCommittee { get; set; } = "عضو";
    }
}
