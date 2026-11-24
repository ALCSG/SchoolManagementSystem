using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using SchoolManagementSystem.Mobile.Models;

namespace SchoolManagementSystem.Mobile.Services
{
    public class ProfileService
    {
        private readonly HttpClient _httpClient;

        public ProfileService(
            HttpClient httpClient,
            AuthService authService)
        {
            _httpClient = httpClient;
        }

        public async Task<StudentProfile> GetProfileAsync()
        {
            using var request = await CreateRequestAsync(
                HttpMethod.Get,
                "api/students/me/profile");

            using var response = await _httpClient.SendAsync(request);

            EnsureSuccess(response);

            var profile = await response.Content
                .ReadFromJsonAsync<StudentProfile>();

            return profile ?? throw new InvalidOperationException(
                "The server returned an invalid profile response.");
        }

        public async Task UpdateProfileAsync(
            string? phoneNumber,
            byte[]? photoBytes,
            string? photoFileName)
        {
            using var request = await CreateRequestAsync(
                HttpMethod.Put,
                "api/students/me/profile");

            var form = new MultipartFormDataContent();

            // The request owns and disposes the multipart content.
            request.Content = form;

            form.Add(
                new StringContent(phoneNumber?.Trim() ?? string.Empty),
                "PhoneNumber");

            if (photoBytes != null)
            {
                if (string.IsNullOrWhiteSpace(photoFileName))
                {
                    throw new ArgumentException(
                        "The photo file name is required.",
                        nameof(photoFileName));
                }

                var extension = Path.GetExtension(photoFileName)
                    .ToLowerInvariant();

                var photoContent = new ByteArrayContent(photoBytes);

                photoContent.Headers.ContentType =
                    new MediaTypeHeaderValue(
                        extension == ".png"
                            ? "image/png"
                            : "image/jpeg");

                form.Add(
                    photoContent,
                    "ProfilePictureFile",
                    Path.GetFileName(photoFileName));
            }

            using var response = await _httpClient.SendAsync(request);

            EnsureSuccess(response);
        }

        public string? GetPhotoUrl(string? photoPath)
        {
            if (string.IsNullOrWhiteSpace(photoPath))
                return null;

            if (Uri.TryCreate(
                photoPath,
                UriKind.Absolute,
                out var absoluteUri)
                && (absoluteUri.Scheme == Uri.UriSchemeHttps
                    || absoluteUri.Scheme == Uri.UriSchemeHttp))
            {
                return absoluteUri.AbsoluteUri;
            }

            var baseAddress = _httpClient.BaseAddress
                ?? throw new InvalidOperationException(
                    "The API base address is not configured.");

            return new Uri(baseAddress, photoPath).AbsoluteUri;
        }

        private async Task<HttpRequestMessage> CreateRequestAsync(
            HttpMethod method,
            string path)
        {
            var token = await SecureStorage.Default
                .GetAsync(AuthService.AccessTokenKey);

            if (string.IsNullOrWhiteSpace(token))
            {
                throw new UnauthorizedAccessException(
                    "Your session has expired. Please sign in again.");
            }

            var request = new HttpRequestMessage(method, path);

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            return request;
        }

        private static void EnsureSuccess(HttpResponseMessage response)
        {
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException(
                    "Your session has expired. Please sign in again.");
            }

            response.EnsureSuccessStatusCode();
        }
    }
}