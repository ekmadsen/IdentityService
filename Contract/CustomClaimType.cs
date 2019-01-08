namespace ErikTheCoder.Identity.Contract
{
    public static class CustomClaimType
    {
        private const string _namespace = "https://schema.erikthecoder.net/claims/";

        // Can't use string interpolation to assign a constant.
        public const string FirstName = _namespace + "firstname";
        public const string LastName = _namespace + "lastname";
        public const string SecurityToken = _namespace + "securitytoken";
    }
}
