namespace Parsenok
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.ground = new System.Windows.Forms.Panel();
            this.logs_collection = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // ground
            // 
            this.ground.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ground.AutoScroll = true;
            this.ground.Location = new System.Drawing.Point(1, 1);
            this.ground.Name = "ground";
            this.ground.Size = new System.Drawing.Size(854, 377);
            this.ground.TabIndex = 0;
            // 
            // logs_collection
            // 
            this.logs_collection.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.logs_collection.Location = new System.Drawing.Point(1, 384);
            this.logs_collection.Multiline = true;
            this.logs_collection.Name = "logs_collection";
            this.logs_collection.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.logs_collection.Size = new System.Drawing.Size(854, 171);
            this.logs_collection.TabIndex = 1;
            this.logs_collection.WordWrap = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(856, 557);
            this.Controls.Add(this.logs_collection);
            this.Controls.Add(this.ground);
            this.Name = "Form1";
            this.Text = "(O_o) 5.2";
            this.Activated += new System.EventHandler(this.Form1_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel ground;
        private System.Windows.Forms.TextBox logs_collection;




    }
}

