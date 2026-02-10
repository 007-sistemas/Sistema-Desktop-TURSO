// using BiometricSystem.Database.Repository;
using BiometricSystem.Models;

namespace BiometricSystem.Database.Services
{
    // DESABILITADO: Usar ApiService para web integration
    /*
    public class TimeRecordService
    {
        private readonly IRepository<TimeRecord> _recordRepository;
        private readonly IRepository<Employee> _employeeRepository;
        private readonly BiometricDbContext _context;

        public TimeRecordService(BiometricDbContext context)
        {
            _context = context;
            _recordRepository = new Repository<TimeRecord>(context);
            _employeeRepository = new Repository<Employee>(context);
        }

        /// <summary>
        /// Registra entrada ou saída de um funcionário
        /// </summary>
        public async Task<TimeRecord> RegisterTimeAsync(int employeeId, string type, string? notes = null)
        {
            // Validar se funcionário existe
            var employee = await _employeeRepository.GetByIdAsync(employeeId);
            if (employee == null || !employee.IsActive)
            {
                throw new InvalidOperationException($"Funcionário com ID {employeeId} não encontrado ou inativo.");
            }

            var timeRecord = new TimeRecord
            {
                EmployeeId = employeeId,
                Timestamp = DateTime.Now,
                Type = type, // "Entrada" ou "Saída"
                Notes = notes
            };

            await _recordRepository.AddAsync(timeRecord);
            await _recordRepository.SaveChangesAsync();

            return timeRecord;
        }

        /// <summary>
        /// Obtém registros de ponto de um funcionário em um período
        /// </summary>
        public async Task<List<TimeRecord>> GetTimeRecordsByEmployeeAsync(int employeeId, 
            DateTime startDate, DateTime endDate)
        {
            return await _recordRepository.FindAsync(t =>
                t.EmployeeId == employeeId &&
                t.Timestamp >= startDate &&
                t.Timestamp <= endDate
            );
        }

        /// <summary>
        /// Obtém todos os registros de um dia específico
        /// </summary>
        public async Task<List<TimeRecord>> GetTimeRecordsByDateAsync(DateTime date)
        {
            var startOfDay = date.Date;
            var endOfDay = startOfDay.AddDays(1);

            return await _recordRepository.FindAsync(t =>
                t.Timestamp >= startOfDay &&
                t.Timestamp < endOfDay
            );
        }

        /// <summary>
        /// Obtém último registro de ponto de um funcionário
        /// </summary>
        public async Task<TimeRecord?> GetLastTimeRecordAsync(int employeeId)
        {
            var records = await _recordRepository.FindAsync(t => t.EmployeeId == employeeId);
            return records.OrderByDescending(t => t.Timestamp).FirstOrDefault();
        }

        /// <summary>
        /// Obtém registros de ponto para gerar relatório
        /// </summary>
        public async Task<List<TimeRecord>> GetTimeRecordsForReportAsync(
            DateTime startDate, DateTime endDate, int? employeeId = null)
        {
            var query = await _recordRepository.FindAsync(t =>
                t.Timestamp >= startDate && t.Timestamp <= endDate
            );

            if (employeeId.HasValue)
            {
                query = query.Where(t => t.EmployeeId == employeeId).ToList();
            }

            return query.OrderBy(t => t.Timestamp).ToList();
        }

        /// <summary>
        /// Calcula horas trabalhadas em um período
        /// </summary>
        public async Task<Dictionary<int, TimeSpan>> CalculateWorkedHoursAsync(
            DateTime startDate, DateTime endDate)
        {
            var records = await GetTimeRecordsForReportAsync(startDate, endDate);
            var workedHours = new Dictionary<int, TimeSpan>();

            var groupedByEmployee = records.GroupBy(r => r.EmployeeId);

            foreach (var group in groupedByEmployee)
            {
                var employeeRecords = group.OrderBy(r => r.Timestamp).ToList();
                var totalHours = TimeSpan.Zero;

                for (int i = 0; i < employeeRecords.Count - 1; i += 2)
                {
                    if (employeeRecords[i].Type == "Entrada" && 
                        i + 1 < employeeRecords.Count && 
                        employeeRecords[i + 1].Type == "Saída")
                    {
                        var duration = employeeRecords[i + 1].Timestamp - employeeRecords[i].Timestamp;
                        if (duration.TotalHours > 0)
                        {
                            totalHours += duration;
                        }
                    }
                }

                workedHours[group.Key] = totalHours;
            }

            return workedHours;
        }

        /// <summary>
        /// Deleta um registro de ponto (apenas admin)
        /// </summary>
        public async Task DeleteTimeRecordAsync(int recordId)
        {
            await _recordRepository.DeleteAsync(recordId);
            await _recordRepository.SaveChangesAsync();
        }
    }
}
*/
}
