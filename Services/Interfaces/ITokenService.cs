using Maiven_Portal_Managment.Models;
using Maiven_Portal_Managment.Services.Security;

namespace Maiven_Portal_Managment.Services.Interfaces;

public interface ITokenService
{
    AccessTokenResult CreateAccessToken(
        UserModel user,
        IReadOnlyCollection<string> roleCodes);
}
