#region

using System;
using System.Windows.Forms;
using M3U.NET;

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
            using (var ofd = new OpenFileDialog {Filter = "M3U File (*.m3u)|*.m3u", Title = "Open Playlist"})
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
                using (var sfd = new SaveFileDialog() {Filter = "M3U File (*.m3u)|*.m3u", Title = "Save Playlist"})
                {
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        _m3uFile.Save(sfd.FileName);
                    }
                }
            }
        }
    }
}