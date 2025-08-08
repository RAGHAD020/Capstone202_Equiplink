using EquipLink.ApplicationDbContext;
using EquipLink.ViewModels.CustomerVMs;
using EquipLink.ViewModels.FrontVMs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EquipLink.Controllers
{
    public class FrontController : Controller
    {
        private readonly EquipmentDbContext _context;

        public FrontController(EquipmentDbContext context)
        {
            _context = context;
        }

        [HttpGet] //~/Views/Customer/Home.cshtml
        public async Task<IActionResult> Home()
        {
            var categories = await _context.Categories.ToListAsync();

            var featuredEquipment = await _context.Equipment
                .Where(e => e.EquIsActive == 1) // EquIsActive is short, not bool
                .Take(4)
                .ToListAsync();

            var viewModel = new HomeViewModel
            {
                Categories = categories,
                FeaturedEquipment = featuredEquipment
            };

            return View("~/Views/Customer/Home.cshtml", viewModel);
        }

        [HttpGet] //~/Views/Customer/AllEquipments
        public async Task<IActionResult> EquipmentIndex(EquipmentFilterViewModel filters)
        {
            var query = _context.Equipment
                .Where(e => e.EquIsActive == 1) // EquIsActive is short, not bool
                .Include(e => e.Categ)
                .AsQueryable();

            // Apply filters
            if (filters.Categories != null && filters.Categories.Any())
            {
                query = query.Where(e => filters.Categories.Contains(e.CategId));
            }

            if (filters.Conditions != null && filters.Conditions.Any())
            {
                query = query.Where(e => filters.Conditions.Contains(e.EquCondition));
            }

            // Changed from EquAvailabilityStatus to EquType
            if (filters.Availabilities != null && filters.Availabilities.Any())
            {
                query = query.Where(e => filters.Availabilities.Contains(e.EquType));
            }

            if (filters.MinPrice.HasValue)
            {
                query = query.Where(e => e.EquPrice >= filters.MinPrice.Value);
            }

            if (filters.MaxPrice.HasValue)
            {
                query = query.Where(e => e.EquPrice <= filters.MaxPrice.Value);
            }

            // Apply sorting
            if (!string.IsNullOrEmpty(filters.SortBy))
            {
                switch (filters.SortBy)
                {
                    case "price_asc":
                        query = query.OrderBy(e => e.EquPrice);
                        break;
                    case "price_desc":
                        query = query.OrderByDescending(e => e.EquPrice);
                        break;
                    case "latest":
                        query = query.OrderByDescending(e => e.EquId); // Assuming EquId is auto-incremented
                        break;
                    case "oldest":
                        query = query.OrderBy(e => e.EquId);
                        break;
                }
            }

            // Pagination
            int pageSize = 9;
            int pageNumber = filters.Page ?? 1;
            int skip = (pageNumber - 1) * pageSize;

            var totalItems = await query.CountAsync();
            var equipment = await query
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            var categories = await _context.Categories.ToListAsync();

            var viewModel = new EquipmentIndexViewModel
            {
                Equipment = equipment,
                Categories = categories,
                Filters = filters,
                CurrentPage = pageNumber,
                TotalPages = (int)Math.Ceiling((double)totalItems / pageSize),
                TotalItems = totalItems
            };

            return View("~/Views/Customer/AllEquipments.cshtml", viewModel);
        }

        [HttpGet] //~/Views/Customer/ShowEquipment
        public async Task<IActionResult> EquipmentShow(int id)
        {
            var equipment = await _context.Equipment
                .Where(e => e.EquIsActive == 1 && e.EquId == id)
                .Include(e => e.Categ)
                .Include(e => e.Provider)
                .Include(e => e.Orderequipments) 
                .FirstOrDefaultAsync();


            if (equipment == null)
                return NotFound();

            // Get reviews for this specific equipment through orders
            var reviews = await _context.Reviews
                .Where(r => r.Ord.Orderequipments.Any(oe => oe.EquId == id))
                .Include(r => r.Customer)
                .Include(r => r.Ord)
                    .ThenInclude(o => o.Orderequipments)
                .ToListAsync();


            var averageRating = reviews.Any() ? reviews.Average(r => r.RevRatingValue) : 0;
            var reviewCount = reviews.Count;

            var viewModel = new EquipmentShowViewModel
            {
                Equipment = equipment,
                Reviews = reviews,
                AverageRating = averageRating,
                ReviewCount = reviewCount,
                ReviewForm = new AddReviewViewModel
                {
                    EquipmentId = id,
                    OrderId = int.TryParse(Request.Query["order_id"], out var orderId) ? orderId : 0
                }
            };

            return View("~/Views/Customer/ShowEquipment.cshtml", viewModel);
        }


        [HttpGet] //~/Views/Customer/AllCategories.cshtml
        public async Task<IActionResult> CategoriesIndex()
        {
            var categories = await _context.Categories.ToListAsync();
            return View("~/Views/Customer/AllCategories.cshtml", categories);
        }

        [HttpGet] //~/Views/Customer/Equipment.cshtml
        public async Task<IActionResult> CategoryShow(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return NotFound();

            // Simplified approach - get equipment in category that are active
            var equipment = await _context.Equipment
                .Where(e => e.EquIsActive == 1 && e.CategId == id)
                .Take(12)
                .ToListAsync();

            var viewModel = new CategoryShowViewModel
            {
                Category = category,
                Equipment = equipment
            };

            return View("~/Views/Customer/Equipment.cshtml", viewModel);
        }
    }
}