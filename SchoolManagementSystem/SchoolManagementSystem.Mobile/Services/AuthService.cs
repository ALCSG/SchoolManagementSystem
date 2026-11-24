using System.Net;
using System.Net.Http.Json;
using SchoolManagementSystem.Mobile.Models;

namespace SchoolManagementSystem.Mobile.Services
{
    public class AuthService
    {
        public const string AccessTokenKey = "access_token";

        private readonly HttpClient _httpClient;

        public AuthService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<LoginResponse> LoginAsync(
            string email,
            string password)
        {
            using var response = await _httpClient.PostAsJsonAsync(
                "api/auth/login",
                new
                {
                    Email = email.Trim(),
                    Password = password
                });

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new InvalidOperationException(
                    "Invalid credentials or login unavailable. " +
                    "If two-factor authentication is enabled, use the website.");
            }

            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                throw new InvalidOperationException(
                    "This app requires a student account " +
                    "linked to a student record.");
            }

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                throw new InvalidOperationException(
                    "Too many login attempts. Please try again later.");
            }

            response.EnsureSuccessStatusCode();

            var result = await response.Content
                .ReadFromJsonAsync<LoginResponse>();

            if (result == null ||
                string.IsNullOrWhiteSpace(result.AccessToken) ||
                result.ExpiresAtUtc <= DateTime.UtcNow)
            {
                throw new InvalidOperationException(
                    "The server returned an invalid login response.");
            }

            await SecureStorage.Default.SetAsync(
                AccessTokenKey,
                result.AccessToken);

            return result;
        }

        public Task LogoutAsync()
        {
            SecureStorage.Default.Remove(AuthService.AccessTokenKey);

            return Task.CompletedTask;
        }
    }
}