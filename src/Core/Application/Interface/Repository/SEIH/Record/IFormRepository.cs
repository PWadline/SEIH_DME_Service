// using Core.Domain.Entity;
// using Core.Domain.Entity.SEIH;

// namespace Core.Application.Interface.Repository.SEIH;

// public interface IFormRepository
// {
//     Task<List<FormEntity>> GetAllAsync();
//     Task<FormEntity?> GetByIdAsync(Guid id);
//     Task<List<FormEntity>> GetByHospitalIdAsync(Guid hospitalId);
//     Task AddAsync(FormEntity entity);
//     Task UpdateAsync(FormEntity entity);
//     Task DeleteAsync(Guid id);
// }

using Core.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Application.Interface.Repository.SEIH
{
    public interface IFormRepository
    {
        Task AddAsync(FormEntity entity);
        Task UpdateAsync(FormEntity entity);
        Task<FormEntity?> GetByIdAsync(Guid id);
        Task<List<FormEntity>> GetByHospitalIdAsync(Guid hospitalId);
        Task DeleteAsync(Guid id);
    }
}