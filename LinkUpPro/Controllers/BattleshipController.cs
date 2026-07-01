using AutoMapper;
using LinkUpPro.Core.Application.Interfaces;
using LinkUpPro.Core.Application.ViewModels.Battleship;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace LinkUpPro.Controllers
{
    [Authorize]
    public class BattleshipController : Controller
    {
        private readonly ILogger<BattleshipController> _logger;
        private readonly IBattleshipMatchService matchService;
        private readonly IMapper mapper;
        private readonly IUserService userService;
        private readonly IFriendshipService friendshipService;

        public BattleshipController(ILogger<BattleshipController> logger, IBattleshipMatchService matchService, IMapper mapper, IUserService userService, IFriendshipService friendshipService)
        {
            _logger = logger;
            this.matchService = matchService;
            this.mapper = mapper;
            this.userService = userService;
            this.friendshipService = friendshipService;
        }

        private async Task PopulateNamesAsync(IEnumerable<BattleshipMatchViewModel> matches)
        {
            foreach (var match in matches)
            {
                var p1 = await userService.GetUserById(match.Player1Id);
                var p2 = await userService.GetUserById(match.Player2Id);
                var winner = !string.IsNullOrEmpty(match.WinnerId) ? await userService.GetUserById(match.WinnerId) : null;
                var turnUser = !string.IsNullOrEmpty(match.CurrentTurnUserId) ? await userService.GetUserById(match.CurrentTurnUserId) : null;

                match.Player1Name = p1 != null ? $"{p1.FirstName} {p1.LastName} (@{p1.UserName})" : match.Player1Id;
                match.Player2Name = p2 != null ? $"{p2.FirstName} {p2.LastName} (@{p2.UserName})" : match.Player2Id;
                match.WinnerName = winner != null ? $"{winner.FirstName} {winner.LastName} (@{winner.UserName})" : (match.WinnerId ?? "Sin ganador");
                match.CurrentTurnUserName = turnUser != null ? $"{turnUser.FirstName} {turnUser.LastName} (@{turnUser.UserName})" : match.CurrentTurnUserId;
            }
        }

        public async Task<IActionResult> Index(string? playerId)
        {
            try
            {
                playerId ??= User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (playerId == null) return RedirectToAction("Login", "User");

                var matches = await matchService.GetByPlayerAsync(playerId);
                var vm = mapper.Map<List<BattleshipMatchViewModel>>(matches);
                await PopulateNamesAsync(vm);

                return View(vm);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View(new List<BattleshipMatchViewModel>());
            }
        }

        public async Task<IActionResult> Create()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != null)
            {
                ViewBag.Friends = await friendshipService.GetFriendsAsync(userId);
            }
            return View();
        }

        public async Task<IActionResult> GetActive(string? playerId)
        {
            try
            {
                playerId ??= User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (playerId == null) return RedirectToAction("Login", "User");

                var match = await matchService.GetActiveMatchAsync(playerId);
                if (match == null)
                {
                    TempData["Info"] = "No tienes partidas activas.";
                    return RedirectToAction(nameof(Index));
                }

                return RedirectToAction(nameof(Details), new { id = match.Id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var match = await matchService.GetByIdAsync(id);
                if (match == null)
                {
                    TempData["Error"] = "La partida no existe.";
                    return RedirectToAction(nameof(Index));
                }

                var vm = mapper.Map<BattleshipMatchViewModel>(match);
                await PopulateNamesAsync(new[] { vm });

                return View(vm);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateMatch(CreateMatchViewModel vm)
        {
            try
            {
                var player1Id = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (player1Id == null) throw new Exception("Debe iniciar sesión.");

                if (!ModelState.IsValid)
                {
                    ViewBag.Friends = await friendshipService.GetFriendsAsync(player1Id);
                    return View("Create", vm);
                }

                await matchService.CreateMatchAsync(player1Id, vm.Player2Id);
                TempData["Success"] = "Partida creada correctamente. Ahora ambos jugadores deben colocar sus barcos.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                var player1Id = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (player1Id != null)
                {
                    ViewBag.Friends = await friendshipService.GetFriendsAsync(player1Id);
                }
                return View("Create", vm);
            }
        }

        [HttpPost]
        public async Task<IActionResult> StartMatch(int id)
        {
            try
            {
                await matchService.StartMatchAsync(id);
                TempData["Success"] = "La partida ha comenzado.";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Surrender(int id, string playerId)
        {
            try
            {
                await matchService.SurrenderAsync(id, playerId);
                TempData["Success"] = "Te has rendido. La partida ha finalizado.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Details), new { id });
            }
        }
    }
}