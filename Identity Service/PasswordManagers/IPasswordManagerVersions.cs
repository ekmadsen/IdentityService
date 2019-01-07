namespace ErikTheCoder.IdentityService.PasswordManagers
{
    public interface IPasswordManagerVersions
    {
        IPasswordManager this[int Version] { get; }
    }
}
