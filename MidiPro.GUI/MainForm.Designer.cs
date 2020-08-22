namespace MidiPro.GUI
{
    partial class MainForm
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

        #region Código generado por el Diseñador de Windows Forms

        /// <summary>
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido de este método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.tbGpFilename = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnSelectInputFile = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.tbMidiFilename = new System.Windows.Forms.TextBox();
            this.btnConvert = new System.Windows.Forms.Button();
            this.ofdGPFile = new System.Windows.Forms.OpenFileDialog();
            this.SuspendLayout();
            // 
            // tbGpFilename
            // 
            this.tbGpFilename.Location = new System.Drawing.Point(90, 12);
            this.tbGpFilename.Name = "tbGpFilename";
            this.tbGpFilename.Size = new System.Drawing.Size(485, 20);
            this.tbGpFilename.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 15);
            this.label1.TabIndex = 1;
            this.label1.Text = "Fichero GPx:";
            // 
            // btnSelectInputFile
            // 
            this.btnSelectInputFile.Location = new System.Drawing.Point(581, 9);
            this.btnSelectInputFile.Name = "btnSelectInputFile";
            this.btnSelectInputFile.Size = new System.Drawing.Size(51, 23);
            this.btnSelectInputFile.TabIndex = 2;
            this.btnSelectInputFile.Text = "Buscar";
            this.btnSelectInputFile.UseVisualStyleBackColor = true;
            this.btnSelectInputFile.Click += new System.EventHandler(this.btnSelectInputFile_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 41);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(80, 15);
            this.label2.TabIndex = 4;
            this.label2.Text = "Fichero MIDI:";
            // 
            // tbMidiFilename
            // 
            this.tbMidiFilename.Enabled = false;
            this.tbMidiFilename.Location = new System.Drawing.Point(90, 38);
            this.tbMidiFilename.Name = "tbMidiFilename";
            this.tbMidiFilename.Size = new System.Drawing.Size(485, 20);
            this.tbMidiFilename.TabIndex = 3;
            this.tbMidiFilename.TextChanged += new System.EventHandler(this.tbMidiFilename_TextChanged);
            // 
            // btnConvert
            // 
            this.btnConvert.Location = new System.Drawing.Point(12, 64);
            this.btnConvert.Name = "btnConvert";
            this.btnConvert.Size = new System.Drawing.Size(620, 23);
            this.btnConvert.TabIndex = 5;
            this.btnConvert.Text = "Convertir";
            this.btnConvert.UseVisualStyleBackColor = true;
            this.btnConvert.Click += new System.EventHandler(this.btnConvert_Click);
            // 
            // ofdGPFile
            // 
            this.ofdGPFile.FileName = "GuitarProFile";
            this.ofdGPFile.Filter = "Archivos GP3|*.gp3|Archivos GP4|*.gp4";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(638, 208);
            this.Controls.Add(this.btnConvert);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tbMidiFilename);
            this.Controls.Add(this.btnSelectInputFile);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tbGpFilename);
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(457, 137);
            this.Name = "MainForm";
            this.Text = "GPx -> MIDI";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tbGpFilename;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnSelectInputFile;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbMidiFilename;
        private System.Windows.Forms.Button btnConvert;
        private System.Windows.Forms.OpenFileDialog ofdGPFile;
    }
}

