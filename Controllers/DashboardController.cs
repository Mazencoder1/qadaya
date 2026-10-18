using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Qadaya.Data;
using Qadaya.Models;
using Qadaya.ViewModels;

namespace Qadaya.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var cases = await _context.Cases.AsNoTracking().ToListAsync();
            var now = DateTime.Now;

            var vm = new DashboardViewModel
            {
                TotalCases = cases.Count,
                OpenCases = cases.Count(c => c.Status != CaseStatus.Closed),
                ClosedThisMonth = cases.Count(c => c.Status == CaseStatus.Closed && c.ClosedAt.HasValue && c.ClosedAt.Value.Month == now.Month && c.ClosedAt.Value.Year == now.Year),
                PendingCommitteeDecisions = cases.Count(c => c.Status == CaseStatus.CommitteeDecision),
                TotalCommittees = await _context.Committees.CountAsync(),
                TotalDisciplinaryRecords = await _context.DisciplinaryRecords.CountAsync(),
                NewCount = cases.Count(c => c.Status == CaseStatus.New),
                UnderReviewCount = cases.Count(c => c.Status == CaseStatus.UnderReview),
                CommitteeDecisionCount = cases.Count(c => c.Status == CaseStatus.CommitteeDecision),
                ClosedCount = cases.Count(c => c.Status == CaseStatus.Closed),
                RecentCases = cases.OrderByDescending(c => c.CreatedAt).Take(6).ToList(),
                DisputeCount = cases.Count(c => c.CaseType == CaseType.Dispute),
                ConsultationCount = cases.Count(c => c.CaseType == CaseType.LegalConsultation),
                ViolationCount = cases.Count(c => c.CaseType == CaseType.Violation),
                ContractCount = cases.Count(c => c.CaseType == CaseType.Contract)
            };

            return View(vm);
        }
    }
}
