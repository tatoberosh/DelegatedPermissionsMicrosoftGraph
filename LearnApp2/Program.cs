using System;
using System.Collections.Generic;
using Microsoft.Identity.Client;
using Microsoft.Graph;
using Microsoft.Extensions.Configuration;
using Helpers;

namespace LearnApp2
{
    class Program
    {
        private static GraphServiceClient _graphClient;
        static void Main(string[] args)
        {
            var config = LoadAppSettings();
            if (config == null)
            {
                Console.Write("Invalid appsettings.json file");
                return;
            }

            var client = GetAuthenticatedGraphClient(config);
            //var graphRequest = client.Groups.Request().Top(5).Expand("members");
            var graphRequest = client.Me.Messages
                                        .Request();
                                        //.Select(u => new { u.DisplayName, u.Mail })
                                        //.Top(15)
                                        //.OrderBy("DisplayName desc")
                                        //.Filter("StartsWith(surname, 'A') or startsWith(surname, 'B')");
            var results = graphRequest.GetAsync().Result;
            foreach (var message in results)
            {
                //Console.WriteLine(user.Id + ": " + user.DisplayName + " <" + user.Mail + ">");
                Console.WriteLine(message);
            }
            Console.WriteLine("\nGraph Request:");
            Console.WriteLine(graphRequest.GetHttpRequestMessage().RequestUri);
        }

        private static IConfigurationRoot LoadAppSettings()
        {
            try
            {
                var config = new ConfigurationBuilder()
                                    .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                                    .AddJsonFile("appsettings.json", false, true)
                                    .Build();
                if (string.IsNullOrEmpty(config["applicationId"]) ||
                    string.IsNullOrEmpty(config["applicationSecret"]) ||
                    string.IsNullOrEmpty(config["redirectUri"]) ||
                    string.IsNullOrEmpty(config["tenantId"]))
                {
                    return null;
                }

                return config;
            }
            catch (System.IO.FileNotFoundException)
            {
                return null;
            }
        }
        private static IAuthenticationProvider CreateAuthorizationProvider(IConfigurationRoot config)
        {
            var clientId = config["applicationId"];
            var clientSecret = config["applicationSecret"];
            var redirectUri = config["redirectUri"];
            var authority = $"https://login.microsoftonline.com/{config["tenantId"]}/v2.0";

            List<string> scopes = new List<string>();

            scopes.Add("https://graph.microsoft.com/.default");

            var cca = ConfidentialClientApplicationBuilder.Create(clientId)
                                                    .WithAuthority(authority)
                                                    .WithRedirectUri(redirectUri)
                                                    .WithClientSecret(clientSecret)
                                                    .Build();
            return new MsalAuthenticationProvider(cca, scopes.ToArray());
        }

        private static GraphServiceClient GetAuthenticatedGraphClient(IConfigurationRoot config)
        {
            var authenticationProvider = CreateAuthorizationProvider(config);
            _graphClient = new GraphServiceClient(authenticationProvider);
            return _graphClient;
        }
    }   
}
