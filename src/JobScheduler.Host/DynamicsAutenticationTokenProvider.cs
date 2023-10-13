using IdentityModel.Client;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JobScheduler.Host
{
    /// <summary>
    /// provider of Dynamics Active Directory base authentication token
    /// </summary>
    public class DynamicsAutenticationTokenProvider
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IDistributedCache cache;
        private readonly ILogger<DynamicsAutenticationTokenProvider> logger;
        private readonly DynamicsAuthenticationTokenProviderOptions authenticationSettings;

        /// <summary>
        /// Instantiate a new token provider
        /// </summary>
        /// <param name="httpClientFactory"></param>
        /// <param name="cache"></param>
        /// <param name="authenticationSettingsOptions"></param>
        /// <param name="logger"></param>
        public DynamicsAutenticationTokenProvider(IHttpClientFactory httpClientFactory, IDistributedCache cache, IOptions<DynamicsAuthenticationTokenProviderOptions> authenticationSettingsOptions, ILogger<DynamicsAutenticationTokenProvider> logger)
        {
            if (authenticationSettingsOptions is null) throw new ArgumentNullException(nameof(authenticationSettingsOptions));
            this.httpClientFactory = httpClientFactory;
            this.cache = cache;
            this.logger = logger;
            authenticationSettings = authenticationSettingsOptions.Value;
            logger.LogInformation("Configured to get token from '{Url}' using '{UserName}'", authenticationSettings.OAuth2TokenEndpointUrl.AbsoluteUri, authenticationSettings.ServiceAccountName);
            if (authenticationSettings.CacheDuration.HasValue)
                logger.LogInformation("Token will be cached for {CacheDuration}", authenticationSettings.CacheDuration);
            else
                logger.LogWarning("Token will not be cached");
        }

        /// <summary>
        /// Gets a new token from the security token service
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<string> GetAccessToken()
        {
            return (authenticationSettings.CacheDuration.HasValue)
                ? await cache.GetOrSetAsync("dynamics_token", GetToken, authenticationSettings.CacheDuration.Value)
                : await GetToken();
        }

        private async Task<string> GetToken()
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

            if (logger.IsEnabled(LogLevel.Trace) && response.Raw != null) logger.LogTrace("GetAccessToken response: {Response}", response.Raw);

            if (response.IsError)
            {
                var exception = response.Exception ?? new InvalidOperationException("Error retreiving access token");
                if (response.Error != null) exception.Data.Add("Error", response.Error);
                exception.Data.Add("ErrorType", response.ErrorType);
                if (response.ErrorDescription != null) exception.Data.Add("ErrorDescription", response.ErrorDescription);
                if (response.HttpErrorReason != null) exception.Data.Add("HttpErrorReason", response.HttpErrorReason);

                logger.LogError(exception, "Failed to get access token: {Error}", response.Error);
            }

            return response.AccessToken ?? string.Empty;
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
        public string ClientId { get; set; } = null!;

        /// <summary>
        /// Gets or sets the client secret
        /// </summary>
        public string ClientSecret { get; set; } = null!;

        /// <summary>
        /// Gets or sets the service account name
        /// </summary>
        public string ServiceAccountName { get; set; } = null!;

        /// <summary>
        /// Gets or sets the service account password
        /// </summary>
        public string ServiceAccountPassword { get; set; } = null!;

        /// <summary>
        /// Gets or sets the resource name in the security token service
        /// </summary>
        public string ResourceName { get; set; } = null!;

        /// <summary>
        /// Gets or sets a cache duration for tokens, if null then caching will be disabled
        /// </summary>
        public TimeSpan? CacheDuration { get; set; }

        /// <inheritdoc/>
        public DynamicsAuthenticationTokenProviderOptions Value => this;
    }
}
