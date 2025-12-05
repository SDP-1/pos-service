using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using pos_service.Services;
using pos_service.Models.Enums;

namespace pos_service.Authorization
{
    /// <summary>
    /// Attribute to require a specific permission for an action.
    /// Usage: [Permission(PermissionType.ORDER_VIEW)] on controller actions.
    /// </summary>
    public class PermissionAttribute : TypeFilterAttribute
    {
        public PermissionAttribute(PermissionType permission) : base(typeof(PermissionFilter))
        {
            Arguments = new object[] { permission };
        }
    }

    public class PermissionFilter : IAsyncActionFilter
    {
        private readonly PermissionType _permission;
        private readonly ICurrentUserService _currentUserService;

        public PermissionFilter(PermissionType permission, ICurrentUserService currentUserService)
        {
            _permission = permission;
            _currentUserService = currentUserService;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            try
            {
                _currentUserService.EnsureAuthenticated();

                if (!_currentUserService.HasPermission(_permission))
                {
                    context.Result = new ForbidResult();
                    return;
                }
            }
            catch (UnauthorizedAccessException)
            {
                context.Result = new ForbidResult();
                return;
            }

            await next();
        }
    }
}
