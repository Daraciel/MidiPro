using System;
using System.Windows.Forms;
using MidiPro.Core.Midi;
using MidiPro.Core.Native;

namespace MidiPro.GUI
{
    public partial class NativeFormatUc : UserControl
    {

        private NativeFormat _song;
        public NativeFormatUc()
        {
            InitializeComponent();
            _song = null;
        }

        public MidiExport GetMidi()
        {
            MidiExport result = null;
            NativeFormat toExport = null;

            toExport = new NativeFormat()
            {
                Title = _song.Title,
                Subtitle = _song.Subtitle,
                Album = _song.Album,
                Annotations = _song.Annotations,
                Artist = _song.Artist,
                BarMaster = _song.BarMaster,
                Directions = _song.Directions,
                Lyrics = _song.Lyrics,
                Music = _song.Music,
                Words = _song.Words
            };
            foreach (DataGridViewRow row in dgvTempos.Rows)
            {
                if (row.Cells != null && 
                    row.Cells.Count > 0 &&
                    row.Cells[0].Value != null &&
                    row.Cells[1].Value != null)
                {
                    toExport.Tempos.Add(new Tempo()
                    {
                        Value = float.Parse(row.Cells[0].Value.ToString()),
                        Position = int.Parse(row.Cells[1].Value.ToString())
                    });
                }
            }

            foreach (DataGridViewRow row in dgvTracks.Rows)
            {
                if (row.Cells != null &&
                    row.Cells.Count > 0 &&
                    row.Cells[0].Value != null &&
                    row.Cells[1].Value != null)
                {
                    if ((bool)row.Cells[0].Value == true)
                    {
                        toExport.Tracks.Add(_song.Tracks.Find(p => p.Name == row.Cells[1].Value.ToString()));
                    }
                }
            }

            result = toExport.ToMidi();

            return result;

        }

        public void SetData(NativeFormat song)
        {
            int rowIndex = 0;
            _song = song;
            if (_song != null)
            {
                tbTitle.Text = _song?.Title;
                tbSubtitle.Text = _song?.Subtitle;
                tbAlbum.Text = _song?.Album;
                tbArtist.Text = _song?.Artist;
                foreach (Tempo tempo in _song.Tempos)
                {
                    dgvTempos.Rows.Add(tempo.Value, tempo.Position);
                }

                foreach (Track track in _song.Tracks)
                {
                    dgvTracks.Rows.Add(true, track.Name, track.Patch, track.Port, track.Channel, track.Capo);
                }
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
