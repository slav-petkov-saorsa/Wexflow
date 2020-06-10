﻿using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.HttpDelete
{
    public class HttpDelete : Task
    {
        public string Url { get; private set; }
        public string Payload { get; private set; }
        public string AuthorizationScheme { get; private set; }
        public string AuthorizationParameter { get; private set; }

        public HttpDelete(XElement xe, Workflow wf) : base(xe, wf)
        {
            Url = GetSetting("url");
            AuthorizationScheme = GetSetting("authorizationScheme");
            AuthorizationParameter = GetSetting("authorizationParameter");
        }

        public override TaskStatus Run()
        {
            Info("Executing DELETE request...");
            var status = WorkflowStatus.Success;
            try
            {
                var deleteTask = Delete(Url, AuthorizationScheme, AuthorizationParameter);
                deleteTask.Wait();
                var result = deleteTask.Result;
                var destFile = Path.Combine(Workflow.WorkflowTempFolder, string.Format("HttpDelete_{0:yyyy-MM-dd-HH-mm-ss-fff}", DateTime.Now));
                File.WriteAllText(destFile, result);
                Files.Add(new FileInf(destFile, Id));
                InfoFormat("DELETE request {0} executed whith success -> {1}", Url, destFile);
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while executing the DELETE request {0}: {1}", Url, e.Message);
                status = WorkflowStatus.Error;
            }
            Info("Task finished.");
            return new TaskStatus(status);
        }

        public async System.Threading.Tasks.Task<string> Delete(string url, string authScheme, string authParam)
        {
            using (var httpClient = new HttpClient())
            {
                if (!string.IsNullOrEmpty(authScheme) && !string.IsNullOrEmpty(authParam))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(authScheme, authParam);
                }

                var httpResponse = await httpClient.DeleteAsync(url);
                if (httpResponse.Content != null)
                {
                    var responseContent = await httpResponse.Content.ReadAsStringAsync();
                    return responseContent;
                }
            }
            return string.Empty;
        }
    }
}
