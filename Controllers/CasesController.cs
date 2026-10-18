using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Qadaya.Data;
using Qadaya.Models;

namespace Qadaya.Controllers
{
    [Authorize]
    public class CasesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CasesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Committees = await _context.Committees.AsNoTracking().ToListAsync();
            return View();
        }

        // AJAX live search/filter endpoint - returns partial HTML table rows
        [HttpGet]
        public async Task<IActionResult> Search(string? term, string? status, string? type)
        {
            var query = _context.Cases.Include(c => c.Committee).AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(term))
            {
                term = term.Trim();
                query = query.Where(c => c.CaseNumber.Contains(term) || c.Title.Contains(term) || c.LinkedEmployeeName.Contains(term));
            }

            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<CaseStatus>(status, out var statusEnum))
            {
                query = query.Where(c => c.Status == statusEnum);
            }

            if (!string.IsNullOrWhiteSpace(type) && Enum.TryParse<CaseType>(type, out var typeEnum))
            {
                query = query.Where(c => c.CaseType == typeEnum);
            }

            var results = await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
            return PartialView("_CasesTable", results);
        }

        public async Task<IActionResult> Details(int id)
        {
            var legalCase = await _context.Cases
                .Include(c => c.Committee)
                .Include(c => c.HistoryEntries)
                .Include(c => c.DisciplinaryRecords)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (legalCase == null) return NotFound();

            legalCase.HistoryEntries = legalCase.HistoryEntries.OrderBy(h => h.ChangedAt).ToList();
            return View(legalCase);
        }

        [Authorize(Roles = "Admin,LegalReviewer")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Committees = await _context.Committees.AsNoTracking().ToListAsync();
            return View(new LegalCase { OpenedDate = DateTime.Now });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,LegalReviewer")]
        public async Task<IActionResult> Create(LegalCase model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Committees = await _context.Committees.AsNoTracking().ToListAsync();
                return View(model);
            }

            var year = DateTime.Now.Year;
            var countThisYear = await _context.Cases.CountAsync(c => c.CaseNumber.StartsWith($"C-{year}-"));
            model.CaseNumber = $"C-{year}-{(countThisYear + 1):D4}";
            model.CreatedAt = DateTime.Now;
            model.Status = CaseStatus.New;

            _context.Cases.Add(model);
            await _context.SaveChangesAsync();

            var user = await _userManager.GetUserAsync(User);
            _context.CaseHistories.Add(new CaseHistory
            {
                CaseId = model.Id,
                OldStatus = CaseStatus.New,
                NewStatus = CaseStatus.New,
                ChangedByUserId = user?.Id ?? "",
                ChangedByName = user?.FullName ?? User.Identity?.Name ?? "النظام",
                ChangedAt = DateTime.Now,
                Note = "تم إنشاء القضية."
            });
            await _context.SaveChangesAsync();

            TempData["Success"] = $"تمت إضافة القضية رقم {model.CaseNumber} بنجاح.";
            return RedirectToAction(nameof(Details), new { id = model.Id });
        }

        [Authorize(Roles = "Admin,LegalReviewer")]
        public async Task<IActionResult> Edit(int id)
        {
            var legalCase = await _context.Cases.FindAsync(id);
            if (legalCase == null) return NotFound();

            ViewBag.Committees = await _context.Committees.AsNoTracking().ToListAsync();
            return View(legalCase);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,LegalReviewer")]
        public async Task<IActionResult> Edit(int id, LegalCase model)
        {
            if (id != model.Id) return NotFound();

            var existing = await _context.Cases.FindAsync(id);
            if (existing == null) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.Committees = await _context.Committees.AsNoTracking().ToListAsync();
                return View(model);
            }

            existing.Title = model.Title;
            existing.CaseType = model.CaseType;
            existing.LinkedEmployeeName = model.LinkedEmployeeName;
            existing.Department = model.Department;
            existing.OpenedDate = model.OpenedDate;
            existing.CommitteeId = model.CommitteeId;
            existing.Description = model.Description;

            await _context.SaveChangesAsync();
            TempData["Success"] = "تم تحديث بيانات القضية بنجاح.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,LegalReviewer")]
        public async Task<IActionResult> ChangeStatus(int id, CaseStatus newStatus, string? note)
        {
            var legalCase = await _context.Cases.FindAsync(id);
            if (legalCase == null) return NotFound();

            var oldStatus = legalCase.Status;
            legalCase.Status = newStatus;
            if (newStatus == CaseStatus.Closed)
            {
                legalCase.ClosedAt = DateTime.Now;
            }
            else
            {
                legalCase.ClosedAt = null;
            }

            var user = await _userManager.GetUserAsync(User);
            _context.CaseHistories.Add(new CaseHistory
            {
                CaseId = id,
                OldStatus = oldStatus,
                NewStatus = newStatus,
                ChangedByUserId = user?.Id ?? "",
                ChangedByName = user?.FullName ?? User.Identity?.Name ?? "النظام",
                ChangedAt = DateTime.Now,
                Note = string.IsNullOrWhiteSpace(note) ? $"تم تغيير الحالة من {oldStatus.ToArabic()} إلى {newStatus.ToArabic()}." : note
            });

            await _context.SaveChangesAsync();
            TempData["Success"] = "تم تحديث حالة القضية بنجاح.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var legalCase = await _context.Cases.Include(c => c.Committee).FirstOrDefaultAsync(c => c.Id == id);
            if (legalCase == null) return NotFound();
            return View(legalCase);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var legalCase = await _context.Cases.FindAsync(id);
            if (legalCase != null)
            {
                _context.Cases.Remove(legalCase);
                await _context.SaveChangesAsync();
                TempData["Success"] = "تم حذف القضية بنجاح.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
