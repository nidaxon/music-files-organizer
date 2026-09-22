using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace MusicFilesOrganizer
{
    public class AboutForm : Form
    {
        public AboutForm()
        {
            Text = "About Music Files Organizer";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            
            // Slightly increased form height to fit the new license notice cleanly
            ClientSize = new Size(420, 195);
            Font = new Font("Segoe UI", 9F);

            // --- App Icon Image Setup ---
            var pbIcon = new PictureBox
            {
                Location = new Point(20, 20),
                Size = new Size(80, 80),
                SizeMode = PictureBoxSizeMode.Zoom
            };
            
            string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appicon.png");
            if (File.Exists(iconPath))
            {
                pbIcon.Image = Image.FromFile(iconPath);
            }

            // --- Text Labels & Links ---
            var lblTitle = new Label
            {
                Text = "Music Files Organizer",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(115, 20)
            };

            var lblVersion = new Label
            {
                Text = "v1.0.0 [INITIAL RELEASE]",
                AutoSize = true,
                Location = new Point(115, 50)
            };

            var lblByPrefix = new Label
            {
                Text = "by",
                AutoSize = true,
                Location = new Point(115, 75)
            };

            var linkAuthor = new LinkLabel
            {
                Text = "Ixnando Ondang",
                AutoSize = true,
                Location = new Point(135, 75)
            };
            linkAuthor.LinkClicked += LinkAuthor_LinkClicked;

            // Added License notice line below the author link
            var linkLicense = new LinkLabel
            {
                Text = "Licensed under GNU GPLv3",
                AutoSize = true,
                Location = new Point(115, 105)
            };
            linkLicense.LinkClicked += LinkLicense_LinkClicked;

            var btnClose = new Button
            {
                Text = "Close",
                DialogResult = DialogResult.OK,
                Location = new Point(310, 150), // Shifted down slightly to match new height
                Size = new Size(90, 28)
            };

            Controls.Add(pbIcon);
            Controls.Add(lblTitle);
            Controls.Add(lblVersion);
            Controls.Add(lblByPrefix);
            Controls.Add(linkAuthor);
            Controls.Add(linkLicense);
            Controls.Add(btnClose);

            AcceptButton = btnClose;
            CancelButton = btnClose;
        }

        private static void LinkAuthor_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "https://github.com/nidaxon",
                UseShellExecute = true
            };
            Process.Start(psi);
        }

        private static void LinkLicense_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "https://www.gnu.org/licenses/gpl-3.0.html",
                UseShellExecute = true
            };
            Process.Start(psi);
        }
    }
}