using System.Linq;

namespace BiometricSystem.Database
{
    public static class ParametrosHelper
    {
        public static ParametrosSistema GetParametros()
        {
            using (var db = new AppDbContext())
            {
                var parametros = db.Parametros.FirstOrDefault();
                if (parametros == null)
                {
                    parametros = new ParametrosSistema { IntervaloAtivo = true, MinutosIntervalo = 60 };
                    db.Parametros.Add(parametros);
                    db.SaveChanges();
                }
                return parametros;
            }
        }

        public static void SalvarParametros(bool intervaloAtivo, int minutosIntervalo)
        {
            using (var db = new AppDbContext())
            {
                var parametros = db.Parametros.FirstOrDefault();
                if (parametros == null)
                {
                    parametros = new ParametrosSistema { IntervaloAtivo = intervaloAtivo, MinutosIntervalo = minutosIntervalo };
                    db.Parametros.Add(parametros);
                }
                else
                {
                    parametros.IntervaloAtivo = intervaloAtivo;
                    parametros.MinutosIntervalo = minutosIntervalo;
                }
                db.SaveChanges();
            }
        }
    }
}
