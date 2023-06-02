using System;
using System.Collections.Generic;
using System.Text;

namespace SolumInfraestructure.Domain.Entities
{
    public class eFile
    {
        public string _Prefix;
        public string _Extent;
        public string _Separator;
        public string _Destino;
        public string _Ordenamiento;
        public string Prefix { get => _Prefix; set => _Prefix = value; }
        public string Extent { get => _Extent; set => _Extent = value; }
        public string Separator { get => _Separator; set => _Separator = value; }
        public string Destino { get => _Destino; set => _Destino = value; }
        public string Ordenamiento { get => _Ordenamiento; set => _Ordenamiento = value; }
    }
}
