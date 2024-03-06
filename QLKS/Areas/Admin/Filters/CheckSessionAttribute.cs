using System.Web.Mvc;

public class CheckSessionAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext filterContext)
    {
        if (filterContext.HttpContext.Session.Contents["admin"] == null)
        {
            filterContext.Result = new RedirectResult("/Admin/Login/Index");
        }

        base.OnActionExecuting(filterContext);
    }

}
