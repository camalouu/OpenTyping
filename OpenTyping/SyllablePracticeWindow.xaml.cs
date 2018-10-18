﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using MahApps.Metro.Controls;

namespace OpenTyping
{
    /// <summary>
    /// SyllablePracticeWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SyllablePracticeWindow : MetroWindow, INotifyPropertyChanged
    {
        private static readonly Random Randomizer = new Random();
        private readonly string syllablesList;

        private char currentSyllable = ' ';
        public char CurrentSyllable
        {
            get => currentSyllable;
            set => SetField(ref currentSyllable, value);
        }

        private readonly Brush incorrectBackground = Brushes.Pink;

        public SyllablePracticeWindow(string syllablesList)
        {
            InitializeComponent();

            void FocusCurrentTextBox(object sender, System.Windows.RoutedEventArgs e) { CurrentTextBox.Focus(); }
            this.Loaded += FocusCurrentTextBox;
            CurrentTextBox.LostFocus += FocusCurrentTextBox;
            // 음절 입력 텍스트 박스 포커스 항상 유지

            this.syllablesList = syllablesList;
            NextSyllable();
        }

        public void NextSyllable()
        {
            char newSyllable;
            do
            {
                newSyllable = syllablesList[Randomizer.Next(syllablesList.Length)];
            } while (newSyllable == currentSyllable); // 새 음절과 전 음절 중복 확인

            CurrentSyllable = newSyllable;
            CurrentTextBox.Clear();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        private void CurrentTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            switch (CurrentTextBox.Text.Length)
            {
                case 0:
                    CurrentTextBox.Background = Brushes.White;
                    return;
                case 1:
                    List<char> decomposedCurrentSyllable = new List<char>(Differ.DecomposeHangul(CurrentSyllable)),
                               decomposedInput = new List<char>(Differ.DecomposeHangul(CurrentTextBox.Text[0]));

                    if (decomposedInput.SequenceEqual(decomposedCurrentSyllable))
                    {
                        NextSyllable();
                        return;
                    }
                    if (decomposedInput.Any() &&
                        decomposedInput.SequenceEqual(decomposedCurrentSyllable.Take(decomposedInput.Count)))
                    {
                        CurrentTextBox.Background = Brushes.White;
                        return;
                    }

                    break;
            }

            CurrentTextBox.Background = incorrectBackground;
        }
    }
}