using AutoMapper;
using LinkUpPro.Core.Application.Interfaces;
using LinkUpPro.Core.Domain.Interfaces;

namespace LinkUpPro.Core.Application.Services
{
    public class GenericService<T, TDto> : IGenericService<TDto>
        where T : class
        where TDto : class
    {
        private readonly IGenericRepository<T> _repository;
        private readonly IMapper _mapper;

        public GenericService(IGenericRepository<T> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }
        public virtual async Task<TDto?> AddAsync(TDto dto)
        {
            var entity = _mapper.Map<T>(dto);
            var result = await _repository.AddAsync(entity);
            return result != null ? _mapper.Map<TDto>(result) : null;
        }

        public virtual async Task<bool> DeleteAsync(int id)
        {
            await _repository.DeleteAsync(id);
            return true;
        }

        public virtual async Task<List<TDto>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public virtual async Task<TDto?> GetByIdAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            return entity != null ? _mapper.Map<TDto>(entity) : null;
        }

        public virtual async Task<TDto?> UpdateAsync(TDto dto, int id)
        {
            var entity = _mapper.Map<T>(dto);
            var result = await _repository.UpdateAsync(id, entity);
            return result != null ? _mapper.Map<TDto>(result) : null;
        }
    }
}
