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
```C#
  <PropertyGroup>
    <TargetFramework>net6.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
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

In [appsettings.json](https://github.com/grantcolley/blazor-auth0/blob/main/src/WebApi/appsettings.json) add the following section:

```C#
  "Auth0": {
    "Domain": "[The Domain For Auth0 Application blazor-auth0-Server]",
    "Audience": "[The Identifier For Auth0 Api blazor-auth0-WebApi]"
  }
```

In [WeatherForecastController](https://github.com/grantcolley/blazor-auth0/blob/main/src/WebApi/Controllers/WeatherForecastController.cs) replace `[Authorize(Roles = "weatheruser")]` with `[Authorize]`.

Delete the file `Startup.cs`.

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
            builder.WithOrigins("https://localhost:[BlazorWebAssemblyApp PORT]", "https://localhost:[BlazorServerApp PORT]")
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

In [_Imports.razor](https://github.com/grantcolley/blazor-auth0/blob/main/src/RazorComponents/_Imports.razor) replace `@using Microsoft.AspNetCore.Components.Authorization` with `@using Microsoft.AspNetCore.Authorization`.

Replace the contents of [FetchData.razor](https://github.com/grantcolley/blazor-auth0/blob/main/src/Razor/Pages/FetchData.razor) with:

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

Add `[CascadingParameter] protected string AppTitle { get; set; }` to [NavMenu.razor](https://github.com/grantcolley/blazor-auth0/blob/main/src/Razor/Shared/NavMenu.razor) as follows:

```C#
<div class="top-row pl-4 navbar navbar-dark">
    <a class="navbar-brand" href="">@AppTitle</a>
    
    // existing code not shown for brevity...

</div>

// existing code not shown for brevity...

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

Delete `Account\UserAccountFactory.cs`.

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
            Navigation.NavigateTo($"{Configuration["Auth0:Authority"]}/v2/logout?client_id={Configuration["Auth0:ClientId"]}");
        }
    </LogOut>
</RemoteAuthenticatorView>

@code{
    [Parameter] public string Action { get; set; }
}
```

Add the following code to [MainLayout.razor](https://github.com/grantcolley/blazor-auth0/blob/main/src/BlazorWebAssemblyApp/Shared/MainLayout.razor):

```C#
@code {
    private string AppTitle = "BlazorWebAssemblyApp";
}
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

