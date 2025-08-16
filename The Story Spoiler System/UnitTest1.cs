using NUnit.Framework;
using System.Net;
using System.Text.Json;
using RestSharp;
using RestSharp.Authenticators;

namespace The_Story_Spoiler_System
{
    [TestFixture]
    public class StorySpoilerTests
    {
        private RestClient _client;
        private static string createdStoryId;
        private const string BaseUrl = "https://d3s5nxhwblsjbi.cloudfront.net";

        [OneTimeSetUp]
        public void Setup()
        {
            string token = GetJwtToken("AHTOH40", "AHTOH40");

            var options = new RestClientOptions(BaseUrl)
            {
                Authenticator = new JwtAuthenticator(token)
            };
            _client = new RestClient(options);
        }

        private string GetJwtToken(string username, string password)
        {
            var loginClient = new RestClient(BaseUrl);
            var request = new RestRequest("/api/User/Authentication", Method.Post);
            request.AddJsonBody(new { username, password });
            var response = loginClient.Execute(request);
            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);
            return json.GetProperty("accessToken").GetString() ?? string.Empty;
        }

        [Test, Order(1)]

        public void CreateStory_ShouldReturnSuccess()
        {
            var story = new
            {
                title = "Test Story",
                description = "This is a test story.",
                url = ""
            };
            var request = new RestRequest("/api/Story/Create", Method.Post);
            request.AddJsonBody(story);

            var response = _client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(response.Content, Does.Contain("storyId"));
            Assert.That(response.Content, Does.Contain("Successfully created!"));

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);
            createdStoryId = json.GetProperty("storyId").GetString() ?? string.Empty;
        }

        [Test, Order(2)]
        public void EditStory_ShouldReturnSuccess()
        {
            var updatedStory = new
            {
                title = "Updated Test Story",
                description = "This is an updated test story.",
                url = ""
            };
            var request = new RestRequest($"/api/Story/Edit/{createdStoryId}", Method.Put);
            request.AddJsonBody(updatedStory);
            var response = _client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content, Does.Contain("Successfully edited"));
        }

        [Test, Order(3)]
        public void GetStory_ShouldReturnSuccess()
        {
            var request = new RestRequest($"/api/Story/All", Method.Get);
            var response = _client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var stories = JsonSerializer.Deserialize<List<object>>(response.Content);
            Assert.That(stories, Is.Not.Empty);
        }

        [Test, Order(4)]
        public void DeleteStory_ShouldReturnSuccess()
        {
            var request = new RestRequest($"/api/Story/Delete/{createdStoryId}", Method.Delete);
            var response = _client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content, Does.Contain("Deleted successfully!"));
        }

        [Test, Order(5)]

        public void CreateStoryWithInvalidData_ShouldReturnBadRequest()
        {
            var invalidStory = new
            {
                title = "",
                description = "This story has an invalid title.",
                url = ""
            };
            var request = new RestRequest("/api/Story/Create", Method.Post);
            request.AddJsonBody(invalidStory);
            var response = _client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test, Order(6)]

        public void EditNonExistentStory_ShouldReturnNotFound()
        {
            var updatedStory = new
            {
                title = "Non-existent Story",
                description = "This story does not exist.",
                url = ""
            };
            var request = new RestRequest("/api/Story/Edit/nonexistent-id", Method.Put);
            request.AddJsonBody(updatedStory);
            var response = _client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(response.Content, Does.Contain("No spoilers..."));
        }

        [Test, Order(7)]

        public void DeleteNonExistentStory_ShouldReturnNotFound()
        {
            var request = new RestRequest("/api/Story/Delete/nonexistent-id", Method.Delete);
            var response = _client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.Content, Does.Contain("Unable to delete this story spoiler!"));
        }

        [OneTimeTearDown]
        public void Teardown()
        {
            _client.Dispose();
        }
    }
}