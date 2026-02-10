namespace BiometricSystem.Forms
{
    partial class DashboardForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DashboardForm));
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabEmployees = new System.Windows.Forms.TabPage();
            this.dgvEmployees = new System.Windows.Forms.DataGridView();
            this.lblTotalEmployees = new System.Windows.Forms.Label();
            this.tabTimeRecords = new System.Windows.Forms.TabPage();
            this.dgvTimeRecords = new System.Windows.Forms.DataGridView();
            this.lblTotalRecords = new System.Windows.Forms.Label();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.btnBack = new System.Windows.Forms.Button();
            this.tabControl1.SuspendLayout();
            this.tabEmployees.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvEmployees)).BeginInit();
            this.tabTimeRecords.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTimeRecords)).BeginInit();
            this.SuspendLayout();
            
            // tabControl1
            this.tabControl1.Controls.Add(this.tabEmployees);
            this.tabControl1.Controls.Add(this.tabTimeRecords);
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(960, 520);
            this.tabControl1.TabIndex = 0;
            
            // tabEmployees
            this.tabEmployees.Controls.Add(this.dgvEmployees);
            this.tabEmployees.Controls.Add(this.lblTotalEmployees);
            this.tabEmployees.Location = new System.Drawing.Point(4, 24);
            this.tabEmployees.Name = "tabEmployees";
            this.tabEmployees.Padding = new System.Windows.Forms.Padding(3);
            this.tabEmployees.Size = new System.Drawing.Size(952, 492);
            this.tabEmployees.TabIndex = 0;
            this.tabEmployees.Text = "Funcionários Cadastrados";
            this.tabEmployees.UseVisualStyleBackColor = true;
            
            // dgvEmployees
            this.dgvEmployees.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvEmployees.Location = new System.Drawing.Point(6, 30);
            this.dgvEmployees.Name = "dgvEmployees";
            this.dgvEmployees.Size = new System.Drawing.Size(940, 430);
            this.dgvEmployees.TabIndex = 0;
            
            // lblTotalEmployees
            this.lblTotalEmployees.AutoSize = true;
            this.lblTotalEmployees.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblTotalEmployees.Location = new System.Drawing.Point(6, 10);
            this.lblTotalEmployees.Name = "lblTotalEmployees";
            this.lblTotalEmployees.Size = new System.Drawing.Size(150, 19);
            this.lblTotalEmployees.TabIndex = 1;
            this.lblTotalEmployees.Text = "Total de funcionários: 0";
            
            // tabTimeRecords
            this.tabTimeRecords.Controls.Add(this.dgvTimeRecords);
            this.tabTimeRecords.Controls.Add(this.lblTotalRecords);
            this.tabTimeRecords.Location = new System.Drawing.Point(4, 24);
            this.tabTimeRecords.Name = "tabTimeRecords";
            this.tabTimeRecords.Padding = new System.Windows.Forms.Padding(3);
            this.tabTimeRecords.Size = new System.Drawing.Size(952, 492);
            this.tabTimeRecords.TabIndex = 1;
            this.tabTimeRecords.Text = "Registros de Produção";
            this.tabTimeRecords.UseVisualStyleBackColor = true;
            
            // dgvTimeRecords
            this.dgvTimeRecords.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvTimeRecords.Location = new System.Drawing.Point(6, 30);
            this.dgvTimeRecords.Name = "dgvTimeRecords";
            this.dgvTimeRecords.Size = new System.Drawing.Size(940, 430);
            this.dgvTimeRecords.TabIndex = 0;
            
            // lblTotalRecords
            this.lblTotalRecords.AutoSize = true;
            this.lblTotalRecords.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblTotalRecords.Location = new System.Drawing.Point(6, 10);
            this.lblTotalRecords.Name = "lblTotalRecords";
            this.lblTotalRecords.Size = new System.Drawing.Size(150, 19);
            this.lblTotalRecords.TabIndex = 1;
            this.lblTotalRecords.Text = "Total de registros de produção: 0";
            
            // btnRefresh
            this.btnRefresh.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(197)))), ((int)(((byte)(94)))));
            this.btnRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRefresh.ForeColor = System.Drawing.Color.White;
            this.btnRefresh.Location = new System.Drawing.Point(12, 540);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(100, 35);
            this.btnRefresh.TabIndex = 1;
            this.btnRefresh.Text = "🔄 Atualizar";
            this.btnRefresh.UseVisualStyleBackColor = false;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            
            // btnBack
            this.btnBack.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(239)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
            this.btnBack.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBack.ForeColor = System.Drawing.Color.White;
            this.btnBack.Location = new System.Drawing.Point(872, 540);
            this.btnBack.Name = "btnBack";
            this.btnBack.Size = new System.Drawing.Size(100, 35);
            this.btnBack.TabIndex = 2;
            this.btnBack.Text = "← Voltar";
            this.btnBack.UseVisualStyleBackColor = false;
            this.btnBack.Click += new System.EventHandler(this.btnBack_Click);
            
            // DashboardForm
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(984, 587);
            this.Controls.Add(this.btnRefresh);
            this.Controls.Add(this.btnBack);
            this.Controls.Add(this.tabControl1);
            this.Name = "DashboardForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "📊 Dashboard - Controle de Produção";
            this.tabControl1.ResumeLayout(false);
            this.tabEmployees.ResumeLayout(false);
            this.tabEmployees.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvEmployees)).EndInit();
            this.tabTimeRecords.ResumeLayout(false);
            this.tabTimeRecords.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTimeRecords)).EndInit();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabEmployees;
        private System.Windows.Forms.DataGridView dgvEmployees;
        private System.Windows.Forms.Label lblTotalEmployees;
        private System.Windows.Forms.TabPage tabTimeRecords;
        private System.Windows.Forms.DataGridView dgvTimeRecords;
        private System.Windows.Forms.Label lblTotalRecords;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnBack;
    }
}
