using Microsoft.Maui.Controls.Shapes;
using SchoolManagementSystem.Mobile.Models;
using SchoolManagementSystem.Mobile.Services;

namespace SchoolManagementSystem.Mobile.Views
{
    public partial class MyLearningPage : ContentPage
    {
        private readonly StudentService _studentService;

        public MyLearningPage(StudentService studentService)
        {
            InitializeComponent();
            _studentService = studentService;
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

                CourseLabel.Text =
                    $"Course: {learning.CourseName ?? "Not assigned"}";

                ClassLabel.Text =
                    $"Class: {learning.ClassGroupName ?? "Not assigned"}";

                ArchivedLabel.Text = "This student record is archived.";
                ArchivedLabel.IsVisible = learning.IsArchived;

                SubjectsLayout.Children.Clear();

                if (learning.Subjects.Count == 0)
                {
                    SubjectsLayout.Children.Add(new Border
                    {
                        BackgroundColor = Colors.White,
                        Stroke = new SolidColorBrush(
                            Color.FromArgb("#e2e8f0")),
                        StrokeThickness = 1,
                        StrokeShape = new RoundRectangle
                        {
                            CornerRadius = new CornerRadius(12)
                        },
                        Padding = new Thickness(22),
                        Content = new Label
                        {
                            Text = "You are not enrolled in any subjects yet.",
                            FontSize = 14,
                            TextColor = Color.FromArgb("#526579")
                        }
                    });
                }
                else
                {
                    foreach (var subject in learning.Subjects)
                    {
                        var gradeText = subject.FinalGrade.HasValue
                            ? subject.FinalGrade.Value.ToString("0.0")
                            : "Not graded";

                        var detailsGrid = new Grid
                        {
                            ColumnSpacing = 12,
                            ColumnDefinitions =
                    {
                        new ColumnDefinition
                        {
                            Width = GridLength.Star
                        },
                        new ColumnDefinition
                        {
                            Width = GridLength.Star
                        },
                        new ColumnDefinition
                        {
                            Width = GridLength.Star
                        }
                    }
                        };

                        var hours = CreateSubjectDetail(
                            "Hours",
                            $"{subject.TotalHours}");

                        var absences = CreateSubjectDetail(
                            "Absences",
                            $"{subject.AbsenceCount}");

                        var grade = CreateSubjectDetail(
                            "Final grade",
                            gradeText);

                        Grid.SetColumn(hours, 0);
                        Grid.SetColumn(absences, 1);
                        Grid.SetColumn(grade, 2);

                        detailsGrid.Children.Add(hours);
                        detailsGrid.Children.Add(absences);
                        detailsGrid.Children.Add(grade);

                        var content = new VerticalStackLayout
                        {
                            Spacing = 16,
                            Children =
                    {
                        new Label
                        {
                            Text = subject.SubjectName,
                            FontSize = 17,
                            FontAttributes = FontAttributes.Bold,
                            TextColor = Color.FromArgb("#163550")
                        },
                        new BoxView
                        {
                            HeightRequest = 1,
                            Color = Color.FromArgb("#eaf0f6")
                        },
                        detailsGrid
                    }
                        };

                        SubjectsLayout.Children.Add(new Border
                        {
                            BackgroundColor = Colors.White,
                            Stroke = new SolidColorBrush(
                                Color.FromArgb("#e2e8f0")),
                            StrokeThickness = 1,
                            StrokeShape = new RoundRectangle
                            {
                                CornerRadius = new CornerRadius(12)
                            },
                            Padding = new Thickness(20),
                            Content = content
                        });
                    }
                }

                LearningLayout.IsVisible = true;
            }
            catch (UnauthorizedAccessException)
            {
                var window = Window;

                await DisplayAlertAsync(
                    "Session expired",
                    "Please sign in again.",
                    "OK");

                if (Application.Current is App app &&
                    window is not null)
                {
                    await app.SignOutAsync(window);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);

                ErrorLabel.Text =
                    "Could not load your learning information.";

                ErrorLabel.IsVisible = true;
            }
            finally
            {
                LoadingIndicator.IsVisible = false;
                LoadingIndicator.IsRunning = false;
            }
        }

        private static VerticalStackLayout CreateSubjectDetail(string title,string value)
        {
            return new VerticalStackLayout
            {
                Spacing = 5,
                Children =
                {
                    new Label
                    {
                    Text = title,
                    FontSize = 12,
                    TextColor = Color.FromArgb("#526579")
                    },
                    new Label
                    {
                    Text = value,
                    FontSize = 16,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb("#163550")
                    }
                }
            };
        }
    }
}