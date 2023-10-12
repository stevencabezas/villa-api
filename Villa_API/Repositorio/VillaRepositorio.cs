using Villa_API.Data;
using Villa_API.Models;
using Villa_API.Repositorio.IRepositorio;

namespace Villa_API.Repositorio
{
    public class VillaRepositorio : Repositorio<Villa>, IVillaRepositorio
    {
        private readonly ApplicationDbContext _db;
        public VillaRepositorio(ApplicationDbContext db): base(db)
        {
            _db = db;
        }
        public async Task<Villa> Actualizar(Villa villa)
        {
            villa.FechaActualizacion = DateTime.Now;
            _db.Villas.Update(villa);
            await _db.SaveChangesAsync();
            return villa;
        }
    }
}
