using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Drink.Infrastructure.Data;

public class BaseDataEntity
{
  [Key]
  [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
  public virtual int Id { get; set; }
}
public interface ICreatedEntity
{
  public DateTime CreatedAt { get; set; }
}
public interface IUpdatedEntity
{
  public DateTime UpdatedAt { get; set; }
}