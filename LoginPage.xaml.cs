using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;

namespace Aviation
{
    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private async void OnLoginButtonClicked(object sender, EventArgs e)
        {
            string username = username_Entry.Text;
            string password = password_Entry.Text;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                await DisplayAlert("Login Failed", "Please enter both username and password", "OK");
                return;
            }

            if (IsValidLogin(username, password))
            {
                // Navigate to the DashboardPage
                await Navigation.PushAsync(new DashboardPage());
            }
            else
            {
                await DisplayAlert("Login Failed", "Invalid username or password", "OK");
            }
        }

        private async void OnSignUpButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SignUpPage());
        }

        private async void OnSignInWithGoogleClicked(object sender, EventArgs e)
        {
            await SignInWithGoogle();
        }

        private bool IsValidLogin(string username, string password)
        {
            // Placeholder validation logic
            return !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password);
        }

        private async Task SignInWithGoogle()
        {
            // Placeholder for real Google sign-in logic
            await DisplayAlert("Google Sign In", "Signing in with Google...", "OK");
            
        }
    }
}