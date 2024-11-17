using Firebase.Database;
using Firebase.Database.Query;
using Firebase.Auth;
using Microsoft.Maui.Controls;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text;

namespace Aviation
{
    public partial class SignUpPage : ContentPage
    {
        // Firebase Database Client 
        private readonly FirebaseClient firebaseClient = new FirebaseClient("https://projectavigo-default-rtdb.firebaseio.com");
        private readonly FirebaseAuthProvider authProvider = new FirebaseAuthProvider(new FirebaseConfig("AIzaSyCpIXqE_wD03-4XtYohqGp0r1H7Wb-7NWA"));

        // Flag to prevent duplicate alerts
        private bool _isAlertShown = false;

        public SignUpPage()
        {
            InitializeComponent();
        }

        // Event handler for the SignUp button
        private async void OnSignUpButtonClicked(object sender, EventArgs e)
        {
            string email = email_Entry.Text;
            string name = name_Entry.Text;
            string surname = surname_Entry.Text;
            string licenseNumber = licenseNumber_Entry.Text;
            string password = password_Entry.Text;
            string confirmPassword = confirm_password_Entry.Text;

            if (!ValidateInputs(email, name, surname, licenseNumber, password, confirmPassword))
                return;

            if (await EmailAddressExistsAsync(email))
            {
                await ShowAlert("Email Already Exists", "This email address has already been registered.");
                return;
            }

            if (await LicenseNumberExistsAsync(licenseNumber))
            {
                await ShowAlert("Duplicate License", "This license number already exists in the system.");
                return;
            }

            try
            {
                // Create user in Firebase Authentication
                var authResult = await authProvider.CreateUserWithEmailAndPasswordAsync(email, password, $"{name} {surname}", true);

                // Save user data to Firebase Realtime Database
                await SaveUserDataToRealtimeDatabase(authResult.User.LocalId, email, name, surname, licenseNumber);

                // Notify user to verify their email
                await ShowAlert("Sign Up Successful", "Please check your email to verify your account.");

                // Send verification email after user data has been saved
                await SendVerificationEmail(authResult.FirebaseToken);

                // Navigate back to the previous page
                await Navigation.PopAsync();
            }
            catch (FirebaseAuthException authEx) when (authEx.Reason == AuthErrorReason.EmailExists)
            {
                await ShowAlert("Sign Up Failed", "This email is already in use. Please use a different email.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during sign-up: {ex.Message}");
                await ShowAlert("Sign Up Failed", $"An error occurred: {ex.Message}");
            }
        }

        private async Task<bool> EmailAddressExistsAsync(string email)
        {
            try
            {
                var users = await firebaseClient.Child("Pilot").OnceAsync<object>();
                return users.Any(user => (user.Object as dynamic)?.Email == email);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking email: {ex.Message}");
                await ShowAlert("Error", "Error occurred while trying to check the email.");
                return false;
            }
        }

        private async Task<bool> LicenseNumberExistsAsync(string licenseNumber)
        {
            try
            {
                var pilots = await firebaseClient.Child("Pilot").OnceAsync<object>();
                return pilots.Any(pilot => (pilot.Object as dynamic)?.LicenseNumber == licenseNumber);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking license number: {ex.Message}");
                await ShowAlert("Database Error", "An error occurred while verifying license number.");
                return false;
            }
        }

        private async Task SaveUserDataToRealtimeDatabase(string userId, string email, string name, string surname, string licenseNumber)
        {
            try
            {
                var user = new
                {
                    Email = email,
                    Name = name,
                    Surname = surname,
                    LicenseNumber = licenseNumber,
                    UserId = userId
                };

                await firebaseClient.Child("Pilot").Child(userId).PutAsync(user);
                await ShowAlert("Sign Up Complete", "Account information saved!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving user data: {ex.Message}");
                await ShowAlert("Database Error", "Failed to save user information.");
            }
        }

        private async void OnSignUpWithGoogle()
        {
            await DisplayAlert("Google Sign Up", "Processing...", "Ok");
        }

        private async Task SendVerificationEmail(string idToken)
        {
            loadingIndicator.IsVisible = true;
            loadingIndicator.IsRunning = true;

            try
            {
                using var httpClient = new HttpClient();
                var requestData = new { requestType = "VERIFY_EMAIL", idToken = idToken };
                var json = JsonSerializer.Serialize(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync($"https://identitytoolkit.googleapis.com/v1/accounts:sendOobCode?key=YOUR_FIREBASE_API_KEY", content);

                await Task.Delay(2500); // Delay to give Firebase time to process

                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Verification email sent.");
                    await ShowAlert("Verification Email Sent", "Please check your email inbox to verify your account.");
                }
                else
                {
                    var errorMessage = ParseFirebaseError(responseContent);
                    Console.WriteLine($"Failed to send verification email. Error: {errorMessage}");
                    await ShowAlert("Verification Email Sent", "Please check your email inbox to verify your account.");
                }
            }
            catch (HttpRequestException httpEx)
            {
                Console.WriteLine($"Network Error: {httpEx.Message}");
                await ShowAlert("Network Error", "Please check your internet connection and try again.");
            }
            catch (Exception ex)
            {
                await ShowAlert("Verification Required", $"Please verify your account next {ex.Message}");
                Console.WriteLine(ex.ToString()); // Log the full exception details for debugging
            }
            finally
            {
                loadingIndicator.IsVisible = false;
                loadingIndicator.IsRunning = false;
            }
        }

        private string ParseFirebaseError(string responseContent)
        {
            try
            {
                var errorResponse = JsonSerializer.Deserialize<FirebaseErrorResponse>(responseContent);
                return errorResponse?.Error?.Message ?? "Unknown error occurred.";
            }
            catch
            {
                return "Failed to parse error response.";
            }
        }

        public class FirebaseErrorResponse
        {
            public FirebaseError Error { get; set; }
        }

        public class FirebaseError
        {
            public string Message { get; set; }
        }

        private bool ValidateInputs(string email, string name, string surname, string licenseNumber, string password, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(email) || !IsValidEmail(email))
            {
                ShowAlert("Input Error", "Please enter a valid email address.");
                return false;
            }
            if (string.IsNullOrWhiteSpace(name) || !IsValidName(name))
            {
                ShowAlert("Input Error", "Name should not contain numbers or special characters.");
                return false;
            }
            if (string.IsNullOrWhiteSpace(surname) || !IsValidName(surname))
            {
                ShowAlert("Input Error", "Surname should not contain numbers or special characters.");
                return false;
            }
            if (string.IsNullOrWhiteSpace(licenseNumber))
            {
                ShowAlert("Input Error", "License Number is required.");
                return false;
            }
            if (password != confirmPassword)
            {
                ShowAlert("Input Error", "Passwords do not match.");
                return false;
            }
            if (password.Length < 6)
            {
                ShowAlert("Input Error", "Password must be at least 6 characters.");
                return false;
            }
            return true;
        }

        private bool IsValidEmail(string email)
        {
            var emailRegex = new Regex(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$");
            return emailRegex.IsMatch(email);
        }

        private bool IsValidName(string name)
        {
            var nameRegex = new Regex("^[a-zA-Z]+$");
            return nameRegex.IsMatch(name);
        }

        private async Task ShowAlert(string title, string message)
        {
            if (!_isAlertShown)
            {
                _isAlertShown = true;
                await DisplayAlert(title, message, "OK");
                _isAlertShown = false;
            }
        }

        private async void OnLoginLinkTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new LoginPage());
        }
    }
}
