using IdentityModel.Client;
using Microsoft.Extensions.Options;

namespace JobScheduler.Host
{
    /// <summary>
    /// provider of Dynamics Active Directory base authentication token
    /// </summary>
    public class DynamicsAutenticationTokenProvider
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly DynamicsAuthenticationTokenProviderOptions authenticationSettings;

        /// <summary>
        /// Instantiate a new token provider
        /// </summary>
        /// <param name="httpClientFactory"></param>
        /// <param name="authenticationSettingsOptions"></param>
        public DynamicsAutenticationTokenProvider(IHttpClientFactory httpClientFactory, IOptions<DynamicsAuthenticationTokenProviderOptions> authenticationSettingsOptions)
        {
            if (authenticationSettingsOptions is null) throw new ArgumentNullException(nameof(authenticationSettingsOptions));
            this.httpClientFactory = httpClientFactory;
            authenticationSettings = authenticationSettingsOptions.Value;
        }

        /// <summary>
        /// Gets a new token from the security token service
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<string> GetAccessToken()
        {
            var httpClient = httpClientFactory.CreateClient("crm");
            using var request = new PasswordTokenRequest
            {
                Address = authenticationSettings.OAuth2TokenEndpointUrl.AbsoluteUri,
                ClientId = authenticationSettings.ClientId,
                ClientSecret = authenticationSettings.ClientSecret,
                Resource = { authenticationSettings.ResourceName },
                UserName = authenticationSettings.ServiceAccountName,
                Password = authenticationSettings.ServiceAccountPassword,
                Scope = "openid",
            };

            var response = await httpClient.RequestPasswordTokenAsync(request);

            if (response.IsError) throw new InvalidOperationException(response.Error);

            return response.AccessToken!;
        }
    }

    /// <summary>
    /// Options for Dynamics authentication token provider
    /// </summary>
    public record DynamicsAuthenticationTokenProviderOptions : IOptions<DynamicsAuthenticationTokenProviderOptions>
    {
        /// <summary>
        /// Oauth2 endpoint url
        /// </summary>
        public Uri OAuth2TokenEndpointUrl { get; set; } = null!;

        /// <summary>
        /// Gets or sets the client id
        /// </summary>
        public string ClientId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the client secret
        /// </summary>
        public string ClientSecret { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the service account name
        /// </summary>
        public string ServiceAccountName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the service account password
        /// </summary>
        public string ServiceAccountPassword { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the resource name in the security token service
        /// </summary>
        public string ResourceName { get; set; } = string.Empty;

        /// <inheritdoc/>
        public DynamicsAuthenticationTokenProviderOptions Value => this;
    }
}
