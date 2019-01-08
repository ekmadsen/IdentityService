using System.Collections.Generic;
using System.Security.Claims;
using JetBrains.Annotations;


namespace ErikTheCoder.Identity.Contract
{
    [UsedImplicitly]
    public class User
    {
        public int Id { get; [UsedImplicitly] set; }
        public string Username { get; [UsedImplicitly] set; }
        public int PasswordManagerVersion { get; [UsedImplicitly] set; }
        public string Salt { get; [UsedImplicitly] set; }
        public string PasswordHash { get; [UsedImplicitly] set; }
        public string EmailAddress { get; [UsedImplicitly] set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DisplayName => $"{FirstName} {LastName}";
        public HashSet<string> Roles { get; [UsedImplicitly] set; }
        public string SecurityToken { get; set; }


        public User()
        {
            Roles = new HashSet<string>();
        }


        public List<Claim> GetClaims()
        {
            List<Claim> claims = new List<Claim>
            {
                // Include standard claims.
                new Claim(ClaimTypes.Name, Username),
                new Claim(ClaimTypes.Email, EmailAddress),
                // Include custom claims.
                new Claim(CustomClaimType.FirstName, FirstName),
                new Claim(CustomClaimType.LastName, LastName),
                new Claim(CustomClaimType.SecurityToken, SecurityToken ?? string.Empty)
            };
            // Include roles.
            foreach (string role in Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            return claims;
        }


        public static User ParseClaims(IEnumerable<Claim> Claims)
        {
            User user = new User();
            foreach (Claim claim in Claims)
            {
                // ReSharper disable once SwitchStatementMissingSomeCases
                switch (claim.Type)
                {
                    case ClaimTypes.Name:
                        user.Username = claim.Value;
                        break;
                    case ClaimTypes.Email:
                        user.EmailAddress = claim.Value;
                        break;
                    case ClaimTypes.Role:
                        user.Roles.Add(claim.Value);
                        break;
                    case CustomClaimType.FirstName:
                        user.FirstName = claim.Value;
                        break;
                    case CustomClaimType.LastName:
                        user.LastName = claim.Value;
                        break;
                    case CustomClaimType.SecurityToken:
                        user.SecurityToken = claim.Value;
                        break;
                }
            }
            return user;
        }
    }
}
