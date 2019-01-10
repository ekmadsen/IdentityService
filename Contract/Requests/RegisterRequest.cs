using JetBrains.Annotations;


namespace ErikTheCoder.Identity.Contract.Requests
{
    public class RegisterRequest

    {
        [UsedImplicitly] public string Username { get; set; }
        [UsedImplicitly] public string Password { get; set; }
        [UsedImplicitly] public string EmailAddress { get; set; }
        [UsedImplicitly] public string FirstName { get; set; }
        [UsedImplicitly] public string LastName { get; set; }
    }
}
