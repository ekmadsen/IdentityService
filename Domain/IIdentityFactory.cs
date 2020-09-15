using ErikTheCoder.ServiceContract;


namespace ErikTheCoder.Identity.Domain
{
    public interface IIdentityFactory
    {
        User CreateUser();
    }
}