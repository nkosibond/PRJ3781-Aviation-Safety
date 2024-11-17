using System.Text;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Authentication;


namespace Aviation
{
    public partial class LoginPage : ContentPage
    {
        private static readonly string FirebaseApiKey = "AIzaSyCpIXqE_wD03-4XtYohqGp0r1H7Wb-7NWA";
        public static readonly string clientId = "1005916989190-aah8kgl9fgc0uf0bnr57dr5rpk9lnmug.apps.googleusercontent.com";
        private const string ClientSecret = "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";
        private const string RedirectUri = "https://projectavigo.firebaseapp.com/__/auth/handler";

        public LoginPage()
        {
            InitializeComponent();
        }

        private async void OnSignUpButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SignUpPage());
        }

        private async void OnForgotPasswordTapped(object sender, EventArgs e)
        {
            string email = username_Entry.Text;

            if (string.IsNullOrWhiteSpace(email))
            {
                await DisplayAlert("Password Reset Failed", "Please enter your email address.", "OK");
                return;
            }

            if (!IsValidEmail(email))
            {
                await DisplayAlert("Password Reset Failed", "Please enter a valid email address.", "OK");
                return;
            }

            try
            {
                using var client = new HttpClient();
                var passwordResetUrl = $"https://identitytoolkit.googleapis.com/v1/accounts:sendOobCode?key={FirebaseApiKey}";

                var resetRequest = new
                {
                    requestType = "PASSWORD_RESET",
                    email
                };

                var content = new StringContent(JsonSerializer.Serialize(resetRequest), Encoding.UTF8, "application/json");
                var response = await client.PostAsync(passwordResetUrl, content);
                response.EnsureSuccessStatusCode();

                await DisplayAlert("Password Reset", "Password reset email has been sent. Please check your inbox.", "OK");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Password Reset Error: {ex.Message}");
                await DisplayAlert("Password Reset Failed", "An error occurred while sending the password reset email. Please try again.", "OK");
            }
        }

        private async void OnLoginButtonClicked(object sender, EventArgs e)
        {
            string email = username_Entry.Text;
            string password = password_Entry.Text;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                await DisplayAlert("Login Failed", "Please enter both email and password", "OK");
                return;
            }

            if (!IsValidEmail(email))
            {
                await DisplayAlert("Login Failed", "Please enter a valid email address.", "OK");
                return;
            }

            try
            {
                var token = await LoginWithFirebase(email, password);
                if (!string.IsNullOrEmpty(token) && await IsEmailVerified(token))
                {
                    await DisplayAlert("Login Success", "Welcome!", "OK");
                    await Navigation.PushAsync(new DashboardPage());
                }
                else
                {
                    await DisplayAlert("Verification Required", "Please verify your email before logging in.", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Login Error: {ex.Message}");
                await DisplayAlert("Login Failed", GetFriendlyErrorMessage(ex.Message), "OK");
            }
        }

        private async void OnSignInWithGoogleClicked(object sender, EventArgs e)
        {
            try
            {
                var authResult = await WebAuthenticator.AuthenticateAsync(
                    new Uri($"https://accounts.google.com/o/oauth2/v2/auth" +
                            $"?client_id={clientId}" +
                            $"&redirect_uri={RedirectUri}" +
                            $"&response_type=code" +
                            $"&scope=openid%20email%20profile"),
                    new Uri(RedirectUri));

                // Ensure we have an authorization code
                if (!authResult.Properties.TryGetValue("code", out var authorizationCode) || string.IsNullOrEmpty(authorizationCode))
                {
                    throw new Exception("Authorization code not received or is empty.");
                }

                // Exchange authorization code for an ID token
                string idToken = await ExchangeAuthCodeForIdToken(authorizationCode);

                // Sign in to Firebase using the Google ID token
                FirebaseUser firebaseUser = await SignInToFirebaseWithGoogleToken(idToken);

                Debug.WriteLine($"Google Sign-In Success: {firebaseUser.DisplayName}");
                await DisplayAlert("Sign-In Successful", $"Welcome, {firebaseUser.DisplayName}!", "OK");

                // Navigate to the DashboardPage
                await Navigation.PushAsync(new DashboardPage());
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Google Sign-In Error: {ex.Message}");
                await DisplayAlert("Google Sign-In Failed", $"Error details: {ex.Message}", "OK");
            }
        }

        private bool IsValidEmail(string email)
        {
            var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, emailPattern);
        }

        private async Task<string> LoginWithFirebase(string email, string password)
        {
            using var client = new HttpClient();
            var authUrl = $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={FirebaseApiKey}";

            var authData = new { email, password, returnSecureToken = true };
            var json = JsonSerializer.Serialize(authData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(authUrl, content);
            var result = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var authResponse = JsonSerializer.Deserialize<AuthResponse>(result);
                return authResponse.IdToken;
            }

            var errorResponse = JsonSerializer.Deserialize<FirebaseErrorResponse>(result);
            throw new Exception(errorResponse?.Error?.Message ?? "Authentication failed.");
        }

        private async Task<bool> IsEmailVerified(string idToken)
        {
            using var client = new HttpClient();
            var verifyUrl = $"https://identitytoolkit.googleapis.com/v1/accounts:lookup?key={FirebaseApiKey}";

            var verifyData = new { idToken };
            var json = JsonSerializer.Serialize(verifyData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(verifyUrl, content);
            var result = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var lookupResponse = JsonSerializer.Deserialize<LookupResponse>(result);
                return lookupResponse.Users[0].EmailVerified;
            }

            Debug.WriteLine("Failed to check email verification status.");
            return false;
        }

        private async Task<string> ExchangeAuthCodeForIdToken(string authorizationCode)
        {
            using var client = new HttpClient();
            var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "https://oauth2.googleapis.com/token")
            {
                Content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("code", authorizationCode),
                    new KeyValuePair<string, string>("client_id", clientId),
                    new KeyValuePair<string, string>("client_secret", ClientSecret),
                    new KeyValuePair<string, string>("redirect_uri", "https://projectavigo.firebaseapp.com/__/auth/handler"),
                    new KeyValuePair<string, string>("grant_type", "authorization_code"),
                })
            };

            var response = await client.SendAsync(tokenRequest);
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(jsonResponse);

            if (jsonDoc.RootElement.TryGetProperty("id_token", out var idTokenElement))
                return idTokenElement.GetString();

            throw new Exception("Failed to retrieve ID Token");
        }

        private async Task<FirebaseUser> SignInToFirebaseWithGoogleToken(string idToken)
        {
            using var client = new HttpClient();
            var firebaseAuthUrl = $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithIdp?key={FirebaseApiKey}";

            var payload = new
            {
                postBody = $"id_token={idToken}&providerId=google.com",
                requestUri = "https://projectavigo.firebaseapp.com/__/auth/handler",
                returnIdpCredential = true,
                returnSecureToken = true
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(firebaseAuthUrl, content);
            var jsonResponse = await response.Content.ReadAsStringAsync();

            var authResult = JsonSerializer.Deserialize<FirebaseAuthResult>(jsonResponse);
            return new FirebaseUser
            {
                DisplayName = authResult.DisplayName,
                Email = authResult.Email,
                PhotoUrl = authResult.PhotoUrl
            };
        }

        private string GetFriendlyErrorMessage(string errorMessage)
        {
            return errorMessage switch
            {
                "EMAIL_NOT_FOUND" => "Email not found. Please check your email or sign up.",
                "INVALID_PASSWORD" => "Invalid password. Please try again.",
                "USER_DISABLED" => "This account has been disabled. Please contact support.",
                _ => "An unknown error occurred. Please try again."
            };
        }

        public class FirebaseUser
        {
            public string DisplayName { get; set; }
            public string Email { get; set; }
            public string PhotoUrl { get; set; }
        }

        public class AuthResponse
        {
            [JsonPropertyName("idToken")]
            public string IdToken { get; set; }
        }

        public class LookupResponse
        {
            [JsonPropertyName("users")]
            public List<LookupUser> Users { get; set; }
        }

        public class LookupUser
        {
            [JsonPropertyName("emailVerified")]
            public bool EmailVerified { get; set; }
        }

        public class FirebaseAuthResult
        {
            [JsonPropertyName("displayName")]
            public string DisplayName { get; set; }

            [JsonPropertyName("email")]
            public string Email { get; set; }

            [JsonPropertyName("photoUrl")]
            public string PhotoUrl { get; set; }
        }

        public class FirebaseErrorResponse
        {
            [JsonPropertyName("error")]
            public FirebaseError Error { get; set; }
        }

        public class FirebaseError
        {
            [JsonPropertyName("message")]
            public string Message { get; set; }
        }
    }
}
