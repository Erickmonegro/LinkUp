using AutoMapper;
using LinkUpPro.Core.Application.Interfaces;
using LinkUpPro.Core.Application.ViewModels.Battleship;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace LinkUpPro.Controllers
{
    public class BattleshipController : Controller
    {
        private readonly ILogger<BattleshipController> _logger;
        private readonly IBattleshipMatchService matchService;
        private readonly IMapper mapper;

        public BattleshipController(ILogger<BattleshipController> logger, IBattleshipMatchService matchService, IMapper mapper)
        {
            _logger = logger;
            this.matchService = matchService;
            this.mapper = mapper;
        }

        public async Task<IActionResult> Index(string playerId)
        {
            try
            {
                var matches = await matchService.GetByPlayerAsync(playerId);

                var vm = mapper.Map<List<BattleshipMatchViewModel>>(matches);

                return View(vm);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;

                return View(new List<BattleshipMatchViewModel>());
            }
        }

        public IActionResult Create()
        {
            return View();
        }

        public async Task<IActionResult> GetActive(string playerId)
        {
            try
            {
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
                if (!ModelState.IsValid)
                    return View(vm);

                var player1Id = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (player1Id == null)
                    throw new Exception("Debe iniciar sesión.");

                await matchService.CreateMatchAsync(player1Id, vm.Player2Id);

                TempData["Success"] = "Partida creada correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;

                return View(vm);
            }
        }

        [HttpPut]
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

        [HttpPut]
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