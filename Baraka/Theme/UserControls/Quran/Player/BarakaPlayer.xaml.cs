﻿using Baraka.Data;
using Baraka.Data.Descriptions;
using Baraka.Streaming;
using Baraka.Theme.UserControls.Quran.Displayer;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace Baraka.Theme.UserControls.Quran.Player
{
    /// <summary>
    /// Logique d'interaction pour BarakaPlayer.xaml
    /// </summary>
    public partial class BarakaPlayer : UserControl
    {
        private bool _playing = false;
        private bool _loopMode = false;

        // UI relative
        private bool _cheikhModification = false;
        private bool _surahModification = false;
        private bool _wasCtrlBtnStylePlay = true;
        private bool _closing = false;
        private int _lastTabShown = -1;

        private CheikhDescription _selectedCheikh;
        private CheikhCard _selectedCheikhCard;

        private SurahDescription _selectedSurah;
        private SurahBar _selectedSurahBar;

        // Bindings
        public QuranStreamer Streamer { get; private set; }
        public BarakaSurahDisplayer Displayer { get; set; }

        #region Settings
        [Category("Baraka")]
        public bool Playing
        {
            get { return _playing; }
            set
            {
                _playing = value;
                RefreshPlayPauseBtn();
                // TODO
            }
        }

        [Category("Baraka")]
        public CheikhDescription SelectedCheikh
        {
            get { return _selectedCheikh; }
        }

        [Category("Baraka")]
        public SurahDescription SelectedSurah
        {
            get { return _selectedSurah; }
        }
        #endregion

        public BarakaPlayer()
        {
            InitializeComponent();

            _selectedSurah = LoadedData.SurahList.ElementAt(0).Key;
            _selectedCheikh = LoadedData.CheikhList.ElementAt(3);

            // Streamer config
            Streamer = new QuranStreamer();

            Streamer.VerseChanged += (object sender, EventArgs e) =>
            {
                if (Displayer.LoopMode)
                {
                    ReinitLoopmodeInfo();
                }
                else
                {
                    Displayer.BrowseToVerse(Streamer.NonRelativeVerse);
                }

                //VersePB.Progress = 0;

                Displayer.ChangeVerse(Streamer.NonRelativeVerse);
            };

            Streamer.FinishedSurah += (object sender, EventArgs e) =>
            {
                _playing = false;
                RefreshPlayPauseBtn();
            };

            /*VersePB.CursorChanged += (object sender, double e) =>
            {
                Streamer.Cursor = e;
            };*/

            // Stories
            ((Storyboard)this.Resources["PlayerCloseStory"]).Begin();
            ((Storyboard)this.Resources["PlayerCloseStory"]).SkipToFill();
            ((Storyboard)this.Resources["PlayerCloseStory"]).Completed += (object sender, EventArgs e) =>
            {
                SelectorGrid.Visibility = Visibility.Hidden;
                _closing = false;
            };
        }

        public void Dispose()
        {
            Streamer.Playing = false;
        }

        public void ChangeVerse(int num)
        {
            Streamer.ChangeVerse(num);
        }

        public void SetPlaying(bool playing)
        {
            Streamer.Playing = playing;
            _playing = playing;
            Displayer.Playing = Playing;

            RefreshPlayPauseBtn();
        }

        public void ReinitLoopmodeInfo()
        {
            Streamer.StartVerse = Displayer.StartVerse;
            Streamer.EndVerse = Displayer.EndVerse;
        }

        public void ReinitLoopmode(bool activated)
        {
            Displayer.SetLoopMode(activated);
            if (activated)
            {
                ReinitLoopmodeInfo();
            }
            Streamer.LoopMode = activated;
        }

        #region Controller Controls
        private void LoopBTN_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_loopMode)
            {
                LoopBTN.Fill = new ImageBrush(
                    new BitmapImage(new Uri(@"pack://application:,,,/Baraka;component/Images/player_loop_off.png"))
                );
            }
            else
            {
                LoopBTN.Fill = new ImageBrush(
                    new BitmapImage(new Uri(@"pack://application:,,,/Baraka;component/Images/player_loop_on.png"))
                );
            }

            _loopMode = !_loopMode;

            ReinitLoopmode(_loopMode);
        }

        private void CheikhTB_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_cheikhModification)
            {
                CheikhTB.Foreground = Brushes.Black;
                ClosePlayer();
            }
            else
            {
                using (new Utils.WaitCursor())
                {
                    if (_surahModification)
                    {
                        SurahTB.Foreground = Brushes.Black;
                        _surahModification = false;
                        SwitchTab(0);
                    }
                    else
                    {
                        OpenPlayer(0);
                    }
                }

                CheikhTB.Foreground = Brushes.Gray;
            }

            _cheikhModification = !_cheikhModification;
        }

        private void SurahTB_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_surahModification)
            {
                SurahTB.Foreground = Brushes.Black;
                ClosePlayer();
            }
            else
            {
                using (new Utils.WaitCursor())
                {
                    if (_cheikhModification)
                    {
                        CheikhTB.Foreground = Brushes.Black;
                        _cheikhModification = false;
                        SwitchTab(1);
                    }
                    else
                    {
                        OpenPlayer(1);
                    }
                }

                SurahTB.Foreground = Brushes.Gray;
            }

            _surahModification = !_surahModification;
        }

        private void PlayPauseBTN_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (Streamer.Verse == 0 || Streamer.Verse == 1) // TODO
            {
                Displayer.ScrollToTop();
            }

            SetPlaying(!_playing);
        }

        private void RefreshPlayPauseBtn()
        {
            if (!_playing)
            {
                PlayPauseBtnPath.Style = (Style)this.Resources["Play"];
                _wasCtrlBtnStylePlay = false;
            }
            else
            {
                PlayPauseBtnPath.Style = (Style)this.Resources["Pause"];
                _wasCtrlBtnStylePlay = true;
            }
        }

        private void BackwardBTN_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            SetPlaying(false);
            ChangeSelectedSurah(LoadedData.SurahList.ElementAt(_selectedSurah.SurahNumber - 1 - 1).Key);
            _resetSurahDisplayer = true;
        }

        private void ForwardBTN_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            SetPlaying(false);
            ChangeSelectedSurah(LoadedData.SurahList.ElementAt(_selectedSurah.SurahNumber + 1 - 1).Key);
            _resetSurahDisplayer = true;
        }
        #endregion

        #region Scrollbar
        private void MainSB_OnScroll(object sender, EventArgs e)
        {
            DisplaySV.ScrollToVerticalOffset(DisplaySV.ScrollableHeight * MainSB.Scrolled);
        }
        #endregion

        #region Scrollviewer Display
        private bool _resetSurahDisplayer = false;

        // tab parameter: 0 => Cheikh selector
        //                1 => Surah selector
        private void OpenPlayer(int tab)
        {
            if (_closing) return;

            if (_playing)
            {
                SetPlaying(false);
                RefreshPlayPauseBtn();
            }

            if (_resetSurahDisplayer)
            {
                ShowSurahSelector();
                _resetSurahDisplayer = false;
            }

            SelectorGrid.Visibility = Visibility.Visible;
            ((Storyboard)this.Resources["PlayerOpenStory"]).Begin();

            SwitchTab(tab, false);

            PlayPauseBTN.IsEnabled = false;
            PlayPauseBTN.Opacity = 0.4;
        }

        private void ClosePlayer()
        {
            _closing = true;
            ((Storyboard)this.Resources["PlayerCloseStory"]).Begin();

            Console.WriteLine($"_isCtrlBtnSetToPlay: {_wasCtrlBtnStylePlay}");

            if (!_wasCtrlBtnStylePlay)
            {
                SetPlaying(true);
            }

            PlayPauseBTN.IsEnabled = true;
            PlayPauseBTN.Opacity = 1;
        }

        private void SwitchTab(int tab, bool animation = true)
        {
            if (animation)
            {
                ((Storyboard)this.Resources["TabSwitchStory"]).Begin();
            }

            if (_lastTabShown == tab)
            {
                return;
            }
            else
            {
                DisplaySV.ScrollToTop();
                MainSB.ResetThumbY();
            }

            switch (tab)
            {
                case 0:
                    ShowCheikhSelector();
                    break;
                case 1:
                    ShowSurahSelector();
                    break;
                default: throw new NotImplementedException();
            }
            _lastTabShown = tab;

        }

        #region Cheikh Selector
        private void ShowCheikhSelector()
        {
            MainSB.TargetValue = (int)Math.Ceiling(LoadedData.CheikhList.Length / 3d);
            MainSB.Accuracy = ScrollAccuracyMode.ACCURATE;

            var panel = new StackPanel();
            panel.HorizontalAlignment = HorizontalAlignment.Stretch;
            panel.VerticalAlignment = VerticalAlignment.Stretch;

            // // Cheikh repartition
            var cheikhPairs = new List<(CheikhDescription, CheikhDescription, CheikhDescription)>();

            var lastThreeCheikhs = new List<CheikhDescription>();
            foreach (var cheikh in LoadedData.CheikhList)
            {
                if (lastThreeCheikhs.Count < 3)
                {
                    lastThreeCheikhs.Add(cheikh);
                }
                else
                {
                    var pair = (lastThreeCheikhs[0], lastThreeCheikhs[1], lastThreeCheikhs[2]);
                    cheikhPairs.Add(pair);

                    lastThreeCheikhs.Clear();
                    lastThreeCheikhs.Add(cheikh);
                }
            }

            // Manage remaining pair
            var remainingPair = new CheikhDescription[3] { null, null, null };
            if (lastThreeCheikhs.ElementAtOrDefault(0) != null) remainingPair[0] = lastThreeCheikhs[0];
            if (lastThreeCheikhs.ElementAtOrDefault(1) != null) remainingPair[1] = lastThreeCheikhs[1];
            if (lastThreeCheikhs.ElementAtOrDefault(2) != null) remainingPair[2] = lastThreeCheikhs[2];
            cheikhPairs.Add((remainingPair[0], remainingPair[1], remainingPair[2]));

            // Create sub-panels
            foreach (var idenPair in cheikhPairs)
            {
                var subPanel = new StackPanel();
                subPanel.Orientation = Orientation.Horizontal;

                // Prevent shadows from getting clipped
                subPanel.Height = 215;
                subPanel.Margin = new Thickness(5, 2, 0, 0);
                // //

                var card1 = new CheikhCard(idenPair.Item1, this)
                {
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Margin = new Thickness(20, 0, 15, 0)
                };
                subPanel.Children.Add(card1);

                if (idenPair.Item2 != null)
                {
                    var card2 = new CheikhCard(idenPair.Item2, this)
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                    };
                    subPanel.Children.Add(card2);
                }

                if (idenPair.Item3 != null)
                {
                    var card3 = new CheikhCard(idenPair.Item3, this)
                    {
                        HorizontalAlignment = HorizontalAlignment.Right,
                        Margin = new Thickness(15, 0, 20, 0)
                    };
                    subPanel.Children.Add(card3);
                }

                    foreach (CheikhCard card in subPanel.Children)
                    {
                        if (card.Cheikh.ToString() == _selectedCheikh.ToString())
                        {
                            ChangeSelectedCheikh(card);
                            card.Select();
                        }
                    }


                panel.Children.Add(subPanel);
            }

            DisplaySV.Content = panel;
        }

        public void ChangeSelectedCheikh(CheikhCard card)
        {
            if (card != _selectedCheikhCard)
            {
                _selectedCheikh = card.Cheikh;
                Streamer.Cheikh = card.Cheikh;

                if (_selectedCheikhCard != null)
                    _selectedCheikhCard.Unselect();

                _selectedCheikhCard = card;

                CheikhTB.Text = _selectedCheikh.ToString();
            }
        }

        public void SetDesignedBar(SurahBar bar)
        {
            if (bar != _selectedSurahBar)
            {
                ChangeSelectedSurah(bar.Surah);
                if (_selectedSurahBar != null)
                    _selectedSurahBar.Unselect();
                _selectedSurahBar = bar;
            }
        }
        public void ChangeSelectedSurah(SurahDescription description)
        {
            if (description != _selectedSurah)
            {
                _selectedSurah = description;

                if (description != Streamer.Surah)
                {
                    Streamer.Surah = description;
                    Streamer.Reset();
                }

                // Change backward/forward buttons opacities
                if (description.SurahNumber == 1)
                {
                    BackwardBTN.IsEnabled = false;
                    BackwardBTN.Opacity = 0.4;
                }
                else if (description.SurahNumber == 114)
                {
                    ForwardBTN.IsEnabled = false;
                    ForwardBTN.Opacity = 0.4;
                }
                else
                {
                    BackwardBTN.IsEnabled = true;
                    ForwardBTN.IsEnabled = true;
                    BackwardBTN.Opacity = 1;
                    ForwardBTN.Opacity = 1;
                }


                SurahTB.Text = _selectedSurah.PhoneticName;

                Displayer.LoadSurah(SelectedSurah);
            }
        }
        #endregion

        #region Surah Selector
        private void ShowSurahSelector()
        {
            MainSB.TargetValue = (int)Math.Ceiling(114 / (LoadedData.CheikhList.Length / 3d));
            MainSB.Accuracy = ScrollAccuracyMode.VAGUE;

            var panel = new StackPanel();
            panel.HorizontalAlignment = HorizontalAlignment.Stretch;
            panel.VerticalAlignment = VerticalAlignment.Stretch;

            foreach (var surahDesc in LoadedData.SurahList)
            {
                var bar = new SurahBar(surahDesc.Key, this);

                if (_selectedSurah != null)
                {
                    if (bar.Surah.PhoneticName == _selectedSurah.PhoneticName)
                    {
                        SetDesignedBar(bar);
                        bar.Select();
                    }
                }
                else
                {
                    if (surahDesc.Key.SurahNumber == 1)
                    {
                        SetDesignedBar(bar);
                        bar.Select();
                    }
                }

                panel.Children.Add(bar);
            }

            DisplaySV.Content = panel;
        }
        #endregion

        #endregion

        #region UI Reactivity
        private void userControl_MouseLeave(object sender, MouseEventArgs e)
        {
            // Close player
            if (_surahModification) SurahTB_PreviewMouseLeftButtonDown(null, null);
            if (_cheikhModification) CheikhTB_PreviewMouseLeftButtonDown(null, null);
        }
        #endregion

        #region Other
        public void DownloadMp3Verse(int verseNum)
        {
            var sfd = new SaveFileDialog();
            sfd.Title = $"Enregistrer le verset {verseNum} de cette sourate";
            sfd.Filter = "Fichier MP3|*.mp3";
            sfd.FileName = $"{_selectedCheikh.LastName.Replace(" ", "").ToLower()}_{_selectedSurah.SurahNumber}_{verseNum}";

            if (sfd.ShowDialog() == true)
            {
                string url = StreamingUtils.GenerateVerseUrl(_selectedCheikh, _selectedSurah, verseNum);
                new WebClient().DownloadFile(url, sfd.FileName);
            }
        }

        private void DisplaySV_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            MainSB.Scrolled = DisplaySV.VerticalOffset / DisplaySV.ScrollableHeight;
        }
        #endregion

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //VersePB.Progress += 0.01;
        }
    }
}