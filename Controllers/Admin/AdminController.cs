using EquipLink.ApplicationDbContext;
using EquipLink.ViewModels.AdminVMs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EquipLink.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly EquipmentDbContext _dbContext;

        public AdminController(EquipmentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            // Total Providers
            var totalProviders = await _dbContext.Users
                .Where(u => u.UserType == "Provider")
                .CountAsync();

            // Active Users (all types)
            var activeUsers = await _dbContext.Users
                .Where(u => u.UserIsActive == 1)
                .CountAsync();

            // Pending Requests (assuming Maintenance with Pending status)
            var pendingRequests = await _dbContext.Maintenances
                .Where(m => m.MainStatus == "Pending")
                .CountAsync();

            // Revenue (last month, assuming sum of Order totals)
            var lastMonth = DateTime.Now.AddMonths(-1);
            var totalRevenue = await _dbContext.Orders
                .Where(o => o.OrdCreatedDate >= lastMonth)
                .SumAsync(o => o.OrdTotalPrice);

            // Recent Providers (last 30 days)
            var thirtyDaysAgo = DateTime.Now.AddDays(-30);
            var recentProviders = await _dbContext.Users
                .Where(u => u.UserType == "Provider" && u.UserCreatedDate >= thirtyDaysAgo)
                .OrderByDescending(u => u.UserCreatedDate)
                .Take(5)
                .ToListAsync();

            // Provider Activity (last 6 months)
            var providerActivity = new List<int>();
            for (int i = 5; i >= 0; i--)
            {
                var targetMonth = DateTime.Now.AddMonths(-i);
                var count = await _dbContext.Users
                    .Where(u => u.UserType == "Provider" &&
                               u.UserCreatedDate.HasValue &&
                               u.UserCreatedDate.Value.Year == targetMonth.Year &&
                               u.UserCreatedDate.Value.Month == targetMonth.Month)
                    .CountAsync();
                providerActivity.Add(count);
            }

            // User Type Distribution
            var userTypeDistribution = new List<int>
            {
                await _dbContext.Users.Where(u => u.UserType == "Provider").CountAsync(),
                await _dbContext.Users.Where(u => u.UserType == "Customer").CountAsync(),
                await _dbContext.Users.Where(u => u.UserType == "Admin").CountAsync()
            };

            var viewModel = new AdminDashboardViewModel
            {
                TotalProviders = totalProviders,
                ActiveUsers = activeUsers,
                PendingRequests = pendingRequests,
                TotalRevenue = totalRevenue,
                RecentProviders = recentProviders,
                ProviderActivity = providerActivity,
                UserTypeDistribution = userTypeDistribution
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Users()
        {
            var users = await _dbContext.Users
                .Where(u => u.UserType != "Admin")
                .ToListAsync();

            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleUserActivation(int userId)
        {
            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            user.UserIsActive = (short)(user.UserIsActive == 1 ? 0 : 1);
            await _dbContext.SaveChangesAsync();

            TempData["Success"] = "User activation status updated successfully!";
            return RedirectToAction(nameof(Users));
        }

        [HttpGet]
        public async Task<IActionResult> Orders()
        {
            var orders = await _dbContext.Orders
                .Include(o => o.Customer)
                .Include(o => o.Orderequipments)
                    .ThenInclude(oe => oe.Equ)
                .ToListAsync();

            return View(orders);
        }

        [HttpGet]
        public async Task<IActionResult> ShowOrder(int orderId)
        {
            var order = await _dbContext.Orders
                .Include(o => o.Customer)
                .Include(o => o.Orderequipments)
                    .ThenInclude(oe => oe.Equ)
                .FirstOrDefaultAsync(o => o.OrdId == orderId);

            if (order == null)
            {
                return NotFound();
            }

            return View("Invoice", order);
        }

        [HttpGet]
        public async Task<IActionResult> Reviews()
        {
            var reviews = await _dbContext.Reviews
                .Include(r => r.Customer)
                .Include(r => r.Provider)
                .Include(r => r.Ord)
                .ToListAsync();

            return View(reviews);
        }

        [HttpGet]
        public async Task<IActionResult> ShowReview(int reviewId)
        {
            var review = await _dbContext.Reviews
                .Include(r => r.Customer)
                .Include(r => r.Provider)
                .Include(r => r.Ord)
                .FirstOrDefaultAsync(r => r.RevId == reviewId);

            if (review == null)
            {
                return NotFound();
            }

            return View("ShowReview", review);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleReviewVerification(int reviewId)
        {
            var review = await _dbContext.Reviews.FindAsync(reviewId);
            if (review == null)
            {
                return NotFound();
            }

            review.RevIsVerified = (short)(review.RevIsVerified == 1 ? 0 : 1);
            await _dbContext.SaveChangesAsync();

            TempData["Success"] = "Review verification status updated successfully!";
            return RedirectToAction(nameof(Reviews));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteReview(int reviewId)
        {
            var review = await _dbContext.Reviews.FindAsync(reviewId);
            if (review == null)
            {
                return NotFound();
            }

            _dbContext.Reviews.Remove(review);
            await _dbContext.SaveChangesAsync();

            TempData["Success"] = "Review deleted successfully!";
            return RedirectToAction(nameof(Reviews));
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var admin = await _dbContext.Users.FindAsync(userId);

            if (admin == null)
            {
                return NotFound();
            }

            var viewModel = new UpdateProfileViewModel
            {
                UserFname = admin.UserFname,
                UserLname = admin.UserLname,
                UserEmail = admin.UserEmail,
                UserPhone = admin.UserPhone
                // Password fields intentionally left empty for security
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(UpdateProfileViewModel model)
        //public async Task<IActionResult> UpdateProfile(UpdateProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var admin = await _dbContext.Users.FindAsync(userId);

            if (admin == null)
            {
                return NotFound();
            }

            // Check if email is unique (excluding current user)
            var emailExists = await _dbContext.Users
                .AnyAsync(u => u.UserEmail == model.UserEmail && u.UserId != userId);

            if (emailExists)
            {
                ModelState.AddModelError("UserEmail", "This email address is already in use by another user.");
                return View(model);
            }

            // Update admin properties
            admin.UserFname = model.UserFname;
            admin.UserLname = model.UserLname;
            admin.UserEmail = model.UserEmail;
            admin.UserPhone = model.UserPhone;

            // Only update password if provided
            if (!string.IsNullOrEmpty(model.UserPassword))
            {
                admin.UserPassword = BCrypt.Net.BCrypt.HashPassword(model.UserPassword);
            }

            try
            {
                await _dbContext.SaveChangesAsync();
                TempData["Success"] = "Profile updated successfully!";
                return RedirectToAction(nameof(Profile));
            }
            catch (Exception ex)
            {
                // Log the exception here if you have logging configured
                ModelState.AddModelError("", "An error occurred while updating your profile. Please try again.");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> RevenueReport()
        {
            var oneYearAgo = DateTime.Now.AddYears(-1);
            var revenueByMonth = await _dbContext.Orders
                .Where(o => o.OrdCreatedDate >= oneYearAgo)
                .GroupBy(o => new
                {
                    Year = o.OrdCreatedDate.Value.Year,
                    Month = o.OrdCreatedDate.Value.Month
                })
                .Select(g => new RevenueByMonthViewModel
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Total = g.Sum(o => o.OrdTotalPrice)
                })
                .OrderByDescending(r => r.Year)
                .ThenByDescending(r => r.Month)
                .ToListAsync();

            var totalRevenue = revenueByMonth.Sum(r => r.Total);

            var viewModel = new RevenueReportViewModel
            {
                RevenueByMonth = revenueByMonth,
                TotalRevenue = totalRevenue
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> OrderReport()
        {
            var ordersByStatus = await _dbContext.Orders
                .GroupBy(o => o.OrdStatus)
                .Select(g => new OrderStatusViewModel
                {
                    Status = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            var totalOrders = await _dbContext.Orders.CountAsync();

            var viewModel = new OrderReportViewModel
            {
                OrdersByStatus = ordersByStatus,
                TotalOrders = totalOrders
            };

            return View(viewModel);
        }

        //Added
        [HttpGet]
        public async Task<IActionResult> Requests()
        {
            var requests = await _dbContext.Requests
                .Include(r => r.Equ)
                .Include(r => r.User)
                .Select(r => new EquipmentRequestViewModel
                {
                    ReqId = r.ReqId,
                    EquipmentName = r.Equ.EquName ?? "N/A",
                    RequestedBy = $"{r.User.UserFname} {r.User.UserLname}" ?? "Guest",
                    RequestDate = r.ReqDate,
                    ApprovalStatus = r.ReqApprovalStatus ?? "Pending",
                    AdminNotes = r.ReqAdminNotes
                })
                .ToListAsync();

            return View(requests);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRequestStatus(UpdateRequestStatusViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Invalid request data.";
                return RedirectToAction(nameof(Requests));
            }

            var request = await _dbContext.Requests.FindAsync(model.ReqId);
            if (request == null)
            {
                return NotFound();
            }

            request.ReqApprovalStatus = model.ReqApprovalStatus;
            request.ReqAdminNotes = model.ReqAdminNotes;

            try
            {
                await _dbContext.SaveChangesAsync();
                TempData["Success"] = "Request status updated successfully!";
            }
            catch (Exception ex)
            {
                // Log the error here
                TempData["Error"] = "An error occurred while updating the request.";
            }

            return RedirectToAction(nameof(Requests));
        }


    }
}
