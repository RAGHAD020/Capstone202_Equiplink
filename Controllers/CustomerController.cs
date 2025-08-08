using EquipLink.ApplicationDbContext;
using EquipLink.Models;
using EquipLink.ViewModels.CustomerVMs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EquipLink.Controllers
{
    [Authorize]
    public class CustomerController : Controller
    {
        private readonly EquipmentDbContext _context;

        public CustomerController(EquipmentDbContext context)
        {
            _context = context;
        }

        [HttpGet] // _profile.cshtml
        public async Task<IActionResult> Home()
        {
            var userId = GetCurrentUserId();
            var user = await _context.Users.FindAsync(userId);
            return View("_profile", user);
        }

        [HttpGet]
        public IActionResult RegisterRedirect()
        {
            TempData["success"] = "تم التسجيل بنجاح، يرجى استكمال ملفك الشخصي!";
            return RedirectToAction("Home");
        }

        //[HttpGet]
        //public async Task<IActionResult> Profile()
        //{
        //    var userId = GetCurrentUserId();
        //    var user = await _context.Users.FindAsync(userId);

        //    var orders = await _context.Orders
        //        .Where(o => o.CustomerId == userId)
        //        .ToListAsync();

        //    var reviews = await _context.Reviews
        //        .Where(r => r.CustomerId == userId)
        //        .Include(r => r.Customer)
        //        .ToListAsync();

        //    var addresses = await _context.Addresses
        //        .Where(a => a.UserId == userId)
        //        .ToListAsync();

        //    var viewModel = new
        //    {
        //        User = user,
        //        Orders = orders,
        //        Reviews = reviews,
        //        Addresses = addresses
        //    };

        //    return View(viewModel);
        //}

        [HttpGet]
        public async Task<IActionResult> Profile(string tab = "profile")
        {
            var userId = GetCurrentUserId();
            var user = await _context.Users
                .Include(u => u.Co)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            var orders = await _context.Orders
                .Where(o => o.CustomerId == userId)
                .Include(o => o.Orderequipments)
                    .ThenInclude(oe => oe.Equ)
                .Include(o => o.Maintenances)
                .Include(o => o.Reviews)  // Make sure to include Reviews
                .OrderByDescending(o => o.OrdCreatedDate)
                .ToListAsync();

            var reviews = await _context.Reviews
                .Where(r => r.CustomerId == userId)
                .Include(r => r.Ord)
                    .ThenInclude(o => o.Orderequipments)
                        .ThenInclude(oe => oe.Equ)
                .ToListAsync();

            var addresses = await _context.Addresses
                .Where(a => a.UserId == userId)
                .ToListAsync();

            var viewModel = new CustomerProfileViewModel
            {
                User = user ?? new User(),
                Orders = orders,
                Reviews = reviews,
                Addresses = addresses,
                UpdateProfile = new UpdateProfileViewModel
                {
                    UserFName = user?.UserFname ?? string.Empty,
                    UserLName = user?.UserLname ?? string.Empty,
                    UserEmail = user?.UserEmail ?? string.Empty,
                    UserPhone = user?.UserPhone,
                    UserNationalId = user?.UserNationalId,  // Add this line
                    CoName = user?.Co?.CoName,
                    CoEmail = user?.Co?.CoEmail,
                    CoPhone = user?.Co?.CoPhone,
                    CoTaxNumber = user?.Co?.CoTaxNumber
                },
                AddAddress = new AddAddressViewModel(),
                UpdateAddress = new UpdateAddressViewModel(),
                RequestMaintenance = new RequestMaintenanceViewModel(),
                AddReview = new AddReviewViewModel()
            };

            ViewBag.ActiveTab = tab;
            return View(viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(
                    [Bind(Prefix = "UpdateProfile")] UpdateProfileViewModel model)
        {
            // Only validate passwords if at least one is provided
            if (!string.IsNullOrEmpty(model.UserPassword))
            {
                if (model.UserPassword != model.UserPasswordConfirmation)
                {
                    ModelState.AddModelError("UpdateProfile.UserPasswordConfirmation", "Passwords don't match");
                }
            }

            // Validate National ID (must be 10 digits)
            if (!string.IsNullOrEmpty(model.UserNationalId) &&
                (model.UserNationalId.Length != 10 || !model.UserNationalId.All(char.IsDigit)))
            {
                ModelState.AddModelError("UpdateProfile.UserNationalId", "National ID must be 10 digits");
            }

            if (!ModelState.IsValid)
            {
                return await LoadProfileWithErrors(model);
            }

            var currentUserId = GetCurrentUserId();
            var currentUser = await _context.Users
                .Include(u => u.Co)
                .FirstOrDefaultAsync(u => u.UserId == currentUserId);

            if (currentUser == null)
                return NotFound();

            // Update user properties
            currentUser.UserFname = model.UserFName;
            currentUser.UserLname = model.UserLName;
            currentUser.UserEmail = model.UserEmail;
            currentUser.UserPhone = model.UserPhone;
            currentUser.UserNationalId = model.UserNationalId; // Add this line

            // Update password if provided
            if (!string.IsNullOrEmpty(model.UserPassword))
            {
                currentUser.UserPassword = BCrypt.Net.BCrypt.HashPassword(model.UserPassword);
            }

            // Update company information
            if (currentUser.UserType == "Customer")
            {
                var company = currentUser.Co;

                if (company == null && (!string.IsNullOrEmpty(model.CoName) || !string.IsNullOrEmpty(model.CoTaxNumber)))
                {
                    company = new Company
                    {
                        UserId = currentUserId,
                        CoName = model.CoName ?? string.Empty,
                        CoEmail = model.CoEmail ?? string.Empty,
                        CoPhone = model.CoPhone ?? string.Empty,
                        CoTaxNumber = model.CoTaxNumber ?? string.Empty
                    };
                    _context.Companies.Add(company);
                    currentUser.Co = company;
                }
                else if (company != null)
                {
                    company.CoName = model.CoName ?? string.Empty;
                    company.CoEmail = model.CoEmail ?? string.Empty;
                    company.CoPhone = model.CoPhone ?? string.Empty;
                    company.CoTaxNumber = model.CoTaxNumber ?? string.Empty;

                    // Validate Tax Number if provided
                    if (!string.IsNullOrEmpty(model.CoTaxNumber) &&
                        (model.CoTaxNumber.Length != 10 || !model.CoTaxNumber.All(char.IsDigit)))
                    {
                        ModelState.AddModelError("UpdateProfile.CoTaxNumber", "Tax number must be 10 digits");
                        return await LoadProfileWithErrors(model);
                    }
                }
            }

            using var transaction = _context.Database.BeginTransaction();
            try
            {
                await _context.SaveChangesAsync();
                transaction.Commit();
                TempData["success"] = "Profile has been updated successfully";
                return RedirectToAction("Profile", new { tab = "profile" });
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                ModelState.AddModelError("", "An error occurred while updating the profile. Please try again.");
                return await LoadProfileWithErrors(model);
            }
        }

        private async Task<IActionResult> LoadProfileWithErrors(UpdateProfileViewModel model)
        {
            var userId = GetCurrentUserId();
            var user = await _context.Users
                .Include(u => u.Co)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            // Fix: Load orders with all related data
            var orders = await _context.Orders
                .Where(o => o.CustomerId == userId)
                .Include(o => o.Orderequipments)
                    .ThenInclude(oe => oe.Equ) // Include equipment details
                .Include(o => o.Maintenances) // Include maintenance requests
                .OrderByDescending(o => o.OrdCreatedDate)
                .ToListAsync();

            var reviews = await _context.Reviews
                .Where(r => r.CustomerId == userId)
                .Include(r => r.Ord)
                    .ThenInclude(o => o.Orderequipments)
                        .ThenInclude(oe => oe.Equ)
                .ToListAsync();

            var addresses = await _context.Addresses
                .Where(a => a.UserId == userId)
                .ToListAsync();

            var viewModel = new CustomerProfileViewModel
            {
                User = user ?? new User(),
                Orders = orders,
                Reviews = reviews,
                Addresses = addresses,
                UpdateProfile = model,
                AddAddress = new AddAddressViewModel(),
                UpdateAddress = new UpdateAddressViewModel(),
                RequestMaintenance = new RequestMaintenanceViewModel(),
                AddReview = new AddReviewViewModel()
            };

            ViewBag.ActiveTab = "profile";
            return View("Profile", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAddress([Bind(Prefix = "AddAddress")] AddAddressViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["error"] = "Please fill in all required fields";
                return RedirectToAction("Profile", new { tab = "addresses" });
            }

            var userId = GetCurrentUserId();

            try
            {
                // Handle default address logic
                if (model.IsDefault)
                {
                    await _context.Addresses
                        .Where(a => a.UserId == userId)
                        .ForEachAsync(a => a.AddIsDefault = 0);
                }

                var address = new Address
                {
                    AddStreet = model.Street,
                    AddCity = model.City,
                    AddState = model.State,
                    AddPostalCode = model.PostalCode,
                    AddCountry = model.Country,
                    UserId = userId,
                    AddIsDefault = (short)(model.IsDefault ? 1 : 0)
                };

                _context.Addresses.Add(address);
                await _context.SaveChangesAsync();

                TempData["success"] = "Address added successfully!";
            }
            catch (Exception ex)
            {
                TempData["error"] = "An error occurred while adding the address";
            }

            return RedirectToAction("Profile", new { tab = "addresses" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAddress(int addressId, [Bind(Prefix = "UpdateAddress")] UpdateAddressViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["error"] = "Please fill in all required fields";
                return RedirectToAction("Profile", new { tab = "addresses" });
            }

            var userId = GetCurrentUserId();

            try
            {
                var address = await _context.Addresses
                    .FirstOrDefaultAsync(a => a.AddId == addressId && a.UserId == userId);

                if (address == null)
                {
                    TempData["error"] = "Address not found";
                    return RedirectToAction("Profile", new { tab = "addresses" });
                }

                // Handle default address logic
                if (model.IsDefault)
                {
                    await _context.Addresses
                        .Where(a => a.UserId == userId && a.AddId != addressId)
                        .ForEachAsync(a => a.AddIsDefault = 0);
                }

                address.AddStreet = model.Street;
                address.AddCity = model.City;
                address.AddState = model.State;
                address.AddPostalCode = model.PostalCode;
                address.AddCountry = model.Country;
                address.AddIsDefault = (short)(model.IsDefault ? 1 : 0);

                await _context.SaveChangesAsync();

                TempData["success"] = "Address updated successfully!";
            }
            catch (Exception ex)
            {
                TempData["error"] = "An error occurred while updating the address";
            }

            return RedirectToAction("Profile", new { tab = "addresses" });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAddress(int addressId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var address = await _context.Addresses
                    .FirstOrDefaultAsync(a => a.AddId == addressId && a.UserId == userId);

                if (address == null)
                {
                    TempData["error"] = "Address not found or you don't have permission to delete it";
                    return RedirectToAction("Profile", new { tab = "addresses" });
                }

                _context.Addresses.Remove(address);
                await _context.SaveChangesAsync();

                TempData["success"] = "Address deleted successfully!";
                return RedirectToAction("Profile", new { tab = "addresses" });
            }
            catch (Exception ex)
            {
                TempData["error"] = "An error occurred while deleting the address";
                return RedirectToAction("Profile", new { tab = "addresses" });
            }
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestMaintenance(int orderId, RequestMaintenanceViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["error"] = "Please provide a valid description";
                return RedirectToAction("Profile", new { tab = "orders" });
            }

            var maintenance = new Maintenance
            {
                MainRegDate = DateOnly.FromDateTime(DateTime.Now),
                MainDescription = model.Description,
                MainStatus = "Scheduled",
                MainCompletedDate = null,
                OrdId = orderId
            };

            _context.Maintenances.Add(maintenance);

            try
            {
                await _context.SaveChangesAsync();
                TempData["success"] = "Maintenance request submitted successfully!";
            }
            catch (Exception ex)
            {
                TempData["error"] = "An error occurred while submitting your request. Please try again.";
            }

            return RedirectToAction("Profile", new { tab = "orders" });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview(int equipmentId, [Bind(Prefix = "ReviewForm")] AddReviewViewModel model)
        {
            // DEBUG: Check what's in the model
            Console.WriteLine($"Equipment ID: {equipmentId}");
            Console.WriteLine($"Rating: {model.Rating}");
            Console.WriteLine($"Comment: {model.Comment}");
            Console.WriteLine($"Order ID: {model.OrderId}");
            Console.WriteLine($"Equipment ID from model: {model.EquipmentId}");

            // Set the equipment ID from the route parameter
            model.EquipmentId = equipmentId;

            // DEBUG: Check ModelState errors
            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState is invalid:");
                foreach (var modelError in ModelState)
                {
                    var key = modelError.Key;
                    var errors = modelError.Value.Errors;
                    Console.WriteLine($"Key: {key}");
                    foreach (var error in errors)
                    {
                        Console.WriteLine($"  Error: {error.ErrorMessage}");
                    }
                }

                TempData["error"] = "Please fill in all required fields correctly";
                return RedirectToAction("EquipmentShow", "Front", new { id = equipmentId });
            }

            var equipment = await _context.Equipment.FindAsync(equipmentId);
            if (equipment == null)
            {
                TempData["error"] = "Equipment not found";
                return RedirectToAction("EquipmentShow", "Front", new { id = equipmentId });
            }

            var userId = GetCurrentUserId();

            try
            {
                var review = new Review
                {
                    RevRatingValue = model.Rating,
                    RevComment = model.Comment,
                    OrdId = model.OrderId,
                    RevDate = DateOnly.FromDateTime(DateTime.Now),
                    RevIsVerified = 1,
                    CustomerId = userId,
                    ProviderId = equipment.ProviderId
                };

                _context.Reviews.Add(review);
                await _context.SaveChangesAsync();

                TempData["success"] = "Review submitted successfully!";
                return RedirectToAction("EquipmentShow", "Front", new { id = equipmentId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occurred: {ex.Message}");
                TempData["error"] = "An error occurred while submitting your review. Please try again.";
                return RedirectToAction("EquipmentShow", "Front", new { id = equipmentId });
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteReview(int reviewId)
        {
            var userId = GetCurrentUserId();
            var review = await _context.Reviews
                .FirstOrDefaultAsync(r => r.RevId == reviewId && r.CustomerId == userId);

            if (review == null)
                return NotFound();

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            TempData["success"] = "تم حذف المراجعة بنجاح!";
            return RedirectToAction("Profile");
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }
            throw new InvalidOperationException("User ID not found");
        }
    }
}