using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;

namespace Aviation
{
    public partial class SignUpPage : ContentPage
    {
        public SignUpPage()
        {
            InitializeComponent();
        }

        private async void OnSignUpButtonClicked(object sender, EventArgs e)
        {
            string email = email_Entry.Text;
            string username = username_Entry.Text;
            string password = password_Entry.Text;
            string confirmPassword = confirm_password_Entry.Text;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(confirmPassword))
            {
                await DisplayAlert("Sign Up Failed", "Please fill in all fields", "OK");
                return;
            }

            if (password != confirmPassword)
            {
                await DisplayAlert("Sign Up Failed", "Passwords do not match", "OK");
                return;
            }

            // Perform registration logic here
            await RegisterNewUser(email, username, password);
        }

        private async void OnLoginLinkTapped(object sender, EventArgs e)
        {
            await Navigation.PopAsync(); // Go back to the login page
        }

        private async Task RegisterNewUser(string email, string username, string password)
        {
            // Placeholder for registration logic
            await DisplayAlert("Sign Up Successful", "Your account has been created!", "OK");
            await Navigation.PopAsync(); // Goes back to the login page after successful registration
        }
    }
}
