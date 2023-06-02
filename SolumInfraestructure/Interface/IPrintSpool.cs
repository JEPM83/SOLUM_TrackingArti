using System;
using System.Collections.Generic;
using System.Text;
using SolumInfraestructure.Domain.Entities;

namespace SolumInfraestructure.Interface
{
    public interface IPrintSpool
    {
        public bool cargar_configuracion(eParametros obj);
        public List<eRuta> cargar_ruta(eParametros obj);
    }
}
