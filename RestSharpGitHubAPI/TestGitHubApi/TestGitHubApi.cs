using NUnit.Framework.Internal;
using RestSharp;
using RestSharp.Authenticators;
using RestSharpServices;
using RestSharpServices.Models;
using System;
using System.Net;
using System.Reflection.Emit;
using System.Text.Json;
using static Microsoft.TestPlatform.AdapterUtilities.HierarchyConstants;

namespace TestGitHubApi
{
    public class TestGitHubApi
    {
        private GitHubApiClient client;
        private static long lastCreatedIssueNumber; // To store the number of the last created issue for later use in comment tests
        private static long lastCreatedCommentId; // To store the ID of the last created comment for later use in edit and delete tests
        private static string repo = "test-nakov-repo";


        [SetUp]
        public void Setup()
        {
            //token - my GitHub token
            client = new GitHubApiClient("https://api.github.com/repos/testnakov/", "echiose", "token");
        }


        [Test, Order(1)]
        public void Test_GetAllIssuesFromARepo()
        {
            var issues = client.GetAllIssues(repo);
            Assert.That(issues, Has.Count.GreaterThan(0), "There should be more than one issue");

            foreach (var issue in issues)
            {
                Assert.That(issue.Id, Is.GreaterThan(0), "Issue ID should  be greater than 0");
                Assert.That(issue.Number, Is.GreaterThan(0), "Issue number should  be greater than 0");
                Assert.That(issue.Title, Is.Not.Null.And.Not.Empty, "Issue title should not be null or empty");
            }

        }

        [Test, Order(2)]
        public void Test_GetIssueByValidNumber()
        {
            int issueNumber = 1;// Assuming issue number 1 exists in the repository

            var issue = client.GetIssueByNumber(repo, issueNumber);

            Assert.That(issue, Is.Not.Null, $"Issue with number {issueNumber} should exist");
            Assert.That(issue.Id, Is.GreaterThan(0), "Issue ID should  be greater than 0");
            Assert.That(issue.Number, Is.EqualTo(issueNumber), $"Issue number should be {issueNumber}");
        }

        [Test, Order(3)]
        public void Test_GetAllLabelsForIssue()
        {
            int issueNumber = 6;// Assuming issue number 1 exists in the repository

            var labels = client.GetAllLabelsForIssue(repo, issueNumber);

            if (labels != null || labels.Count == 0)
            {
                foreach (var label in labels)
                {
                    Assert.That(label.Id, Is.GreaterThan(0), "Label ID should  be greater than 0");
                    Assert.That(label.Name, Is.Not.Null.And.Not.Empty, "Label name should not be null or empty");
                }
            }
            else
            {
                Assert.Pass("No labels found for the issue, but the API call was successful");
            }
        }
        [Test, Order(4)]
        public void Test_GetAllCommentsForIssue()
        {

            int issueNumber = 6;// Assuming issue number 1 exists in the repository

            List<Comment> comments = client.GetAllCommentsForIssue(repo, issueNumber);

            Assert.That(comments.Count, Is.GreaterThan(0));

            if (comments != null || comments.Count == 0)
            {
                foreach (Comment comment in comments)
                {
                    Assert.That(comment.Id, Is.GreaterThan(0), "Comment ID should be greater than 0");
                    Assert.That(comment.Body, Is.Not.Null.And.Not.Empty, "Comment name should not be null or empty");
                }
            }
            else
            {
                Assert.Pass("No comments found for the issue, but the API call was successful");
            }
        }

        [Test, Order(5)]
        public void Test_CreateGitHubIssue()
        {
            string expectedTitle = "New Issue from NUnit Test";
            string expectedBody = "This issue was created as part of NUnit testing";
            var createdIssue = client.CreateIssue(repo, expectedTitle, expectedBody);
            if (createdIssue != null)
            {


                Assert.Multiple(() =>
                {
                    Assert.That(createdIssue, Is.Not.Null, "Created issue should not be null");
                    Assert.That(createdIssue.Id, Is.GreaterThan(0), "Created issue ID should be greater than 0");
                    Assert.That(createdIssue.Number, Is.GreaterThan(0), "Created issue Number should be greater than 0");
                    Assert.That(createdIssue.Title, Is.EqualTo(expectedTitle), $"Created issue title should match the input title - {expectedTitle}");
                    Assert.That(createdIssue.Body, Is.EqualTo(expectedBody), $"Created issue body should match the input body  - {expectedBody}");
                });
                lastCreatedIssueNumber = createdIssue.Number; // Store the number of the created issue for later use in comment tests
            }
            else
                Assert.Fail("Failed to create issue. The API call returned null. "); // if title and body are empty strings
        }

        [Test, Order(6)]
        public void Test_CreateCommentOnGitHubIssue()
        {
            long issueNumber = lastCreatedIssueNumber; // Use the number of the issue created in the previous test
            string expectedCommentBody = "This comment was created as part of NUnit testing";

            var comment = client.CreateCommentOnGitHubIssue(repo, issueNumber, expectedCommentBody);

            if (comment != null)
            {
                Assert.That(comment.Body, Is.EqualTo(expectedCommentBody), "Comment's body should be as expected");
                lastCreatedCommentId = comment.Id; // Store the ID of the created comment for later use in edit and delete tests
            }

        }

        [Test, Order(7)]
        public void Test_GetCommentById()
        {
            long commentId = lastCreatedCommentId; // Use the ID of the comment created in the previous test
            var comment = client.GetCommentById(repo, commentId);
            Assert.That(comment, Is.Not.Null, $"Comment with ID {commentId} should exist");
            Assert.That(comment.Id, Is.EqualTo(commentId), $"Comment ID should be {commentId}");

        }


        [Test, Order(8)]
        public void Test_EditCommentOnGitHubIssue()
        {
            long commentId = lastCreatedCommentId; // Use the ID of the comment created in the previous test
            string newCommentBody = "This comment was edited as part of NUnit testing";
            var editedComment = client.EditCommentOnGitHubIssue(repo, commentId, newCommentBody);
            if (editedComment != null)
            {

                Assert.That(editedComment, Is.Not.Null, "Edited comment should not be null");
                Assert.That(editedComment.Id, Is.EqualTo(commentId), $"Edited comment ID should be {commentId}");
                Assert.That(editedComment.Body, Is.EqualTo(newCommentBody), "Edited comment body should be as expected");
            }
        }

        [Test, Order(9)]
        public void Test_DeleteCommentOnGitHubIssue()
        {
            long commentId = lastCreatedCommentId; // Use the ID of the comment created in the previous test
            bool isDeleted = client.DeleteCommentOnGitHubIssue(repo, commentId);
            Assert.That(isDeleted, Is.True, $"Comment with ID {commentId} should be deleted successfully");

        }

        //Data-driven test to check the behavior of GetIssueByNumber method when an issue with the specified number does not exist in the repository
        [TestCase("test-nakov-repo", 1)]
        [TestCase("test-nakov-repo", 2)]
        [TestCase("test-nakov-repo", 100)]
        public void Test_GetIssueByValidNumber(string repo, long issueNumber)
        {
            var issue = client.GetIssueByNumber(repo, issueNumber);
            Assert.That(issue, Is.Not.Null, $"Issue should not be null");
            Assert.That(issue.Id, Is.GreaterThan(0), "Issue ID should be greater than 0");
            Assert.That(issue.Number, Is.EqualTo(issueNumber), $"Issue number should be {issueNumber}");
        }
        [TestCase("test-nakov-repo", 1)]
        [TestCase("test-nakov-repo", 2)]
        [TestCase("test-nakov-repo", 100)]
        public void Test_GetAllLablesForIssue_DataDriven(string repo, long issueNumber)
        {
            var labels = client.GetAllLabelsForIssue(repo, issueNumber);
            if (labels != null)
            {
                foreach (var label in labels)
                {

                    Assert.That(label, Is.Not.Null, "Label should not be null");
                    Assert.That(label.Id, Is.GreaterThan(0), "Label ID should be greater than 0");
                    Assert.That(label.Name, Is.Not.Null, "Label name should not be null");

                }
            }
        }
    }
}

