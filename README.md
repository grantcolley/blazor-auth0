# blazor-auth0
A solution for a Blazor WebAssembly App and a Blazor Server App and securing them with Auth0 as the Identity Provider.

###### .NET 6.0, Blazor WebAssembly, Blazor Server, Auth0, ASP.NET Core Web API
###### 
\
[![Build status](https://ci.appveyor.com/api/projects/status/6wsbn17wlhuw2oqb?svg=true)](https://ci.appveyor.com/project/grantcolley/blazor-auth0)
###### 


#### Table of Contents
1. [Preparing the Solution](#1-preparing-the-solution)
2. [Create an account with Auth0](#2-create-an-account-with-auth0)
    * [Register the WebApi](#register-the-webapi)
    * [Register the Blazor WASM Client](#register-the-blazor-wasm-client)
    * [Register the Blazor Server Client](#register-the-blazor-server-client)
3. [Securing the WebApi](#3-securing-the-webapi)
4. [Securing Shared Razor Components](#4-securing-shared-razor-components)
5. [Securing the Blazor WASM Client](#5-securing-the-blazor-wasm-client)
6. [Securing the Blazor Server Client](#6-securing-the-blazor-server-client)

## 1. Preparing the Solution

**blazor-auth0** is based on the [blazor-solution-setup](https://github.com/grantcolley/blazor-solution-setup) project with the identity provider project using **IdentityServer4**, and all references and code relating to it, stripped out.

Rename the solution file **BlazorSolutionSetup.sln** to **Blazor-Auth0.sln**.

Remove the **IdentityProvider** project from the solution and delete the folder from the directory.

Upgrade all projects to *net6.0*. In each *\*.proj* file:

Replace
```
  <PropertyGroup>
    <TargetFramework>net6.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
```

with
```
  <PropertyGroup>
    <TargetFramework>net6.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
```

In the **BlazorServerApp.csproj** project remove the following package references:
```
  <ItemGroup>
    <PackageReference Include="IdentityModel" Version="5.0.1" />
    <PackageReference Include="Microsoft.AspNetCore.Authentication.OpenIdConnect" Version="5.0.4" />
  </ItemGroup>
```

For all projects upgrade the package references to the latest stable version. At the time of writing for `Microsoft.AspNetCore.*` packages this is `Version="6.0.4"`.

## 2. Create an account with Auth0
Go to [Auth0](https://auth0.com/) and create a free account.

#### Register the WebApi
In the dashboard go to `Applications >> APIs` and register the WebApi with the **Name** `blazor-auth0-WebApi` and **Identifier** as `https://WebApi.com`. Note the identifier is not a valid web address and is used as the `audience` parameter for authorization calls.

#### Register the Blazor WASM Client
In the dashboard go to `Applications >> Applications` and register the Blazor WASM client with the **Name** `blazor-auth0-WASM` and **Application Type** `Single Page Application`. Set **Allowed Callback URLs** to `https://localhost:[PORT]/authentication/login-callback`, and **Allowed Logout URLs to** `https://localhost:[PORT]`. Note the port to use is set in `profiles:applicationUrl` of the `launchSettings.json` file for the **BlazorWebAssemblyApp** project.

#### Register the Blazor Server Client
In the dashboard go to `Applications >> Applications` and register the Blazor Server client with the **Name** `blazor-auth0-Server` and **Application Type** `Regular Web Application`. Set **Allowed Callback URLs** to `https://localhost:[PORT]/callback`, and **Allowed Logout URLs to** `https://localhost:[PORT]`. Note the port to use is set in `profiles:applicationUrl` of the `launchSettings.json` file for the **BlazorServerApp** project.

## 3. Securing the WebApi

## 4. Securing Shared Razor Components

In the shared **Razor** project add the `Authorize` attribute to [FetchData.razor](https://github.com/grantcolley/blazor-auth0/blob/main/src/Razor/Pages/FetchData.razor)
```C#
@page "/fetchdata"

@attribute [Authorize] 

<PageTitle>Weather forecast</PageTitle>

// existing code omitted for brevity

```
## 5. Securing the Blazor WASM Client

## 6. Securing the Blazor Server Client

#### Register the Blazor Server Client with Auth0
Login to [Auth0](https://auth0.com/) and go to **Applications** and click **Create Application**. Give the application a name and for **Application Type** select `Regular Web Application` and click **Save Changes**.


#### Secure the Blazor Server Client

In [appsettings.json](https://github.com/grantcolley/blazor-auth0/blob/main/src/Blazor.Server.App/appsettings.json) add a section for `Auth0` and entries for `Domain` and `ClientId`.

```C#
  "Auth0": {
    "Domain": "AUTH0_DOMAIN",
    "ClientId": "AUTH0_CLIENTID"
  }

```

Add the nuget package package `Auth0.AspNetCore.Authentication` to the project. More information about the package can be found at [ASP.NET Core Authentication SDK](https://auth0.com/blog/exploring-auth0-aspnet-core-authentication-sdk/**).

In [Program.cs](https://github.com/grantcolley/blazor-auth0/blob/main/src/Blazor.Server.App/Program.cs) call `AddAuth0WebAppAuthentication` to configure authentication and **after ** call `UseAuthentication` and `UseAuthorization` to enable middleware for authentication and authorisation.
```C#

// existing code omitted for brevity

builder.Services.AddAuth0WebAppAuthentication(options =>
{
    options.Domain = builder.Configuration["Auth0:Domain"];
    options.ClientId = builder.Configuration["Auth0:ClientId"];
});

// existing code omitted for brevity

// after UseRouting
app.UseAuthentication();
app.UseAuthorization();

// existing code omitted for brevity

```

