namespace Aviation;

public partial class DashboardPage : ContentPage
{
    public DashboardPage()
    {
        InitializeComponent();
    }

    private async void OnViewChecklistClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new SafetyChecklistPage());
    }

    private async void OnCheckWeatherClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new WeatherPage());
    }

    private void OnPlanFlightClicked(object sender, EventArgs e)
    {
        DisplayAlert("Flight Planner", "Flight planning feature coming soon!", "OK");
    }

    private void OnViewProceduresClicked(object sender, EventArgs e)
    {
        DisplayAlert("Emergency Procedures", "Emergency procedures feature coming soon!", "OK");
    }
}