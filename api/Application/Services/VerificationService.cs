using System.Security.Cryptography;
using Drink.Application.Constants;
using Drink.Application.Interfaces;
using Drink.Application.Mappings;
using Drink.Application.Models;
using Drink.Application.Requests.Admin;
using Drink.Application.Responses;
using Drink.Application.Responses.Admin;
using Drink.Domain.Entities;
using Drink.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Drink.Application.Services;

public class VerificationService : BaseService
{
  private readonly IGenericRepository<VerificationEmail> _verificationRepo;

  public VerificationService(
    ICurrentUserContext currentUser,
    IGenericRepository<VerificationEmail> verificationRepo) : base(currentUser)
  {
    _verificationRepo = verificationRepo;
  }

  /// <summary>
  /// 依類型取得驗證信列表（後台用）
  /// </summary>
  public async Task<ApiResponse<PaginationList<VerificationListResponse>>> GetList(
    VerificationEmailType type, int page, int pageSize, string? sortBy, string? sortOrder,
    string? keyword, bool? isSuccess, bool? isUsed)
  {
    var result = await _verificationRepo.GetPaginationList(
      page, pageSize,
      predicate: v =>
        v.Type == type &&
        (keyword == null || v.User.Name.Contains(keyword) || v.User.Email.Contains(keyword)) &&
        (isSuccess == null || v.IsSuccess == isSuccess) &&
        (isUsed == null || v.IsUsed == isUsed),
      include: q => q.Include(v => v.User),
      order: q => BuildOrder(q, sortBy, sortOrder));

    var mapped = new PaginationList<VerificationListResponse>
    {
      Items = result.Items.Select(v => v.ToVerificationListResponse()).ToList(),
      Total = result.Total,
      Page = result.Page,
      PageSize = result.PageSize
    };

    return Success(mapped);
  }

  /// <summary>
  /// 重發單筆驗證信
  /// </summary>
  public async Task<ApiResponse> Resend(int verificationId)
  {
    var original = await _verificationRepo.GetById(verificationId, include: q => q.Include(v => v.User));

    if (original is null)
      return Fail(ErrorCodes.NotFound, "驗證信紀錄不存在");

    // 註冊驗證信只能重發給未驗證的用戶
    if (original.Type == VerificationEmailType.Register && original.User.EmailVerified)
      return Fail(ErrorCodes.ValidationError, "該用戶已完成 Email 驗證，無需重發");

    // 10 分鐘內重發限制
    if (await IsResendTooFrequent(original.UserId, original.Type))
      return Fail(ErrorCodes.ResendTooFrequent, "同一用戶 10 分鐘內已重發過，請稍後再試");

    await CreateAndSendVerification(original.UserId, original.Type);

    return Success();
  }

  /// <summary>
  /// 批量重發驗證信
  /// </summary>
  public async Task<ApiResponse<BatchResendResponse>> BatchResend(VerificationEmailType type, BatchResendRequest request)
  {
    var originals = await _verificationRepo.GetList(
      predicate: v => request.Ids.Contains(v.Id) && v.Type == type,
      include: q => q.Include(v => v.User));

    var response = new BatchResendResponse();
    var skippedIds = new List<int>();

    foreach (var original in originals)
    {
      // 註冊驗證信跳過已驗證的用戶
      if (type == VerificationEmailType.Register && original.User.EmailVerified)
      {
        skippedIds.Add(original.Id);
        continue;
      }

      // 10 分鐘內重發限制
      if (await IsResendTooFrequent(original.UserId, original.Type))
      {
        skippedIds.Add(original.Id);
        continue;
      }

      await CreateAndSendVerification(original.UserId, original.Type);
      response.SuccessCount++;
    }

    response.SkipCount = skippedIds.Count;
    response.SkippedIds = skippedIds;

    return Success(response);
  }

  private async Task<bool> IsResendTooFrequent(int userId, VerificationEmailType type)
  {
    var tenMinutesAgo = DateTime.UtcNow.AddMinutes(-10);

    return await _verificationRepo.Any(v =>
      v.UserId == userId &&
      v.Type == type &&
      v.SentAt > tenMinutesAgo);
  }

  private async Task CreateAndSendVerification(int userId, VerificationEmailType type)
  {
    var token = GenerateToken();
    var expiresAt = type == VerificationEmailType.Register
      ? DateTime.UtcNow.AddHours(24)
      : DateTime.UtcNow.AddHours(1);

    var verification = new VerificationEmail
    {
      UserId = userId,
      Type = type,
      Token = token,
      ExpiresAt = expiresAt,
      IsUsed = false,
      IsSuccess = true, // TODO: 實際發送郵件後根據結果設定
      SentAt = DateTime.UtcNow
    };

    await _verificationRepo.Insert(verification);

    // TODO: 實際發送郵件邏輯（目前僅建立紀錄）
  }

  private static string GenerateToken()
  {
    return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
      .Replace("+", "-")
      .Replace("/", "_")
      .TrimEnd('=');
  }

  private static IQueryable<VerificationEmail> BuildOrder(IQueryable<VerificationEmail> query, string? sortBy, string? sortOrder)
  {
    var isDesc = string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase);

    var ordered = sortBy?.ToLower() switch
    {
      "id" => isDesc ? query.OrderByDescending(v => v.Id) : query.OrderBy(v => v.Id),
      "sent_at" => isDesc ? query.OrderByDescending(v => v.SentAt) : query.OrderBy(v => v.SentAt),
      "expires_at" => isDesc ? query.OrderByDescending(v => v.ExpiresAt) : query.OrderBy(v => v.ExpiresAt),
      _ => isDesc ? query.OrderByDescending(v => v.SentAt) : query.OrderBy(v => v.SentAt)
    };
    return ordered.ThenBy(v => v.Id);
  }
}
