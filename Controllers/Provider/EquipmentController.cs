using EquipLink.ApplicationDbContext;
using EquipLink.Helpers;
using EquipLink.Models;
using EquipLink.ViewModels.CartVMs;
using EquipLink.ViewModels.EquipmentVMs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace EquipLink.Controllers.Provider
{

    [Authorize]
    [Route("Provider/[controller]")]
    public class EquipmentController : Controller
    {
        private readonly EquipmentDbContext _context;
        private readonly IFileUploadService _fileUploadService;

        public EquipmentController(EquipmentDbContext context, IFileUploadService fileUploadService)
        {
            _context = context;
            _fileUploadService = fileUploadService;
        }

       
        // GET: Provider/Equipment (Enhanced with search)
        [HttpGet]
        public async Task<IActionResult> Index(string searchTerm = "", int page = 1, int pageSize = 10)
        {
            var providerId = GetCurrentUserId();
            var query = _context.Equipment
                .Include(e => e.Categ)
                .Where(e => e.ProviderId == providerId);

            // Apply search filter
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(e =>
                    e.EquName.Contains(searchTerm) ||
                    e.EquDescription.Contains(searchTerm) ||
                    e.Categ.CategType.Contains(searchTerm) ||
                    e.EquCondition.Contains(searchTerm)
                );
            }

            // Apply pagination
            var totalItems = await query.CountAsync();
            var equipments = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Load requests for modals
            var equipmentIds = equipments.Select(e => e.EquId).ToList();
            var requests = await _context.Requests
                .Where(r => r.UserId == providerId && equipmentIds.Contains(r.EquId))
                .OrderByDescending(r => r.ReqDate)
                .ToListAsync();

            ViewBag.Requests = requests.GroupBy(r => r.EquId)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Pass search and pagination data to view
            ViewBag.SearchTerm = searchTerm;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            return View("~/Views/Provider/Equipment/Index.cshtml", equipments);
        }


        // GET: Provider/Equipment/Create
        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            //var categories = await _context.Categories
            //    .Where(c => c.CategIsActive == 1)
            //    .ToListAsync();
            //ViewBag.Categories = new SelectList(categories, "CategId", "CategType");

            await LoadCategoriesForView();
            //return View(new EquipmentCreateViewModel());
            return View("~/Views/Provider/Equipment/Create.cshtml", new EquipmentCreateViewModel());
        }

        // POST: Provider/Equipment/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Store(EquipmentCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadCategoriesForView();
                return View("~/Views/Provider/Equipment/Create.cshtml", model);
            }

            try
            {
                // Validate image
                if (!IsValidImageFile(model.EquImage, out string errorMessage))
                {
                    ModelState.AddModelError("EquImage", errorMessage);
                    await LoadCategoriesForView();
                    return View("~/Views/Provider/Equipment/Create.cshtml", model);
                }

                if (model.EquImage == null || model.EquImage.Length == 0)
                {
                    ModelState.AddModelError("EquImage", "Image is required.");
                    await LoadCategoriesForView();
                    return View("~/Views/Provider/Equipment/Create.cshtml", model);
                }

                // Upload image
                var imagePath = await _fileUploadService.UploadFileAsync(model.EquImage);

                var equipment = new Equipment
                {
                    EquName = model.EquName,
                    EquDescription = model.EquDescription,
                    EquCondition = model.EquCondition,
                    EquAvailabilityStatus = model.EquAvailabilityStatus,
                    EquQuantity = model.EquQuantity,
                    EquPrice = model.EquPrice,
                    CategId = model.CategId,
                    ProviderId = GetCurrentUserId(),
                    EquCreatedDate = DateTime.Now,
                    EquIsActive = 1,
                    EquImage = imagePath,
                    EquType = model.EquType,
                    // New fields
                    EquModel = model.EquModel,
                    EquModelYear = model.EquModelYear,
                    EquBrand = model.EquBrand,
                    EquWorkingHours = model.EquWorkingHours,
                    EquFeatures = model.EquFeatures
                };

                _context.Equipment.Add(equipment);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Equipment added successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An error occurred while saving the equipment: {ex.Message}");
                await LoadCategoriesForView();
                return View("~/Views/Provider/Equipment/Create.cshtml", model);
            }
        }

        // GET: Provider/Equipment/Edit/5
        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var providerId = GetCurrentUserId();
            var equipment = await _context.Equipment
                .Where(e => e.ProviderId == providerId && e.EquId == id)
                .FirstOrDefaultAsync();

            if (equipment == null)
            {
                return NotFound();
            }

            await LoadCategoriesForView(equipment.CategId);

            var model = new EquipmentEditViewModel
            {
                EquId = equipment.EquId,
                EquName = equipment.EquName,
                EquDescription = equipment.EquDescription,
                EquCondition = equipment.EquCondition,
                EquAvailabilityStatus = equipment.EquAvailabilityStatus,
                EquQuantity = equipment.EquQuantity,
                EquPrice = equipment.EquPrice,
                CategId = equipment.CategId,
                EquType = equipment.EquType,
                CurrentImagePath = equipment.EquImage,
                // New fields
                EquModel = equipment.EquModel,
                EquModelYear = equipment.EquModelYear,
                EquBrand = equipment.EquBrand,
                EquWorkingHours = equipment.EquWorkingHours,
                EquFeatures = equipment.EquFeatures
            };

            return View("~/Views/Provider/Equipment/Edit.cshtml", model);
        }

        // Update your Update method
        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, EquipmentEditViewModel model)
        {
            if (id != model.EquId)
            {
                return BadRequest();
            }

            var providerId = GetCurrentUserId();

            try
            {
                // First try to find the existing equipment
                var existingEquipment = await _context.Equipment
                    .FirstOrDefaultAsync(e => e.ProviderId == providerId && e.EquId == id);

                if (existingEquipment == null)
                {
                    return NotFound();
                }

                // Clear image validation if no new file
                if (model.EquImage == null || model.EquImage.Length == 0)
                {
                    ModelState.Remove("EquImage");
                }

                if (!ModelState.IsValid)
                {
                    await LoadCategoriesForView(model.CategId);
                    model.CurrentImagePath = existingEquipment.EquImage;
                    return View("~/Views/Provider/Equipment/Edit.cshtml", model);
                }

                // Update the existing entity
                existingEquipment.EquName = model.EquName;
                existingEquipment.EquDescription = model.EquDescription;
                existingEquipment.EquCondition = model.EquCondition;
                existingEquipment.EquAvailabilityStatus = model.EquAvailabilityStatus;
                existingEquipment.EquQuantity = model.EquQuantity;
                existingEquipment.EquPrice = model.EquPrice;
                existingEquipment.CategId = model.CategId;
                existingEquipment.EquType = model.EquType;
                existingEquipment.EquModel = model.EquModel;
                existingEquipment.EquModelYear = model.EquModelYear;
                existingEquipment.EquBrand = model.EquBrand;
                existingEquipment.EquWorkingHours = model.EquWorkingHours;
                existingEquipment.EquFeatures = model.EquFeatures;

                // Handle new image upload
                if (model.EquImage != null && model.EquImage.Length > 0)
                {
                    if (!IsValidImageFile(model.EquImage, out string errorMessage))
                    {
                        ModelState.AddModelError("EquImage", errorMessage);
                        await LoadCategoriesForView(model.CategId);
                        model.CurrentImagePath = existingEquipment.EquImage;
                        return View("~/Views/Provider/Equipment/Edit.cshtml", model);
                    }

                    // Delete old image if it exists
                    if (!string.IsNullOrEmpty(existingEquipment.EquImage))
                    {
                        await _fileUploadService.DeleteFileAsync(existingEquipment.EquImage);
                    }

                    // Upload new image
                    var imagePath = await _fileUploadService.UploadFileAsync(model.EquImage);
                    existingEquipment.EquImage = imagePath;
                }

                try
                {
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Equipment updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    bool exists = await _context.Equipment.AnyAsync(e => e.EquId == model.EquId);
                    if (!exists)
                    {
                        return NotFound();
                    }
                    else
                    {
                        ModelState.AddModelError("", "The record you attempted to edit was modified by another user after you got the original value. Please try again.");
                        await LoadCategoriesForView(model.CategId);
                        model.CurrentImagePath = existingEquipment.EquImage;
                        return View("~/Views/Provider/Equipment/Edit.cshtml", model);
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An error occurred: {ex.Message}");
                await LoadCategoriesForView(model.CategId);
                model.CurrentImagePath = await _context.Equipment
                    .Where(e => e.EquId == id)
                    .Select(e => e.EquImage)
                    .FirstOrDefaultAsync();
                return View("~/Views/Provider/Equipment/Edit.cshtml", model);
            }
        }

        private bool EquipmentExists(int id)
        {
            return _context.Equipment.Any(e => e.EquId == id);
        }

        // POST: Provider/Equipment/Delete/5
        [HttpPost("Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Destroy(int id)
        {
            var providerId = GetCurrentUserId();
            var equipment = await _context.Equipment
                .Where(e => e.ProviderId == providerId && e.EquId == id)
                .FirstOrDefaultAsync();

            if (equipment == null)
            {
                return NotFound();
            }

            try
            {
                _context.Equipment.Remove(equipment);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Equipment deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while deleting the equipment.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Provider/Equipment/Requests/5
        [HttpGet("Requests/{id}")]
        public async Task<IActionResult> GetRequests(int id)
        {
            var providerId = GetCurrentUserId();
            var equipment = await _context.Equipment
                .Where(e => e.ProviderId == providerId && e.EquId == id)
                .FirstOrDefaultAsync();

            if (equipment == null)
            {
                return NotFound();
            }

            var requests = await _context.Requests
                .Where(r => r.UserId == providerId &&
                           r.ReqDescription.StartsWith($"Equipment Approval Request for {equipment.EquName}"))
                .OrderByDescending(r => r.ReqDate)
                .ToListAsync();

            var viewModel = new EquipmentRequestsViewModel
            {
                Equipment = equipment,
                Requests = requests
            };

            //return View(viewModel);
            return View("~/Views/Provider/Equipment/Requests.cshtml", viewModel);
        }

        // POST: Provider/Equipment/StoreRequest/5
        [HttpPost("StoreRequest/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StoreRequest(int id, RequestCreateViewModel model)
        {
            var providerId = GetCurrentUserId();
            var equipment = await _context.Equipment
                .Where(e => e.ProviderId == providerId && e.EquId == id)
                .FirstOrDefaultAsync();

            if (equipment == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please provide a valid request description.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var request = new Request
                {
                    UserId = providerId,
                    EquId = equipment.EquId,
                    ReqDescription = $"Equipment Approval Request for {equipment.EquName}: {model.ReqDescription}",
                    ReqDate = DateOnly.FromDateTime(DateTime.Now),
                    ReqApprovalStatus = "Pending",
                    ReqAdminNotes = null,
                    ReqInsurancePerDay = model.ReqInsurancePerDay
                };

                _context.Requests.Add(request);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Request submitted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while submitting the request.";
                return RedirectToAction(nameof(Index));
            }
        }

        //Added

        [HttpGet]
        [Route("Provider/Equipment")]  // Explicit route
        [Route("Provider/Equipment/Index")]  // Alternative route
        public async Task<IActionResult> Show()
        {
            // Your index code
            return View();
        }

        // Make Show action more specific  
        [HttpGet]
        [Route("Provider/Equipment/Show/{id:int}")]  // Explicit route with int constraint
        public async Task<IActionResult> Show(int id, bool editRental = false)
        {
            var equipment = await _context.Equipment
                .Include(e => e.Provider)
                .FirstOrDefaultAsync(e => e.EquId == id);

            if (equipment == null)
            {
                return NotFound();
            }

            if (editRental && equipment.EquType == "rent")
            {
                // Get the cart to pre-populate dates if this item is already in cart
                var cart = HttpContext.Session.GetString("Cart");
                if (!string.IsNullOrEmpty(cart))
                {
                    var cartItems = JsonSerializer.Deserialize<Dictionary<string, CartItem>>(cart);
                    if (cartItems.ContainsKey(id.ToString()))
                    {
                        ViewBag.StartDate = cartItems[id.ToString()].StartDate?.ToString("yyyy-MM-dd");
                        ViewBag.EndDate = cartItems[id.ToString()].EndDate?.ToString("yyyy-MM-dd");
                    }
                }
                ViewBag.EditRental = true;
            }

            return View("~/Views/Provider/Equipment/Show.cshtml", equipment);
        }

        #region Private Helper Methods

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        private async Task LoadCategoriesForView(int? selectedCategoryId = null)
        {
            var categories = await _context.Categories
                .Where(c => c.CategIsActive == 1)
                .ToListAsync();

            ViewBag.Categories = new SelectList(categories, "CategId", "CategType", selectedCategoryId);
        }

        private bool IsValidImageFile(IFormFile file, out string errorMessage)
        {
            errorMessage = null;

            if (file == null || file.Length == 0)
            {
                errorMessage = "Please select an image file.";
                return false;
            }

            var allowedExtensions = new[] { ".jpeg", ".jpg", ".png", ".gif" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(fileExtension))
            {
                errorMessage = "Only JPEG, PNG, JPG, and GIF files are allowed.";
                return false;
            }

            if (file.Length > 5 * 1024 * 1024) // 5MB limit
            {
                errorMessage = "File size cannot exceed 5MB.";
                return false;
            }

            // Check if it's actually an image
            try
            {
                using var stream = file.OpenReadStream();
                var buffer = new byte[4];
                stream.Read(buffer, 0, 4);

                // Check for common image file signatures
                var isImage = IsImageFile(buffer, fileExtension);
                if (!isImage)
                {
                    errorMessage = "The selected file is not a valid image.";
                    return false;
                }
            }
            catch
            {
                errorMessage = "Unable to process the selected file.";
                return false;
            }

            return true;
        }

        private bool IsImageFile(byte[] fileHeader, string extension)
        {
            return extension switch
            {
                ".jpg" or ".jpeg" => fileHeader[0] == 0xFF && fileHeader[1] == 0xD8,
                ".png" => fileHeader[0] == 0x89 && fileHeader[1] == 0x50 && fileHeader[2] == 0x4E && fileHeader[3] == 0x47,
                ".gif" => (fileHeader[0] == 0x47 && fileHeader[1] == 0x49 && fileHeader[2] == 0x46),
                _ => true // Allow other extensions to pass through
            };
        }

        #endregion

    }
}