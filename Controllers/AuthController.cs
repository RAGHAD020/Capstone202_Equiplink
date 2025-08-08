using EquipLink.ApplicationDbContext;
using EquipLink.Models;
using EquipLink.ViewModels.AuthVMs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EquipLink.Controllers
{
    public class AuthController : Controller
    {
        private readonly EquipmentDbContext _dbContext;

        public AuthController(EquipmentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // GET: /Auth/Login
        public IActionResult Login()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var user = await _dbContext.Users
                    .FirstOrDefaultAsync(u => u.UserEmail == model.UserEmail);

                if (user == null)
                {
                    ModelState.AddModelError("UserEmail", "Invalid credentials");
                    return View(model);
                }

                // Password verification
                bool isPasswordValid = false;
                try
                {
                    isPasswordValid = BCrypt.Net.BCrypt.Verify(model.UserPassword, user.UserPassword);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Password verification error: {ex.Message}");
                    ModelState.AddModelError("UserPassword", "Authentication error. Please try again.");
                    return View(model);
                }

                if (!isPasswordValid)
                {
                    ModelState.AddModelError("UserPassword", "Invalid credentials");
                    return View(model);
                }

                if (user.UserIsActive != 1)
                {
                    ModelState.AddModelError("UserEmail", "Your account has been deactivated. Please contact support.");
                    return View(model);
                }

                // Update last login date
                user.UserLastLoginDate = DateTime.Now;
                await _dbContext.SaveChangesAsync();

                // Create claims
                        var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Name, $"{user.UserFname} {user.UserLname}"),
                    new Claim(ClaimTypes.Email, user.UserEmail),
                    new Claim(ClaimTypes.Role, user.UserType)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe,
                    ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(30) : DateTimeOffset.UtcNow.AddHours(1)
                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal, authProperties);

                // Redirect based on user type
                switch (user.UserType)
                {
                    case "Admin":
                        TempData["success"] = "Logged in successfully as Admin";
                        return RedirectToAction("Dashboard", "Admin");
                    case "Customer":
                        TempData["success"] = "Logged in successfully as Customer";
                        return RedirectToAction("EquipmentIndex", "Front");
                    case "Provider":
                        TempData["success"] = "Logged in successfully as Provider";
                        return RedirectToAction("Home", "Provider");
                    default:
                        TempData["success"] = "Logged in successfully";
                        return RedirectToAction("Home", "EquipmentIndex");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login error: {ex.Message}");
                ModelState.AddModelError("", "An error occurred during login. Please try again.");
                return View(model);
            }
        }
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Auth/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (model.UserType != "Provider")
            {
                ModelState.Remove(nameof(model.CoName));
                ModelState.Remove(nameof(model.CoEmail));
                ModelState.Remove(nameof(model.CoPhone));
                ModelState.Remove(nameof(model.CoTaxNumber));
            }

            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"Validation error: {error.ErrorMessage}");
                }
                return View(model);
            }

            try
            {
                // Check if email already exists
                var existingUser = await _dbContext.Users
                    .FirstOrDefaultAsync(u => u.UserEmail == model.UserEmail);
                if (existingUser != null)
                {
                    ModelState.AddModelError("UserEmail", "Email already exists");
                    return View(model);
                }

                // Create new user (UserCreatedDate will be set by database default)
                var user = new User
                {
                    UserFname = model.UserFname,
                    UserLname = model.UserLname,
                    UserEmail = model.UserEmail,
                    UserPhone = model.UserPhone,
                    UserPassword = BCrypt.Net.BCrypt.HashPassword(model.UserPassword),
                    UserType = model.UserType
                    // UserIsActive and UserCreatedDate will be set by database defaults
                };

                _dbContext.Users.Add(user);
                await _dbContext.SaveChangesAsync();

                // Create company if user type is Provider
                if (model.UserType == "Provider")
                {
                    var company = new Company
                    {
                        UserId = user.UserId,
                        CoName = model.CoName,
                        CoEmail = model.CoEmail,
                        CoPhone = model.CoPhone,
                        CoTaxNumber = model.CoTaxNumber
                    };
                    _dbContext.Companies.Add(company);
                    await _dbContext.SaveChangesAsync();
                }

                // Sign in the user
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Name, $"{user.UserFname} {user.UserLname}"),
                    new Claim(ClaimTypes.Email, user.UserEmail),
                    new Claim(ClaimTypes.Role, user.UserType)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

                // Redirect based on user type
                if (user.UserType == "Customer")
                {
                    TempData["success"] = "Registered successfully as a Customer.";
                    return RedirectToAction("Index", "Home");
                }
                else if (user.UserType == "Provider")
                {
                    TempData["success"] = "Registered successfully as a Provider.";
                    return RedirectToAction("Home", "Front");
                }

                TempData["success"] = "Registration completed successfully.";
                return RedirectToAction("Home", "Front");
            }
            catch (Exception ex)
            {
                // Log the error (implement proper logging)
                Console.WriteLine($"Registration error: {ex.Message}");
                ModelState.AddModelError("", "An error occurred during registration. Please try again.");
                return View(model);
            }
        }

        // POST: /Auth/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["success"] = "تم تسجيل الخروج";
            return RedirectToAction("Home", "Front");
        }

        // GET: /Auth/ForgotPassword
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                ModelState.AddModelError("email", "Email is required");
                return View();
            }

            try
            {
                var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserEmail == email);
                if (user == null)
                {
                    // For security reasons, don't reveal whether the user exists
                    TempData["success"] = "If your email exists in our system, you'll receive a password reset link";
                    return RedirectToAction("ForgotPassword");
                }

                // Generate password reset token
                var token = GeneratePasswordResetToken();

                // Store the token in database with expiration
                await StorePasswordResetToken(user.UserId, token);

                // Send email with reset link (implementation depends on your email service)
                await SendPasswordResetEmail(user.UserEmail, token);

                TempData["success"] = "If your email exists in our system, you'll receive a password reset link";
                return RedirectToAction("ForgotPassword");
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"ForgotPassword error: {ex.Message}");
                ModelState.AddModelError("", "An error occurred. Please try again.");
                return View();
            }
        }

        // GET: /Auth/ResetPassword
        public IActionResult ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                TempData["error"] = "Invalid reset token";
                return RedirectToAction("ForgotPassword");
            }

            return View(new ResetPasswordViewModel { Token = token });
        }

        // POST: /Auth/ResetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Validate the token
                var userId = await ValidatePasswordResetToken(model.Token);
                if (userId == null)
                {
                    TempData["error"] = "Invalid or expired token";
                    return RedirectToAction("ForgotPassword");
                }

                // Update password
                var user = await _dbContext.Users.FindAsync(userId);
                if (user == null)
                {
                    TempData["error"] = "User not found";
                    return RedirectToAction("ForgotPassword");
                }

                user.UserPassword = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
                await _dbContext.SaveChangesAsync();

                // Invalidate the token
                await InvalidatePasswordResetToken(model.Token);

                TempData["success"] = "Password reset successfully. You can now login with your new password.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"ResetPassword error: {ex.Message}");
                ModelState.AddModelError("", "An error occurred. Please try again.");
                return View(model);
            }
        }

        #region Private Helper Methods

        private string GeneratePasswordResetToken()
        {
            // Generate a secure random token
            return Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");
        }

        private async Task StorePasswordResetToken(int userId, string token)
        {
            // Remove any existing tokens for this user
            var existingTokens = _dbContext.PasswordResetTokens
                .Where(t => t.UserId == userId);
            _dbContext.PasswordResetTokens.RemoveRange(existingTokens);

            // Add new token
            var resetToken = new PasswordResetToken
            {
                UserId = userId,
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                IsUsed = false
            };

            _dbContext.PasswordResetTokens.Add(resetToken);
            await _dbContext.SaveChangesAsync();
        }

        private async Task<int?> ValidatePasswordResetToken(string token)
        {
            var resetToken = await _dbContext.PasswordResetTokens
                .FirstOrDefaultAsync(t => t.Token == token && !t.IsUsed && t.ExpiresAt > DateTime.UtcNow);

            return resetToken?.UserId;
        }

        private async Task InvalidatePasswordResetToken(string token)
        {
            var resetToken = await _dbContext.PasswordResetTokens
                .FirstOrDefaultAsync(t => t.Token == token);

            if (resetToken != null)
            {
                resetToken.IsUsed = true;
                await _dbContext.SaveChangesAsync();
            }
        }

        private async Task SendPasswordResetEmail(string email, string token)
        {
            // This is a placeholder - implement your actual email sending logic here
            var resetLink = Url.Action("ResetPassword", "Auth",
                new { token = token }, Request.Scheme);

            Console.WriteLine($"Password reset link for {email}: {resetLink}");

            // In a real application, you would:
            // 1. Use an email service (SendGrid, MailKit, etc.)
            // 2. Create a proper email template
            // 3. Send the email with the reset link
        }

        #endregion

    }
}