using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UWPMain.ViewModels;
using UWPMain.Views;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace UWPMain
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        /// <summary>
        /// Gets the app-wide <see cref="App.AppViewModel"/> singleton.
        /// </summary>
        public MainViewModel ViewModel => App.AppViewModel;

        public MainPage()
        {
            InitializeComponent();
        }

        private void ExecutionButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(ExecutionPage));
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.PopulationSize = 150;
            ViewModel.TournamentSize = 40;
            ViewModel.MutationRate = 10;
            ViewModel.CrossoverRate = 100;
            ViewModel.TotalGenerations = 500;
        }
    }
}
