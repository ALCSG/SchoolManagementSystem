using System.Net;
using System.Net.Http.Json;
using SchoolManagementSystem.Mobile.Models;

namespace SchoolManagementSystem.Mobile.Services
{
    public class StudentService
    {
        private readonly HttpClient _httpClient;

        public StudentService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<StudentLearning> GetMyLearningAsync()
        {
            using var response = await _httpClient.GetAsync("api/students/me/learning");

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException("Your session has expired. Please sign in again.");
            }

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<StudentLearning>();

            return result ?? throw new InvalidOperationException("The server returned an invalid learning response.");
        }
    }
}