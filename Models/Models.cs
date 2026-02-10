namespace BiometricSystem.Models
{
    public class Employee
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string CPF { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public byte[]? FingerprintTemplate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;
    }

    public class TimeRecord
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string Type { get; set; } = "Entrada"; // Entrada ou Saída
        public string? Notes { get; set; }
        public bool SyncedToCloud { get; set; } = false;
        public string? CloudId { get; set; }
    }

    /// <summary>
    /// Modelo compatível com tabela 'pontos' do Neon
    /// </summary>
    public class RegistroPonto
    {
        public string Id { get; set; } = Guid.NewGuid().ToString(); // UUID
        public string? Codigo { get; set; } // Código numérico legado
        public string CooperadoId { get; set; } = string.Empty; // CPF do funcionário
        public string CooperadoNome { get; set; } = string.Empty; // Nome do funcionário
        public DateTime Timestamp { get; set; } = DateTime.Now; // ISO timestamp
        public string Tipo { get; set; } = "ENTRADA"; // ENTRADA, SAIDA, INTERVALO_IDA, INTERVALO_VOLTA
        public string? Local { get; set; } // Hospital + Setor
        public string? HospitalId { get; set; }
        public string? SetorId { get; set; }
        public string? Date { get; set; }
        public string? Entrada { get; set; }
        public string? Saida { get; set; }
        public string? Observacao { get; set; }
        public string? RelatedId { get; set; } // ID do ponto vinculado (entrada → saída)
        public string Status { get; set; } = "Pendente"; // Aberto, Fechado, Pendente, Rejeitado
        public bool IsManual { get; set; } = true; // Registrado por biometria ou manual
        public string? ValidadoPor { get; set; }
        public string? RejeitadoPor { get; set; }
        public string? MotivoRejeicao { get; set; }
        public string? BiometriaEntradaHash { get; set; }
        public string? BiometriaSaidaHash { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; } = DateTime.Now;
    }
}
