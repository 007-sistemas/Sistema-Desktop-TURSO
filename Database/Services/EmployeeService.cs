// using BiometricSystem.Database.Repository;
using BiometricSystem.Models;

namespace BiometricSystem.Database.Services
{
    // DESABILITADO: Usar ApiService para web integration
    /*
    public class EmployeeService
    {
        private readonly IRepository<Employee> _repository;
        private readonly BiometricDbContext _context;

        public EmployeeService(BiometricDbContext context)
        {
            _context = context;
            _repository = new Repository<Employee>(context);
        }

        /// <summary>
        /// Registra um novo funcionário com template biométrico
        /// </summary>
        public async Task<Employee> RegisterEmployeeAsync(string name, string cpf, string email, 
            string position, byte[] fingerprintTemplate)
        {
            // Validar CPF duplicado
            var existingEmployee = await _repository.FindAsync(e => e.CPF == cpf);
            if (existingEmployee.Any())
            {
                throw new InvalidOperationException($"Funcionário com CPF {cpf} já existe.");
            }

            var employee = new Employee
            {
                Name = name,
                CPF = cpf,
                Email = email,
                Position = position,
                FingerprintTemplate = fingerprintTemplate,
                CreatedAt = DateTime.Now,
                IsActive = true
            };

            await _repository.AddAsync(employee);
            await _repository.SaveChangesAsync();

            return employee;
        }

        /// <summary>
        /// Obtém funcionário pelo ID
        /// </summary>
        public async Task<Employee?> GetEmployeeByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        /// <summary>
        /// Obtém funcionário pelo CPF
        /// </summary>
        public async Task<Employee?> GetEmployeeByCpfAsync(string cpf)
        {
            var employees = await _repository.FindAsync(e => e.CPF == cpf && e.IsActive);
            return employees.FirstOrDefault();
        }

        /// <summary>
        /// Lista todos os funcionários ativos
        /// </summary>
        public async Task<List<Employee>> GetAllActiveEmployeesAsync()
        {
            return await _repository.FindAsync(e => e.IsActive);
        }

        /// <summary>
        /// Atualiza dados do funcionário
        /// </summary>
        public async Task<Employee> UpdateEmployeeAsync(int id, string name, string email, string position)
        {
            var employee = await _repository.GetByIdAsync(id);
            if (employee == null)
            {
                throw new InvalidOperationException($"Funcionário com ID {id} não encontrado.");
            }

            employee.Name = name;
            employee.Email = email;
            employee.Position = position;

            await _repository.UpdateAsync(employee);
            await _repository.SaveChangesAsync();

            return employee;
        }

        /// <summary>
        /// Desativa um funcionário (soft delete)
        /// </summary>
        public async Task DeactivateEmployeeAsync(int id)
        {
            var employee = await _repository.GetByIdAsync(id);
            if (employee != null)
            {
                employee.IsActive = false;
                await _repository.UpdateAsync(employee);
                await _repository.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Atualiza template biométrico
        /// </summary>
        public async Task UpdateFingerprintAsync(int id, byte[] fingerprintTemplate)
        {
            var employee = await _repository.GetByIdAsync(id);
            if (employee == null)
            {
                throw new InvalidOperationException($"Funcionário com ID {id} não encontrado.");
            }

            employee.FingerprintTemplate = fingerprintTemplate;
            await _repository.UpdateAsync(employee);
            await _repository.SaveChangesAsync();
        }
    }
}
*/
}
