using RestSharp;
using RestSharp.Authenticators;
using RestSharpServices.Models;
using System;
using System.Net;
using System.Text.Json;




namespace RestSharpServices
{
    public class GitHubApiClient
    {
        private RestClient client;

        public GitHubApiClient(string baseUrl, string username, string token) // GitHub API Client creation
        {
            //throw new NotImplementedException();
            var options = new RestClientOptions(baseUrl)
            { 
                Authenticator = new HttpBasicAuthenticator(username, token)
            };
            this.client = new RestClient(options);
        }

        public List<Issue> ? GetAllIssues(string repo) // ? - returns list or null; public List<Issue> - should return only list
        {
            var request = new RestRequest($"{repo}/issues", Method.Get);
            var response = client.Execute(request);
            return response.Content != null ? JsonSerializer.Deserialize<List<Issue>>(response.Content) : null;
        }

        public Issue?  GetIssueByNumber(string repo, long issueNumber) //? - returns issue or null;
        {
            var request = new RestRequest($"{repo}/issues/{issueNumber}", Method.Get);
            var response = client.Execute(request);
            return response.Content != null ? JsonSerializer.Deserialize<Issue>(response.Content) : null;


        }

        public Issue ? CreateIssue(string repo, string title, string body)
        {

            var request = new RestRequest($"{repo}/issues", Method.Post);
            request.AddJsonBody(new { title, body });

            var response = client.Execute(request);

            return response.Content != null ? JsonSerializer.Deserialize<Issue>(response.Content) : null;


        }

        public List<Label> ? GetAllLabelsForIssue(string repo, long issueNumber)
        {
            var request = new RestRequest($"{repo}/issues/{issueNumber}/labels", Method.Get);
            var response = client.Execute(request);

            return response.Content != null ? JsonSerializer.Deserialize<List<Label>>(response.Content) : null;


        }

        public List<Comment> ? GetAllCommentsForIssue(string repo, int issueNumber)
        {
            var request = new RestRequest($"{repo}/issues/{issueNumber}/comments", Method.Get);
            var response = client.Execute(request);

            return response.Content != null ? JsonSerializer.Deserialize<List<Comment>>(response.Content) : null;
        }

        public Comment? CreateCommentOnGitHubIssue(string repo, long issueNumber, string body)
        {

            var request = new RestRequest($"{repo}/issues/{issueNumber}/comments ", Method.Post);
            //var request = new RestRequest($"{repo}/issues/{issueNumber}/comments ");
            request.AddJsonBody(new { body });

            var response = client.Execute(request);
            //var response = client.Execute(request, Method.Post);

            return response.Content != null ? JsonSerializer.Deserialize<Comment>(response.Content) : null;
        }

        public Comment ? GetCommentById (string repo, long commentId)
        {
            var request = new RestRequest($"{repo}/issues/comments/{commentId}", Method.Get);
            var response = client.Execute(request);
            return response.Content != null ? JsonSerializer.Deserialize<Comment>(response.Content) : null;

        }

        public Comment ? EditCommentOnGitHubIssue( string repo, long commentId, string newBody)
        {
            var request = new RestRequest($"{repo}/issues/comments/{commentId}", Method.Patch);
            request.AddJsonBody(new { body = newBody });
            var response = client.Execute(request);
            return response.IsSuccessful && response.Content != null ? JsonSerializer.Deserialize<Comment>(response.Content) : null;

        }

        public bool DeleteCommentOnGitHubIssue(string repo, long commentId)
        {
            var request = new RestRequest($"{repo}/issues/comments/{commentId}", Method.Delete);
            var response = client.Execute(request);
            return response.IsSuccessful;
        }

    }
}
