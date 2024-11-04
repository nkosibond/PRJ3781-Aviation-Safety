using Microsoft.Maui.Controls;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Diagnostics;

namespace Aviation
{
    public partial class LoginPage : ContentPage
    {
        private const string FirebaseApiKey = "AIzaSyCpIXqE_wD03-4XtYohqGp0r1H7Wb-7NWA";

        public LoginPage()
        {
            InitializeComponent();
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

            try
            {
                var token = await LoginWithFirebase(email, password);
                if (!string.IsNullOrEmpty(token))
                {
                    await DisplayAlert("Login Success", "Welcome!", "OK");
                    await Navigation.PushAsync(new DashboardPage());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Login Error: {ex.Message}");
                await DisplayAlert("Login Failed", "Invalid email or password. Please try again.", "OK");
            }
        }

        private async void OnSignUpButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SignUpPage());
        }

        private async void OnForgotPasswordTapped(object sender, EventArgs e)
        {
            string email = await DisplayPromptAsync("Reset Password", "Enter your email address:");

            if (!string.IsNullOrWhiteSpace(email))
            {
                await SendPasswordResetEmail(email);
            }
            else
            {
                await DisplayAlert("Error", "Please enter a valid email address", "OK");
            }
        }

        private async void OnSignInWithGoogleClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Google Sign-In", "Google Sign-In is under development.", "OK");
        }

        // Firebase REST API Login Method
        private async Task<string> LoginWithFirebase(string email, string password)
        {
            using (var client = new HttpClient())
            {
                var authUrl = $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={FirebaseApiKey}";

                var authData = new
                {
                    email = email,
                    password = password,
                    returnSecureToken = true
                };

                var json = JsonConvert.SerializeObject(authData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(authUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    var authResponse = JsonConvert.DeserializeObject<AuthResponse>(result);

                    // Check if email is verified
                    if (!authResponse.EmailVerified)
                    {
                        await DisplayAlert("Verification Required", "Please verify your email before logging in.", "OK");
                        return null;
                    }

                    return authResponse.IdToken;
                }
                else
                {
                    throw new Exception("Authentication failed.");
                }
            }
        }

        // Firebase REST API Password Reset Method
        private async Task SendPasswordResetEmail(string email)
        {
            using (var client = new HttpClient())
            {
                var resetUrl = $"https://identitytoolkit.googleapis.com/v1/accounts:sendOobCode?key={FirebaseApiKey}";

                var resetData = new
                {
                    requestType = "PASSWORD_RESET",
                    email = email
                };

                var json = JsonConvert.SerializeObject(resetData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(resetUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    await DisplayAlert("Success", "Password reset email sent. Please check your inbox.", "OK");
                }
                else
                {
                    await DisplayAlert("Error", "Failed to send password reset email. Please try again.", "OK");
                }
            }
        }
    }

    // Model for Firebase response
    public class AuthResponse
    {
        [JsonProperty("idToken")]
        public string IdToken { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("refreshToken")]
        public string RefreshToken { get; set; }

        [JsonProperty("expiresIn")]
        public string ExpiresIn { get; set; }

        [JsonProperty("localId")]
        public string LocalId { get; set; }

        [JsonProperty("emailVerified")]
        public bool EmailVerified { get; set; }
    }
}
