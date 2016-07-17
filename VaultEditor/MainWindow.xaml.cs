/*
    VaultEditor, Fallout Shelter Save-editor
    Copyright (C) 2016 skynetDE

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Microsoft.Win32;

namespace VaultEditor
{
    public partial class MainWindow : Window
    {
        private string _path;
        private readonly Vault _vault;

        public MainWindow()
        {
            _vault = new Vault();
            _path = null;
            InitializeComponent();
        }     

        public void UpdateComponents()
        {
            tbVault.Text = StringHelper.IntegerToString(_vault.Get("vault", "VaultName"));
            tbCaps.Text = _vault.Get("vault", "storage", "resources", "Nuka").ToString();
            tbFood.Text = _vault.Get("vault", "storage", "resources", "Food").ToString();
            tbEnergy.Text = _vault.Get("vault", "storage", "resources", "Energy").ToString();
            tbWater.Text = _vault.Get("vault", "storage", "resources", "Water").ToString();
            tbStimPacks.Text = StringHelper.IntegerToString(_vault.Get("vault", "storage", "resources", "StimPack"));
            tbRadAways.Text = StringHelper.IntegerToString(_vault.Get("vault", "storage", "resources", "RadAway"));
            tbLunchBoxes.Text = StringHelper.IntegerToString(_vault.Get("vault", "LunchBoxesCount"));
            tbNukaQuantum.Text = StringHelper.IntegerToString(_vault.Get("vault", "storage", "resources", "NukaColaQuantum"));
            var lunchboxes = (ArrayList) _vault.Get("vault", "LunchBoxesByType");
            var lbType = true;
            if (lunchboxes.Count > 0)
            {
                switch ((int) lunchboxes[0])
                {
                    case 1:
                        rbMrHandy.IsChecked = true;
                        break;
                    case 2:
                        rbPets.IsChecked = true;
                        break;
                    default:
                        rbLunchBox.IsChecked = true;
                        break;                   
                }
            }
            lbRockCount.Content = _vault.CountList("vault", "rocks");
            lbLRoomsCount.Content = _vault.Count(false, "unlockableMgr", "objectivesInProgress", "completed");
        }

        #region Layout

        private void InitCheckboxes()
        {
            cbTutorials.IsChecked = false;
            cbObjectives.IsChecked = false;
            cbRocks.IsChecked = false;
            cbLRooms.IsChecked = false;
        }

        private void DisableComponents()
        {
            btSave.IsEnabled = false;
            grVault.IsEnabled = false;
        }

        private void EnableComponents()
        {
            btSave.IsEnabled = true;
            grVault.IsEnabled = true;
        }

        #endregion

        private void btExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void btLoad_Click(object sender, RoutedEventArgs e)
        {           
            try
            {
                var dialog = new OpenFileDialog
                {
                    DefaultExt = ".sav",
                    Filter = "Fallout Shelter Save|*.sav;*.bak",
                    Multiselect = false,
                    CheckFileExists = true
                };

                if (string.IsNullOrEmpty(_path))
                {
                    var savePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) +
                                   "\\My Games\\Fallout Shelter";

                    if (Directory.Exists(savePath))
                    {
                        dialog.InitialDirectory = savePath;
                    }
                }
                else
                {
                    dialog.InitialDirectory = Path.GetDirectoryName(_path);
                }

                var result = dialog.ShowDialog();
                if (result != true) return;

                _path = dialog.FileName;
                var readResult = _vault.Read(_path);
                if (readResult == false || _vault.IsEmpty())
                {
                    DisableComponents();
                    MessageBox.Show(this, "Invalid Vault Save", "Error when Loading", MessageBoxButton.OK, MessageBoxImage.Error);                   
                }
                else
                {
                    UpdateComponents();
                    InitCheckboxes();
                    EnableComponents();                    
                }
            }
            catch (Exception x)
            {                
                DisableComponents();
                Debug.WriteLine(x.GetType() + "@btLoad_Click: " + x.Message);
                MessageBox.Show(this, x.Message, "Error when Loading", MessageBoxButton.OK, MessageBoxImage.Error);                
            }
        }

        private void UpdateVault()
        {
            _vault.Set(decimal.Parse(tbVault.Text), "vault", "VaultName");
            _vault.Set(decimal.Parse(tbCaps.Text), "vault", "storage", "resources", "Nuka");
            _vault.Set(decimal.Parse(tbFood.Text), "vault", "storage", "resources", "Food");
            _vault.Set(decimal.Parse(tbEnergy.Text), "vault", "storage", "resources", "Energy");
            _vault.Set(decimal.Parse(tbWater.Text), "vault", "storage", "resources", "Water");
            _vault.Set(decimal.Parse(tbStimPacks.Text), "vault", "storage", "resources", "StimPack");
            _vault.Set(decimal.Parse(tbRadAways.Text), "vault", "storage", "resources", "RadAway");
            _vault.Set(decimal.Parse(tbNukaQuantum.Text), "vault", "storage", "resources", "NukaColaQuantum");

            var lbCount = int.Parse(tbLunchBoxes.Text);
            _vault.Set(lbCount, "vault", "LunchBoxesCount");

            var lunchboxes = new ArrayList();
            if (lbCount > 0)
            {
                var value = 0;
                if (rbMrHandy.IsChecked == true)
                {
                    value = 1;
                }
                else if (rbPets.IsChecked == true)
                {
                    value = 2;
                }

                for (var i = 0; i < lbCount; ++i)
                {
                    lunchboxes.Add(value);
                }
            }
            _vault.Set(lunchboxes, "vault", "LunchBoxesByType");

            if (cbTutorials.IsChecked == true)
            {
                _vault.SetTutorialsDone();
            }

            if (cbObjectives.IsChecked == true)
            {
                _vault.Set(true, "objectiveMgr", "slotArray", "objective", "completed");
            }

            if (cbRocks.IsChecked == true)
            {
                _vault.Set(new ArrayList(), "vault", "rocks");
            }

            if (cbLRooms.IsChecked == true)
            {
                _vault.Set(true, "unlockableMgr", "objectivesInProgress", "completed");
            }
        }

        private void btSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                UpdateVault();                
            }
            catch (Exception x)
            {
                Debug.WriteLine(x.GetType() + "@btSave_Click UpdateVault: " + x.Message);
                MessageBox.Show(this, x.Message, "Error when updating Vault Data", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            if (!string.IsNullOrEmpty(_path))
            {
                try
                {
                    if (File.Exists(_path + ".bak"))
                    {
                        var dialogResult =
                            MessageBox.Show("An already existing backup file will be overwritten. Continue?",
                                "Backup File exists", MessageBoxButton.YesNoCancel);
                        switch (dialogResult)
                        {
                            case MessageBoxResult.Yes:                                
                                break;
                            default:
                                return;
                        }
                    }

                    if (_vault.Write(_path, ".bak"))
                    {
                        UpdateComponents();
                        cbTutorials.IsChecked = false;
                        cbObjectives.IsChecked = false;
                        cbRocks.IsChecked = false;
                        cbLRooms.IsChecked = false;
                        MessageBox.Show(this, "The Vault was updated successfully!", "Vault Saved", MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show(this, "Unable to write the Save Data", "Error when Saving", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception x)
                {
                    Debug.WriteLine(x.GetType() + "@btSave_Click Writing: " + x.Message);
                    MessageBox.Show(this, x.Message, "Error when Saving", MessageBoxButton.OK, MessageBoxImage.Error);
                }               
            }
        }
    }
}