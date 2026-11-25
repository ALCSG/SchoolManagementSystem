using Microsoft.Maui.ApplicationModel;

namespace SchoolManagementSystem.Mobile.Views
{
    public partial class AboutPage : ContentPage
    {
        public AboutPage()
        {
            InitializeComponent();

            VersionLabel.Text =
                $"Version {AppInfo.Current.VersionString} " +
                $"(build {AppInfo.Current.BuildString})";
        }
    }
}