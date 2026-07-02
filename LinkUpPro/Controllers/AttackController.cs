using AutoMapper;
using LinkUpPro.Core.Application.DTOs.Battleship;
using LinkUpPro.Core.Application.Interfaces;
using LinkUpPro.Core.Application.Services;
using LinkUpPro.Core.Application.ViewModels.Battleship;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LinkUpPro.Core.Domain.Common.Enums;
using System.Diagnostics;

namespace LinkUpPro.Controllers
{
    [Authorize]
    public class AttackController : Controller
    {
        private readonly ILogger<AttackController> _logger;
        private readonly IAttackService attackService;
        private readonly IBattleshipMatchService matchService;
        private readonly IShipPositionService shipPositionService;
        private readonly IMapper mapper;
        private readonly IUserService userService;

        public AttackController(ILogger<AttackController> logger,
            IAttackService attackService, IBattleshipMatchService matchService, IMapper mapper, IShipPositionService shipPositionService, IUserService userService)
        {
            _logger = logger;
            this.attackService = attackService;
            this.matchService = matchService;
            this.mapper = mapper;
            this.shipPositionService = shipPositionService;
            this.userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> Play(int matchId)
        {
            try
            {
                bool timedOut = await matchService.CheckAndProcessTurnTimeoutForMatchAsync(matchId);
                if (timedOut)
                {
                    TempData["Warning"] = "La partida finalizó por abandono al transcurrir 48 horas sin realizar un ataque válido.";
                    return RedirectToAction("Details", "Battleship", new { id = matchId });
                }

                var match = await matchService.GetByIdAsync(matchId);

                if (match == null)
                {
                    TempData["Error"] = "La partida no existe.";
                    return RedirectToAction("Index", "Battleship");
                }

                if (match.Status == MatchStatus.Finished || match.Status == MatchStatus.Rejected)
                {
                    return RedirectToAction("Details", "Battleship", new { id = matchId });
                }

                var ships = await shipPositionService.GetByMatchAsync(matchId);
                var attacks = await attackService.GetByMatchAsync(matchId);

                var matchVm = mapper.Map<BattleshipMatchViewModel>(match);
                var p1 = await userService.GetUserById(matchVm.Player1Id);
                var p2 = await userService.GetUserById(matchVm.Player2Id);
                var turnUser = !string.IsNullOrEmpty(matchVm.CurrentTurnUserId) ? await userService.GetUserById(matchVm.CurrentTurnUserId) : null;

                matchVm.Player1Name = p1 != null ? $"{p1.FirstName} {p1.LastName}" : matchVm.Player1Id;
                matchVm.Player2Name = p2 != null ? $"{p2.FirstName} {p2.LastName}" : matchVm.Player2Id;
                matchVm.CurrentTurnUserName = turnUser != null ? $"{turnUser.FirstName} {turnUser.LastName}" : matchVm.CurrentTurnUserId;

                GameBoardViewModel vm = new()
                {
                    Match = matchVm,
                    Ships = mapper.Map<List<ShipPositionViewModel>>(ships),
                    Attacks = mapper.Map<List<AttackViewModel>>(attacks)
                };

                return View(vm);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index", "Battleship");
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

                var updatedMatch = await matchService.GetByIdAsync(vm.MatchId);
                if (updatedMatch != null && updatedMatch.Status == MatchStatus.Finished)
                {
                    TempData["Success"] = "¡Victoria! Has hundido toda la flota enemiga.";
                    return RedirectToAction("Details", "Battleship", new { id = vm.MatchId });
                }

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