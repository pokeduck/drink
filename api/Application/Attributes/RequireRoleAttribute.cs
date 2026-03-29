namespace Drink.Application.Attributes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class RequireRoleAttribute : Attribute
{
  public int MenuId { get; }
  public CrudAction Action { get; }

  public RequireRoleAttribute(int menuId, CrudAction action)
  {
    MenuId = menuId;
    Action = action;
  }
}

public enum CrudAction
{
  Read,
  Create,
  Update,
  Delete
}
