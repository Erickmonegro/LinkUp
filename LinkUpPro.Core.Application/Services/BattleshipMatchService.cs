using AutoMapper;
using LinkUpPro.Core.Application.DTOs.Battleship;
using LinkUpPro.Core.Application.Interfaces;
using LinkUpPro.Core.Domain.Common.Enums;
using LinkUpPro.Core.Domain.Entities;
using LinkUpPro.Core.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LinkUpPro.Core.Application.Services
{
    public class BattleshipMatchService : IBattleshipMatchService
    {
        private readonly IBattleshipMatchRepository repo;
        private readonly IShipPositionRepository _shipRepository;
        private readonly IMapper _mapper;
        public BattleshipMatchService(IBattleshipMatchRepository repo, IShipPositionRepository _shipRepository, IMapper _mapper)
        {
            this.repo = repo;
            this._shipRepository = _shipRepository;
            this._mapper = _mapper;
        }

        public async Task ChangeTurnAsync(int matchId)
        {
            var match = await repo.GetByIdAsync(matchId);

            if (match == null)
                throw new Exception("La partida no existe");

            match.CurrentTurnUserId = match.CurrentTurnUserId == match.Player1Id ? match.Player2Id : match.Player1Id;

            match.TurnAssignedAt = DateTime.UtcNow;

            await repo.UpdateAsync(match.Id, match);
        }

        public async Task<BattleshipMatchDTO> CreateMatchAsync(string player1Id, string player2Id)
        {
            BattleshipMatch match = new()
            {
                Id = 0,
                Player1Id = player1Id,
                Player2Id = player2Id,
                Status = MatchStatus.ChallengePending,
                CurrentTurnUserId = player1Id,
                TurnAssignedAt = DateTime.UtcNow
            };

            var created = await repo.AddAsync(match);

            return _mapper.Map<BattleshipMatchDTO>(created);
        }

        public async Task FinishMatchAsync(int matchId, string winnerId)
        {
            var match = await repo.GetByIdAsync(matchId);

            if (match == null)
                throw new Exception("La partida no existe");

            if (match.Status != MatchStatus.InProgress)
                throw new Exception("La partida no está en progreso.");

            match.Status = MatchStatus.Finished;
            match.WinnerId = winnerId;
            match.EndDate = DateTime.UtcNow;

            await repo.UpdateAsync(match.Id, match);
        }

        public async Task<BattleshipMatchDTO?> GetActiveMatchAsync(string playerId)
        {
            var match = await repo.GetActiveMatchesAsync(playerId);

            return _mapper.Map<BattleshipMatchDTO>(match);
        }

        public async Task<BattleshipMatchDTO?> GetByIdAsync(int id)
        {
            var match = await repo.GetByIdAsync(id);

            return _mapper.Map<BattleshipMatchDTO>(match);
        }

        public async Task<List<BattleshipMatchDTO>> GetByPlayerAsync(string playerId)
        {
            var matches = await repo.GetMatchHistoryForUserAsync(playerId);

            return _mapper.Map<List<BattleshipMatchDTO>>(matches);
        }

        public async Task<BattleshipMatchDTO> StartMatchAsync(int matchId)
        {
            var match = await repo.GetByIdAsync(matchId);

            if (match == null)
                throw new Exception("La partida no existe");

            var ships = await _shipRepository.GetByMatchIdAsync(matchId);

            bool player1Ready = ships.Any(x => x.UserId == match.Player1Id);
            bool player2Ready = ships.Any(x => x.UserId == match.Player2Id);

            if (!player1Ready || !player2Ready)
                throw new Exception("Ambos jugadores deben colocar sus barcos.");

            match.Status = MatchStatus.InProgress;
            match.TurnAssignedAt = DateTime.UtcNow;

            var updatedMatch = await repo.UpdateAsync(match.Id, match);

            return _mapper.Map<BattleshipMatchDTO>(updatedMatch);
        }

        public async Task<bool> SurrenderAsync(int matchId, string playerId)
        {
            var match = await repo.GetByIdAsync(matchId);

            if (match == null)
                throw new Exception("La partida no existe");

            match.Status = MatchStatus.Finished;
            match.WinnerId = playerId == match.Player1Id ? match.Player2Id : match.Player1Id;
            match.EndDate = DateTime.UtcNow;

            await repo.UpdateAsync(match.Id, match);

            return true;
        }

        public async Task<BattleshipMatchDTO> AcceptChallengeAsync(int matchId, string playerId)
        {
            var match = await repo.GetByIdAsync(matchId);
            if (match == null)
                throw new Exception("La partida no existe");

            if (match.Player2Id != playerId)
                throw new Exception("No tienes permiso para aceptar este desafío.");

            if (match.Status != MatchStatus.ChallengePending)
                throw new Exception("El desafío ya fue procesado o no está pendiente.");

            match.Status = MatchStatus.Pending;
            var updated = await repo.UpdateAsync(match.Id, match);
            return _mapper.Map<BattleshipMatchDTO>(updated);
        }

        public async Task<BattleshipMatchDTO> RejectChallengeAsync(int matchId, string playerId)
        {
            var match = await repo.GetByIdAsync(matchId);
            if (match == null)
                throw new Exception("La partida no existe");

            if (match.Player2Id != playerId && match.Player1Id != playerId)
                throw new Exception("No tienes permiso para rechazar o cancelar este desafío.");

            if (match.Status != MatchStatus.ChallengePending)
                throw new Exception("El desafío ya fue procesado o no está pendiente.");

            match.Status = MatchStatus.Rejected;
            match.EndDate = DateTime.UtcNow;
            var updated = await repo.UpdateAsync(match.Id, match);
            return _mapper.Map<BattleshipMatchDTO>(updated);
        }

        public async Task<bool> CheckAndProcessTurnTimeoutsAsync(string? playerId = null)
        {
            List<BattleshipMatch> matchesToCheck;
            if (!string.IsNullOrEmpty(playerId))
            {
                matchesToCheck = await repo.GetActiveMatchesAsync(playerId);
            }
            else
            {
                var all = await repo.GetAllListAsync();
                matchesToCheck = all.Where(m => m.Status == MatchStatus.InProgress).ToList();
            }

            bool anyTimedOut = false;
            foreach (var match in matchesToCheck)
            {
                if (match.Status == MatchStatus.InProgress && DateTime.UtcNow >= match.TurnAssignedAt.AddHours(48))
                {
                    string winnerId = match.CurrentTurnUserId == match.Player1Id ? match.Player2Id : match.Player1Id;
                    await FinishMatchAsync(match.Id, winnerId);
                    anyTimedOut = true;
                }
            }
            return anyTimedOut;
        }

        public async Task<bool> CheckAndProcessTurnTimeoutForMatchAsync(int matchId)
        {
            var match = await repo.GetByIdAsync(matchId);
            if (match == null || match.Status != MatchStatus.InProgress)
                return false;

            if (DateTime.UtcNow >= match.TurnAssignedAt.AddHours(48))
            {
                string winnerId = match.CurrentTurnUserId == match.Player1Id ? match.Player2Id : match.Player1Id;
                await FinishMatchAsync(match.Id, winnerId);
                return true;
            }
            return false;
        }
    }
}