using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Drink.RazorPageAdmin.Pages.Products;

public class Index : PageModel
{
  [BindProperty]
  public string ProductName { get; set; }

  public void OnGet()
  { }

  public IActionResult OnPostCreate()
  {
    // 模擬儲存延遲
    Thread.Sleep(500);

    // 為了演示，直接回傳一段新的 HTML 列
    var newId = new Random().Next(100, 999);
    var html = $@"
            <tr class='table-success'>
                <td>{newId}</td>
                <td>{ProductName}</td>
                <td>剛剛新增</td>
                <td>
                    <button class='btn btn-sm btn-danger' hx-post='/Products?handler=Delete' hx-swap='delete' hx-target='closest tr'>
                        刪除
                    </button>
                </td>
            </tr>";

    return Content(html);
  }

  public IActionResult OnPostDelete()
  {
    // 刪除邏輯...
    return new JsonResult(null); // 200 OK，配合 hx-swap="delete" 讓該行消失
  }
}