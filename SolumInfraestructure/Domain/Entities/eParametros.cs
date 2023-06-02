using System;
using System.Collections.Generic;
using System.Text;

namespace SolumInfraestructure.Domain.Entities
{
    public class eParametros
    {
        private string _cliente;
        private string _proceso;
        private int _tipo;
        private string _hostgroupid;
        private int _titulo;
        private int _datos;
        public string cliente { get => _cliente; set => _cliente = value; }
        public string proceso { get => _proceso; set => _proceso = value; }
        public int tipo { get => _tipo; set => _tipo = value; }
        public int titulo { get => _titulo; set => _titulo = value; }
        public int datos { get => _datos; set => _datos = value; }
        public string hostgroupid { get => _hostgroupid; set => _hostgroupid = value; }
    }
}
