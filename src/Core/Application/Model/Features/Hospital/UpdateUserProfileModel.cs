namespace Core.Application.Model.Features.Hospital;

public class UpdateUserProfileModel
{
    public Guid UserId { get; set; }
    public string? NewPassword { get; set; }
    public Guid? RoleId { get; set; }
    public bool? IsActive { get; set; }
}
