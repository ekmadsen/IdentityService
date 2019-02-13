# IdentityService
Manages user registration, confirmation, credentials, roles, claims, and profile.


## Motivation

I was motivated to write my own Identity service for the following reasons.

1.  I wanted to control access to website and service controller actions using [HTTP cookies](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/cookie?view=aspnetcore-2.2) and [JWT tokens](https://docs.microsoft.com/en-us/dotnet/standard/microservices-architecture/secure-net-microservices-web-applications/#consume-security-tokens).
2.  To accomplish this, I knew I had to write custom ASP.NET Core [policies](https://docs.microsoft.com/en-us/aspnet/core/security/authorization/policies?view=aspnetcore-2.2) that examine the [claims](https://docs.microsoft.com/en-us/aspnet/core/security/authorization/claims?view=aspnetcore-2.2) returned by an Identity service. 
3.  Since I wanted to understand how to store user credentials safely and securely in a database...
4.  I decided to own the entire solution (custom Identity service returning a custom User class that's examined by custom policies) rather than take a dependency on Microsoft's or another third-party's Identity service.
5.  Regarding the "build versus buy versus download free component" decision, I justified writing my own [Logging](https://github.com/ekmadsen/Logging) and [AspNetCore.Middleware](https://github.com/ekmadsen/AspNetCore.Middleware) components because it required about the same effort to write my own components as to learn to use all the features of third-party components.  I could not justify writing my own Identity service with an "equal effort" argument- using a third-party service would have been much easier.  I justified my efforts on this programming project solely by the educational experience it afforded me.


## Features

* **Targets .NET Standard 2.0** so it may be used in .NET Core or .NET Framework runtimes.
* **Safely stores user credentials** as a Salt, PasswordHash, and PasswordManagerVersion.  Does *not* store user passwords, which minimizes severity of a security breach if a malicious actor gains access to the Identity database.
* **Enables multiple password manager versions** so password complexity and hashing can evolve over time.
* **Associates each user with a collection of roles and a collection of claims**.  These can be used by ASP.NET Core MVC or WebAPI clients to enforce [policies](https://docs.microsoft.com/en-us/aspnet/core/security/authorization/policies?view=aspnetcore-2.2).


## Installation

* Use SQL Server Management Studio to locate an existing database or create a new database.
* Run the [CreateDatabase.sql](https://github.com/ekmadsen/IdentityService/blob/master/CreateDatabase.sql) script to create the tables, views, and indices used by this solution. The script creates SQL objects in an "Identity" schema. Obviously, if you install this solution in a dedicated database there's no risk of colliding with the names of existing SQL objects. However, if you install this solution in an existing database the schema minimizes the risk of colliding with existing SQL objects.
* Use Visual Studio to build this solution then deploy it to an [IIS](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/iis/?view=aspnetcore-2.2) or [Azure](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/azure-apps/?view=aspnetcore-2.2) host.
* Reference this service in your web application via its [NuGet package](https://www.nuget.org/packages/ErikTheCoder.Identity.Contract/).


## Usage

Call the Identity service in your solution via an [IAccountService](https://github.com/ekmadsen/IdentityService/blob/master/Contract/IAccountService.cs)-typed [Refit](https://www.nuget.org/packages/Refit/) proxy.  See the [Refit GitHub site](https://github.com/reactiveui/refit) for an explanation of how to use Refit proxies and a detailed description of Refit's features and benefits.  In short, Refit provides strongly-typed C# classes for invoking service methods, whether you own (have source code for) the service endpoint or not.  It in no way precludes writing dynamically-typed JavaScript code (such as AJAX) to invoke the same service methods.  It provides the best of both worlds: strongly-typed server-to-server calls and dynamically-typed browser-to-server calls.

### Dependency Injection

In Startup.ConfigureServices, create a service proxy and inject the dependency:

```C#
// Create service proxies.
Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
IHttpContextAccessor httpContextAccessor = Services.BuildServiceProvider().GetRequiredService<IHttpContextAccessor>();
string accountServiceUrl = Program.AppSettings.ServiceProxies[Keys.IdentityServiceName].Url;
string accountServiceToken = Program.AppSettings.ServiceProxies[Keys.IdentityServiceName].Token;
IAccountService accountService = Proxy.For<IAccountService>(accountServiceUrl, accountServiceToken, () => httpContextAccessor.HttpContext.GetCorrelationId());
// Configure dependency injection.
Services.AddSingleton(typeof(IAppSettings), Program.AppSettings);
Services.AddSingleton(typeof(ILogger), logger);
Services.AddSingleton(typeof(IAccountService), accountService);
```

In the above code I use a custom authentication token.  See the [Usage](https://github.com/ekmadsen/AspNetCore.Middleware#usage) section of my AspNetCore.Middleware documentation for an explanation of custom authentication tokens.  Also, the above code uses my [ServiceProxy](https://github.com/ekmadsen/ServiceProxy) solution to generate Refit service proxies that automatically pass authentication tokens and logging correlation IDs.

In an ASP.NET Core MVC website's authentication controller, inject the IAccountService dependency:

```C#
namespace ErikTheCoder.MadPoker.Website.Controllers
{
    [Authorize(Policy = Policy.Admin)]
    public class AccountController : ControllerBase
    {
        private const string _invalidCredentialsMessage = "Invalid username or password.";
        private readonly IAccountService _accountService;


        public AccountController(IAppSettings AppSettings, ILogger Logger, IAccountService AccountService) :
            base(AppSettings, Logger)
        {
            _accountService = AccountService;
        }
```

### Website AccountController Actions

Write a Login action:

```C#
[AllowAnonymous]
[HttpGet]
public ViewResult Login(string ReturnUrl) => View(new LoginModel(ReturnUrl));


[AllowAnonymous]
[HttpPost]
public async Task<IActionResult> Login(LoginModel Model)
{
    LoginRequest request = new LoginRequest
    {
        Username = Model.Username,
        Password = Model.Password
    };
    User user = await _accountService.LoginAsync(request);
    if (user is null)
    {
        // Credentials are invalid.
        Logger.Log(CorrelationId, $"{Model.Username} user not authenticated.  {_invalidCredentialsMessage}");
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        ModelState.AddModelError(nameof(Model.Username), _invalidCredentialsMessage);
        return View(Model);
    }
    // Credentials are valid.
    // Create claims principal.
    ClaimsIdentity identity = new ClaimsIdentity(user.GetClaims(), CookieAuthenticationDefaults.AuthenticationScheme);
    ClaimsPrincipal principal = new ClaimsPrincipal(identity);
    // Persist claims principal in HTTP cookie.
    AuthenticationProperties authenticationProperties = new AuthenticationProperties {IsPersistent = Model.RememberMe};
    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authenticationProperties);
    Logger.Log(CorrelationId, $"{Model.Username} user authenticated.");
    return Redirect(Model.ReturnUrl ?? "/");
}
```

Write a Logout action:

```C#
[AllowAnonymous]
[HttpGet]
public async Task<ViewResult> Logout()
{
    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    Logger.Log(CorrelationId, $"{GetCallingUsername()} user logged out.");
    return View();
}
```

Write a Register action that enables a user to register an account:

```C#
[AllowAnonymous]
[HttpGet]
public ViewResult Register() =>  View(new RegisterModel());


[AllowAnonymous]
[HttpPost]
public async Task<IActionResult> Register(RegisterModel Model)
{
    RegisterRequest request = new RegisterRequest
    {
        Username = Model.Username,
        Password = Model.Password,
        EmailAddress = Model.EmailAddress,
        FirstName = Model.FirstName,
        LastName =  Model.LastName
    };
    RegisterResponse response = await _accountService.RegisterAsync(request);
    if (!response.PasswordValid)
    {
        // Password does not meet complexity requirements.
        string message = string.Join(Environment.NewLine, response.Messages);
        ModelState.AddModelError(nameof(Model.Password), message);
        return View(Model);
    }
    return RedirectToAction(nameof(Confirm));
}
```

Write a Confirm action that verifies a user posseses the email address they provided:

```C#
[AllowAnonymous]
[HttpGet("/account/confirm")]
public ViewResult ConfirmGet(ConfirmModel Model) => View("Confirm", Model);


[AllowAnonymous]
[HttpPost]
public async Task<IActionResult> Confirm(ConfirmModel Model)
{
    ConfirmRequest request = new ConfirmRequest
    {
        EmailAddress = Model.EmailAddress,
        Code = Model.Code
    };
    // TODO: Handle case where user provides incorrect confirmation code.
    await _accountService.ConfirmAsync(request);
    return RedirectToAction(nameof(Activated));
}
```

Write a ForgotPassword action:

```C#
[AllowAnonymous]
[HttpGet("/account/forgotpassword")]
public ViewResult ForgotPasswordGet(ForgotPasswordModel Model) => View("ForgotPassword", Model);


[AllowAnonymous]
[HttpPost]
public async Task<IActionResult> ForgotPassword(ResetPasswordModel Model)
{
    ForgotPasswordRequest request = new ForgotPasswordRequest{ EmailAddress = Model.EmailAddress };
    await _accountService.ForgotPasswordAsync(request);
    return RedirectToAction(nameof(ResetPassword));
}
```

Write a ResetPassword action:

```C#
[AllowAnonymous]
[HttpGet("/account/resetpassword")]
public ViewResult ResetPasswordGet(ResetPasswordModel Model) => View("ResetPassword", Model);


[AllowAnonymous]
[HttpPost]
public async Task<IActionResult> ResetPassword(ResetPasswordModel Model)
{
    ResetPasswordRequest request = new ResetPasswordRequest
    {
        EmailAddress = Model.EmailAddress,
        Code = Model.Code,
        NewPassword = Model.NewPassword
    };
    ResetPasswordResponse response = await _accountService.ResetPasswordAsync(request);
    if (!response.PasswordValid)
    {
        // Password does not meet complexity requirements.
        string message = string.Join(Environment.NewLine, response.Messages);
        ModelState.AddModelError(nameof(Model.NewPassword), message);
        return View(Model);
    }
    return RedirectToAction(nameof(PasswordReset));
}
```


##  Benefits

### Define and Register Custom Policies

Leverage the Identity service to secure access to ASP.NET Core MVC and WebAPI controllers.

Write custom policies that examine a user's roles and claims:

```C#
using System.Collections.Generic;
using ErikTheCoder.ServiceContract;
using Microsoft.AspNetCore.Authorization;


namespace ErikTheCoder.AspNetCore.Middleware
{
    public static class Policy
    {
        public const string Admin = "Admin";
        public const string TheBigLebowski = "The Big Lebowski";
        public const string Everyone = "Everyone";


        public static void VerifyAdmin(AuthorizationPolicyBuilder PolicyBuilder)
        {
            PolicyBuilder.RequireAssertion(Context =>
            {
                User user = User.ParseClaims(Context.User.Claims);
                return user.Roles.Contains(Admin);
            });
        }


        public static void VerifyTheBigLebowski(AuthorizationPolicyBuilder PolicyBuilder)
        {
            PolicyBuilder.RequireAssertion(Context =>
            {
                User user = User.ParseClaims(Context.User.Claims);
                if (user.Claims.TryGetValue(CustomClaimType.Nickname, out HashSet<string> nicknames))
                {
                    if (nicknames.Contains("The Dude")) 
                    {
                        if (user.Claims.TryGetValue(CustomClaimType.Ability, out HashSet<string> abilities)) return abilities.Contains("Make White Russian") && abilities.Contains("Abide");
                    }
                }
                return false;
            });
        }


        public static void VerifyEveryone(AuthorizationPolicyBuilder PolicyBuilder) => PolicyBuilder.RequireAssertion(Context => true);
    }
}

```

Write an extension method to use the policies.

```C#
public static void UseErikTheCoderPolicies(this AuthorizationOptions AuthorizationOptions)
{
    AuthorizationOptions.AddPolicy(Policy.Admin, Policy.VerifyAdmin);
    AuthorizationOptions.AddPolicy(Policy.TheBigLebowski, Policy.VerifyTheBigLebowski);
    AuthorizationOptions.AddPolicy(Policy.Everyone, Policy.VerifyEveryone);
}
```

In Startup.ConfigureServices, register the policies.

```C#
// Add MVC, filters, policies, and configure routing.
IMvcBuilder mvcBuilder = Services.AddMvc();
mvcBuilder.AddMvcOptions(Options => Options.Filters.Add(new AuthorizeFilter(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build()))); // Require authorization (permission to access controller actions).
Services.AddAuthorization(Options => Options.UseErikTheCoderPolicies()); // Authorize using policies that examine claims.
mvcBuilder.AddJsonOptions(Options => Options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver()); // Preserve case of property names.
Services.AddRouting(Options => Options.LowercaseUrls = true);
```

### Enforce Custom Policies

Limit access to controllers using the Policy attribute on the controller class or controller method:

```C#
namespace ErikTheCoder.MadPoker.Website.Controllers
{
    [Authorize(Policy = Policy.Admin)]
    public class AccountController : ControllerBase
```

In Razor views, pass the User.SecurityToken to JavaScript methods that call WebAPI service methods via AJAX.

```JavaScript
  // TODO: Add example code.
```

### Anti-Tamper Protection of JWT Tokens

If you decrypt the Base64-encoded security token (using [Fiddler's](https://www.telerik.com/fiddler) TextWizard), you'll find the user's claims.

Raw JWT token (I've added line breaks):
```
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR
5L2NsYWltcy9uYW1lIjoiZW1hZHNlbiIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2x
haW1zL2VtYWlsYWRkcmVzcyI6InVzZXJuYW1lQHJlZGFjdGVkLmNvbSIsImh0dHBzOi8vc2NoZW1hLmVyaWt0aGVjb2Rlci5uZXQ
vY2xhaW1zL2ZpcnN0bmFtZSI6IkVyaWsiLCJodHRwczovL3NjaGVtYS5lcmlrdGhlY29kZXIubmV0L2NsYWltcy9sYXN0bmFtZSI
6Ik1hZHNlbiIsImh0dHBzOi8vc2NoZW1hLmVyaWt0aGVjb2Rlci5uZXQvY2xhaW1zL3NlY3VyaXR5dG9rZW4iOiIiLCJodHRwOi8
vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsImh0dHBzOi8vc2N
oZW1hLmVyaWt0aGVjb2Rlci5uZXQvY2xhaW1zL2FiaWxpdHkiOlsiQWJpZGUiLCJNYWtlIFdoaXRlIFJ1c3NpYW4iXSwiaHR0cHM
6Ly9zY2hlbWEuZXJpa3RoZWNvZGVyLm5ldC9jbGFpbXMvbmlja25hbWUiOlsiRHVkZXJpbm8iLCJUaGUgRHVkZSJdLCJuYmYiOiI
xNTQ5NDc0MzUwIiwiZXhwIjoiMTU4MTAxMDM1MCJ9.1eadtmU-sxenCO8t0cdtVwpNmEDLsr9V1KJdkmLI80c
```

Decrypted JWT token:
```Json
{
    "alg": "HS256",
    "typ": "JWT"
}
{
    "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name": "emadsen",
    "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress": "username@redacted.com",
    "https://schema.erikthecoder.net/claims/firstname": "Erik",
    "https://schema.erikthecoder.net/claims/lastname": "Madsen",
    "https://schema.erikthecoder.net/claims/securitytoken": "",
    "http://schemas.microsoft.com/ws/2008/06/identity/claims/role": "Admin",
    "https://schema.erikthecoder.net/claims/ability": [
        "Abide",
        "Make White Russian"
    ],
    "https://schema.erikthecoder.net/claims/nickname": [
        "Duderino",
        "The Dude"
    ],
    "nbf": "1549474350",
    "exp": "1581010350"
}
�杶e>���-��mW
M�@˲�UԢ]�b��G
```

If these claims are altered by a malicious client to attempt an elevation-of-privilege attack, the client will receive an HTTP 401 Unauthorized exception.  Why?  Because the JWT authentication handler in ASP.NET Core MVC websites and WebAPI services hashes the claims using a CredentialSecret string known only to the server (never transmitted to the client), determines it does not match the binary hash contained in the JWT token (the scrambled characters above), and concludes the JWT token has been tampered with.  This anti-tamper technique is known as Hashed Message Authentication Code or [HMAC](https://en.wikipedia.org/wiki/HMAC).

This JWT token may be used by clients to authenticate to the Identity service and to any other service that signs its JWT tokens using the same CredentialSecret string.  Obviously this token is most secure when transmitted over an encrypted HTTPS channel via [SSL / TLS](https://en.wikipedia.org/wiki/Transport_Layer_Security).
