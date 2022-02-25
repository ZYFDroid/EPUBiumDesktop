namespace EPUBium_Desktop
{
    partial class FrmChangeResource
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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmChangeResource));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label2 = new System.Windows.Forms.Label();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.btnDefault = new System.Windows.Forms.Button();
            this.btnUse = new System.Windows.Forms.Button();
            this.btnOpenDir = new System.Windows.Forms.Button();
            this.btnThemeDev = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.tblItems = new System.Windows.Forms.DataGridView();
            this.Column4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column1 = new System.Windows.Forms.DataGridViewImageColumn();
            this.Column2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column3 = new System.Windows.Forms.DataGridViewLinkColumn();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.mnuDevMode = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnuThemeDevUseDir = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuThemeDevUseUrl = new System.Windows.Forms.ToolStripMenuItem();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tblItems)).BeginInit();
            this.mnuDevMode.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.label2, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.tblItems, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(20, 20);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 126F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(786, 916);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 742);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(778, 48);
            this.label2.TabIndex = 3;
            this.label2.Text = "若主题包没有提供更换主题包的入口，您可以移动或删除正在使用的主题包来恢复到默认主题包";
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Controls.Add(this.btnDefault, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.btnUse, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.btnOpenDir, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.btnThemeDev, 1, 1);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 793);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(780, 120);
            this.tableLayoutPanel2.TabIndex = 2;
            // 
            // btnDefault
            // 
            this.btnDefault.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnDefault.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnDefault.Location = new System.Drawing.Point(3, 3);
            this.btnDefault.Name = "btnDefault";
            this.btnDefault.Size = new System.Drawing.Size(384, 54);
            this.btnDefault.TabIndex = 0;
            this.btnDefault.Text = "恢复默认";
            this.btnDefault.UseVisualStyleBackColor = true;
            this.btnDefault.Click += new System.EventHandler(this.btnDefault_Click);
            // 
            // btnUse
            // 
            this.btnUse.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnUse.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnUse.Location = new System.Drawing.Point(393, 3);
            this.btnUse.Name = "btnUse";
            this.btnUse.Size = new System.Drawing.Size(384, 54);
            this.btnUse.TabIndex = 0;
            this.btnUse.Text = "使用选中的主题包";
            this.btnUse.UseVisualStyleBackColor = true;
            this.btnUse.Click += new System.EventHandler(this.btnUse_Click);
            // 
            // btnOpenDir
            // 
            this.btnOpenDir.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnOpenDir.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnOpenDir.Location = new System.Drawing.Point(3, 63);
            this.btnOpenDir.Name = "btnOpenDir";
            this.btnOpenDir.Size = new System.Drawing.Size(384, 54);
            this.btnOpenDir.TabIndex = 1;
            this.btnOpenDir.Text = "打开主题包目录";
            this.btnOpenDir.UseVisualStyleBackColor = true;
            this.btnOpenDir.Click += new System.EventHandler(this.btnOpenDir_Click);
            // 
            // btnThemeDev
            // 
            this.btnThemeDev.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnThemeDev.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnThemeDev.Location = new System.Drawing.Point(393, 63);
            this.btnThemeDev.Name = "btnThemeDev";
            this.btnThemeDev.Size = new System.Drawing.Size(384, 54);
            this.btnThemeDev.TabIndex = 1;
            this.btnThemeDev.Text = "主题包开发模式";
            this.btnThemeDev.UseVisualStyleBackColor = true;
            this.btnThemeDev.Click += new System.EventHandler(this.btnThemeDev_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(778, 144);
            this.label1.TabIndex = 1;
            this.label1.Text = "\r\n通过使用主题包，您可以更改软件的外观，添加新的功能等。\r\n注意：主题包可以完全控制软件显示的内容和行为。因此请确认主题包的来源可信\r\n\r\n可用的主题包列表：";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tblItems
            // 
            this.tblItems.AllowUserToAddRows = false;
            this.tblItems.AllowUserToDeleteRows = false;
            this.tblItems.AllowUserToResizeColumns = false;
            this.tblItems.AllowUserToResizeRows = false;
            this.tblItems.BackgroundColor = System.Drawing.Color.White;
            this.tblItems.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.tblItems.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.tblItems.ColumnHeadersVisible = false;
            this.tblItems.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column4,
            this.Column1,
            this.Column2,
            this.Column3});
            this.tblItems.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblItems.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.tblItems.GridColor = System.Drawing.Color.White;
            this.tblItems.Location = new System.Drawing.Point(3, 147);
            this.tblItems.MultiSelect = false;
            this.tblItems.Name = "tblItems";
            this.tblItems.ReadOnly = true;
            this.tblItems.RowHeadersVisible = false;
            this.tblItems.RowHeadersWidth = 82;
            this.tblItems.RowTemplate.Height = 192;
            this.tblItems.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.tblItems.Size = new System.Drawing.Size(780, 592);
            this.tblItems.TabIndex = 4;
            this.tblItems.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.tblItems_CellClick);
            this.tblItems.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.tblItems_CellContentClick);
            // 
            // Column4
            // 
            this.Column4.HeaderText = "Column4";
            this.Column4.MinimumWidth = 10;
            this.Column4.Name = "Column4";
            this.Column4.ReadOnly = true;
            this.Column4.Visible = false;
            this.Column4.Width = 200;
            // 
            // Column1
            // 
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle4.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle4.ForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle4.NullValue = ((object)(resources.GetObject("dataGridViewCellStyle4.NullValue")));
            dataGridViewCellStyle4.Padding = new System.Windows.Forms.Padding(32);
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.Color.Black;
            this.Column1.DefaultCellStyle = dataGridViewCellStyle4;
            this.Column1.HeaderText = "Column1";
            this.Column1.Image = global::EPUBium_Desktop.Properties.Resources._default;
            this.Column1.ImageLayout = System.Windows.Forms.DataGridViewImageCellLayout.Zoom;
            this.Column1.MinimumWidth = 10;
            this.Column1.Name = "Column1";
            this.Column1.ReadOnly = true;
            this.Column1.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.Column1.Width = 192;
            // 
            // Column2
            // 
            this.Column2.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle5.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle5.ForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle5.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            dataGridViewCellStyle5.SelectionForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.Column2.DefaultCellStyle = dataGridViewCellStyle5;
            this.Column2.HeaderText = "Column2";
            this.Column2.MinimumWidth = 10;
            this.Column2.Name = "Column2";
            this.Column2.ReadOnly = true;
            this.Column2.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            // 
            // Column3
            // 
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.BottomRight;
            dataGridViewCellStyle6.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle6.ForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle6.Padding = new System.Windows.Forms.Padding(0, 0, 20, 20);
            dataGridViewCellStyle6.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            dataGridViewCellStyle6.SelectionForeColor = System.Drawing.Color.Black;
            this.Column3.DefaultCellStyle = dataGridViewCellStyle6;
            this.Column3.HeaderText = "Column3";
            this.Column3.MinimumWidth = 10;
            this.Column3.Name = "Column3";
            this.Column3.ReadOnly = true;
            this.Column3.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.Column3.Width = 120;
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.Filter = "资源包描述文件|meta.json|资源包图标|pack.png";
            this.openFileDialog1.Title = "选择资源包的文件";
            // 
            // mnuDevMode
            // 
            this.mnuDevMode.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.mnuDevMode.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuThemeDevUseDir,
            this.mnuThemeDevUseUrl});
            this.mnuDevMode.Name = "mnuDevMode";
            this.mnuDevMode.Size = new System.Drawing.Size(301, 124);
            // 
            // mnuThemeDevUseDir
            // 
            this.mnuThemeDevUseDir.Name = "mnuThemeDevUseDir";
            this.mnuThemeDevUseDir.Size = new System.Drawing.Size(300, 38);
            this.mnuThemeDevUseDir.Text = "使用本地文件夹";
            this.mnuThemeDevUseDir.Click += new System.EventHandler(this.mnuThemeDevUseDir_Click);
            // 
            // mnuThemeDevUseUrl
            // 
            this.mnuThemeDevUseUrl.Name = "mnuThemeDevUseUrl";
            this.mnuThemeDevUseUrl.Size = new System.Drawing.Size(300, 38);
            this.mnuThemeDevUseUrl.Text = "使用http路径";
            this.mnuThemeDevUseUrl.Click += new System.EventHandler(this.mnuThemeDevUseUrl_Click);
            // 
            // FrmChangeResource
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(192F, 192F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(826, 956);
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.MaximumSize = new System.Drawing.Size(852, 1027);
            this.MinimumSize = new System.Drawing.Size(852, 1027);
            this.Name = "FrmChangeResource";
            this.Padding = new System.Windows.Forms.Padding(20);
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "更换主题包";
            this.Load += new System.EventHandler(this.FrmChangeResource_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.tblItems)).EndInit();
            this.mnuDevMode.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Button btnDefault;
        private System.Windows.Forms.Button btnUse;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnOpenDir;
        private System.Windows.Forms.Button btnThemeDev;
        private System.Windows.Forms.DataGridView tblItems;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column4;
        private System.Windows.Forms.DataGridViewImageColumn Column1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column2;
        private System.Windows.Forms.DataGridViewLinkColumn Column3;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.ContextMenuStrip mnuDevMode;
        private System.Windows.Forms.ToolStripMenuItem mnuThemeDevUseDir;
        private System.Windows.Forms.ToolStripMenuItem mnuThemeDevUseUrl;
    }
}