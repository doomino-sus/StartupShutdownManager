using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using Microsoft.Win32.TaskScheduler;
using System.Security.Principal;
using System.Diagnostics;
using System.Drawing;
using System.ComponentModel;
using System.Xml.Serialization;
using System.Linq;

namespace StartupShutdownManager
{
    public enum ExecutionMoment
    {
        SystemStartup,    // Przy starcie systemu
        UserLogon,        // Przy logowaniu użytkownika
        BeforeShutdown,   // Przed wyłączeniem systemu
        BeforeLogoff      // Przed wylogowaniem użytkownika
    }

    public class Configuration
    {
        public List<ScriptItem> Scripts { get; set; }
        private static string ConfigPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "StartupShutdownManager",
            "config.xml");

        public Configuration()
        {
            Scripts = new List<ScriptItem>();
        }

        public static void Save(List<ScriptItem> scripts)
        {
            var config = new Configuration { Scripts = scripts };
            string dirPath = Path.GetDirectoryName(ConfigPath);

            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);

            using (var writer = new StreamWriter(ConfigPath))
            {
                var serializer = new XmlSerializer(typeof(Configuration));
                serializer.Serialize(writer, config);
            }
        }

        public static Configuration Load()
        {
            if (!File.Exists(ConfigPath))
                return new Configuration();

            try
            {
                using (var reader = new StreamReader(ConfigPath))
                {
                    var serializer = new XmlSerializer(typeof(Configuration));
                    return (Configuration)serializer.Deserialize(reader);
                }
            }
            catch
            {
                return new Configuration();
            }
        }
    }

    public class ScriptItem
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public ExecutionMoment ExecutionMoment { get; set; }
        public bool RunAsAdminWithUserCredentials { get; set; }
        public bool IsEnabled { get; set; } = true;

        public ScriptItem Clone()
        {
            return new ScriptItem
            {
                Name = this.Name,
                Path = this.Path,
                ExecutionMoment = this.ExecutionMoment,
                RunAsAdminWithUserCredentials = this.RunAsAdminWithUserCredentials,
                IsEnabled = this.IsEnabled
            };
        }

        public ScriptItem() { }
    }

    public class ScriptConfigurationDialog : Form
    {
        private TextBox pathTextBox;
        private Button browseButton;
        private ComboBox executionMomentComboBox;
        private CheckBox adminRightsCheckBox;
        private Button okButton;
        private Button cancelButton;

        public string ScriptPath { get; private set; }
        public string ScriptType { get; private set; }
        public ExecutionMoment ExecutionMoment { get; private set; }
        public bool RunAsAdminWithUserCredentials { get; private set; }

        private bool isEditMode;
        private ScriptItem editingScript;

        public ScriptConfigurationDialog(ScriptItem scriptToEdit = null)
        {
            isEditMode = scriptToEdit != null;
            editingScript = scriptToEdit;
            InitializeComponents();
            if (isEditMode)
            {
                LoadScriptData();
            }
        }

        private void LoadScriptData()
        {
            pathTextBox.Text = editingScript.Path;
            ScriptPath = editingScript.Path;
            executionMomentComboBox.SelectedIndex = (int)editingScript.ExecutionMoment;
            adminRightsCheckBox.Checked = editingScript.RunAsAdminWithUserCredentials;

            string extension = Path.GetExtension(ScriptPath).ToLower();
            switch (extension)
            {
                case ".ps1":
                    ScriptType = "PowerShell";
                    break;
                case ".bat":
                    ScriptType = "Batch";
                    break;
                case ".exe":
                    ScriptType = "Executable";
                    break;
                default:
                    ScriptType = "Unknown";
                    break;
            }
        }

        private void InitializeComponents()
        {
            this.Text = isEditMode ? "Edycja skryptu" : "Konfiguracja skryptu";
            this.Size = new Size(500, 220);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

            Label pathLabel = new Label();
            pathLabel.Text = "Ścieżka do skryptu:";
            pathLabel.Location = new Point(10, 10);
            pathLabel.AutoSize = true;

            pathTextBox = new TextBox();
            pathTextBox.Location = new Point(10, 30);
            pathTextBox.Width = 350;
            pathTextBox.ReadOnly = true;

            browseButton = new Button();
            browseButton.Text = "Przeglądaj";
            browseButton.Location = new Point(370, 29);
            browseButton.Click += BrowseButton_Click;

            Label momentLabel = new Label();
            momentLabel.Text = "Moment wykonania:";
            momentLabel.Location = new Point(10, 60);
            momentLabel.AutoSize = true;

            executionMomentComboBox = new ComboBox();
            executionMomentComboBox.Location = new Point(10, 80);
            executionMomentComboBox.Width = 350;
            executionMomentComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            executionMomentComboBox.Items.AddRange(new string[]
            {
                "Przy starcie systemu",
                "Przy logowaniu użytkownika",
                "Przed wyłączeniem systemu",
                "Przed wylogowaniem użytkownika"
            });
            executionMomentComboBox.SelectedIndex = 0;

            adminRightsCheckBox = new CheckBox();
            adminRightsCheckBox.Text = "Uruchom z uprawnieniami administratora";
            adminRightsCheckBox.Location = new Point(10, 110);
            adminRightsCheckBox.Width = 350;
            adminRightsCheckBox.Checked = false;

            okButton = new Button();
            okButton.Text = "OK";
            okButton.DialogResult = DialogResult.OK;
            okButton.Location = new Point(280, 140);

            cancelButton = new Button();
            cancelButton.Text = "Anuluj";
            cancelButton.DialogResult = DialogResult.Cancel;
            cancelButton.Location = new Point(370, 140);

            this.Controls.AddRange(new Control[]
            {
                pathLabel,
                pathTextBox,
                browseButton,
                momentLabel,
                executionMomentComboBox,
                adminRightsCheckBox,
                okButton,
                cancelButton
            });

            this.AcceptButton = okButton;
            this.CancelButton = cancelButton;

            // W trybie edycji, jeśli ścieżka jest już ustawiona
            if (isEditMode)
            {
                browseButton.Enabled = false; // Opcjonalnie, jeśli nie chcemy pozwolić na zmianę ścieżki
            }
        }

        private void BrowseButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Wszystkie obsługiwane pliki (*.exe;*.bat;*.ps1)|*.exe;*.bat;*.ps1|Pliki wykonywalne (*.exe)|*.exe|Pliki wsadowe (*.bat)|*.bat|Skrypty PowerShell (*.ps1)|*.ps1";
                openFileDialog.FilterIndex = 1;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    ScriptPath = openFileDialog.FileName;
                    pathTextBox.Text = ScriptPath;
                    string extension = Path.GetExtension(ScriptPath).ToLower();
                    switch (extension)
                    {
                        case ".ps1":
                            ScriptType = "PowerShell";
                            break;
                        case ".bat":
                            ScriptType = "Batch";
                            break;
                        case ".exe":
                            ScriptType = "Executable";
                            break;
                        default:
                            ScriptType = "Unknown";
                            break;
                    }
                }
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (DialogResult == DialogResult.OK)
            {
                if (string.IsNullOrEmpty(ScriptPath))
                {
                    MessageBox.Show("Proszę wybrać plik skryptu.",
                        "Błąd",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    e.Cancel = true;
                    return;
                }

                switch (executionMomentComboBox.SelectedIndex)
                {
                    case 0:
                        ExecutionMoment = ExecutionMoment.SystemStartup;
                        break;
                    case 1:
                        ExecutionMoment = ExecutionMoment.UserLogon;
                        break;
                    case 2:
                        ExecutionMoment = ExecutionMoment.BeforeShutdown;
                        break;
                    case 3:
                        ExecutionMoment = ExecutionMoment.BeforeLogoff;
                        break;
                    default:
                        ExecutionMoment = ExecutionMoment.SystemStartup;
                        break;
                }

                RunAsAdminWithUserCredentials = adminRightsCheckBox.Checked;
            }
            base.OnClosing(e);
        }
    }
    public class MainForm : Form
    {
        private List<ScriptItem> scripts = new List<ScriptItem>();
        private ContextMenuStrip scriptContextMenu;

        public MainForm()
        {
            InitializeComponent();
            InitializeContextMenu();
            CheckAdminRights();
            LoadConfiguration();
            SynchronizeWithTaskScheduler();
        }

        private void InitializeContextMenu()
        {
            scriptContextMenu = new ContextMenuStrip();

            var editItem = new ToolStripMenuItem("Edytuj");
            editItem.Click += EditScript;

            var enableItem = new ToolStripMenuItem("Włącz/Wyłącz");
            enableItem.Click += ToggleScriptEnabled;

            var removeItem = new ToolStripMenuItem("Usuń");
            removeItem.Click += RemoveScript;

            var testItem = new ToolStripMenuItem("Testuj");
            testItem.Click += TestScript;

            scriptContextMenu.Items.AddRange(new ToolStripItem[]
            {
            editItem,
            enableItem,
            removeItem,
            testItem
            });

            ListView listView = (ListView)Controls.Find("scriptListView", true)[0];
            listView.ContextMenuStrip = scriptContextMenu;
        }

        private void InitializeComponent()
        {
            this.Text = "Menedżer skryptów systemowych";
            this.Size = new Size(1000, 600);

            Label infoLabel = new Label();
            infoLabel.Text = "Obsługiwane typy plików: .exe, .bat, .ps1";
            infoLabel.Dock = DockStyle.Top;
            infoLabel.Height = 30;
            infoLabel.Padding = new Padding(5);

            ListView scriptListView = new ListView();
            scriptListView.Dock = DockStyle.Fill;
            scriptListView.View = View.Details;
            scriptListView.FullRowSelect = true;
            scriptListView.GridLines = true;
            scriptListView.Name = "scriptListView";
            scriptListView.Columns.Add("Nazwa", 150);
            scriptListView.Columns.Add("Ścieżka", 250);
            scriptListView.Columns.Add("Typ", 100);
            scriptListView.Columns.Add("Moment wykonania", 150);
            scriptListView.Columns.Add("Uprawnienia", 150);
            scriptListView.Columns.Add("Stan", 100);

            Panel buttonPanel = new Panel();
            buttonPanel.Dock = DockStyle.Right;
            buttonPanel.Width = 150;

            Button addButton = new Button();
            addButton.Text = "Dodaj skrypt";
            addButton.Location = new Point(10, 10);
            addButton.Width = 130;
            addButton.Click += AddScript;

            Button removeButton = new Button();
            removeButton.Text = "Usuń skrypt";
            removeButton.Location = new Point(10, 40);
            removeButton.Width = 130;
            removeButton.Click += RemoveScript;

            Button testButton = new Button();
            testButton.Text = "Testuj skrypt";
            testButton.Location = new Point(10, 70);
            testButton.Width = 130;
            testButton.Click += TestScript;

            Button editButton = new Button();
            editButton.Text = "Edytuj skrypt";
            editButton.Location = new Point(10, 100);
            editButton.Width = 130;
            editButton.Click += EditScript;

            buttonPanel.Controls.AddRange(new Control[]
            {
            addButton,
            removeButton,
            testButton,
            editButton
            });

            this.Controls.AddRange(new Control[]
            {
            infoLabel,
            scriptListView,
            buttonPanel
            });
        }

        private void EditScript(object sender, EventArgs e)
        {
            ListView listView = (ListView)Controls.Find("scriptListView", true)[0];
            if (listView.SelectedItems.Count > 0)
            {
                int index = listView.SelectedIndices[0];
                ScriptItem originalScript = scripts[index];
                ScriptItem scriptToEdit = originalScript.Clone();

                using (var dialog = new ScriptConfigurationDialog(scriptToEdit))
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            // Usuń stare zadanie
                            using (TaskService ts = new TaskService())
                            {
                                string oldTaskName = $"Script_{originalScript.Name}_{originalScript.ExecutionMoment}";
                                if (ts.RootFolder.AllTasks.Any(t => t.Name == oldTaskName))
                                {
                                    ts.RootFolder.DeleteTask(oldTaskName, false);
                                }
                            }

                            // Zaktualizuj skrypt
                            originalScript.ExecutionMoment = dialog.ExecutionMoment;
                            originalScript.RunAsAdminWithUserCredentials = dialog.RunAsAdminWithUserCredentials;

                            // Zapisz konfigurację i synchronizuj
                            SaveConfiguration();
                            SynchronizeWithTaskScheduler();
                            UpdateListView();

                            MessageBox.Show("Skrypt został zaktualizowany pomyślnie!");
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Błąd podczas aktualizacji skryptu: {ex.Message}",
                                "Błąd",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Proszę wybrać skrypt do edycji.");
            }
        }

        private void LoadConfiguration()
        {
            var config = Configuration.Load();
            scripts = config.Scripts;
            UpdateListView();
        }

        private void SaveConfiguration()
        {
            Configuration.Save(scripts);
        }

        private void SynchronizeWithTaskScheduler()
        {
            try
            {
                using (TaskService ts = new TaskService())
                {
                    // Usuń wszystkie zadania
                    foreach (Task task in ts.RootFolder.AllTasks)
                    {
                        if (task.Name.StartsWith("Script_"))
                        {
                            ts.RootFolder.DeleteTask(task.Name, false);
                        }
                    }

                    // Dodaj ponownie tylko włączone skrypty
                    foreach (var script in scripts.Where(s => s.IsEnabled))
                    {
                        CreateTaskForScript(script);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd podczas synchronizacji z Task Scheduler: {ex.Message}",
                    "Błąd",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void CreateTaskForScript(ScriptItem script)
        {
            using (TaskService ts = new TaskService())
            {
                TaskDefinition td = ts.NewTask();
                string scriptType = GetScriptType(script.Path);
                td.RegistrationInfo.Description = $"Skrypt ({scriptType})";

                switch (script.ExecutionMoment)
                {
                    case ExecutionMoment.SystemStartup:
                        td.Triggers.Add(new BootTrigger());
                        break;
                    case ExecutionMoment.UserLogon:
                        td.Triggers.Add(new LogonTrigger());
                        break;
                    case ExecutionMoment.BeforeShutdown:
                        var shutdownTrigger = new EventTrigger();
                        shutdownTrigger.Subscription = @"<QueryList>
                        <Query Id='0'>
                            <Select Path='System'>
                                *[System[Provider[@Name='User32'] and (EventID=1074)]]
                            </Select>
                        </Query>
                    </QueryList>";
                        td.Triggers.Add(shutdownTrigger);
                        break;
                    case ExecutionMoment.BeforeLogoff:
                        var logoffTrigger = new SessionStateChangeTrigger();
                        logoffTrigger.StateChange = TaskSessionStateChangeType.ConsoleDisconnect;
                        td.Triggers.Add(logoffTrigger);
                        break;
                }

                string scriptToRun = script.Path;
                if (script.RunAsAdminWithUserCredentials)
                {
                    scriptToRun = CreateElevatedScriptWrapper(script.Path, scriptType);
                }

                if (scriptType == "PowerShell" && !script.RunAsAdminWithUserCredentials)
                {
                    td.Actions.Add(new ExecAction("powershell.exe",
                        $"-ExecutionPolicy Bypass -NoProfile -WindowStyle Hidden -File \"{scriptToRun}\""));
                }
                else if (scriptType == "Batch" && !script.RunAsAdminWithUserCredentials)
                {
                    td.Actions.Add(new ExecAction("cmd.exe",
                        $"/c \"{scriptToRun}\""));
                }
                else
                {
                    td.Actions.Add(new ExecAction(scriptToRun));
                }

                td.Settings.AllowHardTerminate = true;
                td.Settings.StartWhenAvailable = true;
                td.Settings.RunOnlyIfNetworkAvailable = false;
                td.Settings.Priority = ProcessPriorityClass.Normal;
                td.Settings.DisallowStartIfOnBatteries = false;
                td.Settings.StopIfGoingOnBatteries = false;
                td.Settings.ExecutionTimeLimit = TimeSpan.FromMinutes(5);

                string taskName = $"Script_{script.Name}_{script.ExecutionMoment}";

                if (script.RunAsAdminWithUserCredentials)
                {
                    td.Principal.RunLevel = TaskRunLevel.Highest;
                    ts.RootFolder.RegisterTaskDefinition(
                        taskName,
                        td,
                        TaskCreation.CreateOrUpdate,
                        WindowsIdentity.GetCurrent().Name,
                        null,
                        TaskLogonType.InteractiveToken
                    );
                }
                else
                {
                    ts.RootFolder.RegisterTaskDefinition(
                        taskName,
                        td,
                        TaskCreation.CreateOrUpdate,
                        "SYSTEM",
                        null,
                        TaskLogonType.ServiceAccount
                    );
                }
            }
        }
        private void ToggleScriptEnabled(object sender, EventArgs e)
        {
            ListView listView = (ListView)Controls.Find("scriptListView", true)[0];
            if (listView.SelectedItems.Count > 0)
            {
                int index = listView.SelectedIndices[0];
                scripts[index].IsEnabled = !scripts[index].IsEnabled;

                SaveConfiguration();
                SynchronizeWithTaskScheduler();
                UpdateListView();
            }
        }

        private void AddScript(object sender, EventArgs e)
        {
            using (var dialog = new ScriptConfigurationDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var script = new ScriptItem
                        {
                            Name = Path.GetFileNameWithoutExtension(dialog.ScriptPath),
                            Path = dialog.ScriptPath,
                            ExecutionMoment = dialog.ExecutionMoment,
                            RunAsAdminWithUserCredentials = dialog.RunAsAdminWithUserCredentials,
                            IsEnabled = true
                        };

                        scripts.Add(script);
                        SaveConfiguration();
                        SynchronizeWithTaskScheduler();
                        UpdateListView();

                        MessageBox.Show("Skrypt został dodany pomyślnie!");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Błąd podczas dodawania skryptu: {ex.Message}",
                            "Błąd",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void RemoveScript(object sender, EventArgs e)
        {
            ListView listView = (ListView)Controls.Find("scriptListView", true)[0];
            if (listView.SelectedItems.Count > 0)
            {
                if (MessageBox.Show("Czy na pewno chcesz usunąć wybrany skrypt?",
                    "Potwierdzenie",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    try
                    {
                        int index = listView.SelectedIndices[0];
                        ScriptItem script = scripts[index];

                        using (TaskService ts = new TaskService())
                        {
                            string taskName = $"Script_{script.Name}_{script.ExecutionMoment}";
                            if (ts.RootFolder.AllTasks.Any(t => t.Name == taskName))
                            {
                                ts.RootFolder.DeleteTask(taskName, false);
                            }
                        }

                        if (script.RunAsAdminWithUserCredentials)
                        {
                            string wrapperPath = Path.Combine(
                                Path.GetDirectoryName(script.Path),
                                "ScriptWrappers",
                                $"elevated_{Path.GetFileName(script.Path)}.cmd"
                            );
                            if (File.Exists(wrapperPath))
                            {
                                File.Delete(wrapperPath);
                            }
                        }

                        scripts.RemoveAt(index);
                        SaveConfiguration();
                        UpdateListView();

                        MessageBox.Show("Skrypt został usunięty pomyślnie!");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Błąd podczas usuwania skryptu: {ex.Message}",
                            "Błąd",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Proszę wybrać skrypt do usunięcia.");
            }
        }

        private void TestScript(object sender, EventArgs e)
        {
            ListView listView = (ListView)Controls.Find("scriptListView", true)[0];
            if (listView.SelectedItems.Count > 0)
            {
                int index = listView.SelectedIndices[0];
                ScriptItem script = scripts[index];
                string scriptType = GetScriptType(script.Path);

                try
                {
                    if (script.RunAsAdminWithUserCredentials)
                    {
                        string wrapperPath = CreateElevatedScriptWrapper(script.Path, scriptType);
                        Process.Start(wrapperPath);
                    }
                    else if (scriptType == "PowerShell")
                    {
                        ExecutePowerShellScript(script.Path);
                    }
                    else
                    {
                        Process.Start(script.Path);
                    }
                    MessageBox.Show("Skrypt został uruchomiony testowo.", "Test", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Błąd podczas testowania skryptu: {ex.Message}",
                        "Błąd",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Proszę wybrać skrypt do przetestowania.");
            }
        }

        private string CreateElevatedScriptWrapper(string originalScriptPath, string scriptType)
        {
            string wrapperDir = Path.Combine(
                Path.GetDirectoryName(originalScriptPath),
                "ScriptWrappers"
            );

            if (!Directory.Exists(wrapperDir))
                Directory.CreateDirectory(wrapperDir);

            string wrapperPath = Path.Combine(
                wrapperDir,
                $"elevated_{Path.GetFileName(originalScriptPath)}.cmd"
            );

            string wrapperContent;
            if (scriptType == "PowerShell")
            {
                wrapperContent = $@"@echo off
powershell.exe -Command ""Start-Process powershell.exe -ArgumentList '-ExecutionPolicy Bypass -NoProfile -File \""{originalScriptPath}\"" ' -Verb RunAs -Wait""";
            }
            else if (scriptType == "Batch")
            {
                wrapperContent = $@"@echo off
powershell.exe -Command ""Start-Process cmd.exe -ArgumentList '/c \""{originalScriptPath}\"" ' -Verb RunAs -Wait""";
            }
            else
            {
                wrapperContent = $@"@echo off
powershell.exe -Command ""Start-Process \""{originalScriptPath}\"" -Verb RunAs -Wait""";
            }

            File.WriteAllText(wrapperPath, wrapperContent);
            return wrapperPath;
        }

        private void ExecutePowerShellScript(string scriptPath)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "powershell.exe";
            startInfo.Arguments = $"-ExecutionPolicy Bypass -NoProfile -File \"{scriptPath}\"";
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.CreateNoWindow = true;

            using (Process process = new Process())
            {
                process.StartInfo = startInfo;
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (!string.IsNullOrEmpty(error))
                {
                    throw new Exception($"Błąd podczas wykonywania skryptu PowerShell: {error}");
                }
            }
        }

        private string GetScriptType(string path)
        {
            string extension = Path.GetExtension(path).ToLower();
            switch (extension)
            {
                case ".ps1":
                    return "PowerShell";
                case ".bat":
                    return "Batch";
                case ".exe":
                    return "Executable";
                default:
                    return "Unknown";
            }
        }

        private void UpdateListView()
        {
            ListView listView = (ListView)Controls.Find("scriptListView", true)[0];
            listView.Items.Clear();

            foreach (var script in scripts)
            {
                string executionMoment = GetExecutionMomentDescription(script.ExecutionMoment);

                ListViewItem item = new ListViewItem(new[]
                {
                script.Name,
                script.Path,
                GetScriptType(script.Path),
                executionMoment,
                script.RunAsAdminWithUserCredentials ? "Administrator (użytkownik)" : "Standardowe",
                script.IsEnabled ? "Włączony" : "Wyłączony"
            });

                if (!script.IsEnabled)
                {
                    item.ForeColor = Color.Gray;
                }

                listView.Items.Add(item);
            }
        }

        private string GetExecutionMomentDescription(ExecutionMoment moment)
        {
            switch (moment)
            {
                case ExecutionMoment.SystemStartup:
                    return "Przy starcie systemu";
                case ExecutionMoment.UserLogon:
                    return "Przy logowaniu użytkownika";
                case ExecutionMoment.BeforeShutdown:
                    return "Przed wyłączeniem systemu";
                case ExecutionMoment.BeforeLogoff:
                    return "Przed wylogowaniem użytkownika";
                default:
                    return "Nieznany";
            }
        }

        private void CheckAdminRights()
        {
            bool isAdmin = new WindowsPrincipal(WindowsIdentity.GetCurrent())
                .IsInRole(WindowsBuiltInRole.Administrator);

            if (!isAdmin)
            {
                MessageBox.Show("Aplikacja wymaga uprawnień administratora!",
                    "Błąd uprawnień",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                Application.Exit();
            }
        }
    }

    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}

