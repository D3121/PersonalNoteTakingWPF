using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using Newtonsoft.Json;
namespace WpfMain
{
    public partial class MainWindow : Window
    {

        private int currentUserId = -1;
        static string connStr = @"Data Source=D:\Code\C#\WPFapp_PersonalToDoListRework\WpfMain\bin\Debug\main.db;Version=3;";

        private List<TodoItem> todos = new List<TodoItem>();
        private static readonly string FilePath = "todo.json";
        public MainWindow()
        {
            InitializeComponent();
            LoadTodos();
            LoadNotes();
            Anims.ObjShiftScriptList(sideBar, sideBar.Width, 738);
            isListOpen = true;
        }

        #region Misc

        private void winCloseBtn_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void winMaxBtn_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
                btn.Content = "\xE922";
            }
            else if (this.WindowState == WindowState.Normal)
            {
                this.WindowState = WindowState.Maximized;
                btn.Content = "\xE923";
            }
        }

        private void winMinBtn_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        #endregion

        #region Main Buttons

        private void openBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDlg = new OpenFileDialog();
            openFileDlg.Filter = "Text Document (*.txt)|*.txt";
            Nullable<bool> result = openFileDlg.ShowDialog();
            if (result == true)
            {
                txtbox.Text = System.IO.File.ReadAllText(openFileDlg.FileName);
            }
        }

        private void saveBtn_Click(object sender, RoutedEventArgs e)
        {
            AddFileGrid.Visibility = Visibility.Visible;
        }


        private bool isListOpen = false;
        private void listBtn_Click(object sender, RoutedEventArgs e)
        {
            LoadNotes();
            if (isListOpen == false)
            {
                Anims.ObjShiftScriptList(sideBar, sideBar.Width, 738);
                isListOpen = true;
                IsToDoListOpen = false;
                IsCloudSavingOpen = false;
                Anims.ObjShiftScriptList(sideBar_ToDoList, sideBar_ToDoList.Width, 0);
                Anims.ObjShiftScriptList(sideBar_CloudSaving, sideBar_CloudSaving.Width, 0);
            }
            else
            {
                Anims.ObjShiftScriptList(sideBar, sideBar.Width, 0);
                isListOpen = false;
            }
        }
        #endregion

        

        #region Main Buttons2

        private void mainTabCtrl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private bool IsToDoListOpen = false;
        private void ToDoList_Click(object sender, RoutedEventArgs e)
        {
            if (IsToDoListOpen == false)
            {
                sideBar_ToDoList.Visibility = Visibility.Visible;
                Anims.FadeIn(sideBar_ToDoList);
                Anims.ObjShiftScriptList(sideBar_ToDoList, sideBar_ToDoList.Width, 720);
                IsToDoListOpen = true;
                isListOpen = false;
                IsCloudSavingOpen = false;
                Anims.ObjShiftScriptList(sideBar, sideBar.Width, 0);
                Anims.ObjShiftScriptList(sideBar_CloudSaving, sideBar_CloudSaving.Width, 0);
            }
            else
            {
                Anims.ObjShiftScriptList(sideBar_ToDoList, sideBar_ToDoList.Width, 0);
                IsToDoListOpen = false;
            }
        }

        private void AddToDoListBtn_Click(object sender, RoutedEventArgs e)
        {
            var text = newTodoTextBox.Text.Trim();
            if (string.IsNullOrEmpty(text)) return;

            var todo = new TodoItem { Description = text, IsDone = false };
            todos.Add(todo);
            AddTodoUI(todo);
            SaveTodos();

            newTodoTextBox.Clear();
        }

        private void SaveTodos()
        {
            File.WriteAllText(FilePath, JsonConvert.SerializeObject(todos, Newtonsoft.Json.Formatting.Indented));
        }

        private void AddTodoUI(TodoItem item)
        {
            var panel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 5, 0, 0) };

            var checkbox = new CheckBox { VerticalAlignment = VerticalAlignment.Center, IsChecked = item.IsDone };
            var label = new TextBlock
            {
                Text = item.Description,
                Foreground = Brushes.White,
                Margin = new Thickness(10, 0, 10, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            var removeBtn = new Button
            {
                Content = "✖",
                Width = 25,
                Height = 25,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand,
                ToolTip = "Remove"
            };

            removeBtn.Click += (s, e) =>
            {
                todoListPanel.Children.Remove(panel);
                todos.RemoveAll(i => i.Id == item.Id);
                SaveTodos();
            };



            panel.Children.Add(checkbox);
            panel.Children.Add(label);
            panel.Children.Add(removeBtn);

            checkbox.Checked += (s, e) => { UpdateTodoStatus(item.Id, true, checkbox, label); };
            checkbox.Unchecked += (s, e) => { UpdateTodoStatus(item.Id, false, checkbox, label); };
            if (checkbox.IsChecked == true)
            {
                UpdateTodoStatus(item.Id, true, checkbox, label);
            }
            else
            {
                UpdateTodoStatus(item.Id, false, checkbox, label);
            }

            todoListPanel.Children.Add(panel);
        }


        private void UpdateTodoStatus(Guid id, bool isCompleted, CheckBox check, TextBlock textla)
        {
            var item = todos.FirstOrDefault(i => i.Id == id);
            if (item != null)
            {
                item.IsDone = isCompleted;
                SaveTodos();
            }
            if (check.IsChecked == true)
            {
                textla.TextDecorations = TextDecorations.Strikethrough;
                textla.Foreground = Brushes.Gray;
                textla.Opacity = 0.5;
            }
            else
            {
                textla.TextDecorations = null;
                textla.Foreground = Brushes.White;
                textla.Opacity = 1.0;
            }
        }

        private void LoadTodos()
        {
            if (!File.Exists(FilePath))
                return;

            try
            {
                var json = File.ReadAllText(FilePath);
                todos = JsonConvert.DeserializeObject<List<TodoItem>>(json) ?? new List<TodoItem>();

                foreach (var todo in todos)
                {
                    AddTodoUI(todo);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load to do list ur nigga: {ex.Message}");
            }

        }

        private bool IsCloudSavingOpen = false;
        private void Cloud_Saving_Click(object sender, RoutedEventArgs e)
        {
            if (IsCloudSavingOpen == false)
            {
                sideBar_CloudSaving.Visibility = Visibility.Visible;
                Anims.FadeIn(sideBar_CloudSaving);
                Anims.ObjShiftScriptList(sideBar_CloudSaving, sideBar_ToDoList.Width, 720);
                IsCloudSavingOpen = true;
                IsToDoListOpen = false;
                isListOpen = false;
                Anims.ObjShiftScriptList(sideBar, sideBar.Width, 0);
                Anims.ObjShiftScriptList(sideBar_ToDoList, sideBar_ToDoList.Width, 0);
            }
            else
            {
                Anims.ObjShiftScriptList(sideBar_CloudSaving, sideBar_CloudSaving.Width, 0);
                IsCloudSavingOpen = false;
            }
        }

        private void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            var user = new { Username = UsernameLogin.Text, Password = PasswordLogin.Text };
            using (var conn = new SQLiteConnection(connStr))
            {
                conn.Open();
                using (var cmd = new SQLiteCommand("SELECT Id FROM Users WHERE Username=@u AND Password=@p", conn))
                {
                    cmd.Parameters.AddWithValue("@u", user.Username);
                    cmd.Parameters.AddWithValue("@p", user.Password);
                    var result = cmd.ExecuteScalar();
                    MessageBox.Show("Logged in : id = " + Convert.ToInt32(result));
                    currentUserId = Convert.ToInt32(result);
                }
            }

        }

        private void RegistryBtn_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameLogin.Text.Trim();
            string password = PasswordLogin.Text.Trim();
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Username and password are required.");
                return;
            }

            using (var conn = new SQLiteConnection(connStr))
            {
                conn.Open();
                var cmd = new SQLiteCommand("INSERT INTO Users (Username, Password) VALUES (@u, @p)", conn);
                cmd.Parameters.AddWithValue("@u", username);
                cmd.Parameters.AddWithValue("@p", password);
                cmd.ExecuteNonQuery();
                MessageBox.Show("ur id: " + ((int)conn.LastInsertRowId).ToString());
            }
        }

        private void UploadBtn_Click(object sender, RoutedEventArgs e)
        {
            UploadFile(currentUserId);
        }

        private void UploadFile(int userId)
        {
            string todoJson = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "todo.json");
            string JsonContent = File.ReadAllText(todoJson);

            string scriptsDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Notes");
            string zipPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Notes.zip");
            if (File.Exists(zipPath))
            {
                File.Delete(zipPath);
            }

            ZipFile.CreateFromDirectory(scriptsDir, zipPath);

            using (var conn = new SQLiteConnection(connStr))
            {
                conn.Open();
                var checkCmd = new SQLiteCommand("DELETE FROM UserData WHERE UserId = @u", conn);
                checkCmd.Parameters.AddWithValue("@u", userId);
                checkCmd.ExecuteNonQuery();
                using (var cmd = new SQLiteCommand(@"INSERT OR REPLACE INTO UserData (UserId, TodoJson, ZipData) VALUES (@id, @json, @zip)", conn))
                {
                    cmd.Parameters.AddWithValue("@id", userId);
                    cmd.Parameters.AddWithValue("@json", JsonContent);
                    cmd.Parameters.AddWithValue("@zip", zipPath);
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Uploaded!!!!");
                }
            }
        }


        private void DownloadBtn_Click(object sender, RoutedEventArgs e)
        {
            string todoJson = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "todo.json");
            string scriptsDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Notes");
            string folderTextPath = @"D:\Code Projects\WPFapp_PersonalToDoList\WpfMain\bin\Debug\Notes";
            if (currentUserId < 1) { MessageBox.Show("Please log in first."); return; }
            using (var conn = new SQLiteConnection(connStr))
            {
                conn.Open();
                using (var cmd = new SQLiteCommand("SELECT TodoJson, ZipData FROM UserData WHERE UserId = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", currentUserId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string json = reader["TodoJson"].ToString();
                            byte[] zipData = (byte[])reader["ZipData"];

                            File.WriteAllText(todoJson, json);

                            if (Directory.Exists(scriptsDir))
                            {



                                string targetFolder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Notes");

                                foreach (string file in Directory.GetFiles(targetFolder))
                                {
                                    File.Delete(file);
                                }

                                foreach (string dir in Directory.GetDirectories(targetFolder))
                                {
                                    Directory.Delete(dir, true);
                                }
                            }
                            ZipFile.ExtractToDirectory("Notes.zip", folderTextPath);
                        }
                    }
                }
            }
        }

        private void SyncBtn_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion

        public class NoteModel
        {
            public string FileName { get; set; }
            public string Preview { get; set; }
            public string FullPath { get; set; }
        }

        private void LoadNotes()
        {

            string notesFolder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Notes");
            var notes = new List<NoteModel>();

            foreach (var file in Directory.GetFiles(notesFolder, "*.txt"))
            {
                string content = File.ReadAllText(file);
                notes.Add(new NoteModel
                {
                    FileName = System.IO.Path.GetFileName(file),
                    Preview = content.Length > 100 ? content.Substring(0, 100) + "..." : content,
                    FullPath = file
                });
            }

            mainGridView.ItemsSource = notes;
        }


        private void NoteCard_Click(object sender, MouseButtonEventArgs e)
        {
            var data = (sender as Border)?.DataContext as NoteModel;
            if (data != null)
            {
                txtbox.Text = File.ReadAllText(data.FullPath);
                isListOpen = false;
                Anims.ObjShiftScriptList(sideBar, sideBar.Width, 0);
            }
        }
        private NoteModel noteToRename = null;
        private void RenameNote_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            noteToRename = menuItem?.DataContext as NoteModel;
            if (noteToRename == null) return;

            string splitTxt = noteToRename.FileName.Substring(0, noteToRename.FileName.Length - 4);

            RenameTextBox.Text = noteToRename.FileName.Substring(0, Math.Min(5, splitTxt.Length));
            RenameFileGrid.Visibility = Visibility.Visible;
        }

        private void RenameCancel_Click(object sender, RoutedEventArgs e)
        {
            RenameFileGrid.Visibility = Visibility.Collapsed;
            noteToRename = null;
        }

        private void RenameConfirm_Click(object sender, RoutedEventArgs e)
        {
            string newName = RenameTextBox.Text.Trim() + ".txt";
            if (string.IsNullOrWhiteSpace(newName))
            {
                MessageBox.Show("Filename cannot be empty.");
                return;
            }

            string newPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(noteToRename.FullPath), newName);

            if (File.Exists(newPath))
            {
                MessageBox.Show("A file with that name already exists.");
                return;
            }

            try
            {
                File.Move(noteToRename.FullPath, newPath);
                RenameFileGrid.Visibility = Visibility.Collapsed;
                noteToRename = null;
                LoadNotes();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Rename failed: {ex.Message}");
            }
        }

        private void DeleteNote_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var context = (menuItem.DataContext as NoteModel);
            if (context == null) return;

            if (MessageBox.Show($"Delete {context.FileName}?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                File.Delete(context.FullPath);
                LoadNotes();
            }
        }

        private NoteModel noteToAdd = null;
        private void CancelAddNoteButton_Click(object sender, RoutedEventArgs e)
        {
            AddFileGrid.Visibility = Visibility.Collapsed;
            noteToAdd = null;
        }

        private void ConfirmAddNoteButton_Click(object sender, RoutedEventArgs e)
        {
            string notesDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Notes");
            string NameFile = AddFileTextBox.Text.Trim() + ".txt";
            if (string.IsNullOrWhiteSpace(NameFile))
            {
                MessageBox.Show("Filename cannot be empty.");
                return;
            }
            string filePath = Path.Combine(notesDir, NameFile);
            File.WriteAllText(filePath, txtbox.Text);
            AddFileGrid.Visibility = Visibility.Collapsed;
        }
    }



}
