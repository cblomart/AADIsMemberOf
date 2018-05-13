using Microsoft.Azure.ActiveDirectory.GraphClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace AADIsMemberOf
{
    class Helpers
    {
        public static ActiveDirectoryClient GetActiveDirectoryClient(string token, string tenantid)
        {
            var servicePointUri = new Uri(Constants.AADResourceUrl);
            var serviceRoot = new Uri(servicePointUri, tenantid);
            return new ActiveDirectoryClient(serviceRoot, () => Task.FromResult(token));
        }

        public static GraphServiceClient GetGraphClient(string token)
        {
            return  new GraphServiceClient(
                new DelegateAuthenticationProvider(
                    (req) =>
                    {
                        req.Headers.Authorization = new AuthenticationHeaderValue("bearer", token);
                        return Task.FromResult(0);
                    })
                );
        }

        public static string GetToken(string tenantid, string clientid, string clientsecret, string resourceurl)
        {
            var authContext = new AuthenticationContext(String.Format(Constants.Issuer, tenantid));
            var clientCreds = new ClientCredential(clientid, clientsecret);
            var resultTask = authContext.AcquireTokenAsync(resourceurl, clientCreds);
            resultTask.Wait();
            return resultTask.Result.AccessToken;
        }

        public static string GetToken(IConfiguration configuration, string resourceurl)
        {
            return GetToken(configuration["TenantId"], configuration["ClientId"], configuration["ClientSecret"], resourceurl);
        }
    }
}
