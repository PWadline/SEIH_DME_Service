namespace Core.Application.Model.Features.Record;

public class ConsentDto
{
    public Guid Id { get; set; }
    public Guid RecordId { get; set; }
    public string? ConsentHash { get; set; } 
    public DateTime SignedAt { get; set; }
}
