#region

using System;
using System.Windows.Forms;
using M3U.NET;

#endregion

namespace PlaylistEditor.Forms
{
    public partial class EditDialog : Form
    {
        private readonly M3UEntry _entry;

        public EditDialog(M3UEntry entry)
        {
            InitializeComponent();
            _entry = entry;

            txtTitle.Text = entry.Title;
            numDuration.Value = (decimal)(entry.Duration.TotalSeconds < 0 ? 0 : entry.Duration.TotalSeconds);
            txtPath.Text = entry.Path.IsFile ? entry.Path.LocalPath : entry.Path.ToString();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            _entry.Title = txtTitle.Text;
            _entry.Duration = TimeSpan.FromSeconds((double) numDuration.Value);
            _entry.Path = new Uri(txtPath.Text);
        }
    }
}