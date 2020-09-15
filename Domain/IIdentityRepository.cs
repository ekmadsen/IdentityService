using System.Threading.Tasks;
using ErikTheCoder.Identity.Contract.Requests;
using ErikTheCoder.Identity.Contract.Responses;
using ErikTheCoder.ServiceContract;


namespace ErikTheCoder.Identity.Domain
{
    public interface IIdentityRepository
    {
        Task<User> LoginAsync(LoginRequest Request);
        Task<RegisterResponse> RegisterAsync(RegisterRequest Request);
    }
}
