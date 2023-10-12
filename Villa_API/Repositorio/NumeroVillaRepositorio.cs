using Villa_API.Data;
using Villa_API.Models;
using Villa_API.Repositorio.IRepositorio;

namespace Villa_API.Repositorio
{
    public class NumeroVillaRepositorio : Repositorio<NumeroVilla>, INumeroVillaRepositorio
    {
        private readonly ApplicationDbContext _db;
        public NumeroVillaRepositorio(ApplicationDbContext db): base(db)
        {
            _db = db;
        }
        public async Task<NumeroVilla> Actualizar(NumeroVilla numeroVilla)
        {
            numeroVilla.FechaActualizacion = DateTime.Now;
            _db.NumeroVillas.Update(numeroVilla);
            await _db.SaveChangesAsync();
            return numeroVilla;
        }
    }
}
