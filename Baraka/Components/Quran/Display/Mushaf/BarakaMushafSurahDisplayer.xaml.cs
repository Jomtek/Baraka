﻿using Baraka.Data;
using Baraka.Data.Descriptions;
using Baraka.Theme.UserControls.Quran.Display.Mushaf.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Baraka.Theme.UserControls.Quran.Display.Mushaf
{
    /// <summary>
    /// Logique d'interaction pour BarakaMushafSurahDisplayer.xaml
    /// </summary>
    public partial class BarakaMushafSurahDisplayer : UserControl, ISurahDisplayer
    {
        private const int TOTAL_SHEETS = 302;
        private int _actualSheet;

        #region Settings
        private int SheetToMushafPage(int sheet)
        {
            return TOTAL_SHEETS * 2 - sheet * 2;
        }
        public int ActualSheet
        {
            get { return _actualSheet; }
            set
            {
                _actualSheet = value;

                // Update UI
                CurrentPageTB.Text = $"Pages {SheetToMushafPage(value) + 1}-{SheetToMushafPage(value)}";
                LastPageBTN.IsEnabled = (value < TOTAL_SHEETS);
                NextPageBTN.IsEnabled = (value > 0);
            }
        }
        #endregion

        public BarakaMushafSurahDisplayer()
        {
            InitializeComponent();
            LoadPages();
        }

        public void LoadPages()
        {
            for (int i = 604; i > 0; i--)
            {
                var page = new BarakaMadinaPage(i)
                {
                    Width = double.NaN,
                    Background = Brushes.Transparent,
                };

                BookComponent.Items.Add(page);
            }

            BookComponent.CurrentSheetIndex = TOTAL_SHEETS; // Navigate to the very first page (Al-Fatiha)
            ActualSheet = BookComponent.CurrentSheetIndex;

            BookComponent.TryPreloadPages(10, false);
            BookComponent.TryPreloadPages(10, true);

            BookComponent.IsInitialized = true;
        }

        private void Run_MouseEnter(object sender, MouseEventArgs e)
        {
            ((Run)sender).Foreground = Brushes.Yellow;
        }

        private void Run_MouseLeave(object sender, MouseEventArgs e)
        {
            ((Run)sender).Foreground = Brushes.Black;
        }

        public async Task LoadSurahAsync(SurahDescription surah)
        {
            //throw new NotImplementedException();
            ((MainWindow)App.Current.MainWindow).ReportLoadingProgress(1);
        }

        public void UnloadActualSurah()
        {
        }

        private async void LeftPage_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Debug purposes

        }

        #region Navigation
        public async Task NaturalBrowse(bool next, int factor = 1)
        {
        /*    int actual = _actualPage;

            await Task.Delay(100);

            if (_actualPage == actual)
            {
                if (next)
                {
                    ActualPage += 2 * factor;
                }
                else
                {
                    ActualPage -= 2 * factor;
                }
            }*/
        }

        private async void LastPageBTN_Click(object sender, RoutedEventArgs e)
        {
            ActualSheet = BookComponent.CurrentSheetIndex + 1;
            await Task.Run(() => {
                App.Current.Dispatcher.Invoke(() =>
                    BookComponent.AnimateToNextPage(false, 450));
            });
        }

        private async void NextPageBTN_Click(object sender, RoutedEventArgs e)
        {
            ActualSheet = BookComponent.CurrentSheetIndex - 1;
            await Task.Run(() => {
                App.Current.Dispatcher.Invoke(() =>
                    BookComponent.AnimateToPreviousPage(false, 450));
            });
        }
        #endregion

        #region Zoom
        public void ApplyScale(double scale)
        {
            var c = Mouse.GetPosition(BookComponent);
            if (c.X < BookComponent.ActualWidth / 2d) // Mouse is over left page
            {
                BookComponent.ApplyScaleOnPage(true, scale);
            }
            else // Mouse is over right page
            {
                BookComponent.ApplyScaleOnPage(false, scale);
            }
        }
        #endregion

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            /*
            LSpine.Width = LeftPage.GetHorizontalBorderWidth();
            RSpine.Width = RightPage.GetHorizontalBorderWidth();
            */
        }

        private void UserControl_KeyUp(object sender, KeyEventArgs e)
        {

        }
    }
}