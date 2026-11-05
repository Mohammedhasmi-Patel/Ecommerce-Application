using Ecommerce.Models;
namespace Ecommerce.Contracts;

public interface ITokenService
{
    public Task<string> GenerateToken(AspNetUsers user);
}
