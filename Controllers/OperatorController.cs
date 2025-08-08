using EquipLink.ApplicationDbContext;
using EquipLink.Models;
using EquipLink.ViewModels.OperatorVMs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EquipLink.Controllers
{
    //[Authorize(Roles = "Operator")]
    [Authorize(Roles = "Provider")]
    public class OperatorController : Controller
    {
        private readonly EquipmentDbContext _context;

        public OperatorController(EquipmentDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> MaintenanceIndex()
        {
            var maintenances = await _context.Maintenances
                .Include(m => m.Ord)
                .ToListAsync();

            return View(maintenances);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MaintenanceUpdateStatus(int maintenanceId, string status)
        {
            if (!new[] { "Scheduled", "In Progress", "Completed" }.Contains(status))
            {
                ModelState.AddModelError("Status", "Invalid status value");
                return RedirectToAction(nameof(MaintenanceIndex));
            }

            var maintenance = await _context.Maintenances.FindAsync(maintenanceId);
            if (maintenance == null)
            {
                return NotFound();
            }

            maintenance.MainStatus = status;
            if (status == "Completed")
            {
                maintenance.MainCompletedDate = DateOnly.FromDateTime(DateTime.Now);
            }

            _context.Update(maintenance);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Maintenance status updated successfully!";
            return RedirectToAction(nameof(MaintenanceIndex));
        }

        public async Task<IActionResult> Profile()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return NotFound();
            }
            var userCompany = await _context.Companies.FirstOrDefaultAsync(c => c.UserId == userId);

            var model = new ProfileUpdateViewModel
            {
                User_FName = user.UserFname,
                User_LName = user.UserLname,
                User_Email = user.UserEmail,
                User_Phone = user.UserPhone,
                Company = userCompany
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProfileUpdate(ProfileUpdateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Profile", model);
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            // Check if email is already taken by another user
            if (await _context.Users.AnyAsync(u => u.UserEmail == model.User_Email && u.UserId != userId))
            {
                ModelState.AddModelError("User_Email", "Email is already taken");
                return View("Profile", model);
            }

            user.UserFname = model.User_FName;
            user.UserLname = model.User_LName;
            user.UserEmail = model.User_Email;
            user.UserPhone = model.User_Phone;

            if (!string.IsNullOrEmpty(model.User_Password))
            {
                user.UserPassword = BCrypt.Net.BCrypt.HashPassword(model.User_Password);
            }

            _context.Update(user);
            await _context.SaveChangesAsync();

            // Update the name in the cookie if it changed
            if (User.FindFirstValue(ClaimTypes.Name) != $"{user.UserFname} {user.UserLname}")
            {
                var identity = User.Identity as ClaimsIdentity;
                if (identity != null)
                {
                    var nameClaim = identity.FindFirst(ClaimTypes.Name);
                    if (nameClaim != null)
                    {
                        identity.RemoveClaim(nameClaim);
                        identity.AddClaim(new Claim(ClaimTypes.Name, $"{user.UserFname} {user.UserLname}"));

                        await HttpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            new ClaimsPrincipal(identity));
                    }
                }
            }

            TempData["Success"] = "Profile updated successfully!";
            return RedirectToAction(nameof(Profile));
        }
    }
}