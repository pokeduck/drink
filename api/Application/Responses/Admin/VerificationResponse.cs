namespace Drink.Application.Responses.Admin;

public class VerificationListResponse
{
  public int Id { get; set; }
  public int UserId { get; set; }
  public string UserName { get; set; } = null!;
  public string UserEmail { get; set; } = null!;
  public bool IsSuccess { get; set; }
  public bool IsUsed { get; set; }
  public DateTime SentAt { get; set; }
  public DateTime ExpiresAt { get; set; }
}

public class BatchResendResponse
{
  public int SuccessCount { get; set; }
  public int SkipCount { get; set; }
  public List<int> SkippedIds { get; set; } = [];
}
