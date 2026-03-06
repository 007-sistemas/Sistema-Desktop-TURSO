using BiometricSystem.Database;
using BiometricSystem.Models;

namespace BiometricSystem.Forms
{
    public partial class DashboardForm : Form
    {
        private readonly DatabaseHelper database;
        private System.Windows.Forms.Timer refreshTimer;

        public DashboardForm()
        {
            InitializeComponent();
            database = new DatabaseHelper();
            
            // Configurar DataGridViews
            ConfigureDataGridViews();
            
            // Carregar dados iniciais
            LoadEmployees();
            LoadTimeRecords();
            
            // Timer para atualizar automaticamente a cada 5 segundos
            refreshTimer = new System.Windows.Forms.Timer();
            refreshTimer.Interval = 5000; // 5 segundos
            refreshTimer.Tick += (s, e) => LoadTimeRecords();
            refreshTimer.Start();
        }

        private void ConfigureDataGridViews()
        {
            // Configurar DataGridView de Funcionários
            dgvEmployees.AutoGenerateColumns = false;
            dgvEmployees.ReadOnly = true;
            dgvEmployees.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvEmployees.AllowUserToAddRows = false;

            dgvEmployees.Columns.Clear();
            dgvEmployees.Columns.Add("Id", "ID");
            dgvEmployees.Columns.Add("Name", "Nome");
            dgvEmployees.Columns.Add("CPF", "CPF");
            dgvEmployees.Columns.Add("Position", "Cargo");
            dgvEmployees.Columns.Add("Email", "Email");
            dgvEmployees.Columns.Add("CreatedAt", "Cadastrado em");

            dgvEmployees.Columns["Id"].Width = 40;
            dgvEmployees.Columns["Name"].Width = 150;
            dgvEmployees.Columns["CPF"].Width = 120;
            dgvEmployees.Columns["Position"].Width = 100;
            dgvEmployees.Columns["Email"].Width = 150;
            dgvEmployees.Columns["CreatedAt"].Width = 150;

            // Configurar DataGridView de Registros de Ponto
            dgvTimeRecords.AutoGenerateColumns = false;
            dgvTimeRecords.ReadOnly = true;
            dgvTimeRecords.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvTimeRecords.AllowUserToAddRows = false;

            dgvTimeRecords.Columns.Clear();
            dgvTimeRecords.Columns.Add("Id", "ID");
            dgvTimeRecords.Columns.Add("EmployeeName", "Funcionário");
            dgvTimeRecords.Columns.Add("Timestamp", "Data/Hora");
            dgvTimeRecords.Columns.Add("Type", "Tipo");
            dgvTimeRecords.Columns.Add("Notes", "Observações");

            dgvTimeRecords.Columns["Id"].Width = 40;
            dgvTimeRecords.Columns["EmployeeName"].Width = 150;
            dgvTimeRecords.Columns["Timestamp"].Width = 150;
            dgvTimeRecords.Columns["Type"].Width = 80;
            dgvTimeRecords.Columns["Notes"].Width = 150;
        }

        private void LoadEmployees()
        {
            try
            {
                var employees = database.GetAllEmployees();
                dgvEmployees.Rows.Clear();

                foreach (var emp in employees)
                {
                    dgvEmployees.Rows.Add(
                        emp.Id,
                        emp.Name,
                        emp.CPF,
                        emp.Position,
                        emp.Email,
                        emp.CreatedAt.ToString("dd/MM/yyyy HH:mm")
                    );
                }

                lblTotalEmployees.Text = $"Total de funcionários: {employees.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar funcionários: {ex.Message}");
            }
        }

        private void LoadTimeRecords()
        {
            try
            {
                // Buscar todos os registros
                var allRecords = new List<(int Id, string EmployeeName, string Timestamp, string Type, string Notes)>();
                
                var employees = database.GetAllEmployees();
                foreach (var emp in employees)
                {
                    var records = database.GetEmployeeTimeRecords(emp.Id);
                    foreach (var record in records)
                    {
                        allRecords.Add((
                            record.Id,
                            emp.Name,
                            DateTime.Parse(record.Timestamp.ToString("o")).ToString("dd/MM/yyyy HH:mm:ss"),
                            record.Type,
                            record.Notes ?? ""
                        ));
                    }
                }

                // Ordenar por data/hora decrescente
                allRecords = allRecords.OrderByDescending(r => r.Timestamp).ToList();

                dgvTimeRecords.Rows.Clear();
                foreach (var record in allRecords)
                {
                    int rowIndex = dgvTimeRecords.Rows.Add(
                        record.Id,
                        record.EmployeeName,
                        record.Timestamp,
                        record.Type,
                        record.Notes
                    );

                    // Colorir as linhas: Entrada = Verde, Saída = Azul
                    if (record.Type == "Entrada")
                    {
                        dgvTimeRecords.Rows[rowIndex].DefaultCellStyle.BackColor = Color.FromArgb(200, 230, 201); // Verde claro
                    }
                    else if (record.Type == "Saída")
                    {
                        dgvTimeRecords.Rows[rowIndex].DefaultCellStyle.BackColor = Color.FromArgb(200, 220, 240); // Azul claro
                    }
                }

                lblTotalRecords.Text = $"Total de registros de produção: {allRecords.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar registros: {ex.Message}");
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadEmployees();
            LoadTimeRecords();
            MessageBox.Show("Dados atualizados!", "Atualização", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            refreshTimer?.Stop();
            refreshTimer?.Dispose();
            base.OnFormClosing(e);
        }
    }
}
