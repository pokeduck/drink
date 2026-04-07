using System.ComponentModel.DataAnnotations;

namespace Drink.Application.Requests.Admin;

public class BatchResendRequest
{
  [Required]
  public List<int> Ids { get; set; } = [];
}
