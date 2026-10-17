using Ritma.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ritma.Api.Controllers;

[Authorize]
public abstract class BusinessScopedController : ControllerBase
{
    protected readonly ICurrentUser _currentUser;

    protected BusinessScopedController(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }

    protected bool HasBusinessAccess(int businessId) => _currentUser.BusinessId == businessId;
    protected int GetBusinessId() => _currentUser.BusinessId;
}
