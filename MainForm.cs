using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MusicFilesOrganizer
{
    public class MainForm : Form
    {
        // ---- Source / destination controls -------------------------------------------------
        private ListBox lstSources;
        private Button btnAddFolder;
        private Button btnAddFiles;
        private Button btnRemoveSelected;
        private Button btnClearAll;

        private TextBox txtDestination;
        private Button btnBrowseDestination;
        private CheckBox chkCopyInsteadOfMove;

        // ---- Folder structure token builder -------------------------------------------------
        private ListBox lstFolderTokens;
        private ComboBox cmbFolderPicker;
        private Button btnFolderAdd;
        private Button btnFolderRemoveTok;
        private Button btnFolderUp;
        private Button btnFolderDown;

        // ---- File naming token builder --------------------------------------------------------
        private ListBox lstNameTokens;
        private ComboBox cmbNamePicker;
        private Button btnNameAdd;
        private Button btnNameRemoveTok;
        private Button btnNameUp;
        private Button btnNameDown;
        private TextBox txtSeparator;

        // ---- Preview / progress / log ---------------------------------------------------------
        private TextBox txtPreview;
        private ProgressBar progressBar1;
        private TextBox txtLog;

        // ---- Bottom buttons ---------------------------------------------------------------------
        private Button btnOrganize;
        private Button btnAbout;
        private Button btnExit;

        private readonly List<string> _sourceFiles = new List<string>();
        private readonly List<Control> _busyControls = new List<Control>();

        public MainForm()
        {
            InitializeComponent();
            PopulateDefaults();
            UpdatePreview();
        }

        // =========================================================================================
        //  UI construction
        // =========================================================================================

        private void InitializeComponent()
        {
            Text = "Music Files Organizer";
            
            FormBorderStyle = FormBorderStyle.Sizable; 
            MaximizeBox = true; 
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 9F);
            
            ClientSize = new Size(940, 660); 
            MinimumSize = new Size(960, 700);

            // Load window title bar icon
            string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appicon.ico");
            if (File.Exists(iconPath))
            {
                this.Icon = new Icon(iconPath);
            }

            Controls.Add(BuildSourceGroup());
            Controls.Add(BuildDestinationGroup());

            // Use a TableLayoutPanel to equally split the space and stretch the two middle groups
            var tableLayoutPanel = new TableLayoutPanel
            {
                Location = new Point(12, 245),
                Size = new Size(916, 210),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                ColumnCount = 2,
                RowCount = 1
            };
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            var folderGroup = BuildFolderStructureGroup();
            var namingGroup = BuildFileNamingGroup();
            
            folderGroup.Dock = DockStyle.Fill;
            namingGroup.Dock = DockStyle.Fill;

            tableLayoutPanel.Controls.Add(folderGroup, 0, 0);
            tableLayoutPanel.Controls.Add(namingGroup, 1, 0);

            Controls.Add(tableLayoutPanel);

            Controls.Add(BuildPreviewGroup());
            Controls.Add(BuildLogGroup());
            BuildBottomButtons();
        }

        private GroupBox BuildSourceGroup()
        {
            var group = new GroupBox
            {
                Text = "Source Files / Folders",
                Location = new Point(12, 10),
                Size = new Size(916, 140),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            lstSources = new ListBox
            {
                Location = new Point(12, 22),
                Size = new Size(734, 85),
                SelectionMode = SelectionMode.MultiExtended,
                HorizontalScrollbar = true,
                AllowDrop = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            lstSources.DragEnter += FileDrag_DragEnter;
            lstSources.DragDrop += LstSources_DragDrop;

            var lblHint = new Label
            {
                Text = "Tip: you can drag & drop audio files or whole folders directly into the list above.",
                Location = new Point(12, 114),
                AutoSize = true,
                ForeColor = Color.DimGray,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };

            btnAddFolder = new Button { Text = "Add Folder...", Location = new Point(758, 22), Size = new Size(146, 25), Anchor = AnchorStyles.Top | AnchorStyles.Right };
            btnAddFolder.Click += BtnAddFolder_Click;

            btnAddFiles = new Button { Text = "Add Files...", Location = new Point(758, 50), Size = new Size(146, 25), Anchor = AnchorStyles.Top | AnchorStyles.Right };
            btnAddFiles.Click += BtnAddFiles_Click;

            btnRemoveSelected = new Button { Text = "Remove Selected", Location = new Point(758, 78), Size = new Size(146, 25), Anchor = AnchorStyles.Top | AnchorStyles.Right };
            btnRemoveSelected.Click += BtnRemoveSelected_Click;

            btnClearAll = new Button { Text = "Clear All", Location = new Point(758, 106), Size = new Size(146, 25), Anchor = AnchorStyles.Top | AnchorStyles.Right };
            btnClearAll.Click += BtnClearAll_Click;

            group.Controls.Add(lstSources);
            group.Controls.Add(lblHint);
            group.Controls.Add(btnAddFolder);
            group.Controls.Add(btnAddFiles);
            group.Controls.Add(btnRemoveSelected);
            group.Controls.Add(btnClearAll);

            _busyControls.AddRange(new Control[] { btnAddFolder, btnAddFiles, btnRemoveSelected, btnClearAll });

            return group;
        }

        private GroupBox BuildDestinationGroup()
        {
            var group = new GroupBox
            {
                Text = "Destination Folder",
                Location = new Point(12, 160),
                Size = new Size(916, 75),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            var tlpDest = new TableLayoutPanel
            {
                Location = new Point(12, 20),
                Size = new Size(892, 32),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                ColumnCount = 3,
                RowCount = 1
            };
            
            tlpDest.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpDest.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tlpDest.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            txtDestination = new TextBox
            {
                ReadOnly = true,
                AllowDrop = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Margin = new Padding(0, 4, 10, 0)
            };
            txtDestination.DragEnter += FileDrag_DragEnter;
            txtDestination.DragDrop += TxtDestination_DragDrop;

            btnBrowseDestination = new Button 
            { 
                Text = "Browse...", 
                Size = new Size(100, 26), 
                Margin = new Padding(0, 1, 10, 0),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnBrowseDestination.Click += BtnBrowseDestination_Click;

            chkCopyInsteadOfMove = new CheckBox
            {
                Text = "Copy (keep source)",
                AutoSize = true,
                Margin = new Padding(0, 5, 0, 0),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };

            tlpDest.Controls.Add(txtDestination, 0, 0);
            tlpDest.Controls.Add(btnBrowseDestination, 1, 0);
            tlpDest.Controls.Add(chkCopyInsteadOfMove, 2, 0);

            var lblHint = new Label
            {
                Text = "Tip: you can also drag & drop a folder here to set it as the destination.",
                Location = new Point(12, 52),
                AutoSize = true,
                ForeColor = Color.DimGray
            };

            group.Controls.Add(tlpDest);
            group.Controls.Add(lblHint);

            _busyControls.AddRange(new Control[] { btnBrowseDestination, chkCopyInsteadOfMove });

            return group;
        }

        private GroupBox BuildFolderStructureGroup()
        {
            var group = new GroupBox
            {
                Text = "Destination Folder Structure",
                Size = new Size(450, 210)
            };

            var lblInfo = new Label
            {
                Text = "Pick the tags that become nested sub-folders (top of the list = outermost folder). Combine as many as you like.",
                Location = new Point(12, 20),
                Size = new Size(426, 30),
                ForeColor = Color.DimGray,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            var lblChosen = new Label { Text = "Chosen order:", Location = new Point(12, 54), AutoSize = true };
            
            lstFolderTokens = new ListBox 
            { 
                Location = new Point(12, 70), 
                Size = new Size(200, 125),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right 
            };

            var lblAvailable = new Label { Text = "Available tag:", Location = new Point(224, 54), AutoSize = true, Anchor = AnchorStyles.Top | AnchorStyles.Right };
            cmbFolderPicker = new ComboBox
            {
                Location = new Point(224, 70),
                Size = new Size(214, 24),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            foreach (var token in new[] { MetadataToken.Artist, MetadataToken.AlbumArtist, MetadataToken.Album, MetadataToken.Year, MetadataToken.Genre })
            {
                cmbFolderPicker.Items.Add(new TokenItem(token));
            }
            cmbFolderPicker.SelectedIndex = 0;

            btnFolderAdd = new Button { Text = "Add >>", Location = new Point(224, 100), Size = new Size(102, 26), Anchor = AnchorStyles.Top | AnchorStyles.Right };
            btnFolderAdd.Click += (s, e) => { AddTokenToList(lstFolderTokens, cmbFolderPicker); UpdatePreview(); };

            btnFolderRemoveTok = new Button { Text = "Remove", Location = new Point(336, 100), Size = new Size(102, 26), Anchor = AnchorStyles.Top | AnchorStyles.Right };
            btnFolderRemoveTok.Click += (s, e) => { RemoveSelectedToken(lstFolderTokens); UpdatePreview(); };

            btnFolderUp = new Button { Text = "Move Up", Location = new Point(224, 132), Size = new Size(102, 26), Anchor = AnchorStyles.Top | AnchorStyles.Right };
            btnFolderUp.Click += (s, e) => { MoveListItem(lstFolderTokens, -1); UpdatePreview(); };

            btnFolderDown = new Button { Text = "Move Down", Location = new Point(336, 132), Size = new Size(102, 26), Anchor = AnchorStyles.Top | AnchorStyles.Right };
            btnFolderDown.Click += (s, e) => { MoveListItem(lstFolderTokens, 1); UpdatePreview(); };

            group.Controls.Add(lblInfo);
            group.Controls.Add(lblChosen);
            group.Controls.Add(lstFolderTokens);
            group.Controls.Add(lblAvailable);
            group.Controls.Add(cmbFolderPicker);
            group.Controls.Add(btnFolderAdd);
            group.Controls.Add(btnFolderRemoveTok);
            group.Controls.Add(btnFolderUp);
            group.Controls.Add(btnFolderDown);

            _busyControls.AddRange(new Control[] { cmbFolderPicker, btnFolderAdd, btnFolderRemoveTok, btnFolderUp, btnFolderDown });

            return group;
        }

        private GroupBox BuildFileNamingGroup()
        {
            var group = new GroupBox
            {
                Text = "Output File Naming",
                Size = new Size(450, 210)
            };

            var lblInfo = new Label
            {
                Text = "Pick the tags that build the file name (joined by the separator below). Combine as many as you like.",
                Location = new Point(12, 20),
                Size = new Size(426, 30),
                ForeColor = Color.DimGray,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            var lblChosen = new Label { Text = "Chosen order:", Location = new Point(12, 54), AutoSize = true };
            
            lstNameTokens = new ListBox 
            { 
                Location = new Point(12, 70), 
                Size = new Size(200, 95),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right 
            };

            var lblAvailable = new Label { Text = "Available tag:", Location = new Point(224, 54), AutoSize = true, Anchor = AnchorStyles.Top | AnchorStyles.Right };
            cmbNamePicker = new ComboBox
            {
                Location = new Point(224, 70),
                Size = new Size(214, 24),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            foreach (var token in new[]
                     {
                         MetadataToken.TrackNumber, MetadataToken.DiscNumber, MetadataToken.Artist,
                         MetadataToken.AlbumArtist, MetadataToken.Album, MetadataToken.Title,
                         MetadataToken.Year, MetadataToken.Genre
                     })
            {
                cmbNamePicker.Items.Add(new TokenItem(token));
            }
            cmbNamePicker.SelectedIndex = 0;

            btnNameAdd = new Button { Text = "Add >>", Location = new Point(224, 100), Size = new Size(102, 26), Anchor = AnchorStyles.Top | AnchorStyles.Right };
            btnNameAdd.Click += (s, e) => { AddTokenToList(lstNameTokens, cmbNamePicker); UpdatePreview(); };

            btnNameRemoveTok = new Button { Text = "Remove", Location = new Point(336, 100), Size = new Size(102, 26), Anchor = AnchorStyles.Top | AnchorStyles.Right };
            btnNameRemoveTok.Click += (s, e) => { RemoveSelectedToken(lstNameTokens); UpdatePreview(); };

            btnNameUp = new Button { Text = "Move Up", Location = new Point(224, 132), Size = new Size(102, 26), Anchor = AnchorStyles.Top | AnchorStyles.Right };
            btnNameUp.Click += (s, e) => { MoveListItem(lstNameTokens, -1); UpdatePreview(); };

            btnNameDown = new Button { Text = "Move Down", Location = new Point(336, 132), Size = new Size(102, 26), Anchor = AnchorStyles.Top | AnchorStyles.Right };
            btnNameDown.Click += (s, e) => { MoveListItem(lstNameTokens, 1); UpdatePreview(); };

            var lblSeparator = new Label { Text = "Separator:", Location = new Point(12, 175), AutoSize = true, Anchor = AnchorStyles.Bottom | AnchorStyles.Left };
            txtSeparator = new TextBox { Text = " - ", Location = new Point(86, 172), Size = new Size(60, 24), Anchor = AnchorStyles.Bottom | AnchorStyles.Left };
            txtSeparator.TextChanged += (s, e) => UpdatePreview();

            var lblSeparatorHint = new Label
            {
                Text = "(placed between each chosen tag)",
                Location = new Point(154, 175),
                AutoSize = true,
                ForeColor = Color.DimGray,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };

            group.Controls.Add(lblInfo);
            group.Controls.Add(lblChosen);
            group.Controls.Add(lstNameTokens);
            group.Controls.Add(lblAvailable);
            group.Controls.Add(cmbNamePicker);
            group.Controls.Add(btnNameAdd);
            group.Controls.Add(btnNameRemoveTok);
            group.Controls.Add(btnNameUp);
            group.Controls.Add(btnNameDown);
            group.Controls.Add(lblSeparator);
            group.Controls.Add(txtSeparator);
            group.Controls.Add(lblSeparatorHint);

            _busyControls.AddRange(new Control[]
            {
                cmbNamePicker, btnNameAdd, btnNameRemoveTok, btnNameUp, btnNameDown, txtSeparator
            });

            return group;
        }

        private GroupBox BuildPreviewGroup()
        {
            var group = new GroupBox
            {
                Text = "Preview",
                Location = new Point(12, 465),
                Size = new Size(916, 50),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            var lblCaption = new Label { Text = "Example output:", Location = new Point(12, 22), AutoSize = true };
            txtPreview = new TextBox
            {
                Location = new Point(120, 20),
                Size = new Size(780, 24),
                ReadOnly = true,
                Font = new Font("Consolas", 9.5F),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            group.Controls.Add(lblCaption);
            group.Controls.Add(txtPreview);
            return group;
        }

        private GroupBox BuildLogGroup()
        {
            var group = new GroupBox
            {
                Text = "Progress / Log",
                Location = new Point(12, 525),
                Size = new Size(916, 85),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            progressBar1 = new ProgressBar { 
                Location = new Point(12, 22), 
                Size = new Size(892, 20),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            txtLog = new TextBox
            {
                Location = new Point(12, 48),
                Size = new Size(892, 25),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Consolas", 8.5F),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            group.Controls.Add(progressBar1);
            group.Controls.Add(txtLog);
            return group;
        }

        private void BuildBottomButtons()
        {
            btnOrganize = new Button
            {
                Text = "Organize Files",
                Location = new Point(12, 620), 
                Size = new Size(170, 32),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            btnOrganize.Click += BtnOrganize_Click;

            btnAbout = new Button { 
                Text = "About", 
                Location = new Point(660, 620), 
                Size = new Size(110, 32),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            btnAbout.Click += (s, e) =>
            {
                using (var about = new AboutForm())
                {
                    about.ShowDialog(this);
                }
            };

            btnExit = new Button { 
                Text = "Exit", 
                Location = new Point(782, 620), 
                Size = new Size(146, 32),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            btnExit.Click += (s, e) => Close();

            Controls.Add(btnOrganize);
            Controls.Add(btnAbout);
            Controls.Add(btnExit);

            _busyControls.Add(btnOrganize);
        }

        private void PopulateDefaults()
        {
            lstFolderTokens.Items.Add(new TokenItem(MetadataToken.Artist));
            lstFolderTokens.Items.Add(new TokenItem(MetadataToken.Album));

            lstNameTokens.Items.Add(new TokenItem(MetadataToken.TrackNumber));
            lstNameTokens.Items.Add(new TokenItem(MetadataToken.Title));
        }

        // =========================================================================================
        //  Small helper wrapper so ListBox/ComboBox show a friendly label for each tag token
        // =========================================================================================

        private sealed class TokenItem
        {
            public MetadataToken Token { get; }
            private readonly string _label;

            public TokenItem(MetadataToken token)
            {
                Token = token;
                _label = TokenResolver.Label(token);
            }

            public override string ToString() => _label;
        }

        // =========================================================================================
        //  Token list management (shared by both the folder-structure and file-naming builders)
        // =========================================================================================

        private static void AddTokenToList(ListBox list, ComboBox picker)
        {
            if (picker.SelectedItem is TokenItem item)
            {
                list.Items.Add(new TokenItem(item.Token));
            }
        }

        private static void RemoveSelectedToken(ListBox list)
        {
            if (list.SelectedIndex >= 0)
            {
                list.Items.RemoveAt(list.SelectedIndex);
            }
        }

        private static void MoveListItem(ListBox list, int direction)
        {
            int index = list.SelectedIndex;
            int newIndex = index + direction;
            if (index < 0 || newIndex < 0 || newIndex >= list.Items.Count) return;

            var item = list.Items[index];
            list.Items.RemoveAt(index);
            list.Items.Insert(newIndex, item);
            list.SelectedIndex = newIndex;
        }

        // =========================================================================================
        //  Source file / folder handling (buttons + drag & drop)
        // =========================================================================================

        private void FileDrag_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop)
                ? DragDropEffects.Copy
                : DragDropEffects.None;
        }

        private void LstSources_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data != null && e.Data.GetData(DataFormats.FileDrop) is string[] paths)
            {
                AddSourcePaths(paths);
            }
        }

        private void TxtDestination_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data != null && e.Data.GetData(DataFormats.FileDrop) is string[] paths && paths.Length > 0)
            {
                string path = paths[0];
                txtDestination.Text = Directory.Exists(path) ? path : (Path.GetDirectoryName(path) ?? path);
                UpdatePreview();
            }
        }

        private void BtnAddFolder_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog { Description = "Select a folder containing music files" })
            {
                if (fbd.ShowDialog(this) == DialogResult.OK)
                {
                    AddSourcePaths(new[] { fbd.SelectedPath });
                }
            }
        }

        private void BtnAddFiles_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog
                   {
                       Multiselect = true,
                       Title = "Select music files",
                       Filter = "Audio Files|*.mp3;*.flac;*.m4a;*.m4b;*.mp4;*.aac;*.ogg;*.oga;*.opus;*.wma;*.wav;*.aiff;*.aif;*.ape;*.wv;*.mpc;*.tta;*.dsf;*.dff|All Files|*.*"
                   })
            {
                if (ofd.ShowDialog(this) == DialogResult.OK)
                {
                    AddSourcePaths(ofd.FileNames);
                }
            }
        }

        private void BtnRemoveSelected_Click(object sender, EventArgs e)
        {
            var selected = lstSources.SelectedItems.Cast<string>().ToList();
            foreach (var s in selected)
            {
                _sourceFiles.Remove(s);
                lstSources.Items.Remove(s);
            }
            UpdatePreview();
        }

        private void BtnClearAll_Click(object sender, EventArgs e)
        {
            _sourceFiles.Clear();
            lstSources.Items.Clear();
            UpdatePreview();
        }

        private void AddSourcePaths(IEnumerable<string> paths)
        {
            var found = new List<string>();
            foreach (var path in paths)
            {
                if (Directory.Exists(path))
                {
                    CollectAudioFiles(path, found);
                }
                else if (File.Exists(path) && TagReader.IsSupported(path))
                {
                    found.Add(path);
                }
            }

            int added = 0;
            foreach (var f in found)
            {
                if (!_sourceFiles.Contains(f, StringComparer.OrdinalIgnoreCase))
                {
                    _sourceFiles.Add(f);
                    lstSources.Items.Add(f);
                    added++;
                }
            }

            Log(string.Format("Added {0} file(s). Total in list: {1}.", added, _sourceFiles.Count));
            UpdatePreview();
        }

        private static void CollectAudioFiles(string folder, List<string> result)
        {
            try
            {
                foreach (var file in Directory.EnumerateFiles(folder))
                {
                    if (TagReader.IsSupported(file)) result.Add(file);
                }
            }
            catch
            {
                // ignore folders we cannot read (permissions, etc.)
            }

            try
            {
                foreach (var sub in Directory.EnumerateDirectories(folder))
                {
                    CollectAudioFiles(sub, result);
                }
            }
            catch
            {
                // ignore folders we cannot read (permissions, etc.)
            }
        }

        private void BtnBrowseDestination_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog { Description = "Select the destination root folder" })
            {
                if (fbd.ShowDialog(this) == DialogResult.OK)
                {
                    txtDestination.Text = fbd.SelectedPath;
                    UpdatePreview();
                }
            }
        }

        // =========================================================================================
        //  Preview
        // =========================================================================================

        private void UpdatePreview()
        {
            if (txtPreview == null) return; // controls not fully built yet during startup

            TagInfo sample = _sourceFiles.Count > 0 ? TagReader.ReadTags(_sourceFiles[0]) : null;
            if (sample == null) sample = DummySample();

            var options = BuildOptionsFromUi(includeDestination: false);
            txtPreview.Text = Organizer.BuildRelativePath(sample, options);
        }

        private static TagInfo DummySample()
        {
            return new TagInfo
            {
                Artist = "Queen",
                AlbumArtist = "Queen",
                Album = "A Night At The Opera",
                Year = "1975",
                Genre = "Rock",
                Title = "Bohemian Rhapsody",
                TrackNumber = "11",
                DiscNumber = "1",
                Extension = ".mp3",
                SourcePath = "sample.mp3"
            };
        }

        private OrganizeOptions BuildOptionsFromUi(bool includeDestination = true)
        {
            var folderTokens = lstFolderTokens.Items.Cast<TokenItem>().Select(t => t.Token).ToList();
            var nameTokens = lstNameTokens.Items.Cast<TokenItem>().Select(t => t.Token).ToList();

            return new OrganizeOptions
            {
                FolderTokens = folderTokens,
                FileNameTokens = nameTokens,
                FileNameSeparator = string.IsNullOrEmpty(txtSeparator.Text) ? " - " : txtSeparator.Text,
                DestinationRoot = includeDestination ? txtDestination.Text.Trim() : "",
                CopyInsteadOfMove = chkCopyInsteadOfMove.Checked
            };
        }

        // =========================================================================================
        //  Organize (runs on a background thread so the UI stays responsive)
        // =========================================================================================

        private async void BtnOrganize_Click(object sender, EventArgs e)
        {
            if (_sourceFiles.Count == 0)
            {
                MessageBox.Show(this, "Please add at least one source file or folder first.",
                    "No Source Files", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtDestination.Text) || !Directory.Exists(txtDestination.Text))
            {
                MessageBox.Show(this, "Please choose a valid destination folder first.",
                    "No Destination", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (lstFolderTokens.Items.Count == 0 && lstNameTokens.Items.Count == 0)
            {
                var confirm = MessageBox.Show(this,
                    "No folder structure or file naming tags are selected, so files will keep their original " +
                    "name and be placed directly in the destination root. Continue anyway?",
                    "Nothing Selected", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (confirm != DialogResult.Yes) return;
            }

            var options = BuildOptionsFromUi();
            var filesSnapshot = _sourceFiles.ToList();

            SetUiEnabled(false);
            progressBar1.Minimum = 0;
            progressBar1.Maximum = filesSnapshot.Count;
            progressBar1.Value = 0;
            txtLog.Clear();

            int okCount = 0;
            int failCount = 0;

            var progress = new Progress<Tuple<int, OrganizeResult>>(update =>
            {
                progressBar1.Value = Math.Min(update.Item1 + 1, progressBar1.Maximum);
                var result = update.Item2;
                if (result.Success)
                {
                    Log(string.Format("OK    {0}  ->  {1}", result.SourcePath, result.DestinationPath));
                }
                else
                {
                    Log(string.Format("FAIL  {0}  ::  {1}", result.SourcePath, result.Message));
                }
            });

            await Task.Run(() =>
            {
                for (int i = 0; i < filesSnapshot.Count; i++)
                {
                    var result = Organizer.ProcessFile(filesSnapshot[i], options);
                    if (result.Success) Interlocked.Increment(ref okCount);
                    else Interlocked.Increment(ref failCount);

                    ((IProgress<Tuple<int, OrganizeResult>>)progress).Report(Tuple.Create(i, result));
                }
            });

            if (!options.CopyInsteadOfMove)
            {
                RemoveMovedFilesFromList(filesSnapshot);
            }

            SetUiEnabled(true);
            Log(string.Format("Done. {0} succeeded, {1} failed.", okCount, failCount));

            MessageBox.Show(this,
                string.Format("Finished.\n\nSucceeded: {0}\nFailed: {1}", okCount, failCount),
                "Music Files Organizer",
                MessageBoxButtons.OK,
                failCount > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
        }

        private void RemoveMovedFilesFromList(List<string> processed)
        {
            foreach (var f in processed)
            {
                if (!File.Exists(f))
                {
                    _sourceFiles.Remove(f);
                    lstSources.Items.Remove(f);
                }
            }
        }

        private void SetUiEnabled(bool enabled)
        {
            foreach (var control in _busyControls)
            {
                control.Enabled = enabled;
            }
        }

        // =========================================================================================
        //  Log helper (safe to call from the background task via Progress<T>, which already
        //  marshals back to the UI thread, but the guard below makes it safe from anywhere)
        // =========================================================================================

        private void Log(string message)
        {
            if (txtLog.InvokeRequired)
            {
                txtLog.Invoke(new Action(() => AppendLog(message)));
            }
            else
            {
                AppendLog(message);
            }
        }

        private void AppendLog(string message)
        {
            txtLog.AppendText(message + Environment.NewLine);
        }
    }
}