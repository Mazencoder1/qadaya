using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Qadaya.Models;

namespace Qadaya.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            await context.Database.MigrateAsync();

            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            string[] roles = { "Admin", "LegalReviewer", "Employee" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            var admin = await EnsureUserAsync(userManager, "admin@qadaya.sa", "Admin@12345", "عبدالله المطيري", "الإدارة القانونية", "مدير النظام", "Admin");
            var reviewer = await EnsureUserAsync(userManager, "reviewer@qadaya.sa", "Reviewer@12345", "سارة العتيبي", "الشؤون القانونية", "مراجع قانوني", "LegalReviewer");
            var employee = await EnsureUserAsync(userManager, "employee@qadaya.sa", "Employee@12345", "خالد الدوسري", "الموارد البشرية", "موظف", "Employee");

            // extra committee members pool
            var member1 = await EnsureUserAsync(userManager, "member1@qadaya.sa", "Member@12345", "نورة الشهري", "الشؤون القانونية", "عضو لجنة", "LegalReviewer");
            var member2 = await EnsureUserAsync(userManager, "member2@qadaya.sa", "Member@12345", "فهد القرني", "الموارد البشرية", "عضو لجنة", "Employee");

            if (!await context.Committees.AnyAsync())
            {
                var committee1 = new Committee
                {
                    Name = "اللجنة القانونية الدائمة",
                    Type = "لجنة قانونية",
                    CreatedDate = DateTime.Now.AddMonths(-14),
                    Description = "تختص بالنظر في المنازعات والقضايا القانونية التي تخص البلدية."
                };
                var committee2 = new Committee
                {
                    Name = "لجنة المخالفات والتأديب",
                    Type = "لجنة تأديبية",
                    CreatedDate = DateTime.Now.AddMonths(-10),
                    Description = "تتولى دراسة المخالفات الوظيفية واتخاذ القرارات التأديبية المناسبة."
                };
                var committee3 = new Committee
                {
                    Name = "لجنة العقود والاستشارات",
                    Type = "لجنة استشارية",
                    CreatedDate = DateTime.Now.AddMonths(-6),
                    Description = "تقدم الاستشارات القانونية وتراجع العقود والاتفاقيات."
                };

                context.Committees.AddRange(committee1, committee2, committee3);
                await context.SaveChangesAsync();

                context.CommitteeMembers.AddRange(
                    new CommitteeMember { CommitteeId = committee1.Id, UserId = admin.Id, RoleInCommittee = "رئيس اللجنة" },
                    new CommitteeMember { CommitteeId = committee1.Id, UserId = reviewer.Id, RoleInCommittee = "عضو" },
                    new CommitteeMember { CommitteeId = committee1.Id, UserId = member1.Id, RoleInCommittee = "عضو" },

                    new CommitteeMember { CommitteeId = committee2.Id, UserId = reviewer.Id, RoleInCommittee = "رئيس اللجنة" },
                    new CommitteeMember { CommitteeId = committee2.Id, UserId = member2.Id, RoleInCommittee = "عضو" },

                    new CommitteeMember { CommitteeId = committee3.Id, UserId = admin.Id, RoleInCommittee = "عضو" },
                    new CommitteeMember { CommitteeId = committee3.Id, UserId = member1.Id, RoleInCommittee = "رئيس اللجنة" }
                );
                await context.SaveChangesAsync();

                if (!await context.Cases.AnyAsync())
                {
                    var sample = new List<(string Title, CaseType Type, CaseStatus Status, string Employee, string Dept, int DaysAgo, int? CommitteeId, string Desc)>
                    {
                        ("نزاع على مستحقات مالية مع مقاول توريد أثاث مكتبي", CaseType.Dispute, CaseStatus.UnderReview, "محمد الحارثي", "إدارة المشتريات", 45, committee1.Id, "مطالبة مالية من مقاول بشأن فروقات في كمية التوريد المتفق عليها."),
                        ("استشارة قانونية بخصوص تحديث نظام ساعات العمل", CaseType.LegalConsultation, CaseStatus.New, "منيرة الزهراني", "الموارد البشرية", 3, null, "طلب رأي قانوني حول مدى توافق تعديل ساعات العمل مع الأنظمة المعمول بها."),
                        ("مخالفة تأخر متكرر عن مواعيد الدوام الرسمي", CaseType.Violation, CaseStatus.CommitteeDecision, "تركي السبيعي", "إدارة المرافق", 60, committee2.Id, "رصد تأخر متكرر خلال الشهرين الماضيين دون إشعار مسبق."),
                        ("منازعة إيجارية حول عقد استئجار موقف عام", CaseType.Dispute, CaseStatus.Closed, "عبدالعزيز الشمري", "إدارة الأصول", 120, committee1.Id, "نزاع بين البلدية والمستأجر حول شروط تجديد عقد الإيجار."),
                        ("مراجعة عقد صيانة الأنظمة الذكية للإنارة", CaseType.Contract, CaseStatus.UnderReview, "ريم القحطاني", "إدارة تقنية المعلومات", 10, committee3.Id, "مراجعة بنود عقد الصيانة السنوي للتأكد من توافقه مع معايير الجودة."),
                        ("مخالفة إفشاء معلومات داخلية لجهة خارجية", CaseType.Violation, CaseStatus.UnderReview, "سلطان العنزي", "إدارة العلاقات العامة", 20, committee2.Id, "بلاغ عن تسريب معلومات تخص مشروعاً داخلياً قبل اعتماده رسمياً."),
                        ("استشارة قانونية حول تملك أرض لمشروع بلدي", CaseType.LegalConsultation, CaseStatus.Closed, "لطيفة البقمي", "إدارة التخطيط العمراني", 200, committee3.Id, "استشارة بخصوص الإجراءات النظامية لنقل ملكية أرض لصالح مشروع تنموي."),
                        ("منازعة مع شركة نقل بخصوص فسخ عقد خدمات", CaseType.Dispute, CaseStatus.New, "ناصر الدوسري", "إدارة النقل", 2, null, "الشركة تطالب بتعويض عن فسخ العقد قبل انتهاء مدته."),
                        ("مخالفة استخدام مركبات الإدارة لأغراض شخصية", CaseType.Violation, CaseStatus.CommitteeDecision, "فيصل المالكي", "إدارة المرافق", 35, committee2.Id, "ورد بلاغ من المراقبة الميدانية حول استخدام غير نظامي لمركبة رسمية."),
                        ("عقد تشغيل وصيانة الحدائق العامة للعام الجديد", CaseType.Contract, CaseStatus.UnderReview, "هند العسيري", "إدارة البيئة", 8, committee3.Id, "مراجعة الشروط الجزائية وآلية التجديد في عقد التشغيل الجديد."),
                        ("استشارة حول نظام حماية المستهلك في الأسواق الشعبية", CaseType.LegalConsultation, CaseStatus.UnderReview, "بدر الحربي", "إدارة الأسواق", 5, committee1.Id, "طلب توضيح الالتزامات النظامية على الباعة في الأسواق الشعبية."),
                        ("منازعة تعويض عن أضرار حفريات مشروع صرف صحي", CaseType.Dispute, CaseStatus.Closed, "وليد الغامدي", "إدارة البنية التحتية", 150, committee1.Id, "مطالبة أحد الملاك بتعويض عن أضرار لحقت بواجهة عقاره.")
                    };

                    int counter = 1;
                    int year = DateTime.Now.Year;
                    foreach (var s in sample)
                    {
                        var opened = DateTime.Now.AddDays(-s.DaysAgo);
                        var legalCase = new LegalCase
                        {
                            CaseNumber = $"C-{year}-{counter:D4}",
                            Title = s.Title,
                            CaseType = s.Type,
                            Status = s.Status,
                            LinkedEmployeeName = s.Employee,
                            Department = s.Dept,
                            OpenedDate = opened,
                            CommitteeId = s.CommitteeId,
                            Description = s.Desc,
                            CreatedAt = opened,
                            ClosedAt = s.Status == CaseStatus.Closed ? opened.AddDays(30) : null
                        };
                        context.Cases.Add(legalCase);
                        counter++;
                    }
                    await context.SaveChangesAsync();

                    // audit trail history for seeded cases
                    foreach (var legalCase in context.Cases.ToList())
                    {
                        var history = new List<CaseHistory>
                        {
                            new CaseHistory
                            {
                                CaseId = legalCase.Id,
                                OldStatus = CaseStatus.New,
                                NewStatus = CaseStatus.New,
                                ChangedByUserId = admin.Id,
                                ChangedByName = admin.FullName,
                                ChangedAt = legalCase.OpenedDate,
                                Note = "تم فتح القضية وتسجيلها في النظام."
                            }
                        };

                        if (legalCase.Status is CaseStatus.UnderReview or CaseStatus.CommitteeDecision or CaseStatus.Closed)
                        {
                            history.Add(new CaseHistory
                            {
                                CaseId = legalCase.Id,
                                OldStatus = CaseStatus.New,
                                NewStatus = CaseStatus.UnderReview,
                                ChangedByUserId = reviewer.Id,
                                ChangedByName = reviewer.FullName,
                                ChangedAt = legalCase.OpenedDate.AddDays(3),
                                Note = "جاري مراجعة أوراق القضية من قبل المراجع القانوني."
                            });
                        }

                        if (legalCase.Status is CaseStatus.CommitteeDecision or CaseStatus.Closed)
                        {
                            history.Add(new CaseHistory
                            {
                                CaseId = legalCase.Id,
                                OldStatus = CaseStatus.UnderReview,
                                NewStatus = CaseStatus.CommitteeDecision,
                                ChangedByUserId = reviewer.Id,
                                ChangedByName = reviewer.FullName,
                                ChangedAt = legalCase.OpenedDate.AddDays(12),
                                Note = "تم رفع القضية للجنة المختصة لاتخاذ القرار المناسب."
                            });
                        }

                        if (legalCase.Status == CaseStatus.Closed)
                        {
                            history.Add(new CaseHistory
                            {
                                CaseId = legalCase.Id,
                                OldStatus = CaseStatus.CommitteeDecision,
                                NewStatus = CaseStatus.Closed,
                                ChangedByUserId = admin.Id,
                                ChangedByName = admin.FullName,
                                ChangedAt = legalCase.ClosedAt ?? legalCase.OpenedDate.AddDays(30),
                                Note = "تم إغلاق القضية بعد تنفيذ القرار الصادر بشأنها."
                            });
                        }

                        context.CaseHistories.AddRange(history);
                    }
                    await context.SaveChangesAsync();
                }
            }

            if (!await context.DisciplinaryRecords.AnyAsync())
            {
                var violationCase = await context.Cases.FirstOrDefaultAsync(c => c.CaseType == CaseType.Violation);
                var committees = await context.Committees.ToListAsync();

                context.DisciplinaryRecords.AddRange(
                    new DisciplinaryRecord
                    {
                        EmployeeName = "تركي السبيعي",
                        ViolationType = "تأخر متكرر عن الدوام",
                        Decision = "لفت نظر مع خصم يوم من الراتب",
                        Date = DateTime.Now.AddDays(-55),
                        CaseId = violationCase?.Id,
                        CommitteeId = committees.FirstOrDefault(c => c.Type == "لجنة تأديبية")?.Id,
                        Notes = "تم التعهد بعدم التكرار من الموظف."
                    },
                    new DisciplinaryRecord
                    {
                        EmployeeName = "سلطان العنزي",
                        ViolationType = "إفشاء معلومات داخلية",
                        Decision = "إنذار كتابي وتحويل القضية للجنة المختصة",
                        Date = DateTime.Now.AddDays(-18),
                        CaseId = (await context.Cases.FirstOrDefaultAsync(c => c.LinkedEmployeeName == "سلطان العنزي"))?.Id,
                        CommitteeId = committees.FirstOrDefault(c => c.Type == "لجنة تأديبية")?.Id,
                        Notes = "القضية قيد المراجعة من الجهات المختصة."
                    },
                    new DisciplinaryRecord
                    {
                        EmployeeName = "فيصل المالكي",
                        ViolationType = "استخدام مركبة رسمية لأغراض شخصية",
                        Decision = "خصم ثلاثة أيام من الراتب",
                        Date = DateTime.Now.AddDays(-30),
                        CaseId = (await context.Cases.FirstOrDefaultAsync(c => c.LinkedEmployeeName == "فيصل المالكي"))?.Id,
                        CommitteeId = committees.FirstOrDefault(c => c.Type == "لجنة تأديبية")?.Id,
                        Notes = "تم اعتماد القرار من رئيس اللجنة."
                    }
                );
                await context.SaveChangesAsync();
            }
        }

        private static async Task<ApplicationUser> EnsureUserAsync(
            UserManager<ApplicationUser> userManager,
            string email,
            string password,
            string fullName,
            string department,
            string jobTitle,
            string role)
        {
            var existing = await userManager.FindByEmailAsync(email);
            if (existing != null)
            {
                return existing;
            }

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FullName = fullName,
                Department = department,
                JobTitle = jobTitle
            };

            var result = await userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, role);
            }
            else
            {
                throw new Exception($"فشل إنشاء المستخدم {email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            return user;
        }
    }
}
