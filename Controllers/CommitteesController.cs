using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Qadaya.Data;
using Qadaya.Models;

namespace Qadaya.Controllers
{
    [Authorize]
    public class CommitteesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CommitteesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var committees = await _context.Committees
                .Include(c => c.Members).ThenInclude(m => m.User)
                .Include(c => c.Cases)
                .AsNoTracking()
                .ToListAsync();
            return View(committees);
        }

        public async Task<IActionResult> Details(int id)
        {
            var committee = await _context.Committees
                .Include(c => c.Members).ThenInclude(m => m.User)
                .Include(c => c.Cases)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (committee == null) return NotFound();
            return View(committee);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create() => View(new Committee());

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Committee model)
        {
            if (!ModelState.IsValid) return View(model);

            _context.Committees.Add(model);
            await _context.SaveChangesAsync();
            TempData["Success"] = "تمت إضافة اللجنة بنجاح.";
            return RedirectToAction(nameof(Details), new { id = model.Id });
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var committee = await _context.Committees.FindAsync(id);
            if (committee == null) return NotFound();
            return View(committee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, Committee model)
        {
            if (id != model.Id) return NotFound();
            var existing = await _context.Committees.FindAsync(id);
            if (existing == null) return NotFound();
            if (!ModelState.IsValid) return View(model);

            existing.Name = model.Name;
            existing.Type = model.Type;
            existing.CreatedDate = model.CreatedDate;
            existing.Description = model.Description;

            await _context.SaveChangesAsync();
            TempData["Success"] = "تم تحديث بيانات اللجنة بنجاح.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ManageMembers(int id)
        {
            var committee = await _context.Committees
                .Include(c => c.Members).ThenInclude(m => m.User)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (committee == null) return NotFound();

            var existingUserIds = committee.Members.Select(m => m.UserId).ToHashSet();
            ViewBag.AvailableUsers = await _userManager.Users
                .Where(u => !existingUserIds.Contains(u.Id))
                .OrderBy(u => u.FullName)
                .ToListAsync();

            return View(committee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddMember(int committeeId, string userId, string roleInCommittee)
        {
            var committee = await _context.Committees.FindAsync(committeeId);
            var user = await _userManager.FindByIdAsync(userId);
            if (committee == null || user == null) return NotFound();

            bool already = await _context.CommitteeMembers.AnyAsync(m => m.CommitteeId == committeeId && m.UserId == userId);
            if (!already)
            {
                _context.CommitteeMembers.Add(new CommitteeMember
                {
                    CommitteeId = committeeId,
                    UserId = userId,
                    RoleInCommittee = string.IsNullOrWhiteSpace(roleInCommittee) ? "عضو" : roleInCommittee
                });
                await _context.SaveChangesAsync();
                TempData["Success"] = $"تمت إضافة {user.FullName} للجنة.";
            }

            return RedirectToAction(nameof(ManageMembers), new { id = committeeId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveMember(int memberId, int committeeId)
        {
            var member = await _context.CommitteeMembers.FindAsync(memberId);
            if (member != null)
            {
                _context.CommitteeMembers.Remove(member);
                await _context.SaveChangesAsync();
                TempData["Success"] = "تمت إزالة العضو من اللجنة.";
            }
            return RedirectToAction(nameof(ManageMembers), new { id = committeeId });
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var committee = await _context.Committees.FindAsync(id);
            if (committee == null) return NotFound();
            return View(committee);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var committee = await _context.Committees.FindAsync(id);
            if (committee != null)
            {
                _context.Committees.Remove(committee);
                await _context.SaveChangesAsync();
                TempData["Success"] = "تم حذف اللجنة بنجاح.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
