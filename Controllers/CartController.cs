using EquipLink.ApplicationDbContext;
using EquipLink.Models;
using EquipLink.ViewModels.CartVMs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace EquipLink.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly EquipmentDbContext _dbContext;
        private readonly ILogger<CartController> _logger;
        private const string CartSessionKey = "Cart";

        public CartController(EquipmentDbContext dbContext, ILogger<CartController> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost]
        [Route("Cart/AddToCartAjax")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCartAjax(int equipmentId, int quantity = 1, DateTime? start_date = null, DateTime? end_date = null)
        {
            if (quantity <= 0)
            {
                return Json(new { success = false, message = "Quantity must be greater than 0" });
            }

            try
            {
                var equipment = await _dbContext.Equipment.FindAsync(equipmentId);
                if (equipment == null)
                {
                    _logger.LogWarning("Equipment with ID {EquipmentId} not found", equipmentId);
                    return Json(new { success = false, message = "Equipment not found" });
                }

                // Validate rental dates for rent equipment
                if (equipment.EquType == "rent")
                {
                    if (!start_date.HasValue || !end_date.HasValue)
                    {
                        return Json(new { success = false, message = "Please select rental dates" });
                    }

                    if (start_date >= end_date)
                    {
                        return Json(new { success = false, message = "End date must be after start date" });
                    }

                    if (start_date < DateTime.Today)
                    {
                        return Json(new { success = false, message = "Start date cannot be in the past" });
                    }
                }

                var cart = GetCart();
                var cartItemKey = equipmentId.ToString();

                if (cart.ContainsKey(cartItemKey))
                {
                    cart[cartItemKey].Quantity += quantity;
                    // Update dates if provided
                    if (start_date.HasValue && end_date.HasValue)
                    {
                        cart[cartItemKey].StartDate = start_date;
                        cart[cartItemKey].EndDate = end_date;
                    }
                    _logger.LogInformation("Updated quantity for equipment {EquipmentId} in cart", equipmentId);
                }
                else
                {
                    cart[cartItemKey] = new CartItem
                    {
                        Quantity = quantity,
                        StartDate = start_date ?? (equipment.EquType == "rent" ? DateTime.Now.AddDays(2) : null),
                        EndDate = end_date ?? (equipment.EquType == "rent" ? DateTime.Now.AddDays(4) : null)
                    };
                    _logger.LogInformation("Added new equipment {EquipmentId} to cart", equipmentId);
                }

                SaveCart(cart);
                var cartCount = GetCartTotal(HttpContext.Session);

                // FIXED: Calculate the correct price for the message
                var displayPrice = equipment.EquPrice;
                if (equipment.EquType == "rent" && start_date.HasValue && end_date.HasValue)
                {
                    var days = Math.Max(1, (end_date.Value - start_date.Value).Days);
                    displayPrice = equipment.EquPrice * days * quantity;
                }
                else
                {
                    displayPrice = equipment.EquPrice * quantity;
                }

                var message = equipment.EquType == "rent"
                    ? $"{equipment.EquName} added to cart for {start_date?.ToString("MMM dd")} - {end_date?.ToString("MMM dd")} (SAR {displayPrice:F2})"
                    : $"{equipment.EquName} added to cart successfully! (SAR {displayPrice:F2})";

                return Json(new
                {
                    success = true,
                    message = message,
                    cartCount = cartCount,
                    equipmentName = equipment.EquName
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding equipment {EquipmentId} to cart", equipmentId);
                return Json(new { success = false, message = "An error occurred while adding item to cart" });
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCart(UpdateCartRequest request)
        {
            if (!ModelState.IsValid)
            {
                // Only show toastr for non-AJAX requests
                if (!Request.Headers.ContainsKey("X-Requested-With"))
                {
                    TempData["Error"] = "Invalid cart update request";
                }
                return RedirectToAction("View");
            }
            try
            {
                var cart = GetCart();
                var cartItemKey = request.EquipmentId.ToString();
                if (cart.ContainsKey(cartItemKey))
                {
                    if (request.Quantity <= 0)
                    {
                        cart.Remove(cartItemKey);
                        _logger.LogInformation("Removed equipment {EquipmentId} from cart", request.EquipmentId);
                    }
                    else
                    {
                        cart[cartItemKey].Quantity = request.Quantity;
                        var equipment = await _dbContext.Equipment.FindAsync(request.EquipmentId);
                        if (equipment != null && equipment.EquType == "rent")
                        {
                            // Validate rental dates
                            if (request.StartDate.HasValue && request.EndDate.HasValue)
                            {
                                if (request.StartDate >= request.EndDate)
                                {
                                    // Only show toastr for non-AJAX requests
                                    if (!Request.Headers.ContainsKey("X-Requested-With"))
                                    {
                                        TempData["Error"] = "End date must be after start date";
                                    }
                                    return RedirectToAction("View");
                                }
                                if (request.StartDate < DateTime.Today)
                                {
                                    // Only show toastr for non-AJAX requests
                                    if (!Request.Headers.ContainsKey("X-Requested-With"))
                                    {
                                        TempData["Error"] = "Start date cannot be in the past";
                                    }
                                    return RedirectToAction("View");
                                }
                            }
                            cart[cartItemKey].StartDate = request.StartDate;
                            cart[cartItemKey].EndDate = request.EndDate;
                        }
                        _logger.LogInformation("Updated equipment {EquipmentId} in cart", request.EquipmentId);
                    }
                    SaveCart(cart);

                    // Only show success toastr for non-AJAX requests
                    if (!Request.Headers.ContainsKey("X-Requested-With"))
                    {
                        TempData["Success"] = "Cart updated successfully!";
                    }
                }
                else
                {
                    // Only show toastr for non-AJAX requests
                    if (!Request.Headers.ContainsKey("X-Requested-With"))
                    {
                        TempData["Error"] = "Item not found in cart";
                    }
                }
                return RedirectToAction("View");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cart for equipment {EquipmentId}", request.EquipmentId);
                // Only show toastr for non-AJAX requests
                if (!Request.Headers.ContainsKey("X-Requested-With"))
                {
                    TempData["Error"] = "An error occurred while updating cart";
                }
                return RedirectToAction("View");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromCart(int equipmentId)
        {
            try
            {
                var cart = GetCart();
                var cartItemKey = equipmentId.ToString();

                if (cart.ContainsKey(cartItemKey))
                {
                    cart.Remove(cartItemKey);
                    SaveCart(cart);
                    _logger.LogInformation("Removed equipment {EquipmentId} from cart", equipmentId);
                    TempData["Success"] = "Item removed from cart successfully!";
                }
                else
                {
                    TempData["Error"] = "Item not found in cart";
                }

                return RedirectToAction("View");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing equipment {EquipmentId} from cart", equipmentId);
                TempData["Error"] = "An error occurred while removing item from cart";
                return RedirectToAction("View");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ClearCart()
        {
            try
            {
                HttpContext.Session.Remove(CartSessionKey);
                _logger.LogInformation("Cart cleared for user {UserId}", GetCurrentUserId());
                TempData["Success"] = "Cart cleared successfully!";
                return RedirectToAction("View");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart");
                TempData["Error"] = "An error occurred while clearing cart";
                return RedirectToAction("View");
            }
        }

        [HttpGet]
        public static int GetCartTotal(ISession session)
        {
            var cartJson = session.GetString(CartSessionKey);
            if (string.IsNullOrEmpty(cartJson))
                return 0;

            try
            {
                var cart = JsonSerializer.Deserialize<Dictionary<string, CartItem>>(cartJson);
                return cart?.Count ?? 0;
            }
            catch
            {
                return 0;
            }
        }

        [HttpGet]
        public async Task<IActionResult> View()
        {
            try
            {
                var cart = GetCart();
                var cartItems = new List<CartItemViewModel>();

                foreach (var item in cart)
                {
                    if (!int.TryParse(item.Key, out int equipmentId))
                        continue;

                    var equipment = await _dbContext.Equipment
                        .Include(e => e.Provider)
                        .FirstOrDefaultAsync(e => e.EquId == equipmentId);

                    if (equipment == null)
                        continue;

                    var price = equipment.EquPrice;
                    var quantity = item.Value.Quantity;
                    var startDate = item.Value.StartDate ?? DateTime.Now.AddDays(2);
                    var endDate = item.Value.EndDate ?? DateTime.Now.AddDays(4);

                    // FIXED: Correct subtotal calculation
                    var subtotal = price * quantity; // Default for purchase items

                    if (equipment.EquType == "rent" && item.Value.StartDate.HasValue && item.Value.EndDate.HasValue)
                    {
                        var days = Math.Max(1, (endDate - startDate).Days);
                        subtotal = price * days * quantity; // For rental: daily_rate × days × quantity
                    }

                    cartItems.Add(new CartItemViewModel
                    {
                        Equipment = equipment,
                        Quantity = quantity,
                        Subtotal = subtotal, // This should now show 210000 for 7 days × 30000
                        Type = equipment.EquType == "rent" ? "Rent" : "Buy",
                        DailyRate = equipment.EquPrice,
                        StartDate = startDate,
                        EndDate = endDate,
                        ProviderName = equipment.Provider?.UserFname + " " + equipment.Provider?.UserLname
                    });
                }

                var total = await GetCartTotalAll();
                var viewModel = new CartViewModel
                {
                    CartItems = cartItems,
                    Total = total
                };

                return View("~/Views/Customer/View.cshtml", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error displaying cart");
                TempData["Error"] = "An error occurred while loading cart";
                return View(new CartViewModel());
            }
        }

        [HttpGet]
        public async Task<decimal> GetCartTotalAll()
        {
            var cart = GetCart();
            decimal total = 0;

            foreach (var item in cart)
            {
                if (!int.TryParse(item.Key, out int equipmentId))
                    continue;

                var equipment = await _dbContext.Equipment.FindAsync(equipmentId);
                if (equipment == null)
                    continue;

                var price = equipment.EquPrice;
                var quantity = item.Value.Quantity;

                if (equipment.EquType == "rent" && item.Value.StartDate.HasValue && item.Value.EndDate.HasValue)
                {
                    // FIXED: Calculate days correctly
                    var days = Math.Max(1, (item.Value.EndDate.Value - item.Value.StartDate.Value).Days);
                    total += price * days * quantity; // daily_rate × days × quantity
                }
                else
                {
                    total += price * quantity; // unit_price × quantity
                }
            }

            return total;
        }

        [HttpGet]
        public async Task<IActionResult> ShowCheckout()
        {
            try
            {
                var cart = GetCart();
                if (!cart.Any())
                {
                    TempData["Error"] = "Your cart is empty.";
                    return RedirectToAction("View");
                }

                var cartItems = new List<CartItemViewModel>();
                foreach (var item in cart)
                {
                    if (!int.TryParse(item.Key, out int equipmentId))
                        continue;

                    var equipment = await _dbContext.Equipment
                        .Include(e => e.Provider)
                        .Include(e => e.Requests)
                        .FirstOrDefaultAsync(e => e.EquId == equipmentId);

                    if (equipment == null)
                        continue;

                    var price = equipment.EquPrice;
                    var quantity = item.Value.Quantity;
                    var startDate = item.Value.StartDate ?? DateTime.Now.AddDays(2);
                    var endDate = item.Value.EndDate ?? DateTime.Now.AddDays(4);
                    var subtotal = price * quantity;

                    if (equipment.EquType == "rent" && item.Value.StartDate.HasValue && item.Value.EndDate.HasValue)
                    {
                        var days = Math.Max(1, (endDate - startDate).Days);
                        subtotal = price * days * quantity;
                    }

                    cartItems.Add(new CartItemViewModel
                    {
                        Equipment = equipment,
                        Quantity = quantity,
                        Subtotal = subtotal,
                        Type = equipment.EquType == "rent" ? "Rent" : "Buy",
                        DailyRate = equipment.EquPrice,
                        StartDate = startDate,
                        EndDate = endDate,
                        ProviderName = equipment.Provider?.UserFname + " " + equipment.Provider?.UserLname,
                        LatestRequest = equipment.Requests.OrderByDescending(r => r.ReqDate).FirstOrDefault()
                    });
                }

                var total = await GetCartTotalAll();
                var userId = GetCurrentUserId();
                var addresses = await _dbContext.Addresses
                    .Where(a => a.UserId == userId)
                    .ToListAsync();

                var viewModel = new CheckoutViewModel
                {
                    CartItems = cartItems,
                    Total = total,
                    Addresses = addresses,
                    TotalDeliveryFee = CalculateDeliveryFee(cartItems),
                    DeliveryMessage = GetDeliveryMessage(cartItems)
                };

                return View("~/Views/Customer/ShowCheckout.cshtml", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error displaying checkout");
                TempData["Error"] = "An error occurred while loading checkout";
                return RedirectToAction("View");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            // Always clear shipping address validation errors since we don't use them
            ModelState.Remove("ShippingAddress.Street");
            ModelState.Remove("ShippingAddress.City");
            ModelState.Remove("ShippingAddress.State");
            ModelState.Remove("ShippingAddress.PostalCode");
            ModelState.Remove("ShippingAddress.Country");
            ModelState.Remove("ShippingAddress.FirstName");
            ModelState.Remove("ShippingAddress.LastName");

            // Clear credit card validation errors if payment method is Cash
            if (model.PaymentMethod == "Cash")
            {
                ModelState.Remove("CardNumber");
                ModelState.Remove("CardExpiry");
                ModelState.Remove("CardCVC");
            }

            // Clear billing address validation errors if existing address is selected
            if (model.BillingAddressSelect != "new" && !string.IsNullOrEmpty(model.BillingAddressSelect))
            {
                ModelState.Remove("BillingAddress.Street");
                ModelState.Remove("BillingAddress.City");
                ModelState.Remove("BillingAddress.State");
                ModelState.Remove("BillingAddress.PostalCode");
                ModelState.Remove("BillingAddress.Country");
                ModelState.Remove("BillingAddress.FirstName");
                ModelState.Remove("BillingAddress.LastName");
            }

            // Manual validation for new billing addresses
            if (model.BillingAddressSelect == "new" || string.IsNullOrEmpty(model.BillingAddressSelect))
            {
                if (string.IsNullOrWhiteSpace(model.BillingAddress.Street))
                {
                    ModelState.AddModelError("BillingAddress.Street", "Street address is required");
                }
                if (string.IsNullOrWhiteSpace(model.BillingAddress.City))
                {
                    ModelState.AddModelError("BillingAddress.City", "City is required");
                }
                if (string.IsNullOrWhiteSpace(model.BillingAddress.State))
                {
                    ModelState.AddModelError("BillingAddress.State", "State is required");
                }
                if (string.IsNullOrWhiteSpace(model.BillingAddress.PostalCode))
                {
                    ModelState.AddModelError("BillingAddress.PostalCode", "Postal code is required");
                }
                if (string.IsNullOrWhiteSpace(model.BillingAddress.Country))
                {
                    ModelState.AddModelError("BillingAddress.Country", "Country is required");
                }
            }

            // Manual validation for credit card fields if Credit Card is selected
            if (model.PaymentMethod == "Credit Card")
            {
                if (string.IsNullOrWhiteSpace(model.CardNumber))
                {
                    ModelState.AddModelError("CardNumber", "Card number is required");
                }
                else if (model.CardNumber.Replace(" ", "").Length != 16 || !model.CardNumber.Replace(" ", "").All(char.IsDigit))
                {
                    ModelState.AddModelError("CardNumber", "Please enter a valid 16-digit card number");
                }

                if (string.IsNullOrWhiteSpace(model.CardExpiry))
                {
                    ModelState.AddModelError("CardExpiry", "Card expiry is required");
                }
                else if (!System.Text.RegularExpressions.Regex.IsMatch(model.CardExpiry, @"^(0[1-9]|1[0-2])\/\d{2}$"))
                {
                    ModelState.AddModelError("CardExpiry", "Please enter expiry date in MM/YY format");
                }

                if (string.IsNullOrWhiteSpace(model.CardCVC))
                {
                    ModelState.AddModelError("CardCVC", "Card CVC is required");
                }
                else if (model.CardCVC.Length < 3 || model.CardCVC.Length > 4 || !model.CardCVC.All(char.IsDigit))
                {
                    ModelState.AddModelError("CardCVC", "Please enter a valid 3 or 4 digit CVC");
                }
            }

            // Check terms acceptance
            if (!model.AcceptTerms)
            {
                ModelState.AddModelError("AcceptTerms", "You must accept the terms and conditions");
            }

            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    _logger.LogWarning("Model validation error: {Error}", error.ErrorMessage);
                }

                var firstError = ModelState.Keys.FirstOrDefault(key => ModelState[key].Errors.Count > 0);
                if (firstError != null)
                {
                    TempData["FocusField"] = firstError;
                }

                TempData["Error"] = "Please correct the errors and try again.";

                // Reload the checkout view with current data
                var cart = GetCart();
                if (!cart.Any())
                {
                    TempData["Error"] = "Your cart is empty.";
                    return RedirectToAction("View");
                }

                // Re-populate the cart items for the view
                var cartItems = new List<CartItemViewModel>();
                foreach (var item in cart)
                {
                    if (!int.TryParse(item.Key, out int equipmentId))
                        continue;

                    var equipment = await _dbContext.Equipment
                        .Include(e => e.Provider)
                        .Include(e => e.Requests)
                        .FirstOrDefaultAsync(e => e.EquId == equipmentId);

                    if (equipment == null)
                        continue;

                    var price = equipment.EquPrice;
                    var quantity = item.Value.Quantity;
                    var startDate = item.Value.StartDate ?? DateTime.Now.AddDays(2);
                    var endDate = item.Value.EndDate ?? DateTime.Now.AddDays(4);
                    var subtotal = price * quantity;

                    if (equipment.EquType == "rent" && item.Value.StartDate.HasValue && item.Value.EndDate.HasValue)
                    {
                        var days = Math.Max(1, (endDate - startDate).Days);
                        subtotal = price * days * quantity;
                    }

                    cartItems.Add(new CartItemViewModel
                    {
                        Equipment = equipment,
                        Quantity = quantity,
                        Subtotal = subtotal,
                        Type = equipment.EquType == "rent" ? "Rent" : "Buy",
                        DailyRate = equipment.EquPrice,
                        StartDate = startDate,
                        EndDate = endDate,
                        ProviderName = equipment.Provider?.UserFname + " " + equipment.Provider?.UserLname,
                        LatestRequest = equipment.Requests.OrderByDescending(r => r.ReqDate).FirstOrDefault()
                    });
                }

                var userId = GetCurrentUserId();
                var addresses = await _dbContext.Addresses
                    .Where(a => a.UserId == userId)
                    .ToListAsync();

                model.CartItems = cartItems;
                model.Total = await GetCartTotalAll();
                model.Addresses = addresses;
                model.TotalDeliveryFee = CalculateDeliveryFee(cartItems);
                model.DeliveryMessage = GetDeliveryMessage(cartItems);

                return View("~/Views/Customer/ShowCheckout.cshtml", model);
            }

            try
            {
                var cart = GetCart();
                if (!cart.Any())
                {
                    TempData["Error"] = "Your cart is empty.";
                    return RedirectToAction("View");
                }

                var userId = GetCurrentUserId();
                var total = await GetCartTotalAll();

                // Add additional fees
                if (model.Ord_InstallationOperation)
                {
                    total += 300m; // Installation fee
                }
                total += model.TotalDeliveryFee; // Delivery fee

                // Handle addresses - since you're only using billing address, use it for both billing and shipping
                var billingAddress = await SaveAddress(model.BillingAddress, model.BillingAddressSelect, userId);
                var shippingAddress = billingAddress; // Use billing address as shipping address

                // Get provider ID from first equipment
                int? providerId = null;
                foreach (var item in cart)
                {
                    if (int.TryParse(item.Key, out int equipmentId))
                    {
                        var equipment = await _dbContext.Equipment.FindAsync(equipmentId);
                        if (equipment != null)
                        {
                            providerId = equipment.ProviderId;
                            break;
                        }
                    }
                }

                using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    // Create order
                    var order = new Order
                    {
                        OrdTotalPrice = total,
                        OrdStatus = "Pending",
                        OrdCreatedDate = DateTime.Now,
                        OrdNotes = $"{model.PaymentNotes}" +
                           $"{(model.Ord_InstallationOperation ? "\nIncludes installation service" : "")}" +
                           $"\nDelivery Instructions: {model.DeliveryNotes}",
                        CustomerId = userId,
                        ProviderId = providerId ?? 0,
                        OrdInstallationOperation = model.Ord_InstallationOperation ? (short)1 : (short)0
                    };

                    _dbContext.Orders.Add(order);
                    await _dbContext.SaveChangesAsync();

                    // Create order equipment entries
                    foreach (var item in cart)
                    {
                        if (!int.TryParse(item.Key, out int equipmentId))
                            continue;

                        var equipment = await _dbContext.Equipment.FindAsync(equipmentId);
                        if (equipment == null)
                            continue;

                        var unitPrice = equipment.EquPrice;
                        var subtotal = unitPrice * item.Value.Quantity;

                        if (equipment.EquType == "rent" && item.Value.StartDate.HasValue && item.Value.EndDate.HasValue)
                        {
                            var days = Math.Max(1, (item.Value.EndDate.Value - item.Value.StartDate.Value).Days);
                            subtotal = unitPrice * days * item.Value.Quantity;
                        }

                        var orderEquipment = new Orderequipment
                        {
                            OrdEqQuantity = item.Value.Quantity,
                            OrdEqUnitPrice = unitPrice,
                            OrdEqSubTotal = subtotal,
                            OrdEqStartDate = item.Value.StartDate.HasValue ? DateOnly.FromDateTime(item.Value.StartDate.Value) : null,
                            OrdEqEndDate = item.Value.EndDate.HasValue ? DateOnly.FromDateTime(item.Value.EndDate.Value) : null,
                            OrdId = order.OrdId,
                            EquId = equipmentId
                        };

                        _dbContext.Orderequipments.Add(orderEquipment);
                    }

                    // Create payment record
                    var payment = new Payment
                    {
                        PayMethod = model.PaymentMethod,
                        PayDate = DateTime.Now,
                        PayStatus = "Pending",
                        PayAmount = total,
                        PayTransactionId = model.PaymentMethod == "Credit Card" ? GenerateTransactionId() : null,
                        PayNotes = model.PaymentNotes,
                        OrdId = order.OrdId
                    };

                    _dbContext.Payments.Add(payment);
                    await _dbContext.SaveChangesAsync();

                    await transaction.CommitAsync();

                    // Clear cart after successful order
                    HttpContext.Session.Remove(CartSessionKey);

                    _logger.LogInformation("Order {OrderId} created successfully", order.OrdId);
                    TempData["Success"] = "Order placed successfully!";
                    return RedirectToAction("OrderConfirmation", new { orderId = order.OrdId });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error during checkout transaction");
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing checkout");
                TempData["Error"] = "An error occurred while processing your order. Please try again.";
                return RedirectToAction("ShowCheckout");
            }
        }

        #region Private Methods
        private decimal CalculateDeliveryFee(List<CartItemViewModel> cartItems)
        {
            // Implement your delivery fee calculation logic
            // Example: Flat rate of 50 SAR + 10 SAR per equipment item
            return 50m + (cartItems.Count * 10m);
        }

        private string GetDeliveryMessage(List<CartItemViewModel> cartItems)
        {
            // Implement your delivery message logic
            bool hasHeavyEquipment = cartItems.Any(i => i.Equipment.EquType == "heavy");

            return hasHeavyEquipment
                ? "Delivery within 5-7 business days (heavy equipment requires special handling)"
                : "Delivery within 3-5 business days";
        }

        // [Other private methods remain unchanged...]
        #endregion

        [HttpGet]
        public async Task<IActionResult> OrderConfirmation(int orderId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var order = await _dbContext.Orders
                    .Include(o => o.Orderequipments)
                        .ThenInclude(oe => oe.Equ) // Fixed: Use Equ instead of Equipment
                    .Include(o => o.Payments)
                    .FirstOrDefaultAsync(o => o.OrdId == orderId && o.CustomerId == userId);

                if (order == null)
                {
                    TempData["Error"] = "Order not found.";
                    return RedirectToAction("Index", "Home");
                }

                return View("~/Views/Customer/OrderConfirmation.cshtml", order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error displaying order confirmation for order {OrderId}", orderId);
                TempData["Error"] = "An error occurred while loading order confirmation";
                return RedirectToAction("Index", "Home");
            }
        }

        #region Private Methods

        private async Task<Address> SaveAddress(AddressModel addressModel, string? addressId, int userId)
        {
            if (addressId == "new" || string.IsNullOrEmpty(addressId))
            {
                var newAddress = new Address
                {
                    AddStreet = addressModel.Street,
                    AddCity = addressModel.City,
                    AddState = addressModel.State,
                    AddPostalCode = addressModel.PostalCode,
                    AddCountry = addressModel.Country,
                    UserId = userId,
                    AddIsDefault = addressModel.IsDefault ? (short)1 : (short)0
                };

                _dbContext.Addresses.Add(newAddress);
                await _dbContext.SaveChangesAsync();
                return newAddress;
            }

            if (int.TryParse(addressId, out int id))
            {
                var address = await _dbContext.Addresses.FindAsync(id);
                if (address != null && address.UserId == userId) // Security check
                {
                    address.AddStreet = addressModel.Street ?? address.AddStreet;
                    address.AddCity = addressModel.City ?? address.AddCity;
                    address.AddState = addressModel.State ?? address.AddState;
                    address.AddPostalCode = addressModel.PostalCode ?? address.AddPostalCode;
                    address.AddCountry = addressModel.Country ?? address.AddCountry;
                    address.AddIsDefault = addressModel.IsDefault ? (short)1 : address.AddIsDefault;

                    await _dbContext.SaveChangesAsync();
                    return address;
                }
            }

            // Fallback: create new address
            var fallbackAddress = new Address
            {
                AddStreet = addressModel.Street,
                AddCity = addressModel.City,
                AddState = addressModel.State,
                AddPostalCode = addressModel.PostalCode,
                AddCountry = addressModel.Country,
                UserId = userId,
                AddIsDefault = addressModel.IsDefault ? (short)1 : (short)0
            };

            _dbContext.Addresses.Add(fallbackAddress);
            await _dbContext.SaveChangesAsync();
            return fallbackAddress;
        }

        private Dictionary<string, CartItem> GetCart()
        {
            var cartJson = HttpContext.Session.GetString(CartSessionKey);
            if (string.IsNullOrEmpty(cartJson))
                return new Dictionary<string, CartItem>();

            try
            {
                return JsonSerializer.Deserialize<Dictionary<string, CartItem>>(cartJson) ?? new Dictionary<string, CartItem>();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error deserializing cart from session");
                return new Dictionary<string, CartItem>();
            }
        }

        private void SaveCart(Dictionary<string, CartItem> cart)
        {
            try
            {
                var cartJson = JsonSerializer.Serialize(cart);
                HttpContext.Session.SetString(CartSessionKey, cartJson);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving cart to session");
                throw;
            }
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }

            _logger.LogWarning("User ID not found in claims");
            throw new UnauthorizedAccessException("User not authenticated");
        }

        private static string GenerateTransactionId()
        {
            return Guid.NewGuid().ToString("N")[..10].ToUpper();
        }

        #endregion
    }
}