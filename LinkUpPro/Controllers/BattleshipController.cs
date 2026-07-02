using AutoMapper;
using LinkUpPro.Core.Application.DTOs.NotificationDTO;
using LinkUpPro.Core.Application.Interfaces;
using LinkUpPro.Core.Application.ViewModels.Battleship;
using LinkUpPro.Core.Domain.Common.Enums;
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
        private readonly IShipPositionService shipPositionService;
        private readonly IAttackService attackService;
        private readonly INotificationService notificationService;

        public BattleshipController(ILogger<BattleshipController> logger, IBattleshipMatchService matchService, IMapper mapper, IUserService userService, IFriendshipService friendshipService, IShipPositionService shipPositionService, IAttackService attackService, INotificationService notificationService)
        {
            _logger = logger;
            this.matchService = matchService;
            this.mapper = mapper;
            this.userService = userService;
            this.friendshipService = friendshipService;
            this.shipPositionService = shipPositionService;
            this.attackService = attackService;
            this.notificationService = notificationService;
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
                if (playerId == null) return RedirectToAction("Login", "Login");

                await matchService.CheckAndProcessTurnTimeoutsAsync(playerId);

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
                ViewBag.Friends = await GetAvailableFriendsForMatchAsync(userId);
            }
            return View();
        }

        public async Task<IActionResult> GetActive(string? playerId)
        {
            try
            {
                playerId ??= User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (playerId == null) return RedirectToAction("Login", "Login");

                await matchService.CheckAndProcessTurnTimeoutsAsync(playerId);

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

        public async Task<IActionResult> Details(int id, string? viewBoardUserId = null)
        {
            try
            {
                await matchService.CheckAndProcessTurnTimeoutForMatchAsync(id);

                var match = await matchService.GetByIdAsync(id);
                if (match == null)
                {
                    TempData["Error"] = "La partida no existe.";
                    return RedirectToAction(nameof(Index));
                }

                var vm = mapper.Map<BattleshipMatchViewModel>(match);
                await PopulateNamesAsync(new[] { vm });

                if (match.Status == MatchStatus.Finished)
                {
                    ViewBag.Ships = await shipPositionService.GetByMatchAsync(id);
                    ViewBag.Attacks = await attackService.GetByMatchAsync(id);
                    ViewBag.ViewBoardUserId = viewBoardUserId;
                }

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
                    ViewBag.Friends = await GetAvailableFriendsForMatchAsync(player1Id);
                    return View("Create", vm);
                }

                var availableFriends = await GetAvailableFriendsForMatchAsync(player1Id);
                if (!availableFriends.Any(f => f.FriendId == vm.Player2Id))
                {
                    throw new Exception("Ya tienes una partida activa o un desafío pendiente con este amigo.");
                }

                var createdMatch = await matchService.CreateMatchAsync(player1Id, vm.Player2Id);

                var currentUser = await userService.GetUserById(player1Id);
                var triggerName = currentUser != null ? $"{currentUser.FirstName} {currentUser.LastName}" : "Un usuario";
                var notification = new NotificationDTO
                {
                    Id = 0,
                    ReceiverId = vm.Player2Id,
                    TriggerUserId = player1Id,
                    TriggerUserName = triggerName,
                    TriggerUserPhotoUrl = currentUser?.PhotoUrl,
                    ActivityType = NotificationType.BattleChallenge,
                    Description = $"{triggerName} te ha desafiado a una partida de Batalla Naval.",
                    RelatedEntityId = createdMatch.Id,
                    Status = NotificationStatus.Unread
                };
                await notificationService.AddAsync(notification);

                TempData["Success"] = "¡Desafío enviado correctamente! Esperando a que el contrincante acepte la batalla.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                var player1Id = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (player1Id != null)
                {
                    ViewBag.Friends = await GetAvailableFriendsForMatchAsync(player1Id);
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
        public async Task<IActionResult> Surrender(int id, string? playerId)
        {
            try
            {
                playerId ??= User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (playerId == null) return RedirectToAction("Login", "Login");

                await matchService.SurrenderAsync(id, playerId);
                TempData["Success"] = "Te has rendido. La partida ha finalizado y tu contrincante ha sido el ganador.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptChallenge(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null) return RedirectToAction("Login", "Login");

                var match = await matchService.AcceptChallengeAsync(id, userId);

                var currentUser = await userService.GetUserById(userId);
                var triggerName = currentUser != null ? $"{currentUser.FirstName} {currentUser.LastName}" : "Un usuario";
                var notification = new NotificationDTO
                {
                    Id = 0,
                    ReceiverId = match.Player1Id,
                    TriggerUserId = userId,
                    TriggerUserName = triggerName,
                    TriggerUserPhotoUrl = currentUser?.PhotoUrl,
                    ActivityType = NotificationType.BattleAccepted,
                    Description = $"{triggerName} ha aceptado tu desafío de Batalla Naval. ¡A colocar los barcos!",
                    RelatedEntityId = match.Id,
                    Status = NotificationStatus.Unread
                };
                await notificationService.AddAsync(notification);

                TempData["Success"] = "¡Has aceptado el desafío! Ahora coloca tus barcos en el tablero.";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectChallenge(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null) return RedirectToAction("Login", "Login");

                var match = await matchService.RejectChallengeAsync(id, userId);

                if (match.Player2Id == userId)
                {
                    var currentUser = await userService.GetUserById(userId);
                    var triggerName = currentUser != null ? $"{currentUser.FirstName} {currentUser.LastName}" : "Un usuario";
                    var notification = new NotificationDTO
                    {
                        Id = 0,
                        ReceiverId = match.Player1Id,
                        TriggerUserId = userId,
                        TriggerUserName = triggerName,
                        TriggerUserPhotoUrl = currentUser?.PhotoUrl,
                        ActivityType = NotificationType.BattleRejected,
                        Description = $"{triggerName} ha rechazado tu solicitud de Batalla Naval.",
                        RelatedEntityId = match.Id,
                        Status = NotificationStatus.Unread
                    };
                    await notificationService.AddAsync(notification);
                    TempData["Success"] = "Has rechazado la solicitud de batalla.";
                }
                else
                {
                    TempData["Success"] = "Has cancelado tu solicitud de batalla.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<IActionResult> SurrenderConfirm(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null) return RedirectToAction("Login", "Login");

                var match = await matchService.GetByIdAsync(id);
                if (match == null) return RedirectToAction(nameof(Index));

                if (match.Status == MatchStatus.Finished || match.Status == MatchStatus.Rejected)
                {
                    TempData["Info"] = "Esta partida ya ha finalizado.";
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

        private async Task<List<LinkUpPro.Core.Application.DTOs.FriendshipDTO.FriendshipDTO>> GetAvailableFriendsForMatchAsync(string userId)
        {
            var friends = await friendshipService.GetFriendsAsync(userId);
            var matches = await matchService.GetByPlayerAsync(userId);
            var activeOpponents = matches
                .Where(m => m.Status != MatchStatus.Finished && m.Status != MatchStatus.Rejected)
                .Select(m => m.Player1Id == userId ? m.Player2Id : m.Player1Id)
                .ToHashSet();

            return friends.Where(f => !activeOpponents.Contains(f.FriendId)).ToList();
        }
    }
}