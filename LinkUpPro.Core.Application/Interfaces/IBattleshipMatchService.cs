using LinkUpPro.Core.Application.DTOs.Battleship;

namespace LinkUpPro.Core.Application.Interfaces
{
    public interface IBattleshipMatchService
    {
        Task<BattleshipMatchDTO> CreateMatchAsync(string player1Id, string player2Id);
        Task<BattleshipMatchDTO> StartMatchAsync(int matchId);
        Task FinishMatchAsync(int matchId, string winnerId);
        Task<BattleshipMatchDTO?> GetActiveMatchAsync(string playerId);
        Task<List<BattleshipMatchDTO>> GetByPlayerAsync(string playerId);
        Task<BattleshipMatchDTO?> GetByIdAsync(int id);
        Task ChangeTurnAsync(int matchId);
        Task<bool> SurrenderAsync(int matchId, string playerId);
        Task<BattleshipMatchDTO> AcceptChallengeAsync(int matchId, string playerId);
        Task<BattleshipMatchDTO> RejectChallengeAsync(int matchId, string playerId);
        Task<bool> CheckAndProcessTurnTimeoutsAsync(string? playerId = null);
        Task<bool> CheckAndProcessTurnTimeoutForMatchAsync(int matchId);
    }
}