using Microsoft.Toolkit.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using UWPMain.ViewModels;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace UWPMain.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ExecutionPage : Page, INotifyPropertyChanged
    {
        /// <summary>
        /// Gets the app-wide <see cref="App.AppViewModel"/> singleton.
        /// </summary>
        public MainViewModel ViewModel => App.AppViewModel;

        /// <summary>
        /// Fired when a property value changes. 
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Notifies listeners that a property value changed. 
        /// </summary>
        private void OnPropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public ExecutionPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

            ViewModel.BenchmarkName = e.Parameter as string;
            base.OnNavigatedTo(e);
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.BeginExecutionAsync();
        }

        private void TournamentSize_BeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            if (args.NewText.IsNumeric() && args.NewText.Length < ViewModel.PopulationSize)
                args.Cancel = false;

            else
                args.Cancel = true;
        }

        private void EditParameters_Click(object sender, RoutedEventArgs e) => ParametersDialog.ShowAsync();

        private void PauseButton_Click(object sender, RoutedEventArgs e) => ViewModel.PauseExecution();

        private async void ResumeButton_Click(object sender, RoutedEventArgs e)
        {
            await ViewModel.ResumeExecutionAsync();
        }

        private void Back_Click(object sender, RoutedEventArgs e) => Frame.Navigate(typeof(MainPage));
    }
}
