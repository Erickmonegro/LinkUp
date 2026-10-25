using AutoMapper;
using LinkUpPro.Core.Application.DTOs.ReactionDTO;
using LinkUpPro.Core.Application.Interfaces;
using LinkUpPro.Core.Domain.Entities;
using LinkUpPro.Core.Domain.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LinkUpPro.Core.Application.Services
{
    public class ReactionService : GenericService<Reaction, ReactionDTO>, IReactionService
    {
        private readonly IReactionRepository _repository;
        private readonly IMapper _mapper;

        public ReactionService(IReactionRepository repository, IMapper mapper) : base(repository, mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<(int Likes, int Dislikes)> GetReactionCountsAsync(int postId)
        {
            var query = _repository.GetAllQuery().Where(r => r.PostId == postId).ToList();
            var likes = query.Count(r => r.ReactionType == LinkUpPro.Core.Domain.Common.Enums.ReactionType.Like);
            var dislikes = query.Count(r => r.ReactionType == LinkUpPro.Core.Domain.Common.Enums.ReactionType.Dislike);
            return (likes, dislikes);
        }

        public async Task<bool> ToggleReactionAsync(int postId, string userId, bool isLike)
        {
            var reactionType = isLike 
                ? LinkUpPro.Core.Domain.Common.Enums.ReactionType.Like 
                : LinkUpPro.Core.Domain.Common.Enums.ReactionType.Dislike;
                
            var existingReaction = await _repository.GetReactionByUserAndPostAsync(userId, postId);

            if (existingReaction != null)
            {
                if (existingReaction.ReactionType == reactionType)
                {
                    await _repository.DeleteAsync(existingReaction.Id);
                }
                else
                {
                    existingReaction.ReactionType = reactionType;
                    await _repository.UpdateAsync(existingReaction.Id, existingReaction);
                }
            }
            else
            {
                var newReaction = new Reaction
                {
                    Id = 0,
                    PostId = postId,
                    UserId = userId,
                    ReactionType = reactionType
                };
                await _repository.AddAsync(newReaction);
            }

            return true;
        }

        public async Task<bool> DeleteReactionAsync(int postId, string userId)
        {
            var existingReaction = await _repository.GetReactionByUserAndPostAsync(userId, postId);
            if (existingReaction != null)
            {
                await _repository.DeleteAsync(existingReaction.Id);
                return true;
            }
            return false;
        }
    }
}
