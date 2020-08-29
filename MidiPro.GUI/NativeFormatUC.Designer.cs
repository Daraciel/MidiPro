namespace MidiPro.GUI
{
    partial class NativeFormatUc
    {
        /// <summary> 
        /// Variable del diseñador necesaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Limpiar los recursos que se estén usando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben desechar; false en caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de componentes

        /// <summary> 
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido de este método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblTitle = new System.Windows.Forms.Label();
            this.tbTitle = new System.Windows.Forms.TextBox();
            this.tbAlbum = new System.Windows.Forms.TextBox();
            this.lblAlbum = new System.Windows.Forms.Label();
            this.tbArtist = new System.Windows.Forms.TextBox();
            this.lblArtist = new System.Windows.Forms.Label();
            this.tbSubtitle = new System.Windows.Forms.TextBox();
            this.lblSubtitle = new System.Windows.Forms.Label();
            this.dgvTempos = new System.Windows.Forms.DataGridView();
            this.Tempo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Position = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.lblTempos = new System.Windows.Forms.Label();
            this.lblTracks = new System.Windows.Forms.Label();
            this.dgvTracks = new System.Windows.Forms.DataGridView();
            this.trackToOutput = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.trackName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.trackPatch = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.trackPort = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.trackChannel = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.trackCapo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTempos)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTracks)).BeginInit();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.Location = new System.Drawing.Point(3, 14);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(81, 17);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Title:";
            // 
            // tbTitle
            // 
            this.tbTitle.Location = new System.Drawing.Point(90, 11);
            this.tbTitle.Name = "tbTitle";
            this.tbTitle.Size = new System.Drawing.Size(184, 20);
            this.tbTitle.TabIndex = 1;
            // 
            // tbAlbum
            // 
            this.tbAlbum.Location = new System.Drawing.Point(90, 63);
            this.tbAlbum.Name = "tbAlbum";
            this.tbAlbum.Size = new System.Drawing.Size(184, 20);
            this.tbAlbum.TabIndex = 3;
            // 
            // lblAlbum
            // 
            this.lblAlbum.Location = new System.Drawing.Point(3, 66);
            this.lblAlbum.Name = "lblAlbum";
            this.lblAlbum.Size = new System.Drawing.Size(81, 17);
            this.lblAlbum.TabIndex = 2;
            this.lblAlbum.Text = "Album:";
            // 
            // tbArtist
            // 
            this.tbArtist.Location = new System.Drawing.Point(90, 89);
            this.tbArtist.Name = "tbArtist";
            this.tbArtist.Size = new System.Drawing.Size(184, 20);
            this.tbArtist.TabIndex = 5;
            // 
            // lblArtist
            // 
            this.lblArtist.Location = new System.Drawing.Point(3, 92);
            this.lblArtist.Name = "lblArtist";
            this.lblArtist.Size = new System.Drawing.Size(81, 17);
            this.lblArtist.TabIndex = 4;
            this.lblArtist.Text = "Artist:";
            // 
            // tbSubtitle
            // 
            this.tbSubtitle.Location = new System.Drawing.Point(90, 37);
            this.tbSubtitle.Name = "tbSubtitle";
            this.tbSubtitle.Size = new System.Drawing.Size(184, 20);
            this.tbSubtitle.TabIndex = 7;
            // 
            // lblSubtitle
            // 
            this.lblSubtitle.Location = new System.Drawing.Point(3, 40);
            this.lblSubtitle.Name = "lblSubtitle";
            this.lblSubtitle.Size = new System.Drawing.Size(81, 17);
            this.lblSubtitle.TabIndex = 6;
            this.lblSubtitle.Text = "Subtitle:";
            // 
            // dgvTempos
            // 
            this.dgvTempos.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvTempos.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Tempo,
            this.Position});
            this.dgvTempos.Location = new System.Drawing.Point(367, 11);
            this.dgvTempos.Name = "dgvTempos";
            this.dgvTempos.Size = new System.Drawing.Size(284, 98);
            this.dgvTempos.TabIndex = 8;
            // 
            // Tempo
            // 
            this.Tempo.HeaderText = "Tempo";
            this.Tempo.Name = "Tempo";
            // 
            // Position
            // 
            this.Position.HeaderText = "Position";
            this.Position.Name = "Position";
            // 
            // lblTempos
            // 
            this.lblTempos.Location = new System.Drawing.Point(280, 14);
            this.lblTempos.Name = "lblTempos";
            this.lblTempos.Size = new System.Drawing.Size(81, 17);
            this.lblTempos.TabIndex = 9;
            this.lblTempos.Text = "Tempos:";
            this.lblTempos.Click += new System.EventHandler(this.label1_Click);
            // 
            // lblTracks
            // 
            this.lblTracks.Location = new System.Drawing.Point(3, 122);
            this.lblTracks.Name = "lblTracks";
            this.lblTracks.Size = new System.Drawing.Size(81, 17);
            this.lblTracks.TabIndex = 10;
            this.lblTracks.Text = "Tracks:";
            // 
            // dgvTracks
            // 
            this.dgvTracks.AllowUserToAddRows = false;
            this.dgvTracks.AllowUserToDeleteRows = false;
            this.dgvTracks.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvTracks.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvTracks.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.trackToOutput,
            this.trackName,
            this.trackPatch,
            this.trackPort,
            this.trackChannel,
            this.trackCapo});
            this.dgvTracks.Location = new System.Drawing.Point(3, 142);
            this.dgvTracks.Name = "dgvTracks";
            this.dgvTracks.Size = new System.Drawing.Size(648, 171);
            this.dgvTracks.TabIndex = 11;
            // 
            // trackToOutput
            // 
            this.trackToOutput.HeaderText = "ToOutput";
            this.trackToOutput.Name = "trackToOutput";
            // 
            // trackName
            // 
            this.trackName.HeaderText = "Name";
            this.trackName.Name = "trackName";
            // 
            // trackPatch
            // 
            this.trackPatch.HeaderText = "Patch";
            this.trackPatch.Name = "trackPatch";
            // 
            // trackPort
            // 
            this.trackPort.HeaderText = "Port";
            this.trackPort.Name = "trackPort";
            // 
            // trackChannel
            // 
            this.trackChannel.HeaderText = "Channel";
            this.trackChannel.Name = "trackChannel";
            // 
            // trackCapo
            // 
            this.trackCapo.HeaderText = "Capo";
            this.trackCapo.Name = "trackCapo";
            // 
            // NativeFormatUc
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.dgvTracks);
            this.Controls.Add(this.lblTracks);
            this.Controls.Add(this.lblTempos);
            this.Controls.Add(this.dgvTempos);
            this.Controls.Add(this.tbSubtitle);
            this.Controls.Add(this.lblSubtitle);
            this.Controls.Add(this.tbArtist);
            this.Controls.Add(this.lblArtist);
            this.Controls.Add(this.tbAlbum);
            this.Controls.Add(this.lblAlbum);
            this.Controls.Add(this.tbTitle);
            this.Controls.Add(this.lblTitle);
            this.Name = "NativeFormatUc";
            this.Size = new System.Drawing.Size(656, 316);
            ((System.ComponentModel.ISupportInitialize)(this.dgvTempos)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTracks)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.TextBox tbTitle;
        private System.Windows.Forms.TextBox tbAlbum;
        private System.Windows.Forms.Label lblAlbum;
        private System.Windows.Forms.TextBox tbArtist;
        private System.Windows.Forms.Label lblArtist;
        private System.Windows.Forms.TextBox tbSubtitle;
        private System.Windows.Forms.Label lblSubtitle;
        private System.Windows.Forms.DataGridView dgvTempos;
        private System.Windows.Forms.DataGridViewTextBoxColumn Tempo;
        private System.Windows.Forms.DataGridViewTextBoxColumn Position;
        private System.Windows.Forms.Label lblTempos;
        private System.Windows.Forms.Label lblTracks;
        private System.Windows.Forms.DataGridView dgvTracks;
        private System.Windows.Forms.DataGridViewCheckBoxColumn trackToOutput;
        private System.Windows.Forms.DataGridViewTextBoxColumn trackName;
        private System.Windows.Forms.DataGridViewTextBoxColumn trackPatch;
        private System.Windows.Forms.DataGridViewTextBoxColumn trackPort;
        private System.Windows.Forms.DataGridViewTextBoxColumn trackChannel;
        private System.Windows.Forms.DataGridViewTextBoxColumn trackCapo;
    }
}
