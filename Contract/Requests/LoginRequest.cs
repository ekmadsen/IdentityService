using JetBrains.Annotations;


namespace ErikTheCoder.Identity.Contract.Requests
{
    public class LoginRequest

    {
        [UsedImplicitly] public string Username { get; set; }
        [UsedImplicitly] public string Password { get; set; }
    }
}
