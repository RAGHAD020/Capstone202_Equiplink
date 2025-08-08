using EquipLink.ApplicationDbContext;
using EquipLink.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EquipLink.Controllers.Provider
{
    [Authorize]
    public class ProviderController : Controller
    {
        private readonly EquipmentDbContext _dbContext;

        public ProviderController(EquipmentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim ?? "0");
        }

        [HttpGet]
        public async Task<IActionResult> Home()
        {
            var providerId = GetCurrentUserId();

            // Total Equipment
            var totalEquipment = await _dbContext.Equipment
                .Where(e => e.ProviderId == providerId)
                .CountAsync();

            // Active Orders (non-cancelled and non-delivered)
            var activeOrders = await _dbContext.Orders
                .Where(o => o.ProviderId == providerId &&
                           o.OrdStatus != "Cancelled" &&
                           o.OrdStatus != "Delivered")
                .CountAsync();

            // Maintenance Requests
            var maintenanceRequests = await _dbContext.Maintenances
                .Where(m => m.Ord.ProviderId == providerId)
                .CountAsync();

            // Average Rating
            var reviews = await _dbContext.Reviews
                .Where(r => r.ProviderId == providerId)
                .ToListAsync();
            var averageRating = reviews.Any() ? reviews.Average(r => r.RevRatingValue) : 0;

            // Recent Orders (last 30 days)
            var thirtyDaysAgo = DateTime.Now.AddDays(-30);
            var recentOrders = await _dbContext.Orders
                .Where(o => o.ProviderId == providerId &&
                           o.OrdCreatedDate.HasValue &&
                           o.OrdCreatedDate.Value >= thirtyDaysAgo)
                .Include(o => o.Customer)
                .OrderByDescending(o => o.OrdCreatedDate)
                .Take(5)
                .ToListAsync();

            // Order Trends (last 6 months)
            var orderTrends = new List<int>();
            for (int i = 5; i >= 0; i--)
            {
                var targetDate = DateTime.Now.AddMonths(-i);
                var count = await _dbContext.Orders
                    .Where(o => o.ProviderId == providerId &&
                               o.OrdCreatedDate.HasValue &&
                               o.OrdCreatedDate.Value.Year == targetDate.Year &&
                               o.OrdCreatedDate.Value.Month == targetDate.Month)
                    .CountAsync();
                orderTrends.Add(count);
            }

            // Equipment Availability
            var equipments = await _dbContext.Equipment
                .Where(e => e.ProviderId == providerId)
                .ToListAsync();

            var equipmentAvailability = new List<int>
            {
                equipments.Count(e => e.EquAvailabilityStatus == "Available"),
                equipments.Count(e => e.EquAvailabilityStatus == "Rented"),
                equipments.Count(e => e.EquAvailabilityStatus == "Maintenance"),
                equipments.Count(e => e.EquAvailabilityStatus == "Unavailable")
            };

            var viewModel = new
            {
                TotalEquipment = totalEquipment,
                ActiveOrders = activeOrders,
                MaintenanceRequests = maintenanceRequests,
                AverageRating = averageRating,
                RecentOrders = recentOrders,
                OrderTrends = orderTrends,
                EquipmentAvailability = equipmentAvailability
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> OrdersIndex()
        {
            var providerId = GetCurrentUserId();
            var orders = await _dbContext.Orders
                .Where(o => o.ProviderId == providerId)
                .Include(o => o.Customer)
                .Include(o => o.Orderequipments)
                    .ThenInclude(oe => oe.Equ)
                .ToListAsync();

            return View("~/Views/Provider/Orders/OrdersIndex.cshtml", orders);
        }

        [HttpGet]
        public async Task<IActionResult> OrdersShow(int orderId)
        {
            var providerId = GetCurrentUserId();
            var order = await _dbContext.Orders
                .Include(o => o.Customer)
                .Include(o => o.Orderequipments)
                    .ThenInclude(oe => oe.Equ)
                .FirstOrDefaultAsync(o => o.OrdId == orderId);

            if (order == null)
            {
                return NotFound();
            }

            if (order.ProviderId != providerId)
            {
                TempData["Error"] = "Unauthorized access.";
                return RedirectToAction("OrdersIndex");
            }

            return View("~/Views/Provider/Orders/OrdersShow.cshtml", order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OrderUpdateStatus(int orderId, string status)
        {
            var providerId = GetCurrentUserId();
            var order = await _dbContext.Orders.FindAsync(orderId);

            if (order == null)
            {
                return NotFound();
            }

            if (order.ProviderId != providerId)
            {
                TempData["Error"] = "Unauthorized access.";
                return RedirectToAction("OrdersIndex");
            }

            var validStatuses = new[] { "Pending", "Processing", "Shipped", "Delivered", "Cancelled" };
            if (!validStatuses.Contains(status))
            {
                TempData["Error"] = "Invalid status.";
                return RedirectToAction("OrdersShow", new { orderId = orderId });
            }

            order.OrdStatus = status;
            await _dbContext.SaveChangesAsync();

            TempData["Success"] = "Order status updated successfully!";
            return RedirectToAction("OrdersShow", new { orderId = orderId });
        }

        [HttpGet]
        public async Task<IActionResult> ReviewsIndex()
        {
            var providerId = GetCurrentUserId();
            var reviews = await _dbContext.Reviews
                .Where(r => r.ProviderId == providerId)
                .Include(r => r.Customer)
                .Include(r => r.Ord)
                .ToListAsync();

            return View("~/Views/Provider/Reviews/ReviewsIndex.cshtml", reviews);
        }

        [HttpGet]
        public async Task<IActionResult> MaintenanceIndex()
        {
            var providerId = GetCurrentUserId();
            var maintenances = await _dbContext.Maintenances
                .Where(m => m.Ord.ProviderId == providerId)
                .Include(m => m.Ord)
                .ToListAsync();

            return View(maintenances);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MaintenanceUpdateStatus(int maintenanceId, string status)
        {
            var providerId = GetCurrentUserId();
            var maintenance = await _dbContext.Maintenances
                .Include(m => m.Ord)
                .FirstOrDefaultAsync(m => m.MainId == maintenanceId);

            if (maintenance == null)
            {
                return NotFound();
            }

            if (maintenance.Ord?.ProviderId != providerId)
            {
                TempData["Error"] = "Unauthorized access.";
                return RedirectToAction("MaintenanceIndex");
            }

            var validStatuses = new[] { "Scheduled", "In Progress", "Completed" };
            if (!validStatuses.Contains(status))
            {
                TempData["Error"] = "Invalid status.";
                return RedirectToAction("MaintenanceIndex");
            }

            maintenance.MainStatus = status;
            if (status == "Completed")
            {
                maintenance.MainCompletedDate = DateOnly.FromDateTime(DateTime.Now);
            }

            await _dbContext.SaveChangesAsync();

            TempData["Success"] = "Maintenance status updated successfully!";
            return RedirectToAction("MaintenanceIndex");
        }

        [HttpGet]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Profile()
        {
            var userId = GetCurrentUserId();
            var user = await _dbContext.Users
                .Include(u => u.Co)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProfileUpdate(User model)
        {
            var userId = GetCurrentUserId();
            var user = await _dbContext.Users
                .Include(u => u.Co)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                return NotFound();
            }

            // Remove password validation from ModelState
            ModelState.Remove("UserPassword");
            ModelState.Remove("UserType"); // Add this for other required fields not in form

            // Validate password confirmation only if password is provided
            if (!string.IsNullOrEmpty(model.UserPassword))
            {
                var passwordConfirmation = Request.Form["UserPasswordConfirmation"];
                if (model.UserPassword != passwordConfirmation)
                {
                    ModelState.AddModelError("UserPassword", "Password and confirmation do not match.");
                }
            }

            // Validate email uniqueness
            var existingUser = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.UserEmail == model.UserEmail && u.UserId != userId);

            if (existingUser != null)
            {
                ModelState.AddModelError("UserEmail", "Email is already in use.");
            }

            // Check if model state is valid
            if (!ModelState.IsValid)
            {
                //  Return the MODEL with validation errors, not the original user
                // Preserve company data for display
                model.Co = user.Co;
                return View("Profile", model);
            }

            // Update user properties
            user.UserFname = model.UserFname;
            user.UserLname = model.UserLname;
            user.UserEmail = model.UserEmail;
            user.UserPhone = model.UserPhone;

            // Update password only if provided
            if (!string.IsNullOrEmpty(model.UserPassword))
            {
                user.UserPassword = BCrypt.Net.BCrypt.HashPassword(model.UserPassword);
            }

            await _dbContext.SaveChangesAsync();

            TempData["Success"] = "Profile updated successfully!";
            return RedirectToAction("Profile");
        }

    }
}