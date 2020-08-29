using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using MidiPro.Core.GP.Files;
using MidiPro.Core.Native;

namespace MidiPro.GUI
{
    public partial class MainForm : Form
    {
        private GpFile _gpFile;

        private NativeFormat _song;

        public MainForm()
        {
            InitializeComponent();
            _gpFile = null;
            _song = null;
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
                var midi = nfSong.GetMidi();
                List<byte> data = midi.CreateBytes();
                var dataArray = data.ToArray();
                using (var fs = new FileStream("output.mid", FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    fs.Write(dataArray, 0, dataArray.Length);

                }
            }
            else
            {
                MessageBox.Show("El fichero especificado no existe", "Fallo", MessageBoxButtons.OK);
            }
        }

        private void ReadFile(string path)
        {
            string extension = string.Empty;


            extension = Path.GetExtension(path).ToUpper();

            switch (extension)
            {
                case ".GP3":
                    _gpFile = new Gp3File(File.ReadAllBytes(path));
                    break;
                case ".GP4":
                    _gpFile = new Gp4File(File.ReadAllBytes(path));
                    break;
            }
            _gpFile?.ReadSong();
        }

        private void btnReadPgxFile_Click(object sender, EventArgs e)
        {
            if (File.Exists(tbGpFilename.Text))
            {
                ReadFile(tbGpFilename.Text);

                _song = new NativeFormat(_gpFile);
                nfSong.SetData(_song);
            }
            else
            {
                MessageBox.Show("El fichero especificado no existe", "Fallo", MessageBoxButtons.OK);
            }
        }
    }
}
