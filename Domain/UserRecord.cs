using JetBrains.Annotations;


namespace ErikTheCoder.Identity.Domain
{
    internal class UserRecord
    {
        [UsedImplicitly] public int Id;
        [UsedImplicitly] public string Username;
        [UsedImplicitly] public int PasswordManagerVersion;
        [UsedImplicitly] public string Salt;
        [UsedImplicitly] public string PasswordHash;
        [UsedImplicitly] public string EmailAddress;
        [UsedImplicitly] public string FirstName;
        [UsedImplicitly] public string LastName;
    }
}
