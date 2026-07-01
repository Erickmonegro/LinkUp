using LinkUpPro.Core.Application.DTOs.Battleship;

namespace LinkUpPro.Core.Application.Interfaces
{
    public interface IAttackService
    {
        Task<AttackDTO> AttackAsync(AttackDTO dto);
        Task<List<AttackDTO>> GetByMatchAsync(int matchId);
    }
}