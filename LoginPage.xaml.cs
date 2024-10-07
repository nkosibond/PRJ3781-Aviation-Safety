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
            string username = username_Entry.Text;
            string password = password_Entry.Text;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                await DisplayAlert("Sign Up Failed", "Please enter both username and password", "OK");
                return;
            }

            // Perform registration logic
            await RegisterNewUser(username, password);
        }

        private async void OnSignInWithGoogleClicked(object sender, EventArgs e)
        {
            await SignInWithGoogle();
        }

        private bool IsValidLogin(string username, string password)
        {
            // Placeholder for real validation logic
            // In a real app, you would check these credentials against a database or API
            return !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password);
        }

        private async Task RegisterNewUser(string username, string password)
        {
            // Placeholder for real registration logic
            await DisplayAlert("Sign Up Successful", "User registered successfully!", "OK");
            // Optionally, navigate to the dashboard after successful registration
            // await Navigation.PushAsync(new DashboardPage());
        }

        private async Task SignInWithGoogle()
        {
            // Placeholder for real Google sign-in logic
            await DisplayAlert("Google Sign In", "Signing in with Google...", "OK");
            // Implement actual Google Sign-In logic here
        }
    }
}