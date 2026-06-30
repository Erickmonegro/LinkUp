using LinkUpPro.Core.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkUpPro.Core.Application.Interfaces
{
    public interface IGenericService<TDto>        
    where TDto : class
    {
        Task<TDto?> AddAsync(TDto dto);
        Task<TDto?> UpdateAsync(TDto dto,int id);
        Task<bool> DeleteAsync(int id);
        Task<TDto?> GetByIdAsync(int id);
        Task<List<TDto>> GetAllAsync();     
    }
}
