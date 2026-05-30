using ClosedXML.Excel;
using Drink.Application.Constants;
using Drink.Application.Interfaces;
using Drink.Application.Responses;

namespace Drink.Application.Services;

public class AdminOrderExportService : BaseService
{
  private readonly AdminOrderService _orderService;

  private static readonly char[] UnsafeChars = ['\\', '/', ':', '*', '?', '"', '<', '>', '|'];

  public AdminOrderExportService(
    ICurrentUserContext currentUser,
    AdminOrderService orderService) : base(currentUser)
  {
    _orderService = orderService;
  }

  public async Task<ApiResponse<(byte[] Bytes, string FileName)>> ExportAsync(int orderId)
  {
    var order = await _orderService.GetOrderWithItemsAsync(orderId);
    if (order is null)
      return Fail<(byte[], string)>(ErrorCodes.OrderNotFound, "揪團不存在");

    var items = order.OrderItems
      .OrderBy(i => i.RecipientName)
      .ThenBy(i => i.CreatedAt)
      .ToList();

    using var workbook = new XLWorkbook();
    var ws = workbook.Worksheets.Add("訂單明細");

    // headers
    ws.Cell(1, 1).Value = "填單人";
    ws.Cell(1, 2).Value = "收件人";
    ws.Cell(1, 3).Value = "品項";
    ws.Cell(1, 4).Value = "尺寸";
    ws.Cell(1, 5).Value = "甜度";
    ws.Cell(1, 6).Value = "冰塊";
    ws.Cell(1, 7).Value = "加料";
    ws.Cell(1, 8).Value = "品項價";
    ws.Cell(1, 9).Value = "甜度加價";
    ws.Cell(1, 10).Value = "加料加價";
    ws.Cell(1, 11).Value = "小計";
    ws.Cell(1, 12).Value = "數量";
    ws.Cell(1, 13).Value = "實付金額";
    ws.Cell(1, 14).Value = "備註";
    ws.Cell(1, 15).Value = "建立時間";

    var row = 2;
    foreach (var item in items)
    {
      var toppings = string.Join("、", item.Toppings.Select(t => t.Topping.Name));
      ws.Cell(row, 1).Value = item.User.Name;
      ws.Cell(row, 2).Value = item.RecipientName;
      ws.Cell(row, 3).Value = item.MenuItem.DrinkItem.Name;
      ws.Cell(row, 4).Value = item.Size.Name;
      ws.Cell(row, 5).Value = item.Sugar.Name;
      ws.Cell(row, 6).Value = item.Ice.Name;
      ws.Cell(row, 7).Value = toppings;
      ws.Cell(row, 8).Value = item.ItemPrice;
      ws.Cell(row, 9).Value = item.SugarPrice;
      ws.Cell(row, 10).Value = item.ToppingPrice;
      ws.Cell(row, 11).Value = item.TotalPrice;
      ws.Cell(row, 12).Value = item.Quantity;
      ws.Cell(row, 13).Value = item.TotalPrice * item.Quantity;
      ws.Cell(row, 14).Value = item.Note ?? string.Empty;
      ws.Cell(row, 15).Value = item.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss");
      row++;
    }

    ws.Columns().AdjustToContents();

    using var ms = new MemoryStream();
    workbook.SaveAs(ms);
    var bytes = ms.ToArray();

    var safeTitle = SanitizeFileName(order.Title);
    var fileName = $"{safeTitle}_{DateTime.UtcNow:yyyyMMdd}.xlsx";

    return Success((bytes, fileName));
  }

  private static string SanitizeFileName(string title)
  {
    foreach (var c in UnsafeChars)
      title = title.Replace(c, '_');
    return title;
  }
}
