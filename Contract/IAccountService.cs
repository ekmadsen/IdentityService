using System.Threading.Tasks;
using ErikTheCoder.Identity.Contract.Requests;
using ErikTheCoder.Identity.Contract.Responses;
using Refit;


namespace ErikTheCoder.Identity.Contract
{
    public interface IAccountService
    {
        // Refit requires a string literal URL, not a constant.  Ensure the implementing service uses the same URL.
        // See https://stackoverflow.com/questions/47537005/how-to-share-service-method-url-between-refit-and-web-api
        [Post("/account/login")]
        Task<User> LoginAsync([Body] LoginRequest Request);


        [Post("/account/register")]
        Task<RegisterResponse> RegisterAsync([Body] RegisterRequest Request);


        [Post("/account/confirm")]
        Task ConfirmAsync([Body] ConfirmRequest Request);


        [Post("/account/forgotpassword")]
        Task ForgotPasswordAsync([Body] ForgotPasswordRequest Request);


        [Post("/account/resetpassword")]
        Task<ResetPasswordResponse> ResetPasswordAsync([Body] ResetPasswordRequest Request);
    }
}
