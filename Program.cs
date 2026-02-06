using BiometricSystem.Forms;
using Microsoft.Extensions.Configuration;

namespace BiometricSystem
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            try
            {
                ApplicationConfiguration.Initialize();

                // Caminho seguro para configuração do usuário
                string? appDataRoot = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                if (string.IsNullOrEmpty(appDataRoot))
                    appDataRoot = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                if (string.IsNullOrEmpty(appDataRoot))
                    appDataRoot = "C:\\Temp";
                string appDataDir = Path.Combine(appDataRoot, "BiometricSystem");
                Directory.CreateDirectory(appDataDir);
                string appSettingsPath = Path.Combine(appDataDir, "appsettings.json");

                // Se não existir em %APPDATA%, copiar modelo do diretório do executável
                if (!File.Exists(appSettingsPath))
                {
                    string exeDir = AppDomain.CurrentDomain.BaseDirectory ?? "C:\\Temp";
                    string modelPath = Path.Combine(exeDir, "appsettings.json");
                    if (File.Exists(modelPath))
                    {
                        try
                        {
                            File.Copy(modelPath, appSettingsPath, true);
                            System.Diagnostics.Debug.WriteLine($"✅ appsettings.json copiado de {modelPath} para {appSettingsPath}");
                        }
                        catch (Exception copyEx)
                        {
                            System.Diagnostics.Debug.WriteLine($"❌ Erro ao copiar appsettings.json: {copyEx.Message}");
                            // Se falhar, cria um novo padrão com Turso
                            File.WriteAllText(appSettingsPath, 
                                "{\n" +
                                "  \"ConnectionStrings\": {\n" +
                                "    \"LocalDb\": \"Data Source=%LOCALAPPDATA%\\\\BiometricSystem\\\\biometric.db\"\n" +
                                "  },\n" +
                                "  \"TursoDb\": {\n" +
                                "    \"Url\": \"libsql://idev-bd-007-sistemas.aws-us-east-1.turso.io\",\n" +
                                "    \"AuthToken\": \"\"\n" +
                                "  },\n" +
                                "  \"Hospital\": { \"Id\": \"\", \"Name\": \"\", \"LocalHospitalId\": \"\" },\n" +
                                "  \"Logging\": { \"LogLevel\": { \"Default\": \"Information\" } }\n" +
                                "}\n");
                        }
                    }
                    else
                    {
                        File.WriteAllText(appSettingsPath, 
                            "{\n" +
                            "  \"ConnectionStrings\": {\n" +
                            "    \"LocalDb\": \"Data Source=%LOCALAPPDATA%\\\\BiometricSystem\\\\biometric.db\"\n" +
                            "  },\n" +
                            "  \"TursoDb\": {\n" +
                            "    \"Url\": \"libsql://idev-bd-007-sistemas.aws-us-east-1.turso.io\",\n" +
                            "    \"AuthToken\": \"\"\n" +
                            "  },\n" +
                            "  \"Hospital\": { \"Id\": \"\", \"Name\": \"\", \"LocalHospitalId\": \"\" },\n" +
                            "  \"Logging\": { \"LogLevel\": { \"Default\": \"Information\" } }\n" +
                            "}\n");
                    }
                }

                // Carregar configurações do appsettings.json
                var config = new ConfigurationBuilder()
                    .AddJsonFile(Path.Combine(appDataDir, "appsettings.json"), optional: true, reloadOnChange: true)
                    .Build();

                // Verificar se a unidade está configurada
                var hospitalId = config["Hospital:Id"];
                if (string.IsNullOrEmpty(hospitalId))
                {
                    // Tentar mostrar tela de configuração de unidade (Turso)
                    try
                    {
                        using (var configForm = new ConfigurarHospitalForm(config))
                        {
                            if (configForm.ShowDialog() != DialogResult.OK)
                            {
                                // Usuário cancelou, fechar aplicação
                                return;
                            }
                        }

                        // Recarregar configuração após salvar
                        config = new ConfigurationBuilder()
                            .AddJsonFile(Path.Combine(appDataDir, "appsettings.json"), optional: true, reloadOnChange: true)
                            .Build();
                    }
                    catch (Exception configEx)
                    {
                        // Se falhar ao configurar, mostre o erro e permita continuar em modo offline
                        var result = MessageBox.Show(
                            $"Não foi possível conectar ao Turso:\n\n{configEx.Message}\n\n" +
                            "Deseja continuar em modo offline?\n\n" +
                            "Nota: Sem a configuração de unidade, o sistema funcionará apenas com o banco local.",
                            "Erro de Configuração Turso",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Warning
                        );

                        if (result != DialogResult.Yes)
                        {
                            return;
                        }
                    }
                }

                // Passar configuração para a form
                Application.Run(new LoginForm(config));
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Erro fatal ao inicializar a aplicação:\n\n{ex.Message}\n\nStack trace:\n{ex.StackTrace}",
                    "Erro de Inicialização",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
    }
}

