# blazor-auth0
A solution for a Blazor WebAssembly App and a Blazor Server App and securing them with [Auth0](https://auth0.com/) as the Identity Provider.


**blazor-auth0** is based on the [blazor-solution-setup](https://github.com/grantcolley/blazor-solution-setup) solution that uses [IdentityServer4](https://identityserver4.readthedocs.io/en/latest/) as its identity provider. This project will take a copy of [blazor-solution-setup](https://github.com/grantcolley/blazor-solution-setup) and strip out all references and code relating to [IdentityServer4](https://identityserver4.readthedocs.io/en/latest/) and replace it with [Auth0](https://auth0.com/).

###### .NET 6.0, Blazor WebAssembly, Blazor Server, ASP.NET Core Web API, [Auth0](https://auth0.com/)
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
7. [Authorising Users by Role](#7-authorising-users-by-role)
    * [Create the Auth0 Role](#create-the-auth0-role)
    * [Restrict the Client and WebApi](#restrict-the-client-and-webapi)
    * [Consume roles in the Blazor WASM Client](#consume-roles-in-the-blazor-wasm-client)
9. [Running the Solution](#8-running-the-solution)

## 1. Preparing the Solution

Rename the solution file **BlazorSolutionSetup.sln** to **Blazor-Auth0.sln**.

Remove the **IdentityProvider** project from the solution and delete the folder from the directory.

Upgrade all projects to *net6.0*. In each *\*.proj* file:

Replace
```C#
  <PropertyGroup>
    <TargetFramework>net5.0</TargetFramework>
  </PropertyGroup>
```

with
```C#
  <PropertyGroup>
    <TargetFramework>net6.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
```

In the **BlazorServerApp.csproj** project remove the following package references:
```C#
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
In the dashboard go to `Applications >> Applications` and register the Blazor WASM client with the **Name** `blazor-auth0-WASM` and **Application Type** `Single Page Application`. Set **Allowed Callback URLs** to `https://localhost:[PORT]/authentication/login-callback`, and **Allowed Logout URLs to** `https://localhost:[PORT]`.

> Note the port to use is set in `profiles:applicationUrl` of the `launchSettings.json` file for the **BlazorWebAssemblyApp** project.

#### Register the Blazor Server Client
In the dashboard go to `Applications >> Applications` and register the Blazor Server client with the **Name** `blazor-auth0-Server` and **Application Type** `Regular Web Application`. Set **Allowed Callback URLs** to `https://localhost:[PORT]/callback`, and **Allowed Logout URLs to** `https://localhost:[PORT]`. Note the port to use is set in `profiles:applicationUrl` of the `launchSettings.json` file for the **BlazorServerApp** project.

## 3. Securing the WebApi

Delete the file `Startup.cs`.

In [appsettings.json](https://github.com/grantcolley/blazor-auth0/blob/main/src/WebApi/appsettings.json) add the following section:

```C#
  "Auth0": {
    "Domain": "[The Domain For Auth0 Application blazor-auth0-Server]",
    "Audience": "[The Identifier For Auth0 Api blazor-auth0-WebApi]"
  }
```

In [WeatherForecastController](https://github.com/grantcolley/blazor-auth0/blob/main/src/WebApi/Controllers/WeatherForecastController.cs) replace `[Authorize(Roles = "weatheruser")]` with `[Authorize]`.

Replace the contents of [Program.cs](https://github.com/grantcolley/blazor-auth0/blob/main/src/WebApi/Program.cs) with:

```C#
using Core.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Repository.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.Authority = $"https://{builder.Configuration["Auth0:Domain"]}";
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = builder.Configuration["Auth0:Domain"],
        ValidAudience = builder.Configuration["Auth0:Audience"]
    };
});

builder.Services.AddScoped<IWeatherForecastRepository, WeatherForecastRepository>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("local",
        builder =>
            builder.WithOrigins(
                        "https://localhost:[BlazorWebAssemblyApp PORT]", 
                        "https://localhost:[BlazorServerApp PORT]")
                        .AllowAnyHeader());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors("local");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
```

> Note when adding the CORS policy, the ports to specify is set in `profiles:applicationUrl` of the `launchSettings.json` file for the **BlazorWebAssemblyApp** and **BlazorServerApp** projects.

## 4. Securing Shared Razor Components

In [_Imports.razor](https://github.com/grantcolley/blazor-auth0/blob/main/src/RazorComponents/_Imports.razor) add `@using Microsoft.AspNetCore.Authorization`.

Replace the contents of [FetchData.razor](https://github.com/grantcolley/blazor-auth0/blob/main/src/RazorComponents/Pages/FetchData.razor) with:

```C#
@page "/fetchdata"

@attribute [Authorize] 

<PageTitle>Weather forecast</PageTitle>

<h1>Weather forecast</h1>

<p>This component demonstrates fetching data from the server.</p>

@if (forecasts == null)
{
    <p><em>Loading...</em></p>
}
else
{
    <table class="table">
        <thead>
            <tr>
                <th>Date</th>
                <th>Temp. (C)</th>
                <th>Temp. (F)</th>
                <th>Summary</th>
            </tr>
        </thead>
        <tbody>
            @foreach (var forecast in forecasts)
            {
                <tr>
                    <td>@forecast.Date.ToShortDateString()</td>
                    <td>@forecast.TemperatureC</td>
                    <td>@forecast.TemperatureF</td>
                    <td>@forecast.Summary</td>
                </tr>
            }
        </tbody>
    </table>
}

@code {
    protected IEnumerable<WeatherForecast>? forecasts;

    [Inject]
    public IWeatherForecastService? WeatherForecastService { get; set; }

    protected override async Task OnInitializedAsync()
    {
        forecasts = await WeatherForecastService.GetWeatherForecasts();
    }
}
```

Replace the contents of [NavMenu.razor](https://github.com/grantcolley/blazor-auth0/blob/main/src/RazorComponents/Shared/NavMenu.razor) with the following:

```C#
<div class="top-row pl-4 navbar navbar-dark">
    <a class="navbar-brand" href="">@AppTitle</a>
    <button class="navbar-toggler" @onclick="ToggleNavMenu">
        <span class="navbar-toggler-icon"></span>
    </button>
</div>

<div class="@NavMenuCssClass" @onclick="ToggleNavMenu">
    <ul class="nav flex-column">
        <li class="nav-item px-3">
            <NavLink class="nav-link" href="" Match="NavLinkMatch.All">
                <span class="oi oi-home" aria-hidden="true"></span> Home
            </NavLink>
        </li>
        <li class="nav-item px-3">
            <NavLink class="nav-link" href="counter">
                <span class="oi oi-plus" aria-hidden="true"></span> Counter
            </NavLink>
        </li>
        <AuthorizeView>
            <li class="nav-item px-3">
                <NavLink class="nav-link" href="fetchdata">
                    <span class="oi oi-list-rich" aria-hidden="true"></span> Fetch data
                </NavLink>
            </li>
        </AuthorizeView>
        <AuthorizeView>
            <li class="nav-item px-3">
                <NavLink class="nav-link" href="user">
                    <span class="oi oi-person" aria-hidden="true"></span> User
                </NavLink>
            </li>
        </AuthorizeView>
    </ul>
</div>

@code {
    [CascadingParameter]
    protected string? AppTitle { get; set; }

    private bool collapseNavMenu = true;

    private string? NavMenuCssClass => collapseNavMenu ? "collapse" : null;

    private void ToggleNavMenu()
    {
        collapseNavMenu = !collapseNavMenu;
    }
}
```

## 5. Securing the Blazor WASM Client

Delete files `Account\UserAccountFactory.cs` and `Shared\RedirectToLogin.razor`.

Add `@using Microsoft.AspNetCore.Authorization` to [_Imports.razor](https://github.com/grantcolley/blazor-auth0/blob/main/src/BlazorWebAssemblyApp/_Imports.razor).

Replace the contents of [appsettings.json](https://github.com/grantcolley/blazor-auth0/blob/main/src/BlazorWebAssemblyApp/wwwroot/appsettings.json) with:

```C#
{
  "Auth0": {
    "Authority": "https://[The Domain For Auth0 Application blazor-auth0-WASM]",
    "ClientId": "[The Client ID For Auth0 Application blazor-auth0-WASM]",
    "Audience": "[The Identifier For Auth0 Api blazor-auth0-WebApi]"
  }
}
```

Replace the contents of [Authentication.razor](https://github.com/grantcolley/blazor-auth0/blob/main/src/BlazorWebAssemblyApp/Pages/Authentication.razor) with:

```C#
@page "/authentication/{action}"
@using Microsoft.AspNetCore.Components.WebAssembly.Authentication
@using Microsoft.Extensions.Configuration

@inject NavigationManager Navigation
@inject IConfiguration Configuration

<RemoteAuthenticatorView Action="@Action">
    <LogOut>
        @{
            Navigation.NavigateTo(
               $"{Configuration["Auth0:Authority"]}/v2/logout?client_id={Configuration["Auth0:ClientId"]}");
        }
    </LogOut>
</RemoteAuthenticatorView>

@code{
    [Parameter] public string Action { get; set; }
}
```

Replace the contents of [MainLayout.razor](https://github.com/grantcolley/blazor-auth0/blob/main/src/BlazorWebAssemblyApp/Shared/MainLayout.razor) with:

```C#
@inherits LayoutComponentBase

<CascadingValue Value="@AppTitle">
    <MainLayoutBase>
        <LoginDisplayFragment>
            <LoginDisplay/>
        </LoginDisplayFragment>
        <BodyFragment>
            @Body
        </BodyFragment>
    </MainLayoutBase>
</CascadingValue>

@code {
    private string AppTitle = "BlazorWebAssemblyApp";
}
```

Replace the contents of [App.razor](https://github.com/grantcolley/blazor-auth0/blob/main/src/BlazorWebAssemblyApp/App.razor) with:

```C#
<CascadingAuthenticationState>
    <Router AppAssembly="@typeof(App).Assembly"
            AdditionalAssemblies="new[] { typeof(NavMenu).Assembly}" PreferExactMatches="@true">
        <Found Context="routeData">
            <AuthorizeRouteView RouteData="@routeData" DefaultLayout="@typeof(MainLayout)">
                <Authorizing>
                    <p><i>Authorizing...</i></p>
                </Authorizing>
                <NotAuthorized>
                    <p>Access denied.</p>
                </NotAuthorized>
            </AuthorizeRouteView>
            <FocusOnNavigate RouteData="@routeData" Selector="h1" />
        </Found>
        <NotFound>
            <PageTitle>Not found</PageTitle>
            <LayoutView Layout="@typeof(MainLayout)">
                <p role="alert">Sorry, there's nothing at this address.</p>
            </LayoutView>
        </NotFound>
    </Router>
</CascadingAuthenticationState>
```

Replace the contents of [Program.cs](https://github.com/grantcolley/blazor-auth0/blob/main/src/BlazorWebAssemblyApp/Program.cs) with:

```C#
using BlazorWebAssemblyApp;
using Core.Interface;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddOidcAuthentication(options =>
{
    builder.Configuration.Bind("Auth0", options.ProviderOptions);
    options.ProviderOptions.ResponseType = "code";
    options.ProviderOptions.AdditionalProviderParameters.Add("audience", builder.Configuration["Auth0:Audience"]);
});

builder.Services.AddHttpClient("WebApi",
      client => client.BaseAddress = new Uri("https://localhost:[WebApi PORT]"))
    .AddHttpMessageHandler(sp =>
    {
        var httpMessageHandler = sp.GetService<AuthorizationMessageHandler>()?
        .ConfigureHandler(authorizedUrls: new[] { "https://localhost:[WebApi PORT]" });
        return httpMessageHandler ?? throw new NullReferenceException(nameof(AuthorizationMessageHandler));
    });

builder.Services.AddTransient<IWeatherForecastService, WeatherForecastService>(sp =>
{
    var httpClient = sp.GetRequiredService<IHttpClientFactory>();
    var weatherForecastServiceHttpClient = httpClient.CreateClient("WebApi");
    return new WeatherForecastService(weatherForecastServiceHttpClient);
});

await builder.Build().RunAsync();
```
> Note when adding the HttpClient the port to specify is set in `profiles:applicationUrl` of the `launchSettings.json` file for the **WebApi** project.

## 6. Securing the Blazor Server Client

Delete the file `Startup.cs`.

Delete the `Areas` folder and its contents.

Delete the file `Shared\RedirectToLogin.razor`.

Add the package package `Auth0.AspNetCore.Authentication` to the project. More information about the package can be found at [Auth0 - ASP.NET Core Authentication SDK](https://auth0.com/blog/exploring-auth0-aspnet-core-authentication-sdk/).

Add the following section to [appsettings.json](https://github.com/grantcolley/blazor-auth0/blob/main/src/BlazorServerApp/appsettings.json):

```C#
  "Auth0": {
    "Authority": "https://[The Domain For Auth0 Application blazor-auth0-Server]",
    "ClientId": "[The Client ID For Auth0 Application blazor-auth0-Server]",
    "ClientSecret": "[The Client Secret For Auth0 Application blazor-auth0-Server]",
    "Audience": "[The Identifier For Auth0 Api blazor-auth0-WebApi]"
  }
```

In the `Pages` folder create empty razor component [Login.cshtml](https://github.com/grantcolley/blazor-auth0/blob/main/src/BlazorServerApp/Pages/Login.cshtml) and replace the `OnGet` method of [Login.cshtml.cs](https://github.com/grantcolley/blazor-auth0/blob/main/src/BlazorServerApp/Pages/Login.cshtml.cs) as follows:

```C#
using Auth0.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BlazorServerApp.Pages
{
    public class LoginModel : PageModel
    {
        public async Task OnGet(string redirectUri)
        {
            var authenticationProperties = new LoginAuthenticationPropertiesBuilder()
                .WithRedirectUri(redirectUri)
                .Build();

            await HttpContext.ChallengeAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
        }
    }
}
```

In the `Pages` folder create empty razor component [Logout.cshtml](https://github.com/grantcolley/blazor-auth0/blob/main/src/BlazorServerApp/Pages/Logout.cshtml) and replace the `OnGet` method of [Logout.cshtml.cs](https://github.com/grantcolley/blazor-auth0/blob/main/src/BlazorServerApp/Pages/Logout.cshtml.cs) as follows:

```C#
using Auth0.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Blazor.Server.App.Pages
{
    [Authorize]
    public class LogoutModel : PageModel
    {        
        public async Task OnGet()
        {
            var authenticationProperties = new LogoutAuthenticationPropertiesBuilder()
                 .WithRedirectUri("/")
                 .Build();

            await HttpContext.SignOutAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }
}
```

Replace the contents of [LoginDisplay.razor](https://github.com/grantcolley/blazor-auth0/blob/main/src/BlazorServerApp/Shared/LoginDisplay.razor) with the following:

```C#
@using Microsoft.AspNetCore.Components.Authorization

<AuthorizeView>
    <Authorized>
        @context.User.Identity.Name!
        <a href="logout">Log out</a>
    </Authorized>
    <NotAuthorized>
        <a href="login?redirectUri=/">Log in</a>
    </NotAuthorized>
</AuthorizeView>
```

Replace the contents of [MainLayout.razor](https://github.com/grantcolley/blazor-auth0/blob/main/src/BlazorServerApp/Shared/MainLayout.razor) with:

```C#
@inherits LayoutComponentBase

<CascadingValue Value="@AppTitle">
    <MainLayoutBase>
        <LoginDisplayFragment>
            <LoginDisplay />
        </LoginDisplayFragment>
        <BodyFragment>
            @Body
        </BodyFragment>
    </MainLayoutBase>
</CascadingValue>

@code {
    private string AppTitle = "BlazorServerApp";
}
```

Replace the contents of [App.razor](https://github.com/grantcolley/blazor-auth0/blob/main/src/BlazorServerApp/App.razor) with:

```C#
@using Core.Model
@using BlazorServerApp.Model

@inject TokenProvider TokenProvider

<CascadingAuthenticationState>
    <Router AppAssembly="@typeof(App).Assembly"
            AdditionalAssemblies="new[] { typeof(NavMenu).Assembly}" PreferExactMatches="@true">
        <Found Context="routeData">
            <AuthorizeRouteView RouteData="@routeData" DefaultLayout="@typeof(MainLayout)">
                <Authorizing>
                    <p><i>Authorizing...</i></p>
                </Authorizing>
                <NotAuthorized>
                    <p>Access denied.</p>
                </NotAuthorized>
            </AuthorizeRouteView>
            <FocusOnNavigate RouteData="@routeData" Selector="h1" />
        </Found>
        <NotFound>
            <PageTitle>Not found</PageTitle>
            <LayoutView Layout="@typeof(MainLayout)">
                <p role="alert">Sorry, there's nothing at this address.</p>
            </LayoutView>
        </NotFound>
    </Router>
</CascadingAuthenticationState>

@code {
    [Parameter]
    public InitialApplicationState InitialState { get; set; }

    protected override Task OnInitializedAsync()
    {
        TokenProvider.AccessToken = InitialState.AccessToken;
        TokenProvider.RefreshToken = InitialState.RefreshToken;
        TokenProvider.IdToken = InitialState.IdToken;

        return base.OnInitializedAsync();
    }
}
```

Replace the contents of [Program.cs](https://github.com/grantcolley/blazor-auth0/blob/main/src/BlazorServerApp/Program.cs) with the following:

```C#
using Auth0.AspNetCore.Authentication;
using Core.Authentication;
using Core.Interfaces;
using Service.Services;
using System.IdentityModel.Tokens.Jwt;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

builder.Services
    .AddAuth0WebAppAuthentication(Auth0Constants.AuthenticationScheme, options =>
    {
        options.Domain = builder.Configuration["Auth0:Domain"];
        options.ClientId = builder.Configuration["Auth0:ClientId"];
        options.ClientSecret = builder.Configuration["Auth0:ClientSecret"];
        options.ResponseType = "code";
    }).WithAccessToken(options =>
    {
        options.Audience = builder.Configuration["Auth0:Audience"];
    });

builder.Services.AddScoped<TokenProvider>();

builder.Services.AddHttpClient("webapi", client =>
{
    client.BaseAddress = new Uri("https://localhost:[WebApi PORT]");
});

builder.Services.AddTransient<IWeatherForecastService, WeatherForecastService>(sp =>
{
    var tokenProvider = sp.GetRequiredService<TokenProvider>();
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient("webapi");
    return new WeatherForecastService(httpClient, tokenProvider);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapBlazorHub();

app.MapFallbackToPage("/_Host");

app.Run();
```
> Note when adding the HttpClient the port to specify is set in `profiles:applicationUrl` of the `launchSettings.json` file for the **WebApi** project.

## 7. Authorising Users by Role
#### Create the Auth0 Role
Create a role and add it to the Access and ID Token.

In the [Auth0](https://auth0.com/) dashboard go to `User Management >> Roles` and create a role called `blazor-auth0`. Add your user to the role.

Go to `Auth Pipeline >> Rules` and create a rule called `blazor-auth0-token`. Add the following to the Script:

```javascript
function (user, context, callback) {
   const accessTokenClaims = context.accessToken || {};
   const idTokenClaims = context.idToken || {};
   const assignedRoles = (context.authorization || {}).roles;
   accessTokenClaims['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] = assignedRoles;
   idTokenClaims['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] = assignedRoles;
   return callback(null, user, context);
}
```

#### Restrict the Client and WebApi
In the **RazorComponents** project update the `@attribute [Authorize]` inside [FetchData.razor](https://github.com/grantcolley/blazor-auth0/blob/main/src/RazorComponents/Pages/FetchData.razor) to `@attribute [Authorize(Roles = "blazor-auth0")]`.

In the **WebApi** project update the `[Authorize]` inside [WeatherForecastController](https://github.com/grantcolley/blazor-auth0/blob/main/src/WebApi/Controllers/WeatherForecastController.cs) to `[Authorize(Roles = "blazor-auth0")]`.

#### Consume roles in the Blazor WASM Client
The identity provider sends the roles as an array stored in a single claim in the access and ID tokens. The array of roles must be separated by the token consumer.

To do this create a [UserAccountFactory](https://github.com/grantcolley/blazor-auth0/blob/main/src/BlazorWebAssemblyApp/Account/UserAccountFactory.cs) class that inherits from [AccountClaimsPrincipalFactory<TAccount>](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.components.webassembly.authentication.accountclaimsprincipalfactory-1?view=aspnetcore-6.0) as follows:
   
```C#
    public class UserAccountFactory : AccountClaimsPrincipalFactory<RemoteUserAccount>
    {
        public UserAccountFactory(IAccessTokenProviderAccessor accessor) : base(accessor)
        {
        }

        public async override ValueTask<ClaimsPrincipal> CreateUserAsync(RemoteUserAccount account, RemoteAuthenticationUserOptions options)
        {
            var user = await base.CreateUserAsync(account, options);

            if (user?.Identity?.IsAuthenticated ?? false)
            {
                var identity = (ClaimsIdentity)user.Identity;
                account.AdditionalProperties.TryGetValue(ClaimTypes.Role, out var roleClaims);

                if (roleClaims != null
                    && roleClaims is JsonElement element
                    && element.ValueKind == JsonValueKind.Array)
                {
                    identity.RemoveClaim(identity.FindFirst(ClaimTypes.Role));

                    var claims = element.EnumerateArray()
                        .Select(c => new Claim(ClaimTypes.Role, c.ToString()));

                    identity.AddClaims(claims);
                }
            }

            return user ?? new ClaimsPrincipal();
        }
    }
```

In [Program.cs](https://github.com/grantcolley/blazor-auth0/blob/main/src/BlazorWebAssemblyApp/Program.cs) register the [UserAccountFactory](https://github.com/grantcolley/blazor-auth0/blob/main/src/BlazorWebAssemblyApp/Account/UserAccountFactory.cs) so it is called everytime the user logs in, as follows:
   
```C#
builder.Services.AddOidcAuthentication(options =>
{
    builder.Configuration.Bind("Auth0", options.ProviderOptions);
    options.ProviderOptions.ResponseType = "code";
    options.ProviderOptions.AdditionalProviderParameters.Add("audience", builder.Configuration["Auth0:Audience"]);
}).AddAccountClaimsPrincipalFactory<UserAccountFactory>();
```

## 8. Running the Solution
In the solution's properties window select Multiple startup projects and set the Action of the **WebApi**, **BlazorWebAssemblyApp**, and **BlazorServerApp** to Startup.

Compile and run the solution...
