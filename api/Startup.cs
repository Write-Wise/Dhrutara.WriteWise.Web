using System;
using Azure.Identity;
using Dhrutara.WriteWise.Web.Api;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph;


[assembly: FunctionsStartup(typeof(Startup))]
namespace Dhrutara.WriteWise.Web.Api
{
    public class Startup : FunctionsStartup
    {
        const string COSMOS_ENDPOINT_URI_CONFIG_KEY = "CosmosEndpointUri";
        const string COSMOS_AUTH_KEY_KEY = "CosmosAuthKey";
        const string COSMOS_DATABASE_CONFIG_KEY = "Database";
        const string COSMOS_CONTAINER_CONFIG_KEY = "Container";
        const string AUTH_TENANT_ID_CONFIG_KEY = "AuthTenantId";
        const string AUTH_CLIENT_ID_CONFIG_KEY = "AuthClientId";
        const string AUTH_CLIENT_SECRET_CONFIG_KEY = "AuthClientSecret";
       
        public override void Configure(IFunctionsHostBuilder builder)
        {
            Uri cosmosEndpoint = GetConfiguredUri(COSMOS_ENDPOINT_URI_CONFIG_KEY);
                string cosmosAuthKey = GetConfiguredString(COSMOS_AUTH_KEY_KEY);
                CosmosClient cosmosClient = new CosmosClientBuilder(cosmosEndpoint.AbsoluteUri, cosmosAuthKey)
                .Build();

                string cosmosDatabase = GetConfiguredString(COSMOS_DATABASE_CONFIG_KEY);
                string containerId = GetConfiguredString(COSMOS_CONTAINER_CONFIG_KEY);
                Container container = cosmosClient.GetDatabase(cosmosDatabase).GetContainer(containerId); 

                _ = builder.Services
                    .AddSingleton(s => container);

                TokenCredentialOptions options = new()
                {
                    AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
                };

                string authTenantId = GetConfiguredString(AUTH_TENANT_ID_CONFIG_KEY);
                string authClientId = GetConfiguredString(AUTH_CLIENT_ID_CONFIG_KEY);
                string authClientSecret = GetConfiguredString(AUTH_CLIENT_SECRET_CONFIG_KEY);

                ClientSecretCredential clientSecretCredential = new(authTenantId, authClientId, authClientSecret, options);

                GraphServiceClient graphClient = new(clientSecretCredential, new[] { "https://graph.microsoft.com/.default" });

                _ = builder.Services
                    .AddSingleton(s => graphClient);
        }
        private static string GetConfiguredString(string key)
        {
            string val = Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.Process)?? throw new InValidConfigurationException(key);
            if (string.IsNullOrWhiteSpace(val)){
                throw new InValidConfigurationException(key);
            }
            return val;
        }

        private static Uri GetConfiguredUri(string key)
        {
            return Uri.TryCreate(Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.Process), UriKind.Absolute, out Uri? uri)
               ? uri
               : throw new InValidConfigurationException(key);

        }

        internal class InValidConfigurationException : ApplicationException
        {
            internal InValidConfigurationException(string configurationKey):base($"Configure a valid value for {configurationKey}")
            {
            }
        }
    }
}
