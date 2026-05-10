using Core.Domain.Entity;
using Core.Domain.Entity.SEIH;

namespace Core.Application.Interface.Repository.SEIH;

public interface IRecordFieldValueRepository
{
    Task CreateAsync(RecordFieldValueEntity value);
    Task UpdateAsync(RecordFieldValueEntity value);
    Task<List<RecordFieldValueEntity>> GetByRecordIdAsync(Guid recordId);
}
