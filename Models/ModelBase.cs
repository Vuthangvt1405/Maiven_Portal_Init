namespace Maiven_Portal_Managment.Models;

public abstract class ModelBase
{
    public long Id { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
