#region

using System;
using System.Windows.Forms;
using M3U.NET;
using System.IO;

#endregion

namespace PlaylistEditor.Forms
{
    public partial class MainForm : Form
    {
        private M3UFile _m3uFile;

        public MainForm()
        {
            InitializeComponent();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog {Filter = "(*.m3u,*.m3u8)|*.m3u;*.m3u8", Title = "Open Playlist"})
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    _m3uFile = null;

                    try
                    {
                        _m3uFile = new M3UFile();
                        _m3uFile.Load(ofd.FileName);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    PopulateEntries();
                }
            }
        }

        private void PopulateEntries()
        {
            listEntries.Items.Clear();

            if (_m3uFile == null)
                return;

            foreach (var entry in _m3uFile)
            {
                UpdateEntryItem(entry);
            }

            listEntries.AutoResizeColumn(listEntries.Columns.Count - 1, ColumnHeaderAutoResizeStyle.ColumnContent);
        }

        private void UpdateEntryItem(M3UEntry entry, int index = -1)
        {
            if (index == -1)
                index = listEntries.Items.Count;

            var lvi = new ListViewItem(new[]
            {
                entry.Title,
                string.Format("{0:D2}:{1:D2}", entry.Duration.Minutes, entry.Duration.Seconds),
                entry.Path.IsFile ? entry.Path.LocalPath : entry.Path.ToString()
            });

            if (index > listEntries.Items.Count - 1)
                listEntries.Items.Add(lvi);
            else
                listEntries.Items[index] = lvi;
        }

        private void listEntries_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var info = listEntries.HitTest(e.X, e.Y);
            var item = info.Item;

            if (item != null)
            {
                var entry = _m3uFile[item.Index];

                using (var ed = new EditDialog(entry))
                {
                    if (ed.ShowDialog() == DialogResult.OK)
                    {
                        UpdateEntryItem(entry, item.Index);
                    }
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (_m3uFile != null)
            {
                using (var sfd = new SaveFileDialog() {Filter = "M3U File (*.m3u)|*.m3u|M3U8 File (*.m3u8)|*.m3u8", Title = "Save Playlist"})
                {
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        _m3uFile.Save(sfd.FileName);
                    }
                }
            }
        }

        private void btnListToFiles_Click(object sender, EventArgs e)
        {
            if (_m3uFile != null)
            {
                FolderBrowserDialog dialog = new FolderBrowserDialog();
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }
                _m3uFile.SaveToFiles(dialog.SelectedPath);
            }
        }

        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            System.Array pathList = ((System.Array)e.Data.GetData(DataFormats.FileDrop));
            int fileCount = pathList.GetLength(0);
            if (fileCount > 1)
            {
                MessageBox.Show("Dragdrop too many files", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            string FilePath = pathList.GetValue(0).ToString();
            if (!File.Exists(FilePath))//Path is a directory
                return;
            _m3uFile = null;
            try
            {
                _m3uFile = new M3UFile();
                _m3uFile.Load(FilePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            PopulateEntries();
        }
        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(DataFormats.FileDrop) != null)
                e.Effect = DragDropEffects.Copy;
        }
    }
}