namespace Aviation;

public partial class SafetyChecklistPage : ContentPage
{
    public SafetyChecklistPage()
    {
        InitializeComponent();
    }

    private void OnConfirmChecklistClicked(object sender, EventArgs e)
    {
        bool allChecked = CheckAircraft.IsChecked &&
                          CheckWeather.IsChecked &&
                          CheckFuel.IsChecked &&
                          CheckWeight.IsChecked;

        if (allChecked)
        {
            DisplayAlert("Checklist Complete", "All items have been checked. You're ready for takeoff!", "OK");
        }
        else
        {
            DisplayAlert("Checklist Incomplete", "Please ensure all items are checked before proceeding.", "OK");
        }
    }
}