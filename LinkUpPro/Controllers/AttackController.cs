using AutoMapper;
using LinkUpPro.Core.Application.DTOs.Battleship;
using LinkUpPro.Core.Application.Interfaces;
using LinkUpPro.Core.Application.Services;
using LinkUpPro.Core.Application.ViewModels.Battleship;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace LinkUpPro.Controllers
{
    public class AttackController : Controller
    {
        private readonly ILogger<AttackController> _logger;
        private readonly IAttackService attackService;
        private readonly IBattleshipMatchService matchService;
        private readonly IShipPositionService shipPositionService;
        private readonly IMapper mapper;

        public AttackController(ILogger<AttackController> logger,
            IAttackService attackService, IBattleshipMatchService matchService, IMapper mapper, IShipPositionService shipPositionService)
        {
            _logger = logger;
            this.attackService = attackService;
            this.matchService = matchService;
            this.mapper = mapper;
            this.shipPositionService = shipPositionService;
        }

        [HttpGet]
        public async Task<IActionResult> Play(int matchId)
        {
            try
            {
                var match = await matchService.GetByIdAsync(matchId);

                if (match == null)
                {
                    TempData["Error"] = "La partida no existe.";

                    return RedirectToAction("Index", "BattleshipMatch");
                }

                var ships = await shipPositionService.GetByMatchAsync(matchId);
                var attacks = await attackService.GetByMatchAsync(matchId);

                GameBoardViewModel vm = new()
                {
                    Match = mapper.Map<BattleshipMatchViewModel>(match),
                    Ships = mapper.Map<List<ShipPositionViewModel>>(ships),
                    Attacks = mapper.Map<List<AttackViewModel>>(attacks)
                };

                return View();
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;

                return RedirectToAction("Index", "BattleshipMatch");
            }
        }

        [HttpGet]
        public async Task<IActionResult> History(int matchId)
        {
            try
            {
                var attacks = await attackService.GetByMatchAsync(matchId);

                var vm = mapper.Map<List<AttackViewModel>>(attacks);

                return View(vm);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;

                return RedirectToAction(nameof(Play), new { matchId });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Attack(AttackViewModel vm)
        {
            try
            {
                if (!ModelState.IsValid)
                    return RedirectToAction(nameof(Play), new { matchId = vm.MatchId });

                var dto = mapper.Map<AttackDTO>(vm);

                await attackService.AttackAsync(dto);

                TempData["Success"] = "Ataque realizado correctamente.";

                return RedirectToAction(nameof(Play), new { matchId = vm.MatchId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;

                return RedirectToAction(nameof(Play), new { matchId = vm.MatchId });
            }
        }

    }
}