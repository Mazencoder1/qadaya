using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Qadaya.Data;
using Qadaya.Models;

namespace Qadaya.Controllers
{
    [Authorize(Roles = "Admin,LegalReviewer")]
    public class DisciplinaryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DisciplinaryController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var records = await _context.DisciplinaryRecords
                .Include(d => d.Case)
                .Include(d => d.Committee)
                .OrderByDescending(d => d.Date)
                .AsNoTracking()
                .ToListAsync();
            return View(records);
        }

        public async Task<IActionResult> Create()
        {
            await PopulateDropdowns();
            return View(new DisciplinaryRecord { Date = DateTime.Now });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DisciplinaryRecord model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdowns();
                return View(model);
            }

            _context.DisciplinaryRecords.Add(model);
            await _context.SaveChangesAsync();
            TempData["Success"] = "تمت إضافة السجل التأديبي بنجاح.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var record = await _context.DisciplinaryRecords.FindAsync(id);
            if (record == null) return NotFound();
            await PopulateDropdowns();
            return View(record);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DisciplinaryRecord model)
        {
            if (id != model.Id) return NotFound();
            var existing = await _context.DisciplinaryRecords.FindAsync(id);
            if (existing == null) return NotFound();
            if (!ModelState.IsValid)
            {
                await PopulateDropdowns();
                return View(model);
            }

            existing.EmployeeName = model.EmployeeName;
            existing.ViolationType = model.ViolationType;
            existing.Decision = model.Decision;
            existing.Date = model.Date;
            existing.CaseId = model.CaseId;
            existing.CommitteeId = model.CommitteeId;
            existing.Notes = model.Notes;

            await _context.SaveChangesAsync();
            TempData["Success"] = "تم تحديث السجل التأديبي بنجاح.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var record = await _context.DisciplinaryRecords
                .Include(d => d.Case).Include(d => d.Committee)
                .FirstOrDefaultAsync(d => d.Id == id);
            if (record == null) return NotFound();
            return View(record);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var record = await _context.DisciplinaryRecords.FindAsync(id);
            if (record != null)
            {
                _context.DisciplinaryRecords.Remove(record);
                await _context.SaveChangesAsync();
                TempData["Success"] = "تم حذف السجل بنجاح.";
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateDropdowns()
        {
            ViewBag.Cases = await _context.Cases.AsNoTracking().OrderByDescending(c => c.CreatedAt).ToListAsync();
            ViewBag.Committees = await _context.Committees.AsNoTracking().ToListAsync();
        }
    }
}
