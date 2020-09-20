using System.Threading.Tasks;
using ErikTheCoder.Identity.Contract.Requests;
using ErikTheCoder.Identity.Contract.Responses;
using ErikTheCoder.ServiceContract;
using JetBrains.Annotations;
using Refit;


namespace ErikTheCoder.Identity.Contract
{
    [UsedImplicitly]
    public interface IAccountService
    {
        // Refit requires a string literal URL, not a constant.  Ensure the implementing service uses the same URL.
        // See https://stackoverflow.com/questions/47537005/how-to-share-service-method-url-between-refit-and-web-api
        [UsedImplicitly]
        [Post("/account/login")]
        Task<User> LoginAsync([Body] LoginRequest Request);


        [UsedImplicitly]
        [Post("/account/register")]
        Task<RegisterResponse> RegisterAsync([Body] RegisterRequest Request);


        //[UsedImplicitly]
        //[Post("/account/confirm")]
        //Task ConfirmAsync([Body] ConfirmRequest Request);


        //[UsedImplicitly]
        //[Post("/account/forgotpassword")]
        //Task ForgotPasswordAsync([Body] ForgotPasswordRequest Request);


        //[UsedImplicitly]
        //[Post("/account/resetpassword")]
        //Task<ResetPasswordResponse> ResetPasswordAsync([Body] ResetPasswordRequest Request);
    }
}
