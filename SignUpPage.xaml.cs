using FirebaseAdmin.Auth;
using Firebase.Database;
using Firebase.Database.Query;
using Microsoft.Maui.Controls;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Linq;

namespace Aviation
{
    public partial class SignUpPage : ContentPage
    {
        // Firebase Database Client 
        private readonly FirebaseClient firebaseClient = new FirebaseClient("https://projectavigo-default-rtdb.firebaseio.com");

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

            // Check if license number already exists
            if (await LicenseNumberExistsAsync(licenseNumber))
            {
                await DisplayAlert("Duplicate License", "This license number already exists in the system.", "OK");
                return;
            }

            try
            {
                // Creating the user in Firebase Authentication
                var userRecordArgs = new UserRecordArgs()
                {
                    Email = email,
                    EmailVerified = false,
                    Password = password,
                    DisplayName = $"{name} {surname}",
                    Disabled = false,
                };

                UserRecord userRecord = await FirebaseAuth.DefaultInstance.CreateUserAsync(userRecordArgs);
                await DisplayAlert("Sign Up Successful", "User created successfully!", "OK");

                // Save user data in Firebase Realtime Database
                await SaveUserDataToRealtimeDatabase(userRecord.Uid, email, name, surname, licenseNumber);

                // Navigate to the next page or login page
                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during sign-up: {ex.Message}");
                await DisplayAlert("Sign Up Failed", $"An error occurred: {ex.Message}", "OK");
            }
        }

        // Method to check if license number already exists in the database
        private async Task<bool> LicenseNumberExistsAsync(string licenseNumber)
        {
            try
            {
                var pilots = await firebaseClient
                    .Child("Pilot")
                    .OnceAsync<object>();

                return pilots.Any(pilot => (pilot.Object as dynamic)?.LicenseNumber == licenseNumber);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking license number: {ex.Message}");
                await DisplayAlert("Database Error", "An error occurred while verifying license number.", "OK");
                return false;
            }
        }

        // Save user data to Firebase Realtime Database
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

                await firebaseClient
                    .Child("Pilot")
                    .Child(userId)
                    .PutAsync(user);

                await DisplayAlert("Sign Up Complete", "User information saved to the database!", "OK");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving user data: {ex.Message}");
                await DisplayAlert("Database Error", "Failed to save user information.", "OK");
            }
        }

        // Validation methods
        private bool ValidateInputs(string email, string name, string surname, string licenseNumber, string password, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(email) || !IsValidEmail(email))
            {
                DisplayAlert("Input Error", "Please enter a valid email address.", "OK");
                return false;
            }
            if (string.IsNullOrWhiteSpace(name) || !IsValidName(name))
            {
                DisplayAlert("Input Error", "Name should not contain numbers or special characters.", "OK");
                return false;
            }
            if (string.IsNullOrWhiteSpace(surname) || !IsValidName(surname))
            {
                DisplayAlert("Input Error", "Surname should not contain numbers or special characters.", "OK");
                return false;
            }
            if (string.IsNullOrWhiteSpace(licenseNumber))
            {
                DisplayAlert("Input Error", "License Number is required.", "OK");
                return false;
            }
            if (password != confirmPassword)
            {
                DisplayAlert("Input Error", "Passwords do not match.", "OK");
                return false;
            }
            if (password.Length < 6)
            {
                DisplayAlert("Input Error", "Password must be at least 6 characters.", "OK");
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

        private async void OnLoginLinkTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new LoginPage());
        }
    }
}
