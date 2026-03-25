using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System.Linq;
using WebsitePhim.Models;
using System.Collections.Generic;
using System;

namespace WebsitePhim.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UserADController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserADController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var userRoles = new Dictionary<string, IList<string>>();
            foreach (var user in users)
            {
                userRoles[user.Id] = await _userManager.GetRolesAsync(user);
            }
            ViewBag.UserRoles = userRoles;
            return View(users);
        }

        public async Task<IActionResult> Details(string id)
        {
            if (id == null) return NotFound();
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            ViewBag.UserRoles = await _userManager.GetRolesAsync(user);
            return View(user);
        }

        public IActionResult Create()
        {
            ViewBag.Roles = _roleManager.Roles.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            string email,
            string userName,
            string fullName,
            string? address,
            string? age,
            string password,
            string confirmPassword,
            string selectedRole)
        {
            if (password != confirmPassword)
            {
                ModelState.AddModelError(string.Empty, "Mật khẩu và xác nhận mật khẩu không khớp.");
            }
            if (string.IsNullOrEmpty(password) || password.Length < 6)
            {
                ModelState.AddModelError(string.Empty, "Mật khẩu phải có ít nhất 6 ký tự.");
            }
            if (string.IsNullOrEmpty(email))
            {
                ModelState.AddModelError(string.Empty, "Email không được để trống.");
            }
            if (string.IsNullOrEmpty(fullName))
            {
                ModelState.AddModelError(string.Empty, "Họ và tên là bắt buộc.");
            }

            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    Email = email,
                    UserName = string.IsNullOrEmpty(userName) ? email : userName,
                    FullName = fullName,
                    Address = address,
                    Age = age,
                    CreatedAt = DateTime.Now,
                    IsActive = true
                };

                var result = await _userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(selectedRole))
                    {
                        await _userManager.AddToRoleAsync(user, selectedRole);
                    }
                    return RedirectToAction(nameof(Index));
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            ViewBag.Roles = _roleManager.Roles.ToList();
            var tempUser = new ApplicationUser
            {
                Email = email,
                UserName = userName,
                FullName = fullName,
                Address = address,
                Age = age,
                CreatedAt = DateTime.Now,
                IsActive = true
            };
            return View(tempUser);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (id == null) return NotFound();
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            ViewBag.UserRoles = await _userManager.GetRolesAsync(user);
            ViewBag.Roles = _roleManager.Roles.ToList();
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,Email,UserName,FullName,Address,Age,IsActive")] ApplicationUser updatedUser, string[] selectedRoles)
        {
            if (id != updatedUser.Id)
            {
                return NotFound();
            }

            var userToUpdate = await _userManager.FindByIdAsync(id);
            if (userToUpdate == null)
            {
                return NotFound();
            }

            // Validate Email and UserName uniqueness
            if (updatedUser.Email != userToUpdate.Email)
            {
                var emailExists = await _userManager.FindByEmailAsync(updatedUser.Email);
                if (emailExists != null)
                {
                    ModelState.AddModelError("Email", "Email đã được sử dụng.");
                }
            }

            if (updatedUser.UserName != userToUpdate.UserName)
            {
                var userNameExists = await _userManager.FindByNameAsync(updatedUser.UserName);
                if (userNameExists != null)
                {
                    ModelState.AddModelError("UserName", "Tên người dùng đã được sử dụng.");
                }
            }

            // Validate FullName (required)
            if (string.IsNullOrEmpty(updatedUser.FullName))
            {
                ModelState.AddModelError("FullName", "Họ và tên là bắt buộc.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Update user properties
                    userToUpdate.Email = updatedUser.Email;
                    userToUpdate.UserName = updatedUser.UserName;
                    userToUpdate.FullName = updatedUser.FullName;
                    userToUpdate.Address = updatedUser.Address;
                    userToUpdate.Age = updatedUser.Age;
                    userToUpdate.IsActive = updatedUser.IsActive;

                    var updateResult = await _userManager.UpdateAsync(userToUpdate);
                    if (updateResult.Succeeded)
                    {
                        // Update roles
                        selectedRoles ??= Array.Empty<string>(); // Handle null selectedRoles
                        var currentRoles = await _userManager.GetRolesAsync(userToUpdate);
                        var rolesToAdd = selectedRoles.Except(currentRoles);
                        var rolesToRemove = currentRoles.Except(selectedRoles);

                        if (rolesToAdd.Any())
                        {
                            var addResult = await _userManager.AddToRolesAsync(userToUpdate, rolesToAdd);
                            if (!addResult.Succeeded)
                            {
                                foreach (var error in addResult.Errors)
                                {
                                    ModelState.AddModelError(string.Empty, error.Description);
                                }
                                throw new Exception("Failed to add roles.");
                            }
                        }

                        if (rolesToRemove.Any())
                        {
                            var removeResult = await _userManager.RemoveFromRolesAsync(userToUpdate, rolesToRemove);
                            if (!removeResult.Succeeded)
                            {
                                foreach (var error in removeResult.Errors)
                                {
                                    ModelState.AddModelError(string.Empty, error.Description);
                                }
                                throw new Exception("Failed to remove roles.");
                            }
                        }

                        TempData["SuccessMessage"] = "Cập nhật người dùng thành công!";
                        return RedirectToAction(nameof(Index));
                    }

                    foreach (var error in updateResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"Đã xảy ra lỗi: {ex.Message}");
                }
            }

            // Re-populate ViewBag for error case
            ViewBag.UserRoles = await _userManager.GetRolesAsync(userToUpdate); // Use userToUpdate
            ViewBag.Roles = await _roleManager.Roles.ToListAsync();
            return View(updatedUser);
        }

        public async Task<IActionResult> Delete(string id)
        {
            if (id == null) return NotFound();
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            ViewBag.UserRoles = await _userManager.GetRolesAsync(user);
            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            user.IsActive = false;
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Khóa tài khoản thành công!";
                return RedirectToAction(nameof(Index));
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(user);
        }
    }
}