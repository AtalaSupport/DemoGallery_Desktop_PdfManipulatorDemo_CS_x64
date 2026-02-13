using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace PdfManipulator
{
    public partial class Instructions : Form
    {
        private bool _loading;

        public Instructions()
        {
            InitializeComponent();
        }

        private void chkShowAtStartup_CheckedChanged(object sender, EventArgs e)
        {
            if (_loading) return;
            string settings = Application.UserAppDataPath + "\\PdfManipulatorSettings.txt";
            using (System.IO.FileStream writer = new System.IO.FileStream(settings, System.IO.FileMode.Create, System.IO.FileAccess.Write))
            {
                writer.WriteByte((byte)(this.chkShowAtStartup.Checked ? 1 : 0));
            }
        }

        private void Instructions_Load(object sender, EventArgs e)
        {
            System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();
            System.IO.Stream stream = asm.GetManifestResourceStream("PdfManipulator.Resources.Instructions.rtf");
            if (stream != null)
            {
                this.rtfInstructions.LoadFile(stream, RichTextBoxStreamType.RichText);
                stream.Close();
            }

            _loading = true;
            string settings = Application.UserAppDataPath + "\\PdfManipulatorSettings.txt";
            if (System.IO.File.Exists(settings))
            {
                using (System.IO.FileStream reader = new System.IO.FileStream(settings, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read))
                {
                    this.chkShowAtStartup.Checked = (reader.ReadByte() == 1);
                }
            }
            _loading = false;
        }

        private void btnClose_Click(object sender, System.EventArgs e)
        {
            this.Close();
        }
    }
}
