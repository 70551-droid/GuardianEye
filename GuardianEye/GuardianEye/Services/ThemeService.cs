using System.Windows;

namespace GuardianEye.Services
{
    public class ThemeService : IThemeService
    {
        public void ApplyDarkTheme()
        {
            var app = Application.Current;
            var dict = new ResourceDictionary { Source = new Uri("Themes/DarkTheme.xaml", UriKind.Relative) };
            app.Resources.MergedDictionaries.Add(dict);
        }

        public void ApplyLightTheme()
        {
            var app = Application.Current;
            var dict = new ResourceDictionary { Source = new Uri("Themes/LightTheme.xaml", UriKind.Relative) };
            app.Resources.MergedDictionaries.Add(dict);
        }
    }
}