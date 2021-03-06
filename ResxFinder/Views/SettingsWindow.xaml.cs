﻿using ResxFinder.Interfaces;
using ResxFinder.Model;
using ResxFinder.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ResxFinder.Views
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }

        public SettingsWindow(ISettings settings)
          : this()
        {
            m_Settings = settings;

            this.cbIgnoreUpToNCharactersStrings.IsChecked = m_Settings.IsIgnoreStringLength;
            this.nudIgnoreStringLength.Value = (decimal)m_Settings.IgnoreStringLength;
            this.cbIgnoreWhiteSpaceStrings.IsChecked = m_Settings.IsIgnoreWhiteSpaceStrings;
            this.cbIgnoreNumberStrings.IsChecked = m_Settings.IsIgnoreNumberStrings;
            this.cbIgnoreVerbatimStrings.IsChecked = m_Settings.IsIgnoreVerbatimStrings;
            this.cbUseGlobalResourceFile.IsChecked = m_Settings.IsUseGlobalResourceFile;
            this.txtGlobalResourceFileName.Text = m_Settings.GlobalResourceFileName;
            this.cbDontUseResourceAlias.IsChecked = m_Settings.IsDontUseResourceAlias;

            this.ignoredFiles.Items = m_Settings.IgnoredFiles;
            this.ignoredProjects.Items = m_Settings.IgnoredProjects;

            this.lstIgnoreStrings.Items = m_Settings.IgnoreStrings;
            this.lstIgnoreSubStrings.Items = m_Settings.IgnoreSubStrings;

            this.lstIgnoreMethods.Items = m_Settings.IgnoreMethods;
            this.lstIgnoreArguments.Items = m_Settings.IgnoreMethodsArguments;

            this.regexIgnore.Items = m_Settings.IgnoreRegex;

            this.lstIgnoreStrings.IsEnabled = !m_Settings.IgnoreStrings.Contains("@@@disabled@@@");
            this.lstIgnoreSubStrings.IsEnabled = !m_Settings.IgnoreSubStrings.Contains("@@@disabled@@@");
            this.lstIgnoreMethods.IsEnabled = !m_Settings.IgnoreMethods.Contains("@@@disabled@@@");
            this.lstIgnoreArguments.IsEnabled = !m_Settings.IgnoreMethodsArguments.Contains("@@@disabled@@@");
        }

        private ISettings m_Settings;
        public ISettings Settings
        {
            get { return (m_Settings); }
            //set { m_Settings = value; }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (this.cbUseGlobalResourceFile.IsChecked ?? false)
            {

                if (this.txtGlobalResourceFileName.Text.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) >= 0)
                {
                    MessageBox.Show("The global resource file name contains at least one illegal charter.",
                                    "Settings", MessageBoxButton.OK, MessageBoxImage.Error);

                    if (!this.tabiOptions.IsSelected)
                        this.tabiOptions.IsSelected = true;

                    this.txtGlobalResourceFileName.Focus();
                    return;
                }
            }

            if (m_Settings == null)
                m_Settings = new Settings();

            m_Settings.IsIgnoreStringLength = this.cbIgnoreUpToNCharactersStrings.IsChecked ?? false;
            m_Settings.IgnoreStringLength = (int)this.nudIgnoreStringLength.Value;
            m_Settings.IsIgnoreWhiteSpaceStrings = this.cbIgnoreWhiteSpaceStrings.IsChecked ?? false;
            m_Settings.IsIgnoreNumberStrings = this.cbIgnoreNumberStrings.IsChecked ?? false;
            m_Settings.IsIgnoreVerbatimStrings = this.cbIgnoreVerbatimStrings.IsChecked ?? false;
            m_Settings.IsUseGlobalResourceFile = this.cbUseGlobalResourceFile.IsChecked ?? false;
            m_Settings.GlobalResourceFileName = (this.txtGlobalResourceFileName.Text ?? string.Empty).Trim();
            m_Settings.IsDontUseResourceAlias = this.cbDontUseResourceAlias.IsChecked ?? false;

            m_Settings.IgnoredProjects.Clear();
            m_Settings.IgnoredProjects.AddRange(this.ignoredProjects.Items);

            m_Settings.IgnoredFiles.Clear();
            m_Settings.IgnoredFiles.AddRange(this.ignoredFiles.Items);

            m_Settings.IgnoreRegex.Clear();
            m_Settings.IgnoreRegex.AddRange(this.regexIgnore.Items);

            m_Settings.IgnoreStrings.Clear();
            m_Settings.IgnoreStrings.AddRange(this.lstIgnoreStrings.Items);
            m_Settings.IgnoreSubStrings.Clear();
            m_Settings.IgnoreSubStrings.AddRange(this.lstIgnoreSubStrings.Items);

            m_Settings.IgnoreMethods.Clear();
            m_Settings.IgnoreMethods.AddRange(this.lstIgnoreMethods.Items);
            m_Settings.IgnoreMethodsArguments.Clear();
            m_Settings.IgnoreMethodsArguments.AddRange(this.lstIgnoreArguments.Items);

            this.DialogResult = true;
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

    }
}
