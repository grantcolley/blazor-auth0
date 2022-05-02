# blazor-auth0
A solution for a Blazor WebAssembly App and a Blazor Server App and securing them with Auth0 as the Identity Provider.

###### .NET 6.0, Blazor WebAssembly, Blazor Server, Auth0, ASP.NET Core Web API
###### 
\
[![Build status](https://ci.appveyor.com/api/projects/status/6wsbn17wlhuw2oqb?svg=true)](https://ci.appveyor.com/project/grantcolley/blazor-auth0)
###### 


#### Table of Contents
1. [Preparing the Solution](#1-preparing-the-solution)
    * [Create the Solution Projects](#create-the-solution-projects)
    * [Create an account with Auth0](#create-an-account-with-auth0)
3. [Securing the WebApi](#2-securing-the-webapi)
    * [Register the WebApi with Auth0](#register-the-webapi-with-auth0)
    * [Secure the WebApi](#secure-the-webapi)
4. [Securing the Blazor WASM Client](#3-securing-the-blazor-wasm-client)
    * [Register the Blazor WASM Client with Auth0](#register-the-blazor-wasm-client-with-auth0)
    * [Secure the Blazor WASM Client](#secure-the-blazor-wasm-client)
5. [Securing the Blazor Server Client](#4-securing-the-blazor-server-client)
    * [Register the Blazor Server Client with Auth0](#register-the-blazor-server-client-with-auth0)
    * [Secure the Blazor Server Client](#secure-the-blazor-server-client)

## 1. Preparing the Solution

#### Create the Solution Projects
**blazor-auth0** is based on the [blazor-solution-setup](https://github.com/grantcolley/blazor-solution-setup) project with the identity provider project and all references and code relating to authentication stripped out.

#### Create an account with Auth0
Go to [Auth0](https://auth0.com/) and create a free account.

## 2. Securing the WebApi

#### Register the WebApi with Auth0

#### Secure the WebApi

## 3. Securing the Blazor WASM Client

#### Register the Blazor WASM Client with Auth0

#### Secure the Blazor WASM Client

## 4. Securing the Blazor Server Client

#### Register the Blazor Server Client with Auth0

#### Secure the Blazor Server Client

In [appsettings.json](https://github.com/grantcolley/blazor-auth0/blob/main/src/Blazor.Server.App/appsettings.json) add a section for `Auth0` and entries for `Domain` and `ClientId`.

```C#
  "Auth0": {
    "Domain": "AUTH0_DOMAIN",
    "ClientId": "AUTH0_CLIENTID"
  }
}
```

Add the nuget package package `Auth0.AspNetCore.Authentication` to the project. More information about the package can be found at [ASP.NET Core Authentication SDK](https://auth0.com/blog/exploring-auth0-aspnet-core-authentication-sdk/**).

In [Program.cs](https://github.com/grantcolley/blazor-auth0/blob/main/src/Blazor.Server.App/Program.cs) call `AddAuth0WebAppAuthentication` to configure authentication and **after ** call `UseAuthentication` and `UseAuthorization` to enable middleware for authentication and authorisation.
```C#

// existing code removed for brevity

builder.Services.AddAuth0WebAppAuthentication(options =>
{
    options.Domain = builder.Configuration["Auth0:Domain"];
    options.ClientId = builder.Configuration["Auth0:ClientId"];
});

// existing code removed for brevity

// after UseRouting
app.UseAuthentication();
app.UseAuthorization();

// existing code removed for brevity

```

