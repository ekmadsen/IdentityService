# IdentityService
Manages user registration, confirmation, credentials, roles, claims, and profile.


# Motivation #

I was motivated to write my own Identity service for the following reasons.

1.  I wanted to control access to website and service controller actions using [HTTP cookies](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/cookie?view=aspnetcore-2.2) and [JWT tokens](https://docs.microsoft.com/en-us/dotnet/standard/microservices-architecture/secure-net-microservices-web-applications/#consume-security-tokens).
2.  To accomplish this, I knew I had to write custom ASP.NET Core [policies](https://docs.microsoft.com/en-us/aspnet/core/security/authorization/policies?view=aspnetcore-2.2) that examine the [claims](https://docs.microsoft.com/en-us/aspnet/core/security/authorization/claims?view=aspnetcore-2.2) returned by an Identity service. 
3.  Since I wanted to understand how to store user credentials safely and securely in a database...
4.  I decided to own the entire solution (custom Identity service returning a custom User class that's examined by custom policies) rather than take a dependency on Microsoft's or another third-party's Identity service.
5.  Regarding the "build versus buy versus download free component" decision, I justified writing my own [Logging](https://github.com/ekmadsen/Logging) and [Middleware](https://github.com/ekmadsen/AspNetCore.Middleware) components because it required about the same effort to write my own components as to learn to use all the features of third-party components.  I could not justify writing my own Identity service with an "equal effort" argument- using a third-party service would have been much easier.  I justified my efforts on this programming project solely by the educational experience it afforded me.


# Features #

* **Targets .NET Standard 2.0** so it may be used in .NET Core or .NET Framework runtimes.
* **Safely stores user credentials** as a Salt, PasswordHash, and PasswordManagerVersion.  Does *not* store user passwords, which minimizes severity of a security breach if a malicious actor gains access to the Identity database.
* **Enables multiple password manager versions** so password complexity and hashing can evolve over time.
* **Associates each user with a collection of roles and a collection of claims**.  These can be used by ASP.NET Core MVC or WebAPI clients to enforce [policies](https://docs.microsoft.com/en-us/aspnet/core/security/authorization/policies?view=aspnetcore-2.2).


# Installation #

* Use SQL Server Management Studio to locate an existing database or create a new database.
* Run the [CreateDatabase.sql](https://github.com/ekmadsen/IdentityService/blob/master/CreateDatabase.sql) script to create the tables and views used by this solution. The script creates SQL objects in an "Identity" schema. Obviously, if you install this solution in a dedicated database there's no risk of colliding with the names of existing SQL objects. However, if you install this solution in an existing database the schema minimizes the risk of colliding with existing SQL objects.
* Use Visual Studio to build this solution then deploy it to an [IIS](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/iis/?view=aspnetcore-2.2) or [Azure](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/azure-apps/?view=aspnetcore-2.2) host.
* Reference this service in your web application via its [NuGet package](https://www.nuget.org/packages/ErikTheCoder.Identity.Contract/).


# Usage #

Call the Identity service in your solution via an [IAccountService](https://github.com/ekmadsen/IdentityService/blob/master/Contract/IAccountService.cs)-typed [Refit](https://www.nuget.org/packages/Refit/) proxy.  See the [Refit GitHub site](https://github.com/reactiveui/refit) for an explanation of how to use Refit proxies and a detailed description of Refit's features and benefits.  In short, Refit provides strongly-typed C# classes for invoking service methods, whether you own (have source code for) the service endpoint or not.  It in no way precludes writing dynamically-typed javaScript code (such as AJAX) to invoke the same service methods.  It provides the best of both worlds: strongly-typed server-to-server calls and dynamically-typed browser-to-server calls.

In Startup.ConfigureServices, create a service proxy and inject the dependency.  

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

In the above code I use a custom authentication token.  See the [Usage](https://github.com/ekmadsen/AspNetCore.Middleware#usage) section of my AspNetCore.Middleware documentation for an explanation of custom authentication tokens.  Also, this code uses my [ServiceProxy](https://github.com/ekmadsen/ServiceProxy) solution to generate Refit service proxies that automatically pass authentication tokens and logging correlation IDs.

In an ASP.NET Core MVC website's authetication controller, write a login action:

```C#
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


#  Benefits # 

