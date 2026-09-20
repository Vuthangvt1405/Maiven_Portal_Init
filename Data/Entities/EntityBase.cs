namespace Maiven_Portal_Managment.Data.Entities;

public abstract class EntityBase
{
    public long Id { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
