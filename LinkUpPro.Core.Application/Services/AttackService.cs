using AutoMapper;
using LinkUpPro.Core.Application.DTOs.Battleship;
using LinkUpPro.Core.Application.Helpers;
using LinkUpPro.Core.Application.Interfaces;
using LinkUpPro.Core.Domain.Common.Enums;
using LinkUpPro.Core.Domain.Entities;
using LinkUpPro.Core.Domain.Interfaces;


namespace LinkUpPro.Core.Application.Services
{
    public class AttackService : IAttackService
    {
        private readonly IAttackRepository repo;
        private readonly IShipPositionRepository _shipPositionRepository;
        private readonly IBattleshipMatchService _matchService;
        private readonly IBattleshipMatchRepository _matchRepository;
        private readonly IMapper mapper;

        public AttackService(IAttackRepository repo, IShipPositionRepository _shipPositionRepository,
            IBattleshipMatchService _matchService, IBattleshipMatchRepository _matchRepository, IMapper mapper)
        {
            this.repo = repo;
            this._shipPositionRepository = _shipPositionRepository;
            this._matchService = _matchService;
            this._matchRepository = _matchRepository;
            this.mapper = mapper;
        }

        public async Task<AttackDTO> AttackAsync(AttackDTO dto)
        {
            var match = await _matchRepository.GetByIdAsync(dto.MatchId);
            if (match == null)
                throw new Exception("La partida no existe");

            if (match.Status != MatchStatus.InProgress)
                throw new Exception("La partida ya finalizó y no se permiten más intentos.");

            await ValidateTurnExpirationAsync(match);

            var turn = match.CurrentTurnUserId;

            if (turn != dto.AttackerId)
                throw new Exception("No es tu turno para atacar");

            bool attacked = await repo.ExistsAttackAsync(dto.MatchId, dto.AttackerId, dto.X, dto.Y);

            if (attacked)
                throw new Exception("Esta posicion ya fue atacada");

            string enemyId =
                dto.AttackerId == match.Player1Id
                ? match.Player2Id : match.Player1Id;

            var ships = await _shipPositionRepository.GetByPlayerAsync(dto.MatchId, enemyId);

            bool hit = false;

            foreach (var ship in ships)
            {
                if (ShipHelper.IsHit(ship, dto.X, dto.Y))
                {
                    hit = true;

                    ship.Hits++;

                    if (ship.Hits >= ship.ShipSize)
                    {
                        ship.IsSunk = true;
                    }

                    await _shipPositionRepository.UpdateAsync(ship.Id, ship);
                    break;
                }
            }

            var attack = new Attack
            {
                Id = 0,
                MatchId = dto.MatchId,
                AttackerId = dto.AttackerId,
                X = dto.X,
                Y = dto.Y,
                IsHit = hit
            };

            attack = await repo.AddAsync(attack);

            bool winner = ships.All(s => s.IsSunk);

            if (winner)
            {
                await _matchService.FinishMatchAsync(dto.MatchId, dto.AttackerId);
            }
            else
            {
                await _matchService.ChangeTurnAsync(dto.MatchId);
            }

            return mapper.Map<AttackDTO>(attack);
        }

        public async Task<List<AttackDTO>> GetByMatchAsync(int matchId)
        {
            var attacks = await repo.GetByMatchAsync(matchId);

            return mapper.Map<List<AttackDTO>>(attacks);
        }

        private async Task ValidateTurnExpirationAsync(BattleshipMatch match)
        {
            if (match.Status != MatchStatus.InProgress)
                return;

            if (DateTime.Now <= match.TurnAssignedAt.AddHours(48))
                return;

            string winnerId = match.CurrentTurnUserId == match.Player1Id
                ? match.Player2Id
                : match.Player1Id;

            await _matchService.FinishMatchAsync(match.Id, winnerId);

            throw new Exception("La partida finalizó por abandono al transcurrir 48 horas sin realizar un ataque válido.");
        }
    }
}