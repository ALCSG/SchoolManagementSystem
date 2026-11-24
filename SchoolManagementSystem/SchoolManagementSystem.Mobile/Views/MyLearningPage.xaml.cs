using SchoolManagementSystem.Mobile.Models;
using SchoolManagementSystem.Mobile.Services;

namespace SchoolManagementSystem.Mobile.Views
{
    public partial class MyLearningPage : ContentPage
    {
        private readonly StudentService _studentService;
        private readonly AuthService _authService;

        public MyLearningPage(StudentService studentService,AuthService authService)
        {
            InitializeComponent();
            _studentService = studentService;
            _authService = authService;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadLearningAsync();
        }

        private async Task LoadLearningAsync()
        {
            ErrorLabel.IsVisible = false;
            LearningLayout.IsVisible = false;

            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;

            try
            {
                var learning = await _studentService.GetMyLearningAsync();

                FullNameLabel.Text = learning.FullName;
                CourseLabel.Text = $"Course: {learning.CourseName}";
                ClassLabel.Text = $"Class: {learning.ClassName}";

                ArchivedLabel.Text = learning.IsArchived ? "This student record is archived." : string.Empty;

                SubjectsLayout.Children.Clear();

                if (learning.Subjects.Count == 0)
                {
                    SubjectsLayout.Children.Add(new Label
                    {
                        Text = "You are not enrolled in any subjects yet."
                    });
                }
                else
                {
                    foreach (var subject in learning.Subjects)
                    {
                        SubjectsLayout.Children.Add(new Label
                        {
                            Text = subject.SubjectName
                        });
                    }
                }

                LearningLayout.IsVisible = true;
            }
            catch (UnauthorizedAccessException)
            {
                await DisplayAlertAsync("Session expired","Please sign in again.","OK");

                await Navigation.PopToRootAsync();
            }
            catch (Exception)
            {
                ErrorLabel.Text = "Could not load your learning information.";
                ErrorLabel.IsVisible = true;
            }
            finally
            {
                LoadingIndicator.IsVisible = false;
                LoadingIndicator.IsRunning = false;
            }
        }

        private async void OnLogoutClicked(object? sender,EventArgs e)
        {
            bool confirmed = await DisplayAlertAsync(
                "Log out",
                "Do you want to sign out?",
                "Yes",
                "No");

            if (!confirmed)
                return;

            await _authService.LogoutAsync();

            await Navigation.PopToRootAsync();
        }
    }
}