namespace Core.Application.Model.Features;

public class SeihSection
{
   public string Label { get; set; } = default!;
    public List<SeihField> Fields { get; set; } = new();
}
