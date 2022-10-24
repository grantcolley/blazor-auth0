namespace BlazorHybridApp.Auth0
{
    public class Auth0AuthenticationStateProviderOptions
    {
        public Auth0AuthenticationStateProviderOptions()
        {
            Scope = "openid";
            RedirectUri = "myapp://callback";
            Browser = new WebBrowserAuthenticator();
            Parameters = new Dictionary<string, string>();
        }

        public string Domain { get; set; }

        public string ClientId { get; set; }

        public string RedirectUri { get; set; }

        public string Scope { get; set; }

        public string RoleClaim { get; set; }

        public Dictionary<string, string> Parameters { get; set; }

        public IdentityModel.OidcClient.Browser.IBrowser Browser { get; set; }
    }
}
