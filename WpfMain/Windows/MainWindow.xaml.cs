using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Xps;
using System.Windows.Xps.Packaging;


namespace WpfMain
{
    public class TodoItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Description { get; set; }
        public bool IsDone { get; set; }
    }

    public static class StringExtensions
    {
        public static int LevenshteinDistance(this string s, string t)
        {
            int n = s.Length;
            int m = t.Length;
            var d = new int[n + 1, m + 1];

            for (int i = 0; i <= n; d[i, 0] = i++) ;
            for (int j = 0; j <= m; d[0, j] = j++) ;

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                    d[i, j] = new int[] {
                    d[i - 1, j] + 1,
                    d[i, j - 1] + 1,
                    d[i - 1, j - 1] + cost
                }.Min();
                }
            }

            return d[n, m];
        }
    }
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
                Anims.ObjShiftScriptList(sideBar_ToDoList, sideBar_ToDoList.Width, 0);
            }
            else
            {
                Anims.ObjShiftScriptList(sideBar, sideBar.Width, 0);
                isListOpen = false;
            }
        }
        #endregion

       

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
                Anims.ObjShiftScriptList(sideBar, sideBar.Width, 0);
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


        private void UpdateTodoStatus(Guid id, bool isCompleted, CheckBox check, TextBlock textB)
        {
            var item = todos.FirstOrDefault(i => i.Id == id);
            if (item != null)
            {
                item.IsDone = isCompleted;
                SaveTodos();
            }
            if (check.IsChecked == true)
            {
                textB.TextDecorations = TextDecorations.Strikethrough;
                textB.Foreground = Brushes.Gray;
                textB.Opacity = 0.5;
            }
            else
            {
                textB.TextDecorations = null;
                textB.Foreground = Brushes.White;
                textB.Opacity = 1.0;
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
        

        public class NoteModel
        {
            public string FileName { get; set; }
            public string Preview { get; set; }
            public DateTime DateCreate { get; set; }
            public string FullPath { get; set; }
            public string TagColor { get; set; }
        }

        private void LoadNotes(string tagFilter = null)
        {
            string notesFolder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Notes");
            var notes = new List<NoteModel>();
            string searchText = searchTextBox.Text;

            var files = Directory.GetFiles(notesFolder, "*.txt");

            foreach (var file in files)
            {
                string fileName = System.IO.Path.GetFileName(file);
                string content = File.ReadAllText(file);
                var note = new NoteModel
                {
                    FileName = fileName,
                    Preview = content.Length > 100 ? content.Substring(0, 50) + "..." : content,
                    DateCreate = File.GetCreationTime(file),
                    FullPath = file,
                    TagColor = LoadNoteTagColor(file)
                };

                bool isFilenameMatch = false;
                bool isContentMatch = false;

                if (!string.IsNullOrEmpty(searchText))
                {
                    isFilenameMatch = fileName.LevenshteinDistance(searchText) < 6;
                    isContentMatch = content.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0;
                }
                else
                {
                    isFilenameMatch = true;
                    isContentMatch = true;
                }

                bool isTagMatch = string.IsNullOrEmpty(tagFilter) || tagFilter == "Gray" || note.TagColor == tagFilter;

                if ((isFilenameMatch || isContentMatch) && isTagMatch)
                {
                    notes.Add(note);
                }

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
            string notesDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Notes");
            string NameFile = AddFileTextBox.Text.Trim() + ".txt";
            if (string.IsNullOrWhiteSpace(NameFile))
            {
                MessageBox.Show("Filename cannot be empty.");
                return;
            }
            string filePath = System.IO.Path.Combine(notesDir, NameFile);
            File.WriteAllText(filePath, txtbox.Text);
            AddFileGrid.Visibility = Visibility.Collapsed;
        }


        public List<TodoItem> LoadTodoListForPDF()
        {
            string todoFilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "todo.json");
            if (File.Exists(todoFilePath))
            {
                string json = File.ReadAllText(todoFilePath);
                return JsonConvert.DeserializeObject<List<TodoItem>>(json);
            }
            return new List<TodoItem>();
        }

        private void ExportToPDFBtn_Click(object sender, RoutedEventArgs e)
        {

            string xpsPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "output.xps");

            if (File.Exists(xpsPath))
            {
                File.Delete(xpsPath);
            }

            string[] txtfiles = Directory.GetFiles(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Notes"), "*.txt");

            FixedDocument doc = new FixedDocument();

            foreach (var file in txtfiles)
            {
                string document = File.ReadAllText(file);
                string filename = System.IO.Path.GetFileNameWithoutExtension(file);
                var page = createNotePage(filename, document);
                doc.Pages.Add(page);
            }

            List<TodoItem> todoList = LoadTodoListForPDF();
            var todoPage = createTodoListPage(todoList);
            doc.Pages.Add(todoPage);

            using (var xpsDoc = new XpsDocument(xpsPath, FileAccess.ReadWrite))
            {
                XpsDocumentWriter writer = XpsDocument.CreateXpsDocumentWriter(xpsDoc);
                writer.Write(doc);
            }

            PrintPDF(xpsPath, @"D:\code\C#\WPFapp_PersonalToDoListRework\WpfMain\bin\Debug\Note.pdf");
        }

        private PageContent createNotePage(string filename, string text)
        {
            var pageContent = new PageContent();
            var fixedPage = new FixedPage();

            var titleTextBlock = new TextBlock
            {
                Text = filename,
                FontWeight = FontWeights.Bold,
                FontSize = 18,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(40, 20, 40, 10)
            };

            var contentTextBlock = new TextBlock
            {
                Text = text,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(40),
                FontSize = 14,
                Width = 800
            };

            FixedPage.SetLeft(titleTextBlock, 0);
            FixedPage.SetTop(titleTextBlock, 0);
            fixedPage.Children.Add(titleTextBlock);

            FixedPage.SetLeft(contentTextBlock, 0);
            FixedPage.SetTop(contentTextBlock, 50);
            fixedPage.Children.Add(contentTextBlock);

            fixedPage.Width = 816;
            fixedPage.Height = 1056;

            ((IAddChild)pageContent).AddChild(fixedPage);
            return pageContent;
        }

        private PageContent createTodoListPage(List<TodoItem> todoList)
        {
            var pageContent = new PageContent();
            var fixedPage = new FixedPage();

            var stackPanel = new StackPanel { Orientation = Orientation.Vertical, Margin = new Thickness(40) };

            double totalHeight = 0;
            double pageHeight = 1056;

            var titleTextBlock = new TextBlock
            {
                Text = "To-Do List",
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 20)
            };
            stackPanel.Children.Add(titleTextBlock);
            totalHeight += titleTextBlock.RenderSize.Height;

            foreach (var todo in todoList)
            {
                var taskTextBlock = new TextBlock
                {
                    Text = $"{todo.Description} - {(todo.IsDone ? "[Completed]" : "[Not Completed]")}",
                    FontSize = 14,
                    Margin = new Thickness(0, 0, 0, 5)
                };
                stackPanel.Children.Add(taskTextBlock);
                totalHeight += taskTextBlock.RenderSize.Height;
                if (totalHeight > pageHeight)
                {
                    fixedPage = new FixedPage();
                    stackPanel = new StackPanel { Orientation = Orientation.Vertical, Margin = new Thickness(40) };
                    totalHeight = 0;
                }
            }

            fixedPage.Children.Add(stackPanel);
            fixedPage.Width = 816;
            fixedPage.Height = 1056;

            ((IAddChild)pageContent).AddChild(fixedPage);
            return pageContent;
        }


        private void PrintPDF(string filepath, string pdffilePath)
        {
            var printDialog = new PrintDialog();
            var queue = new LocalPrintServer().GetPrintQueue("Microsoft Print to PDF");
            using (var xpsDoc = new XpsDocument(filepath, FileAccess.Read))
            {
                var pages = xpsDoc.GetFixedDocumentSequence().DocumentPaginator;
                var writer = PrintQueue.CreateXpsDocumentWriter(queue);
                var printticket = queue.DefaultPrintTicket;
                printticket.PageMediaSize = new PageMediaSize(PageMediaSizeName.ISOA4);

                queue.UserPrintTicket = printticket;
                queue.DefaultPrintTicket = printticket;

                writer.Write(pages);
            }
        }

        private void searchTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            LoadNotes();
        }

        private void searchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void SetTagColor_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var selectedColor = menuItem?.Tag.ToString();
            var noteToTag = menuItem?.DataContext as NoteModel;

            if (noteToTag == null || string.IsNullOrEmpty(selectedColor)) return;

            noteToTag.TagColor = selectedColor;

            SaveNoteTagColor(noteToTag);
            LoadNotes();
        }

        private void SaveNoteTagColor(NoteModel note)
        {
            string metadataFilePath = System.IO.Path.ChangeExtension(note.FullPath, ".tag");
            File.WriteAllText(metadataFilePath, note.TagColor);
        }

        private void TagColorFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selectedTag = (TagColorFilterComboBox.SelectedItem as ComboBoxItem)?.Tag.ToString();
            LoadNotes(selectedTag);
        }

        private string LoadNoteTagColor(string filePath)
        {
            string metadataFilePath = System.IO.Path.ChangeExtension(filePath, ".tag");
            if (File.Exists(metadataFilePath))
            {
                return File.ReadAllText(metadataFilePath);
            }
            return "Gray";
        }
    }



}
