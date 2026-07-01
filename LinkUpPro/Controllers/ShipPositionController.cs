using AutoMapper;
using LinkUpPro.Core.Application.DTOs.Battleship;
using LinkUpPro.Core.Application.Interfaces;
using LinkUpPro.Core.Application.ViewModels.Battleship;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace LinkUpPro.Controllers
{
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
        public IActionResult PlaceShip(int matchId)
        {
            ViewBag.MatchId = matchId;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> PlaceShip(ShipPositionViewModel vm)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(vm);

                var dto = mapper.Map<ShipPositionDTO>(vm);

                await shipService.PlaceShipsAsync(
                    vm.BattleshipMatchId,
                    vm.UserId,
                    dto);

                TempData["Success"] = "Barco colocado correctamente.";

                return RedirectToAction(nameof(MyShips),
                    new
                    {
                        matchId = vm.BattleshipMatchId,
                        playerId = vm.UserId
                    });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;

                return View(vm);
            }
        }

        public async Task<IActionResult> MyShips(int matchId, string playerId)
        {
            try
            {
                var ships = await shipService.GetByPlayerAsync(
                    matchId,
                    playerId);

                var vm = mapper.Map<List<ShipPositionViewModel>>(ships);

                return View(vm);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;

                return RedirectToAction("Index", "BattleshipMatch");
            }
        }

        public async Task<IActionResult> MatchShips(int matchId)
        {
            try
            {
                var ships = await shipService.GetByMatchAsync(matchId);

                var vm = mapper.Map<List<ShipPositionViewModel>>(ships);

                return View(vm);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;

                return RedirectToAction("Details",
                    "BattleshipMatch",
                    new { id = matchId });
            }
        }

        public async Task<IActionResult> AreShipsPlaced(int matchId, string playerId)
        {
            try
            {
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
        public async Task<IActionResult> Ready(int matchId)
        {
            try
            {
                var match = await _matchService.GetByIdAsync(matchId);

                if (match == null)
                {
                    TempData["Error"] = "La partida no existe.";
                    return RedirectToAction("Index", "BattleshipMatch");
                }

                bool player1Ready = await shipService.AreShipsPlacedAsync(matchId, match.Player1Id);
                bool player2Ready = await shipService.AreShipsPlacedAsync(matchId, match.Player2Id);

                if (!player1Ready || !player2Ready)
                {
                    TempData["Info"] = "Ambos jugadores deben colocar todos sus barcos.";

                    return RedirectToAction("Details", "BattleshipMatch", new { id = matchId });
                }

                await _matchService.StartMatchAsync(matchId);

                TempData["Success"] = "La partida ha comenzado.";

                return RedirectToAction("Play", "Attack", new { matchId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;

                return RedirectToAction("Details", "BattleshipMatch", new { id = matchId });
            }
        }
    }
}