using System;
using SolumInfraestructure.Service;
using SolumInfraestructure.Domain.Entities;

namespace SolumTrackingService
{
    public class Program
    {
        public void Servicio() {
            Console.WriteLine("Iniciando servicio de tracking de cliente ARTI!.");

            TrackingService obj = new TrackingService();
            eParametros model = new eParametros();
            
            try
            {
                string hostgroupid = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                model.cliente = "20100310288";
                model.proceso = "SAP_PER_TRACKING";
                //model.proceso = "SAP_INT_DELIVERY";
                model.tipo = 1;
                model.titulo = 0; //Numero de fila con los datos de cabecera (el conteo empieza en 0)
                model.datos = 1; //Numero de fila donde arranca los datos (el conteo empieza en 0)
                model.hostgroupid = hostgroupid;
                obj.ejecucion(model);
            }
            catch (Exception ex)
            {
                obj = null;
                model = null;
                throw new Exception(ex.Message);
            }
        }
        static void Main(string[] args)
        {
           
        }


    }
}
