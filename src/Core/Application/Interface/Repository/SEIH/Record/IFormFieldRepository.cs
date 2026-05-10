using Core.Domain.Entity;
using Core.Domain.Entity.SEIH;

namespace Core.Application.Interface.Repository.SEIH;

public interface IFormFieldRepository
{
    Task<List<FormFieldEntity>> GetByFormIdAsync(Guid formId);
    Task CreateAsync(FormFieldEntity entity);
    Task UpdateAsync(FormFieldEntity entity);
}

