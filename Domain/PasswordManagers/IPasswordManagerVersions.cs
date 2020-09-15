namespace ErikTheCoder.Identity.Domain.PasswordManagers
{
    public interface IPasswordManagerVersions
    {
        IPasswordManager this[int Version] { get; }
    }
}
