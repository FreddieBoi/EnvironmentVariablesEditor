using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using Microsoft.Win32;

namespace EnvironmentVariablesEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {

        #region Fields

        private readonly ObservableCollection<EnvironmentVariable> _environmentVariables = new ObservableCollection<EnvironmentVariable>();

        private readonly ObservableCollection<PathEntry> _pathEntries = new ObservableCollection<PathEntry>();

        private const string DefaultFileDialogFileName = "EnvironmentVariables";

        private const string DefaultFileDialogDefaultExt = ".bak";

        private const string DefaultFileDialogFilter = "Backup|*.bak|Plain Text|*.txt|XML|*.xml";

        private IEditableObject _editedObject;

        private bool _edited;

        private bool IsEdited
        {
            get
            {
                return _edited;
            }
            set
            {
                _edited = value;
                OnPropertyChanged("IsEdited");
            }
        }

        #endregion

        public MainWindow()
        {
            InitializeComponent();
        }

        #region Private Helpers

        private void LoadEnvironmentVariables()
        {
            workingStatus.Content = "Load...";

            // Clear lists
            _pathEntries.Clear();
            _environmentVariables.Clear();

            // Get the environment variables
            var machineVariables = Environment.GetEnvironmentVariables(EnvironmentVariableTarget.Machine);
            var userVariables = Environment.GetEnvironmentVariables(EnvironmentVariableTarget.User);

            // Parse the result and put the variables in the correct lists
            foreach (var key in userVariables.Keys)
            {
                _environmentVariables.Add(new EnvironmentVariable(key.ToString(), userVariables[key].ToString(), EnvironmentVariableTarget.User));
            }
            foreach (var key in machineVariables.Keys)
            {
                if (key.ToString().Equals("Path"))
                {
                    foreach (var part in machineVariables[key].ToString().Split(';'))
                    {
                        _pathEntries.Add(new PathEntry(part));
                    }
                }
                else
                {
                    _environmentVariables.Add(new EnvironmentVariable(key.ToString(), machineVariables[key].ToString(), EnvironmentVariableTarget.Machine));
                }
            }

            // Set as unedited (since they're just loaded)
            IsEdited = false;

            // Update the status
            workingStatus.Content = String.Format("Loaded {0} path entries and {1} other environment variables.", _pathEntries.Count, _environmentVariables.Count);
        }

        private void SaveEnvironmentVariables()
        {
            workingStatus.Content = "Save...";

            Environment.SetEnvironmentVariable("Path", String.Join(";", _pathEntries.Select(pathEntry => pathEntry.Value)), EnvironmentVariableTarget.Machine);

            foreach (var environmentVariable in _environmentVariables)
            {
                Environment.SetEnvironmentVariable(environmentVariable.Key, environmentVariable.Value,
                                                   environmentVariable.Target);
            }

            // Set as unedited (since they're saved now)
            IsEdited = false;

            // Update the status
            workingStatus.Content = String.Format("Saved {0} path entries and {1} other environment variables.", _pathEntries.Count, _environmentVariables.Count);
        }

        private void Save()
        {
            // Make sure something was edited
            if (!IsEdited)
            {
                return;
            }

            // Save the variables
            SaveEnvironmentVariables();
        }

        private void Load()
        {
            if (IsEdited)
            {
                switch (MessageBox.Show("You have unsaved changes. Do you want to save them before reloading?", "Save changes before reloading?", MessageBoxButton.YesNoCancel, MessageBoxImage.Question))
                {
                    case MessageBoxResult.Cancel:
                        // Don't do anything, get out!
                        return;
                    case MessageBoxResult.No:
                        // Don't save changes, but reload.
                        break;
                    case MessageBoxResult.Yes:
                        // Save changes before reload!
                        SaveEnvironmentVariables();
                        break;
                }
            }
            LoadEnvironmentVariables();
        }

        private static void About()
        {
            var version = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;
            MessageBox.Show(String.Format(@"Environment Variables Editor {0}{1}2012 {2} Freddie Pettersson{3}{4}Environment Variables Editor is a simple editor to use when you want to modify the environment variables of a system. Mainly constructed to simplify editing of the Path variable. Written by Freddie Pettersson (freddieboi@gmail.com).", version, Environment.NewLine, "\u00a9", Environment.NewLine, Environment.NewLine), "About Environment Variables Editor", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private bool Exit()
        {
            if (IsEdited)
            {
                switch (MessageBox.Show("You have unsaved changes. Do you want to save them before exiting?", "Save changes before exiting?", MessageBoxButton.YesNoCancel, MessageBoxImage.Question))
                {
                    case MessageBoxResult.Cancel:
                        // Don't do anything, cancel the closing
                        return false;
                    case MessageBoxResult.No:
                        // Don't save changes, but exit.
                        break;
                    case MessageBoxResult.Yes:
                        // Save changes before exit!
                        SaveEnvironmentVariables();
                        break;
                }
            }
            return true;
        }

        private void Export()
        {
            workingStatus.Content = "Export...";

            // Create a SaveFileDialog.
            var saveFileDialog = new SaveFileDialog
                                     {
                                         FileName = DefaultFileDialogFileName,
                                         DefaultExt = DefaultFileDialogDefaultExt,
                                         Filter = DefaultFileDialogFilter
                                     };

            // Show save file dialog box, abort if not successful
            if (saveFileDialog.ShowDialog() != true) return;

            // Save the objects to the specified file
            var writer = new StreamWriter(saveFileDialog.FileName);
            var serializer = new XmlSerializer(typeof(List<ExportableObject>));
            var objects = new List<ExportableObject>();
            objects.AddRange(_pathEntries);
            objects.AddRange(_environmentVariables);
            serializer.Serialize(writer, objects);
            writer.Close();

            // Update the status
            workingStatus.Content = String.Format("Exported {0} path entries and {1} other environment variables to {2}.", _pathEntries.Count, _environmentVariables.Count, saveFileDialog.FileName);
        }

        private void Restore()
        {
            workingStatus.Content = "Restore...";

            // Create an OpenFileDialog.
            var openFileDialog = new OpenFileDialog
                                     {
                                         FileName = DefaultFileDialogFileName,
                                         DefaultExt = DefaultFileDialogDefaultExt,
                                         Filter = DefaultFileDialogFilter
                                     };

            // Show open file dialog box, abort if not successful
            if (openFileDialog.ShowDialog() != true) return;

            // Load objects from the specified file
            var reader = new StreamReader(openFileDialog.FileName);
            var serializer = new XmlSerializer(typeof(List<ExportableObject>));
            var objects = (List<ExportableObject>)serializer.Deserialize(reader);
            reader.Close();

            // Put the objects in the appropriate lists (clear first)
            _pathEntries.Clear();
            _environmentVariables.Clear();
            foreach (var obj in objects)
            {
                if (obj is PathEntry)
                {
                    _pathEntries.Add(obj as PathEntry);
                }
                else if (obj is EnvironmentVariable)
                {
                    _environmentVariables.Add(obj as EnvironmentVariable);
                }
            }

            // Set as edited (for safety)
            IsEdited = true;

            // Update status
            workingStatus.Content = String.Format("Restored {0} path entries and {1} other environment variables from {2}.", _pathEntries.Count, _environmentVariables.Count, openFileDialog.FileName);
        }

        #endregion

        #region Property Changed Event Handling

        public event PropertyChangedEventHandler PropertyChanged;

        // Create the OnPropertyChanged method to raise the event
        protected void OnPropertyChanged(string name)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        private void HandlePropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (!propertyChangedEventArgs.PropertyName.Equals("IsEdited")) return;

            if (IsEdited)
            {
                editStatus.Content = "Unsaved changes!";
                editStatus.Foreground = Brushes.Orange;
            }
            else
            {
                editStatus.Content = "No changes.";
                editStatus.Foreground = Brushes.Green;
            }
        }

        #endregion

        #region WPF Events

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            // Load the environment variables
            LoadEnvironmentVariables();

            // Set the data context (populate the DataGrids)
            pathDataGrid.DataContext = _pathEntries;
            environmentVariablesDataGrid.DataContext = _environmentVariables;

            PropertyChanged = HandlePropertyChanged;
        }

        private void LoadMenuItemClick(object sender, RoutedEventArgs e)
        {
            Load();
        }

        private void SaveMenuItemClick(object sender, RoutedEventArgs e)
        {
            Save();
        }

        private void ExitMenuItemClick(object sender, RoutedEventArgs e)
        {
            if (Exit())
            {
                Close();
            }
        }

        private void AboutMenuItemClick(object sender, RoutedEventArgs e)
        {
            About();
        }

        private void BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (sender.Equals(pathDataGrid))
            {
                _editedObject = pathDataGrid.Items.GetItemAt(e.Row.GetIndex()) as PathEntry;
            }
            else if (sender.Equals(environmentVariablesDataGrid))
            {
                _editedObject = environmentVariablesDataGrid.Items.GetItemAt(e.Row.GetIndex()) as EnvironmentVariable;
            }
        }

        private void CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction != DataGridEditAction.Commit) return;

            if (sender.Equals(pathDataGrid))
            {
                if (((PathEntry)_editedObject).Value != ((TextBox)e.EditingElement).Text)
                {
                    IsEdited = true;
                }
            }
            else if (sender.Equals(environmentVariablesDataGrid))
            {
                if (_editedObject == null || e.EditingElement == null || ((EnvironmentVariable)_editedObject).Value != ((TextBox)e.EditingElement).Text)
                {
                    IsEdited = true;
                }
            }
        }

        private void WindowClosing(object sender, CancelEventArgs e)
        {
            e.Cancel = !Exit();
        }

        private void ExportMenuItemClick(object sender, RoutedEventArgs e)
        {
            Export();
        }

        private void RestoreMenuItemClick(object sender, RoutedEventArgs e)
        {
            Restore();
        }

        #endregion

        #region Command Events

        private void Load(object sender, ExecutedRoutedEventArgs e)
        {
            Load();
        }

        private void Save(object sender, ExecutedRoutedEventArgs e)
        {
            Save();
        }

        private void About(object sender, ExecutedRoutedEventArgs e)
        {
            About();
        }

        private void Export(object sender, ExecutedRoutedEventArgs e)
        {
            Export();
        }

        private void Restore(object sender, ExecutedRoutedEventArgs e)
        {
            Restore();
        }

        #endregion

    }

}
