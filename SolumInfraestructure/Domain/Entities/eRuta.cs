using System;
using System.Collections.Generic;
using System.Text;

namespace SolumInfraestructure.Domain.Entities
{
    public class eRuta
    {
        public string _Server;
        public int _Port;
        public string _Route;
        public bool _Sftp;
        public string _Historic;
        public string _UserSap;
        public string _Password;
        public bool _Email;
        public bool _Attached;
        public int _Code;
        public string _Modelo;
        public string _LogErr;
        public string _Subject;
        public string Server { get => _Server; set => _Server = value; }
        public int Port { get => _Port; set => _Port = value; }
        public string Route { get => _Route; set => _Route = value; }
        public bool Sftp { get => _Sftp; set => _Sftp = value; }
        public string Historic { get => _Historic; set => _Historic = value; }
        public string UserSap { get => _UserSap; set => _UserSap = value; }
        public string Password { get => _Password; set => _Password = value; }
        public bool Email { get => _Email; set => _Email = value; }
        public bool Attached { get => _Attached; set => _Attached = value; }
        public int Code { get => _Code; set => _Code = value; }
        public string Modelo { get => _Modelo; set => _Modelo = value; }
        public string LogErr { get => _LogErr; set => _LogErr = value; }
        public string Subject { get => _Subject; set => _Subject = value; }
    }
}
