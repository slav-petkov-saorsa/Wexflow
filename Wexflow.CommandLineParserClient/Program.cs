using CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using Wexflow.CommandLineParserClient.Common.Contents;
using Wexflow.CommandLineParserClient.Common.Extensions;
using Wexflow.CommandLineParserClient.Configuration;
using Wexflow.CommandLineParserClient.Options.Workflow;
using Wexflow.CommandLineParserClient.Resources;

namespace Wexflow.CommandLineParserClient
{
    class Program
    {
        private static IConfigurationRoot Configuration;
        private static HttpClient Client;

        static void Main(string[] args)
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            ConfigureHttpClient();

            var result = Parser.Default.ParseArguments<CreateWorkflowOptions, ListWorkflowsOptions, StartWorfklowOptions>(args)
                .MapResult(
                    (CreateWorkflowOptions opts) => RunCreateWorkflow(opts),
                    (ListWorkflowsOptions opts) => RunListWorkflow(opts),
                    (StartWorfklowOptions opts) => RunStartWorkflow(opts),
                    errors => RunErrors(errors)
                );

            Console.ReadLine();
        }

        private static int RunCreateWorkflow(CreateWorkflowOptions options)
        {
            var workflowCreateResource = WorkflowCreate.FromCommandLineOptions(options);
            var requestBodyPayload = new JsonContent(new { WorkflowInfo = workflowCreateResource, workflowCreateResource.Tasks });
            var createWorkflowUrl = Configuration["Wexflow:CreateWorkflow"];
            using (var response = Client.PostAsync(createWorkflowUrl, requestBodyPayload).Result)
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Console.WriteLine($"Workflow {options.Name} created successfully");
                    return 0;
                }
                else
                {
                    Console.WriteLine($"An error occurred while creating workfliw {options.Name}");
                    var responseMessage = response.Content.ReadAsStringAsync().Result;
                    Console.Error.WriteLine(responseMessage);
                    return (int)response.StatusCode;//think for error codes
                }
            }
        }

        private static int RunListWorkflow(ListWorkflowsOptions options)
        {
            if (options.Active)
            {
                var listActiveOptionsUrl = Configuration["Wexflow:GetActiveWorkflows"];
                using (var response = Client.GetAsync(listActiveOptionsUrl).Result)
                {
                    var responseText = response.Content.ReadAsStringAsync().Result;
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        var activeWorkflows = JsonSerializer.Deserialize<IEnumerable<ActiveWorkflowInstance>>(responseText);
                        if (activeWorkflows.Any())
                        {
                            foreach (var workflow in activeWorkflows)
                            {
                                Console.WriteLine(workflow);
                            }
                        }
                        else
                        {
                            Console.WriteLine("There are no active workflow instances at the moment");
                        }

                        return 0;
                    }
                    else
                    {
                        Console.WriteLine("An error occurred while listing active workflows");
                        var responseMessage = responseText;
                        Console.Error.WriteLine(responseMessage);
                        return (int)response.StatusCode;
                    }
                }
            }
            
            return -1;
        }

        private static int RunStartWorkflow(StartWorfklowOptions options)
        {
            var startWorkflowUrl = Configuration["Wexflow:StartWorkflow"];
            using (var response = Client.PostAsync($"{startWorkflowUrl}?w={options.WorkflowId}", JsonContent.Empty).Result)
            {
                var responseText = response.Content.ReadAsStringAsync().Result;
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Console.WriteLine($"Workflow instance {responseText} successfully started");
                    return 0;
                }
                else
                {
                    Console.WriteLine("An error orccurred");
                    Console.Error.WriteLine(responseText);
                    return (int)response.StatusCode;
                }
            }
        }

        private static int RunErrors(IEnumerable<Error> errors)
        {
            var result = errors.Any(x => x is HelpRequestedError || x is VersionRequestedError)
                ? -1 : -2;
            Console.WriteLine("Exit code {0}", result);
            return result;
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            // Build configuration
            Configuration = new ConfigurationBuilder()
                .SetBasePath(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile))
                .AddJsonFile("appsettings.json", false)
                .Build();

            services.AddSingleton(Configuration);
        }

        private static void ConfigureHttpClient()
        {
            var wexflowSection = Configuration.GetSection("Wexflow");
            var baseUrl = wexflowSection["ApiRoot"];
            var user = wexflowSection["User"];
            var password = wexflowSection["Password"].EncodeWithMd5();

            var stringToBeEncoded = $"{user}:{password}";
            var bytes = Encoding.UTF8.GetBytes(stringToBeEncoded);
            var base64Encoded = Convert.ToBase64String(bytes);

            Client = new HttpClient()
            {
                BaseAddress = new Uri(baseUrl),
                Timeout =  Timeout.InfiniteTimeSpan
            };
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64Encoded);

        }


    }
}
