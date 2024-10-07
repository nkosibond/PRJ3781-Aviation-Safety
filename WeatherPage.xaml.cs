namespace Aviation;

public partial class WeatherPage : ContentPage
{
    public WeatherPage()
    {
        InitializeComponent();
    }

    private void OnRefreshWeatherClicked(object sender, EventArgs e)
    {
        //Fetch new data from a weather API
        DisplayAlert("Weather Updated", "Weather data has been refreshed.", "OK");
    }
}