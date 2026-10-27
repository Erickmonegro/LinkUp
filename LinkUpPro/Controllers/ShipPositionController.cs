using AutoMapper;
using LinkUpPro.Core.Application.DTOs.Battleship;
using LinkUpPro.Core.Application.Interfaces;
using LinkUpPro.Core.Application.ViewModels.Battleship;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LinkUpPro.Core.Domain.Common.Enums;
using System.Diagnostics;
using System.Security.Claims;

namespace LinkUpPro.Controllers
{
    [Authorize]
    public class ShipPositionController : Controller
    {
        private readonly ILogger<ShipPositionController> _logger;
        private readonly IBattleshipMatchService _matchService;
        private readonly IShipPositionService shipService;
        private readonly IMapper mapper;

        public ShipPositionController(ILogger<ShipPositionController> logger,
            IBattleshipMatchService _matchService, IShipPositionService shipService, IMapper mapper)
        {
            _logger = logger;
            this._matchService = _matchService;
            this.shipService = shipService;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> PlaceShip(int matchId, int? shipSize)
        {
            await _matchService.CheckAndProcessTurnTimeoutForMatchAsync(matchId);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
                return RedirectToAction("Login", "Login");

            var match = await _matchService.GetByIdAsync(matchId);
            if (match == null || (match.Player1Id != userId && match.Player2Id != userId))
            {
                TempData["Error"] = "No posee permisos sobre esta partida.";
                return RedirectToAction("Index", "Battleship");
            }

            var existingShips = await shipService.GetByPlayerAsync(matchId, userId);

            if (existingShips.Count >= 5)
            {
                TempData["Info"] = "Ya has colocado todos tus barcos en el tablero.";
                return RedirectToAction(nameof(MyShips), new { matchId, playerId = userId });
            }

            var occupiedCells = new HashSet<string>();
            foreach (var ship in existingShips)
            {
                for (int i = 0; i < ship.ShipSize; i++)
                {
                    int x = ship.StartX;
                    int y = ship.StartY;
                    if (ship.Direction == ShipDirection.Up) y -= i;
                    else if (ship.Direction == ShipDirection.Down) y += i;
                    else if (ship.Direction == ShipDirection.Left) x -= i;
                    else if (ship.Direction == ShipDirection.Right) x += i;
                    occupiedCells.Add($"{x},{y}");
                }
            }

            ViewBag.ExistingShips = existingShips;
            ViewBag.OccupiedCells = occupiedCells;
            ViewBag.Count2 = existingShips.Count(s => s.ShipSize == 2);
            ViewBag.Count3 = existingShips.Count(s => s.ShipSize == 3);
            ViewBag.Count4 = existingShips.Count(s => s.ShipSize == 4);
            ViewBag.Count5 = existingShips.Count(s => s.ShipSize == 5);

            var vm = new ShipPositionViewModel
            {
                Id = 0,
                BattleshipMatchId = matchId,
                UserId = userId,
                ShipSize = shipSize ?? 0
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceShip(ShipPositionViewModel vm)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null) return RedirectToAction("Login", "Login");

                var match = await _matchService.GetByIdAsync(vm.BattleshipMatchId);
                if (match == null || (match.Player1Id != userId && match.Player2Id != userId))
                {
                    TempData["Error"] = "No posee permisos sobre esta partida.";
                    return RedirectToAction("Index", "Battleship");
                }

                vm.UserId = userId;

                if (!ModelState.IsValid)
                {
                    var existingShips = await shipService.GetByPlayerAsync(vm.BattleshipMatchId, userId);
                    var occupiedCells = new HashSet<string>();
                    foreach (var ship in existingShips)
                    {
                        for (int i = 0; i < ship.ShipSize; i++)
                        {
                            int x = ship.StartX;
                            int y = ship.StartY;
                            if (ship.Direction == ShipDirection.Up) y -= i;
                            else if (ship.Direction == ShipDirection.Down) y += i;
                            else if (ship.Direction == ShipDirection.Left) x -= i;
                            else if (ship.Direction == ShipDirection.Right) x += i;
                            occupiedCells.Add($"{x},{y}");
                        }
                    }
                    ViewBag.ExistingShips = existingShips;
                    ViewBag.OccupiedCells = occupiedCells;
                    return View(vm);
                }

                var dto = mapper.Map<ShipPositionDTO>(vm);
                dto.UserId = userId;

                await shipService.PlaceShipsAsync(
                    vm.BattleshipMatchId,
                    userId,
                    dto);

                var updatedShips = await shipService.GetByPlayerAsync(vm.BattleshipMatchId, userId);

                if (updatedShips.Count >= 5)
                {
                    TempData["Success"] = "¡Felicidades! Has colocado todos los barcos de tu flota correctamente.";
                    return RedirectToAction(nameof(MyShips),
                        new
                        {
                            matchId = vm.BattleshipMatchId,
                            playerId = userId
                        });
                }
                else
                {
                    TempData["Success"] = "Barco colocado correctamente. Selecciona tu siguiente barco.";
                    return RedirectToAction(nameof(PlaceShip),
                        new
                        {
                            matchId = vm.BattleshipMatchId
                        });
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? vm.UserId;
                var existingShips = await shipService.GetByPlayerAsync(vm.BattleshipMatchId, userId);
                var occupiedCells = new HashSet<string>();
                foreach (var ship in existingShips)
                {
                    for (int i = 0; i < ship.ShipSize; i++)
                    {
                        int x = ship.StartX;
                        int y = ship.StartY;
                        if (ship.Direction == ShipDirection.Up) y -= i;
                        else if (ship.Direction == ShipDirection.Down) y += i;
                        else if (ship.Direction == ShipDirection.Left) x -= i;
                        else if (ship.Direction == ShipDirection.Right) x += i;
                        occupiedCells.Add($"{x},{y}");
                    }
                }
                ViewBag.ExistingShips = existingShips;
                ViewBag.OccupiedCells = occupiedCells;
                return View(vm);
            }
        }

        public async Task<IActionResult> MyShips(int matchId, string playerId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null) return RedirectToAction("Login", "Login");

                var match = await _matchService.GetByIdAsync(matchId);
                if (match == null || (match.Player1Id != userId && match.Player2Id != userId))
                {
                    TempData["Error"] = "No posee permisos sobre esta partida.";
                    return RedirectToAction("Index", "Battleship");
                }

                playerId = userId;

                await _matchService.CheckAndProcessTurnTimeoutForMatchAsync(matchId);

                var ships = await shipService.GetByPlayerAsync(
                    matchId,
                    playerId);

                ViewBag.MatchId = matchId;
                ViewBag.PlayerId = playerId;

                var vm = mapper.Map<List<ShipPositionViewModel>>(ships);

                return View(vm);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;

                return RedirectToAction("Index", "Battleship");
            }
        }

        public async Task<IActionResult> MatchShips(int matchId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null) return RedirectToAction("Login", "Login");

                var match = await _matchService.GetByIdAsync(matchId);
                if (match == null || (match.Player1Id != userId && match.Player2Id != userId))
                {
                    TempData["Error"] = "No posee permisos sobre esta partida.";
                    return RedirectToAction("Index", "Battleship");
                }

                var ships = match.Status == MatchStatus.Finished
                    ? await shipService.GetByMatchAsync(matchId)
                    : await shipService.GetByPlayerAsync(matchId, userId);

                var vm = mapper.Map<List<ShipPositionViewModel>>(ships);

                return View(vm);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;

                return RedirectToAction("Details",
                    "Battleship",
                    new { id = matchId });
            }
        }

        public async Task<IActionResult> AreShipsPlaced(int matchId, string playerId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null) return RedirectToAction("Login", "Login");

                var match = await _matchService.GetByIdAsync(matchId);
                if (match == null || (match.Player1Id != userId && match.Player2Id != userId))
                {
                    TempData["Error"] = "No posee permisos sobre esta partida.";
                    return RedirectToAction("Index", "Battleship");
                }

                playerId = userId;

                bool result = await shipService.AreShipsPlacedAsync(
                    matchId,
                    playerId);

                if (result)
                    TempData["Success"] = "Todos los barcos fueron colocados.";

                else
                    TempData["Info"] = "Todavía faltan barcos por colocar.";

                return RedirectToAction(nameof(MyShips),
                    new
                    {
                        matchId,
                        playerId
                    });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;

                return RedirectToAction(nameof(MyShips),
                    new
                    {
                        matchId,
                        playerId
                    });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ready(int matchId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null) return RedirectToAction("Login", "Login");

                var match = await _matchService.GetByIdAsync(matchId);

                if (match == null)
                {
                    TempData["Error"] = "La partida no existe.";
                    return RedirectToAction("Index", "Battleship");
                }

                if (match.Player1Id != userId && match.Player2Id != userId)
                {
                    TempData["Error"] = "No posee permisos sobre esta partida.";
                    return RedirectToAction("Index", "Battleship");
                }

                bool player1Ready = await shipService.AreShipsPlacedAsync(matchId, match.Player1Id);
                bool player2Ready = await shipService.AreShipsPlacedAsync(matchId, match.Player2Id);

                if (!player1Ready || !player2Ready)
                {
                    TempData["Info"] = "Ambos jugadores deben colocar todos sus barcos.";

                    return RedirectToAction("Details", "Battleship", new { id = matchId });
                }

                await _matchService.StartMatchAsync(matchId);

                TempData["Success"] = "La partida ha comenzado.";

                return RedirectToAction("Play", "Attack", new { matchId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;

                return RedirectToAction("Details", "Battleship", new { id = matchId });
            }
        }
    }
}
