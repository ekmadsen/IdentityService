namespace ErikTheCoder.Identity.Service.PasswordManagers
{
    public interface IPasswordManagerVersions
    {
        IPasswordManager this[int Version] { get; }
    }
}
