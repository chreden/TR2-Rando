﻿using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using TR2RandomizerCore;
using TR2RandomizerCore.Helpers;
using TR2RandomizerView.Utilities;

namespace TR2RandomizerView.Windows
{
    /// <summary>
    /// Interaction logic for OpenProgressWindow.xaml
    /// </summary>
    public partial class OpenProgressWindow : Window
    {
        #region Dependency Properties
        public static readonly DependencyProperty ProgressValueProperty = DependencyProperty.Register
        (
            "ProgressValue", typeof(int), typeof(OpenProgressWindow), new PropertyMetadata(0)
        );

        public static readonly DependencyProperty ProgressTargetProperty = DependencyProperty.Register
        (
            "ProgressTarget", typeof(int), typeof(OpenProgressWindow), new PropertyMetadata(100)
        );

        public static readonly DependencyProperty ProgressDescriptionProperty = DependencyProperty.Register
        (
            "ProgressDescription", typeof(string), typeof(OpenProgressWindow), new PropertyMetadata("Checking backup status")
        );

        public int ProgressValue
        {
            get => (int)GetValue(ProgressValueProperty);
            set => SetValue(ProgressValueProperty, value);
        }

        public int ProgressTarget
        {
            get => (int)GetValue(ProgressTargetProperty);
            set => SetValue(ProgressTargetProperty, value);
        }

        public string ProgressDescription
        {
            get => (string)GetValue(ProgressDescriptionProperty);
            set => SetValue(ProgressDescriptionProperty, value);
        }
        #endregion

        private volatile bool _complete;
        private readonly string _folderPath;

        public Exception OpenException { get; private set; }
        public TR2RandomizerController OpenedController { get; private set; }

        public OpenProgressWindow(string folderPath)
        {
            InitializeComponent();
            Owner = WindowUtils.GetActiveWindow();
            DataContext = this;
            _complete = false;
            _folderPath = folderPath;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WindowUtils.EnableCloseButton(this, false);
            WindowUtils.TidyMenu(this);
            new Thread(Open).Start();
        }

        private void Open()
        {
            TR2RandomizerCoord.Instance.OpenProgressChanged += Coord_OpenProgressChanged;

            try
            {
                OpenedController = TR2RandomizerCoord.Instance.Open(_folderPath);
            }
            catch (Exception e)
            {
                OpenException = e;
            }
            finally
            {
                TR2RandomizerCoord.Instance.OpenProgressChanged -= Coord_OpenProgressChanged;

                Dispatcher.Invoke(delegate
                {
                    _complete = true;
                    WindowUtils.EnableCloseButton(this, true);
                    DialogResult = OpenException == null;
                });
            }
        }

        private void Coord_OpenProgressChanged(object sender, TROpenRestoreEventArgs e)
        {
            Dispatcher.Invoke(delegate
            {
                ProgressTarget = e.ProgressTarget;
                ProgressValue = e.ProgressValue;
            });
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (!_complete)
            {
                e.Cancel = true;
            }
        }
    }
}