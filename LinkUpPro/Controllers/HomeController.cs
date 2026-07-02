using LinkUpPro.Core.Application.Interfaces;
using LinkUpPro.Core.Domain.Common.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LinkUpPro.Controllers
{
    public class HomeController : Controller
    {
        private readonly IPostService _postService;

        public HomeController(IPostService postService)
        {
            _postService = postService;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index(
            string? textFilter,
            ContentType? contentType,
            DateTime? dateFrom,
            DateTime? dateTo,
            EditionStatus? editionStatus)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                // Render the original design (which contains the login form)
                var vm = new LinkUpPro.Core.Application.ViewModels.User.LoginViewModel
                {
                    UserName = "",
                    Password = ""
                };
                return View("Index", vm);
            }

            // User is authenticated, show their timeline feed
            if (dateFrom.HasValue && dateTo.HasValue && dateFrom.Value > dateTo.Value)
            {
                ModelState.AddModelError("", "La fecha inicial no puede ser posterior a la fecha final.");
                TempData["ErrorMessage"] = "La fecha inicial (Desde) no puede ser posterior a la fecha final (Hasta).";
            }

            var posts = await _postService.GetHomePostsFilteredAsync(
                userId,
                ModelState.IsValid ? textFilter : null,
                ModelState.IsValid ? contentType : null,
                ModelState.IsValid ? dateFrom : null,
                ModelState.IsValid ? dateTo : null,
                ModelState.IsValid ? editionStatus : null
            );

            ViewBag.TextFilter = textFilter;
            ViewBag.ContentType = contentType ?? ContentType.All;
            ViewBag.DateFrom = dateFrom?.ToString("yyyy-MM-dd");
            ViewBag.DateTo = dateTo?.ToString("yyyy-MM-dd");
            ViewBag.EditionStatus = editionStatus ?? EditionStatus.All;

            return View("Feed", posts);
        }

        [AllowAnonymous]
        public IActionResult Privacy()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult Terms()
        {
            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}
