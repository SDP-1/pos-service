using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using pos_service.Services;
using pos_service.Models.Enums;
using pos_service.Exceptions;

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
            _permission         = permission;
            _currentUserService = currentUserService;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            try
            {
                _currentUserService.EnsureAuthenticated();

                if (!_currentUserService.HasPermission(_permission))
                {
                    // throw PermissionDeniedException so callers can handle it explicitly
                    throw new pos_service.Exceptions.PermissionDeniedException($"You does not have required permission: {_permission}");
                }
            }
            catch (PermissionDeniedException) {
                throw;
            }
            catch (UnauthorizedAccessException)
            {
                // not authenticated -> forbid
                context.Result = new ForbidResult();
            }

            await next();
        }
    }
}
