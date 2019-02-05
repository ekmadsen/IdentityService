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

Call the Identity service in your solution via an [IAccountService](https://github.com/ekmadsen/IdentityService/blob/master/Contract/IAccountService.cs)-typed [Refit](https://www.nuget.org/packages/Refit/) proxy.  See [Refit GitHub site](https://github.com/reactiveui/refit) for an explanation of how to use Refit proxies and a detailed description of Refit's features and benefits.  In short, Refit provides strongly-typed C# classes for invoking service methods, whether you own (have source code for) the service endpoint or not.  It in no way precludes writing weakly-typed javaScript code (such as AJAX) to invoke the same service methods.  It provides the best of both worlds (server-to-server calls and browser-to-server calls).


#  Benefits # 


