using MidiPro.Core.GpFiles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MidiPro.GUI
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void btnSelectInputFile_Click(object sender, EventArgs e)
        {
            if (ofdGPFile.ShowDialog() == DialogResult.OK)
            {
                tbGpFilename.Text = ofdGPFile.FileName;
            }
        }

        private void tbMidiFilename_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnConvert_Click(object sender, EventArgs e)
        {
            if (File.Exists(tbGpFilename.Text))
            {
                ReadFile(tbGpFilename.Text);
            }
            else
            {
                MessageBox.Show("El fichero especificado no existe", "Fallo", MessageBoxButtons.OK);
            }
        }

        private void ReadFile(string path)
        {
            string extension = string.Empty;
            GpFile gpfile = null;


            extension = Path.GetExtension(path).ToUpper();

            switch (extension)
            {
                case ".GP3":
                    gpfile = new Gp3File(File.ReadAllBytes(path));
                    break;
                case ".GP4":
                    gpfile = new Gp4File(File.ReadAllBytes(path));
                    break;
            }
            gpfile?.ReadSong();
        }
    }
}
