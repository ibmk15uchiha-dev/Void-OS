using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace VoidOS {
    public class Program {
        public static bool SoundEnabled = true;

        [STAThread]
        public static void Main(string[] args) {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new DesktopForm(args.Length > 0 ? args[0] : "Dark"));
        }
        
        public static void PlayBeep(int freq, int dur) {
            if (SoundEnabled) {
                Task.Run(() => Console.Beep(freq, dur));
            }
        }
    }

    public class VoidColorTable : ProfessionalColorTable {
        private Color bg, text, highlight;
        public VoidColorTable(string theme) {
            if (theme == "Dark") { bg = Color.FromArgb(40,40,40); text = Color.White; highlight = Color.FromArgb(70,70,70); }
            else if (theme == "Cyber") { bg = Color.FromArgb(20,0,40); text = Color.Cyan; highlight = Color.FromArgb(255,0,150); }
            else { bg = Color.FromArgb(60,15,45); text = Color.White; highlight = Color.FromArgb(233,84,32); }
        }
        public override Color MenuItemSelected { get { return highlight; } }
        public override Color MenuItemSelectedGradientBegin { get { return highlight; } }
        public override Color MenuItemSelectedGradientEnd { get { return highlight; } }
        public override Color MenuItemPressedGradientBegin { get { return highlight; } }
        public override Color MenuItemPressedGradientEnd { get { return highlight; } }
        public override Color ToolStripDropDownBackground { get { return bg; } }
        public override Color ImageMarginGradientBegin { get { return bg; } }
        public override Color ImageMarginGradientMiddle { get { return bg; } }
        public override Color ImageMarginGradientEnd { get { return bg; } }
        public override Color MenuBorder { get { return highlight; } }
    }

    public static class MenuBuilder {
        public static ContextMenuStrip Create(string theme) {
            ContextMenuStrip strip = new ContextMenuStrip();
            strip.Renderer = new ToolStripProfessionalRenderer(new VoidColorTable(theme));
            strip.ForeColor = (theme == "Cyber") ? Color.Cyan : Color.White;
            strip.Font = new Font("Segoe UI", 10);
            return strip;
        }
    }

    public static class InputDialog {
        public static string Show(string prompt, string title, string defaultValue = "") {
            Form f = new Form() { Width = 400, Height = 150, FormBorderStyle = FormBorderStyle.FixedDialog, Text = title, StartPosition = FormStartPosition.CenterScreen, ControlBox = false };
            Label l = new Label() { Left = 20, Top = 20, Text = prompt, AutoSize = true };
            TextBox t = new TextBox() { Left = 20, Top = 45, Width = 340, Text = defaultValue };
            Button bOK = new Button() { Text = "OK", Left = 150, Top = 75, Width = 100, DialogResult = DialogResult.OK };
            Button bCancel = new Button() { Text = "Cancel", Left = 260, Top = 75, Width = 100, DialogResult = DialogResult.Cancel };
            f.Controls.Add(l); f.Controls.Add(t); f.Controls.Add(bOK); f.Controls.Add(bCancel);
            f.AcceptButton = bOK;
            f.CancelButton = bCancel;
            return f.ShowDialog() == DialogResult.OK ? t.Text : null;
        }
    }

    public class NotepadForm : Form {
        private TextBox editor;
        private string filePath;

        public NotepadForm(string path, string theme) {
            filePath = path;
            this.Text = "Void Notepad - " + Path.GetFileName(path);
            this.Width = 600; this.Height = 400;
            this.StartPosition = FormStartPosition.CenterScreen;
            
            string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "void.ico");
            if (File.Exists(iconPath)) this.Icon = new Icon(iconPath);
            this.ShowInTaskbar = false;
            this.Load += (s, e) => { if (DesktopForm.Instance != null) DesktopForm.Instance.RegisterWindow(this, "Notepad"); };

            editor = new TextBox() { Multiline = true, Dock = DockStyle.Fill, ScrollBars = ScrollBars.Both, Font = new Font("Consolas", 11) };
            if (File.Exists(path)) editor.Text = File.ReadAllText(path);
            this.Controls.Add(editor);

            if (theme == "Dark") {
                this.BackColor = Color.FromArgb(30, 30, 30); editor.BackColor = Color.FromArgb(40, 40, 40); editor.ForeColor = Color.White;
            } else if (theme == "Cyber") {
                this.BackColor = Color.FromArgb(10, 0, 20); editor.BackColor = Color.FromArgb(20, 0, 40); editor.ForeColor = Color.Cyan;
            } else {
                this.BackColor = Color.FromArgb(48, 10, 36); editor.BackColor = Color.FromArgb(60, 15, 45); editor.ForeColor = Color.White;
            }

            this.FormClosing += (s, e) => {
                try { File.WriteAllText(filePath, editor.Text); } catch { }
            };
        }
    }

    public class ExplorerForm : Form {
        private ListView fileList;
        private TextBox pathBox;
        private Button upButton;
        private string currentPath;
        private string theme;

        public ExplorerForm(string startPath, string theme) {
            this.theme = theme;
            currentPath = startPath;
            this.Text = "Void Explorer";
            this.Width = 700; this.Height = 500;
            this.StartPosition = FormStartPosition.CenterScreen;
            
            string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "void.ico");
            if (File.Exists(iconPath)) this.Icon = new Icon(iconPath);
            this.ShowInTaskbar = false;
            this.Load += (s, e) => { if (DesktopForm.Instance != null) DesktopForm.Instance.RegisterWindow(this, "Explorer"); };

            Panel topPanel = new Panel() { Dock = DockStyle.Top, Height = 35 };
            this.Controls.Add(topPanel);

            upButton = new Button() { Text = "↑", Width = 35, Dock = DockStyle.Left, FlatStyle = FlatStyle.Flat };
            upButton.FlatAppearance.BorderSize = 0;
            upButton.Click += (s, e) => {
                DirectoryInfo parent = Directory.GetParent(currentPath);
                if (parent != null) { currentPath = parent.FullName; LoadPath(); }
            };
            topPanel.Controls.Add(upButton);

            pathBox = new TextBox() { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 11), ReadOnly = true, BorderStyle = BorderStyle.None };
            topPanel.Controls.Add(pathBox);
            pathBox.BringToFront();

            fileList = new ListView() { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, Font = new Font("Segoe UI", 10), BorderStyle = BorderStyle.None };
            fileList.Columns.Add("Name", 300);
            fileList.Columns.Add("Type", 100);
            fileList.Columns.Add("Size", 100);
            fileList.DoubleClick += FileList_DoubleClick;
            fileList.MouseUp += FileList_MouseUp;
            this.Controls.Add(fileList);
            fileList.BringToFront();

            if (theme == "Dark") {
                this.BackColor = Color.Black; topPanel.BackColor = Color.FromArgb(30,30,30); pathBox.BackColor = Color.FromArgb(40,40,40); pathBox.ForeColor = Color.White;
                upButton.BackColor = Color.FromArgb(50,50,50); upButton.ForeColor = Color.White;
                fileList.BackColor = Color.FromArgb(20,20,20); fileList.ForeColor = Color.White;
            } else if (theme == "Cyber") {
                this.BackColor = Color.FromArgb(10,0,20); topPanel.BackColor = Color.FromArgb(20,0,40); pathBox.BackColor = Color.FromArgb(30,0,50); pathBox.ForeColor = Color.Cyan;
                upButton.BackColor = Color.FromArgb(255,0,150); upButton.ForeColor = Color.Yellow;
                fileList.BackColor = Color.FromArgb(10,0,30); fileList.ForeColor = Color.Magenta;
            } else {
                this.BackColor = Color.FromArgb(48,10,36); topPanel.BackColor = Color.FromArgb(60,15,45); pathBox.BackColor = Color.FromArgb(70,20,50); pathBox.ForeColor = Color.White;
                upButton.BackColor = Color.FromArgb(233,84,32); upButton.ForeColor = Color.White;
                fileList.BackColor = Color.FromArgb(48,10,36); fileList.ForeColor = Color.White;
            }

            LoadPath();
        }

        private void LoadPath() {
            try {
                fileList.Items.Clear();
                pathBox.Text = currentPath;
                foreach (string d in Directory.GetDirectories(currentPath)) {
                    ListViewItem item = new ListViewItem(new DirectoryInfo(d).Name);
                    item.SubItems.Add("Folder"); item.SubItems.Add(""); item.Tag = d;
                    fileList.Items.Add(item);
                }
                foreach (string f in Directory.GetFiles(currentPath)) {
                    FileInfo fi = new FileInfo(f);
                    ListViewItem item = new ListViewItem(fi.Name);
                    item.SubItems.Add(fi.Extension); item.SubItems.Add((fi.Length / 1024) + " KB"); item.Tag = f;
                    fileList.Items.Add(item);
                }
            } catch (Exception ex) { MessageBox.Show("Error accessing path: " + ex.Message); }
        }

        private void FileList_DoubleClick(object sender, EventArgs e) {
            if (fileList.SelectedItems.Count > 0) {
                string path = fileList.SelectedItems[0].Tag.ToString();
                if (Directory.Exists(path)) { currentPath = path; LoadPath(); }
                else if (File.Exists(path)) {
                    if (path.EndsWith(".txt")) new NotepadForm(path, theme).Show();
                    else Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
                }
            }
        }

        private void FileList_MouseUp(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Right && fileList.SelectedItems.Count > 0) {
                string path = fileList.SelectedItems[0].Tag.ToString();
                bool isDir = Directory.Exists(path);
                ContextMenuStrip ctx = MenuBuilder.Create(theme);
                ctx.Items.Add("Rename", null, (s, ev) => {
                    string newName = InputDialog.Show("New name:", "Rename", Path.GetFileName(path));
                    if (!string.IsNullOrEmpty(newName)) {
                        string newPath = Path.Combine(currentPath, newName);
                        if (isDir) Directory.Move(path, newPath); else File.Move(path, newPath); LoadPath();
                    }
                });
                ctx.Items.Add("Move", null, (s, ev) => {
                    string dest = InputDialog.Show("Destination absolute path:", "Move");
                    if (!string.IsNullOrEmpty(dest)) {
                        try { if (isDir) Directory.Move(path, Path.Combine(dest, Path.GetFileName(path))); else File.Move(path, Path.Combine(dest, Path.GetFileName(path))); LoadPath(); }
                        catch (Exception ex) { MessageBox.Show(ex.Message); }
                    }
                });
                ctx.Items.Add("Delete", null, (s, ev) => {
                    if (MessageBox.Show("Delete this item?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes) {
                        if (isDir) Directory.Delete(path, true); else File.Delete(path); LoadPath();
                    }
                });
                ctx.Items.Add(new ToolStripSeparator());
                ctx.Items.Add("Properties", null, (s, ev) => {
                    if (isDir) MessageBox.Show(string.Format("Folder: {0}\nPath: {1}\nCreated: {2}", new DirectoryInfo(path).Name, path, Directory.GetCreationTime(path)), "Properties");
                    else { FileInfo fi = new FileInfo(path); MessageBox.Show(string.Format("File: {0}\nSize: {1} bytes\nPath: {2}\nCreated: {3}", fi.Name, fi.Length, path, fi.CreationTime), "Properties"); }
                });
                ctx.Items.Add("Copy Path", null, (s, ev) => { Clipboard.SetText(path); });
                ctx.Show(fileList, e.Location);
            } else if (e.Button == MouseButtons.Right) {
                ContextMenuStrip ctx = MenuBuilder.Create(theme);
                ctx.Items.Add("Open Terminal Here", null, (s, ev) => { new TerminalForm(theme, currentPath).Show(); });
                ctx.Items.Add(new ToolStripSeparator());
                ctx.Items.Add("New Folder", null, (s, ev) => {
                    string name = InputDialog.Show("Folder name:", "New Folder");
                    if (!string.IsNullOrEmpty(name)) { Directory.CreateDirectory(Path.Combine(currentPath, name)); LoadPath(); }
                });
                ctx.Items.Add("New Text File", null, (s, ev) => {
                    string name = InputDialog.Show("File name (no .txt):", "New File");
                    if (!string.IsNullOrEmpty(name)) { File.WriteAllText(Path.Combine(currentPath, name + ".txt"), ""); LoadPath(); }
                });
                ctx.Items.Add("-");
                ctx.Items.Add("Refresh", null, (s, ev) => LoadPath());
                ctx.Show(fileList, e.Location);
            }
        }
    }

    public class DesktopForm : Form {
        public static DesktopForm Instance;
        private Panel taskbar;
        private FlowLayoutPanel windowManagerPanel;
        private Button startButton;
        private Label clockLabel;
        private Timer clockTimer;
        private string currentTheme;
        private ContextMenuStrip startMenu;
        private Panel desktopIconsPanel;
        private string desktopPath;

        public DesktopForm(string theme) {
            Instance = this;
            currentTheme = theme;
            this.Text = "Void OS";
            this.WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = FormBorderStyle.None;
            this.DoubleBuffered = true;
            
            string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "void.ico");
            if (File.Exists(iconPath)) this.Icon = new Icon(iconPath);

            desktopPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Desktop");
            if (!Directory.Exists(desktopPath)) Directory.CreateDirectory(desktopPath);

            taskbar = new Panel() { Height = 45, Dock = DockStyle.Bottom };
            this.Controls.Add(taskbar);

            startButton = new Button() { Text = "❖", Width = 60, Height = 45, Dock = DockStyle.Left, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 20, FontStyle.Bold) };
            startButton.FlatAppearance.BorderSize = 0;
            startButton.Click += StartButton_Click;
            taskbar.Controls.Add(startButton);
            
            windowManagerPanel = new FlowLayoutPanel() { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, Padding = new Padding(10,5,0,0) };
            taskbar.Controls.Add(windowManagerPanel);
            windowManagerPanel.BringToFront();

            startMenu = MenuBuilder.Create(currentTheme);
            startMenu.Items.Add("Terminal", null, (s, args) => { Program.PlayBeep(1500, 100); new TerminalForm(currentTheme).Show(); });
            startMenu.Items.Add("Virtual Explorer", null, (s, args) => { Program.PlayBeep(1200, 100); new ExplorerForm(desktopPath, currentTheme).Show(); });
            startMenu.Items.Add(new ToolStripSeparator());
            startMenu.Items.Add("Toggle Sound", null, (s, args) => { 
                Program.SoundEnabled = !Program.SoundEnabled;
                MessageBox.Show("Sound Effects: " + (Program.SoundEnabled ? "ON" : "OFF"), "Settings");
                Program.PlayBeep(2000, 100);
            });
            startMenu.Items.Add(new ToolStripSeparator());
            startMenu.Items.Add("Shut Down", null, (s, args) => { 
                if (Program.SoundEnabled) { Console.Beep(1000, 150); Console.Beep(800, 150); Console.Beep(600, 200); }
                Application.Exit();
            });

            clockLabel = new Label() { AutoSize = false, Width = 120, Height = 45, Dock = DockStyle.Right, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 11) };
            taskbar.Controls.Add(clockLabel);
            clockTimer = new Timer() { Interval = 1000 };
            clockTimer.Tick += (s, e) => { clockLabel.Text = DateTime.Now.ToString("hh:mm tt"); };
            clockTimer.Start();

            desktopIconsPanel = new Panel() { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            this.Controls.Add(desktopIconsPanel);

            ContextMenuStrip desktopMenu = MenuBuilder.Create(currentTheme);
            desktopMenu.Items.Add("Open Terminal Here", null, (s, e) => { new TerminalForm(currentTheme, desktopPath).Show(); });
            desktopMenu.Items.Add(new ToolStripSeparator());
            desktopMenu.Items.Add("New Text File", null, (s, e) => {
                string name = InputDialog.Show("Enter file name (without .txt):", "New File");
                if (!string.IsNullOrEmpty(name)) { File.WriteAllText(Path.Combine(desktopPath, name + ".txt"), ""); LoadDesktopIcons(); }
            });
            desktopMenu.Items.Add("New Folder", null, (s, e) => {
                string name = InputDialog.Show("Enter folder name:", "New Folder");
                if (!string.IsNullOrEmpty(name)) { Directory.CreateDirectory(Path.Combine(desktopPath, name)); LoadDesktopIcons(); }
            });
            desktopMenu.Items.Add("-");
            desktopMenu.Items.Add("Refresh", null, (s, e) => LoadDesktopIcons());
            desktopIconsPanel.ContextMenuStrip = desktopMenu;

            ApplyTheme();
            LoadDesktopIcons();

            this.Load += (s, e) => { 
                if (Program.SoundEnabled) { Task.Run(() => { Console.Beep(800, 150); Console.Beep(1200, 150); Console.Beep(1600, 200); }); }
            };
        }

        public void RegisterWindow(Form f, string title) {
            Button b = new Button() { 
                Text = title, Width = 140, Height = 35, 
                FlatStyle = FlatStyle.Flat, 
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Margin = new Padding(10, 5, 0, 0),
                Cursor = Cursors.Hand
            };
            b.FlatAppearance.BorderSize = 1;
            
            if (currentTheme == "Dark") { 
                b.BackColor = Color.FromArgb(40,40,40); b.ForeColor = Color.White; 
                b.FlatAppearance.BorderColor = Color.FromArgb(80,80,80);
            }
            else if (currentTheme == "Cyber") { 
                b.BackColor = Color.FromArgb(20,0,40); b.ForeColor = Color.Cyan; 
                b.FlatAppearance.BorderColor = Color.FromArgb(255,0,150);
            }
            else { 
                b.BackColor = Color.FromArgb(60,15,45); b.ForeColor = Color.White; 
                b.FlatAppearance.BorderColor = Color.FromArgb(233,84,32);
            }
            
            b.Click += (s, e) => {
                if (!f.Visible || f.WindowState == FormWindowState.Minimized) {
                    f.Visible = true;
                    f.WindowState = FormWindowState.Normal;
                    f.BringToFront();
                } else {
                    f.Visible = false;
                }
            };
            
            f.Resize += (s, e) => {
                if (f.WindowState == FormWindowState.Minimized) {
                    f.WindowState = FormWindowState.Normal;
                    f.Visible = false;
                }
            };
            
            f.FormClosed += (s, e) => {
                windowManagerPanel.Controls.Remove(b);
                b.Dispose();
            };
            windowManagerPanel.Controls.Add(b);
        }

        private void LoadDesktopIcons() {
            desktopIconsPanel.Controls.Clear();
            int x = 30; int y = 30;

            AddDesktopIcon(">_", "Terminal", x, y, (s, e) => { Program.PlayBeep(1500, 100); new TerminalForm(currentTheme).Show(); }, null);
            y += 110;

            foreach (string dir in Directory.GetDirectories(desktopPath)) {
                if (y > this.Height - 150) { y = 30; x += 100; }
                string name = new DirectoryInfo(dir).Name;
                AddDesktopIcon("[]", name, x, y, (s, e) => { Program.PlayBeep(1200, 100); new ExplorerForm(dir, currentTheme).Show(); }, dir, true);
                y += 110;
            }

            foreach (string file in Directory.GetFiles(desktopPath)) {
                if (y > this.Height - 150) { y = 30; x += 100; }
                string name = Path.GetFileName(file);
                AddDesktopIcon("==", name, x, y, (s, e) => { 
                        Program.PlayBeep(1200, 100); 
                        if (name.EndsWith(".txt")) new NotepadForm(file, currentTheme).Show();
                        else Process.Start(new ProcessStartInfo(file) { UseShellExecute = true });
                    }, file, false);
                y += 110;
            }
        }

        private void AddDesktopIcon(string symbol, string label, int x, int y, EventHandler onClick, string path, bool isDir = false) {
            Button icon = new Button() { Text = symbol, Width = 60, Height = 60, Location = new Point(x, y), FlatStyle = FlatStyle.Flat, Font = new Font("Consolas", 18, FontStyle.Bold) };
            icon.FlatAppearance.BorderSize = 0;
            icon.Click += onClick;
            
            Label lbl = new Label() { Text = label, AutoSize = true, Location = new Point(x, y + 65), Font = new Font("Segoe UI", 9, FontStyle.Bold), BackColor = Color.Transparent };
            lbl.MaximumSize = new Size(80, 0);

            if (currentTheme == "Dark") { icon.BackColor = Color.FromArgb(40, 40, 40); icon.ForeColor = Color.LightGray; lbl.ForeColor = Color.White; }
            else if (currentTheme == "Cyber") { icon.BackColor = Color.FromArgb(30, 0, 50); icon.ForeColor = Color.Yellow; lbl.ForeColor = Color.Cyan; }
            else { icon.BackColor = Color.FromArgb(50, 50, 50); icon.ForeColor = Color.White; lbl.ForeColor = Color.White; }

            if (path != null) {
                ContextMenuStrip ctx = MenuBuilder.Create(currentTheme);
                ctx.Items.Add("Open", null, onClick);
                ctx.Items.Add("Rename", null, (s, e) => {
                    string newName = InputDialog.Show("New name:", "Rename", Path.GetFileName(path));
                    if (!string.IsNullOrEmpty(newName)) {
                        string newPath = Path.Combine(desktopPath, newName);
                        if (isDir) Directory.Move(path, newPath); else File.Move(path, newPath);
                        LoadDesktopIcons();
                    }
                });
                ctx.Items.Add("Move", null, (s, e) => {
                    string dest = InputDialog.Show("Destination absolute path:", "Move");
                    if (!string.IsNullOrEmpty(dest)) {
                        try { if (isDir) Directory.Move(path, Path.Combine(dest, Path.GetFileName(path))); else File.Move(path, Path.Combine(dest, Path.GetFileName(path))); LoadDesktopIcons(); }
                        catch (Exception ex) { MessageBox.Show(ex.Message); }
                    }
                });
                ctx.Items.Add("Delete", null, (s, e) => {
                    if (MessageBox.Show("Delete " + label + "?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes) {
                        if (isDir) Directory.Delete(path, true); else File.Delete(path);
                        LoadDesktopIcons();
                    }
                });
                ctx.Items.Add(new ToolStripSeparator());
                ctx.Items.Add("Properties", null, (s, ev) => {
                    if (isDir) MessageBox.Show(string.Format("Folder: {0}\nPath: {1}\nCreated: {2}", new DirectoryInfo(path).Name, path, Directory.GetCreationTime(path)), "Properties");
                    else { FileInfo fi = new FileInfo(path); MessageBox.Show(string.Format("File: {0}\nSize: {1} bytes\nPath: {2}\nCreated: {3}", fi.Name, fi.Length, path, fi.CreationTime), "Properties"); }
                });
                ctx.Items.Add("Copy Path", null, (s, ev) => { Clipboard.SetText(path); });
                icon.ContextMenuStrip = ctx;
            }

            desktopIconsPanel.Controls.Add(icon);
            desktopIconsPanel.Controls.Add(lbl);
        }

        private void StartButton_Click(object sender, EventArgs e) {
            Program.PlayBeep(1800, 50);
            startMenu.Show(startButton, new Point(0, -startMenu.Items.Count * 25));
        }

        private void ApplyTheme() {
            if (currentTheme == "Dark") { taskbar.BackColor = Color.FromArgb(30, 30, 30); startButton.BackColor = Color.FromArgb(50, 50, 50); startButton.ForeColor = Color.White; clockLabel.ForeColor = Color.White; windowManagerPanel.BackColor = Color.FromArgb(30,30,30); }
            else if (currentTheme == "Cyber") { taskbar.BackColor = Color.FromArgb(15, 0, 30); startButton.BackColor = Color.FromArgb(255, 0, 150); startButton.ForeColor = Color.Cyan; clockLabel.ForeColor = Color.Cyan; windowManagerPanel.BackColor = Color.FromArgb(15,0,30); }
            else { taskbar.BackColor = Color.FromArgb(44, 44, 44); startButton.BackColor = Color.FromArgb(233, 84, 32); startButton.ForeColor = Color.White; clockLabel.ForeColor = Color.White; windowManagerPanel.BackColor = Color.FromArgb(44,44,44); }
        }

        protected override void OnPaintBackground(PaintEventArgs e) {
            base.OnPaintBackground(e);
            Graphics g = e.Graphics;
            Rectangle rect = this.ClientRectangle;
            if (currentTheme == "Dark") { using (LinearGradientBrush b = new LinearGradientBrush(rect, Color.FromArgb(45, 45, 48), Color.FromArgb(20, 20, 22), 45F)) { g.FillRectangle(b, rect); } }
            else if (currentTheme == "Cyber") { 
                using (LinearGradientBrush b = new LinearGradientBrush(rect, Color.FromArgb(20, 0, 40), Color.FromArgb(0, 20, 40), 90F)) { g.FillRectangle(b, rect); }
                using (Pen gridPen = new Pen(Color.FromArgb(30, 0, 255, 255), 1)) {
                    for (int i = 0; i < rect.Width; i += 40) g.DrawLine(gridPen, i, 0, i, rect.Height);
                    for (int i = 0; i < rect.Height; i += 40) g.DrawLine(gridPen, 0, i, rect.Width, i);
                }
            }
            else { using (LinearGradientBrush b = new LinearGradientBrush(rect, Color.FromArgb(70, 40, 60), Color.FromArgb(40, 20, 30), 120F)) { g.FillRectangle(b, rect); } }
        }
    }

    public class TerminalForm : Form {
        private RichTextBox outputBox;
        private TextBox inputBox;
        private Label promptLabel;
        private string currentDir = AppDomain.CurrentDomain.BaseDirectory;

        public TerminalForm(string theme, string startDir = null) {
            if (startDir != null && Directory.Exists(startDir)) currentDir = startDir;
            this.Text = "Void Terminal";
            this.Width = 800; this.Height = 500;
            this.StartPosition = FormStartPosition.CenterScreen;
            
            string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "void.ico");
            if (File.Exists(iconPath)) this.Icon = new Icon(iconPath);
            this.ShowInTaskbar = false;
            this.Load += (s, e) => { if (DesktopForm.Instance != null) DesktopForm.Instance.RegisterWindow(this, "Terminal"); };

            TableLayoutPanel bottomLayout = new TableLayoutPanel() { Dock = DockStyle.Bottom, AutoSize = true, ColumnCount = 2, RowCount = 1 };
            bottomLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            bottomLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            this.Controls.Add(bottomLayout);

            promptLabel = new Label() { Text = "hima@void:~$ ", AutoSize = true, Font = new Font("Consolas", 14), Anchor = AnchorStyles.Left };
            bottomLayout.Controls.Add(promptLabel, 0, 0);

            inputBox = new TextBox() { Dock = DockStyle.Fill, Font = new Font("Consolas", 14), BorderStyle = BorderStyle.None };
            inputBox.KeyDown += InputBox_KeyDown;
            bottomLayout.Controls.Add(inputBox, 1, 0);

            outputBox = new RichTextBox() { ReadOnly = true, Dock = DockStyle.Fill, ScrollBars = RichTextBoxScrollBars.Vertical, Font = new Font("Consolas", 12), BorderStyle = BorderStyle.None };
            this.Controls.Add(outputBox);
            outputBox.BringToFront();
            
            outputBox.Click += (s, e) => inputBox.Focus();
            this.Click += (s, e) => inputBox.Focus();

            if (theme == "Dark") {
                this.BackColor = Color.Black;
                outputBox.BackColor = Color.Black; outputBox.ForeColor = Color.LightGray;
                bottomLayout.BackColor = Color.Black; promptLabel.ForeColor = Color.Lime;
                inputBox.BackColor = Color.Black; inputBox.ForeColor = Color.White;
            } else if (theme == "Cyber") {
                this.BackColor = Color.FromArgb(10, 0, 20);
                outputBox.BackColor = Color.FromArgb(10, 0, 20); outputBox.ForeColor = Color.Cyan;
                bottomLayout.BackColor = Color.FromArgb(10, 0, 20); promptLabel.ForeColor = Color.Yellow;
                inputBox.BackColor = Color.FromArgb(10, 0, 20); inputBox.ForeColor = Color.Magenta;
            } else { 
                this.BackColor = Color.FromArgb(48, 10, 36);
                outputBox.BackColor = Color.FromArgb(48, 10, 36); outputBox.ForeColor = Color.White;
                bottomLayout.BackColor = Color.FromArgb(48, 10, 36); promptLabel.ForeColor = Color.Lime;
                inputBox.BackColor = Color.FromArgb(48, 10, 36); inputBox.ForeColor = Color.White;
            }

            AppendColoredText("Void OS Terminal (GUI Edition)\r\n", Color.Yellow);
            AppendColoredText("System ready. Welcome hima.\r\n", Color.Cyan);
            AppendColoredText("Type '/esscmds' for a list of essential commands.\r\n\r\n", Color.Gray);
            this.Shown += (s, e) => inputBox.Focus();
        }
        
        private void AppendColoredText(string text, Color color) {
            outputBox.SelectionStart = outputBox.TextLength;
            outputBox.SelectionLength = 0;
            outputBox.SelectionColor = color;
            outputBox.AppendText(text);
            outputBox.SelectionColor = outputBox.ForeColor;
        }

        private void InputBox_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Enter) {
                e.SuppressKeyPress = true;
                string cmd = inputBox.Text.Trim();
                inputBox.Text = "";
                
                AppendColoredText(promptLabel.Text, promptLabel.ForeColor);
                
                if (!string.IsNullOrEmpty(cmd)) {
                    string[] parts = cmd.Split(' ');
                    string command = parts[0].ToLower();
                    Color cmdColor = (command == "exit" || command == "error") ? Color.Red : Color.Cyan;
                    
                    AppendColoredText(parts[0], cmdColor);
                    if (cmd.Length > parts[0].Length) {
                        AppendColoredText(cmd.Substring(parts[0].Length), inputBox.ForeColor);
                    }
                    outputBox.AppendText("\r\n");
                    ExecuteCommand(cmd);
                } else {
                    outputBox.AppendText("\r\n");
                }
                
                outputBox.SelectionStart = outputBox.Text.Length;
                outputBox.ScrollToCaret();
            }
        }

               private async void ExecuteCommand(string cmd) {
            if (cmd.ToLower() == "clear") { outputBox.Text = ""; return; }
            if (cmd.ToLower() == "exit") { this.Close(); return; }
            
            if (cmd.ToLower() == "/esscmds") {
                AppendColoredText("--- Void OS Essential Commands ---\r\n", Color.Magenta);
                AppendColoredText(" /esscmds", Color.Cyan); outputBox.AppendText("  - Show this help menu\r\n");
                AppendColoredText(" clear", Color.Cyan); outputBox.AppendText("     - Clear terminal output\r\n");
                AppendColoredText(" cd <dir>", Color.Cyan); outputBox.AppendText("  - Change current directory\r\n");
                AppendColoredText(" dir", Color.Cyan); outputBox.AppendText("       - List files in current directory\r\n");
                AppendColoredText(" echo <txt>", Color.Cyan); outputBox.AppendText(" - Print text to screen\r\n");
                AppendColoredText(" time", Color.Cyan); outputBox.AppendText("      - Show system time\r\n");
                AppendColoredText(" date", Color.Cyan); outputBox.AppendText("      - Show system date\r\n");
                AppendColoredText(" exit", Color.Cyan); outputBox.AppendText("      - Close the terminal\r\n");
                return;
            }

            if (cmd.StartsWith("cd ")) {
                string target = cmd.Substring(3).Trim();
                try {
                    string newPath = Path.GetFullPath(Path.Combine(currentDir, target));
                    if (Directory.Exists(newPath)) currentDir = newPath;
                    else { AppendColoredText("cd: no such file or directory: " + target + "\r\n", Color.Red); Program.PlayBeep(300, 200); }
                } catch (Exception ex) { AppendColoredText(ex.Message + "\r\n", Color.Red); Program.PlayBeep(300, 200); }
                return;
            }

            if (cmd.ToLower() == "time" || cmd.ToLower() == "date") {
                cmd = cmd + " /t";
            }

            try {
                inputBox.Enabled = false;
                
                string result = await Task.Run(() => {
                    ProcessStartInfo psi = new ProcessStartInfo() { FileName = "cmd.exe", Arguments = "/c " + cmd, WorkingDirectory = currentDir, UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true };
                    Process p = new Process() { StartInfo = psi };
                    p.Start();
                    string res = p.StandardOutput.ReadToEnd() + p.StandardError.ReadToEnd();
                    p.WaitForExit(5000);
                    if (!p.HasExited) {
                        try { p.Kill(); } catch { }
                        res += "\n[Process timed out after 5 seconds]\n";
                    }
                    return res;
                });
                
                if (result != "") AppendColoredText(result, outputBox.ForeColor);
            } catch (Exception ex) {
                AppendColoredText("Error: " + ex.Message + "\r\n", Color.Red);
                Program.PlayBeep(300, 200);
            } finally {
                inputBox.Enabled = true;
                inputBox.Focus();
                outputBox.SelectionStart = outputBox.Text.Length;
                outputBox.ScrollToCaret();
            }
        }
    }
}
