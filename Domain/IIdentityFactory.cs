using ErikTheCoder.ServiceContract;
using JetBrains.Annotations;


namespace ErikTheCoder.Identity.Domain
{
    public interface IIdentityFactory
    {
        // Create domain classes.
        [UsedImplicitly] User CreateUser();


        // Create repository classes.
        [UsedImplicitly] IIdentityRepository CreateIdentityRepository();
    }
}