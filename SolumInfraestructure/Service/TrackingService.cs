using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using SolumInfraestructure.Domain.DBContext;
using SolumInfraestructure.Domain.Entities;
using SolumInfraestructure.Interface;
using Zebra.Sdk.Comm;
using Zebra.Sdk.Printer;
using Zebra.Sdk.Printer.Discovery;
using static System.Net.Mime.MediaTypeNames;

namespace SolumInfraestructure.Service
{
    public class TrackingService : IPrintSpool
    {
        //List<List<long>> KeySolum = new List<List<long>>();
        public void limpiar_variables()
        {
            eRuta model = new eRuta();
            model.Server = null;
            model.Route = null;
            model.Historic = null;
            model.UserSap = null;
            model.Password = null;
            model.Modelo = null;
        }
        public void ejecucion(eParametros objParametros)
        {
            limpiar_variables();
            Console.WriteLine("Entrando a Configuracion: " + objParametros.proceso);
            if (cargar_configuracion(objParametros))
            {
                Console.WriteLine("Entrando a ruta: " + objParametros.proceso);
                cargar_ruta(objParametros);
            }
        }
        public bool cargar_configuracion(eParametros objParametros)
        {
            bool resp = new bool();
            DataContextDB trackingObjects = new DataContextDB();
            try
            {
                resp = trackingObjects.cargar_configuracion(objParametros);
            }
            catch (Exception ex)
            {
                resp = false;
                throw new Exception(ex.Message);
            }
            return resp;
        }
        public List<eRuta> cargar_ruta(eParametros objParametros)
        {
            DataContextDB trackingObjects = new DataContextDB();
            List<eRuta> resp = new List<eRuta>();
            List<eFile> file = new List<eFile>();
            bool Resultado = false;
            try
            {
                resp = trackingObjects.cargar_ruta(objParametros);
                foreach (var ruta in resp)
                {
                    file = cargar_file(objParametros, ruta);
                    foreach (var efile in file) {
                        Resultado = resultado(file, ruta,objParametros);
                    }
                    if (ruta.Email == true) {
                        if (Resultado == true)
                        {
                            trackingObjects.sendemail(objParametros,ruta);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return resp;
        }
        public List<eFile> cargar_file(eParametros objParametros, eRuta objRuta)
        {
            DataContextDB trackingObjects = new DataContextDB();
            List<eFile> file = new List<eFile>();
            List<List<string>> lista = new List<List<string>>();
            try
            {
                file = trackingObjects.cargar_file(objParametros, objRuta);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return file;
        }
        public bool resultado(List<eFile> objFile, eRuta objRuta, eParametros objParametros)
        {
            bool Resultado = false;
            bool Resultado_Final = false;
            List<List<string>> lista = new List<List<string>>();
            DataContextDB trackingObjects = new DataContextDB();
            string company = trackingObjects.cargar_company(objParametros);

            foreach (var file in objFile)
            {
                if (String.IsNullOrEmpty(file.Ordenamiento))
                {
                    Resultado = obtener_datos(company, objParametros,file,objRuta,0);
                    if (Resultado == true)
                    {
                        Resultado_Final = true;
                    }
                }
                else
                {
                    lista.Add(new List<string> { file.Prefix, file.Extent, file.Separator, file.Destino, file.Ordenamiento });
                }
            }
            if (objFile.Count == 1 && !String.IsNullOrEmpty(objFile[0].Ordenamiento))
            {
                eFile objFile_ = new eFile();
                objFile_ = objFile[0];
                    Resultado = obtener_datos(company,objParametros, objFile_, objRuta,0);
                if (Resultado == true)
                {
                    Resultado_Final = true;
                }
            }
            else if (objFile.Count > 1 && lista.Count > 0)
            {
                List<string> detalle_lista = new List<string>();
                detalle_lista = lista[0];
                string[] files = Directory.GetFiles(objRuta.Route, detalle_lista[0] + "*." + detalle_lista[1]);
                foreach (string names in files)
                {
                    Resultado = recorrer_grupo_archivo(company, objParametros, objRuta, lista, Path.GetFileName(names));
                    if (Resultado == true)
                    {
                        Resultado_Final = true;
                    }
                }
            }
            return Resultado_Final;
        }
        private bool recorrer_grupo_archivo(string company, eParametros objParametros, eRuta objRuta, List<List<string>> lista, string nombre)
        {
            List<List<string>> lista_trabajo = new List<List<string>>();
            string name_prefijo = null;
            string nombre_final = null;
            string[] files;
            int i = 0;
            bool estado = true;
            bool Resultado = false;
            bool Resultado_Final = false;
            foreach (List<string> detalle_lista in lista)
            {
                if (i == 0)
                {
                    files = Directory.GetFiles(objRuta.Route, nombre);
                }
                else
                {
                    nombre_final = detalle_lista[0] + name_prefijo;
                    files = Directory.GetFiles(objRuta.Route, nombre_final);
                    if (files.Count() == 0)
                    {
                        estado = false;
                        break;
                    }
                }
                name_prefijo = Path.GetFileName(files[0]).Substring(detalle_lista[0].Length, Path.GetFileName(files[0]).Length - detalle_lista[0].Length);
                lista_trabajo.Add(new List<string> { detalle_lista[0], name_prefijo, detalle_lista[2], detalle_lista[3], detalle_lista[4] });
                i++;
            }
            if (estado == true)
            {
                foreach (List<string> procesar_lista in lista_trabajo)
                {
                    eFile objFile_ =  new eFile();
                    objFile_.Prefix = procesar_lista[0];
                    objFile_.Extent = procesar_lista[1];
                    objFile_.Separator = procesar_lista[2];
                    objFile_.Destino = procesar_lista[3];
                    objFile_.Ordenamiento = procesar_lista[4];
                    Resultado = obtener_datos(company, objParametros, objFile_,objRuta, 1);
                    if (Resultado == true)
                    {
                        Resultado_Final = true;
                    }
                }
                //KeySolum.Clear();
            }
            return Resultado_Final;
        }
        private bool obtener_datos(string company, eParametros objParametros,eFile objFile,eRuta objRuta,int modo)
        {
            bool Resultado = false;
            DataContextDB trackingObjects = new DataContextDB();
            string[] files;
            if (modo == 0)
            {
                files = Directory.GetFiles(objRuta.Route, objFile.Prefix + "*." + objFile.Extent);
            }
            else
            {
                files = Directory.GetFiles(objRuta.Route, objFile.Prefix + objFile.Extent);
            }
            foreach (string names in files)
            {
                long q = 0;
                string namefile = Path.GetFileName((names));
                try
                {
                    using (System.IO.StreamReader file = new StreamReader(objRuta.Route + namefile,true))
                    {
                        string line = null;
                        string sql = null;
                        int i = 0;
                        if (objParametros.tipo == 0)
                        {
                            sql = "insert into " + objFile.Destino + " (";
                        }
                        else
                        {
                            sql = "insert into " + objFile.Destino + " (HostGroupID, Sociedad, U_EX_CLIENTE,";
                        }
                        string sql_campos = null;
                        string sql_values = null;
                        string strQuote = "\"";
                        while (!file.EndOfStream)
                        {
                            if ((line = file.ReadLine()) != null)
                            {
                                string[] words = line.Split('\t');
                                int j = 0;
                                foreach (string s in words)
                                {
                                    if (i == objParametros.titulo)
                                    {
                                        if (string.IsNullOrEmpty((sql_campos)))
                                        {
                                            sql_campos = strQuote + s.ToString().Replace("'", "''").Trim() + strQuote;
                                        }
                                        else
                                        {
                                            sql_campos = sql_campos + "," + strQuote + s.ToString().Replace("'", "''").Trim() + strQuote;
                                        }
                                    }
                                    if (i >= objParametros.datos)
                                    {
                                        if (string.IsNullOrEmpty(sql_values))
                                        {
                                            sql_values = objParametros.tipo == 1 
                                                ? " select " + "'" + objParametros.hostgroupid + "','" + company + "','" + objParametros.cliente + "','" 
                                                : " select ";
                                            if (!string.IsNullOrEmpty(s.Trim()))
                                            {
                                                sql_values = sql_values + s.ToString().Replace("'", "''").Trim() + "'";
                                            }
                                            else
                                            {
                                                sql_values = sql_values + "null";

                                            }
                                        }
                                        else {
                                            if (!string.IsNullOrEmpty(s.Trim()))
                                            {
                                                sql_values = sql_values + "," + "'" + s.ToString().Replace("'", "''").Trim() + "'";
                                            }
                                            else
                                            {
                                                sql_values = sql_values + "," + "null";

                                            }
                                        }
                                    }
                                    j = j + 1;
                                }
                                if (i == 0)
                                {
                                    sql = sql + sql_campos + ")";
                                }
                                if (i == 1)
                                {
                                    sql = sql + sql_values;
                                    sql_values = string.Empty;
                                }
                                if (i > 1)
                                {
                                    sql = sql + " union all " + sql_values;
                                    sql_values = string.Empty;
                                }
                                i = i + 1;
                            }
                        }
                        Console.WriteLine(sql);
                        if (trackingObjects.EjecutarScript(sql, objParametros.cliente) == true)
                        {
                            Console.WriteLine("Ejecutar Tracking");
                            trackingObjects.EjecutarTracking(objParametros);
                        }
                        file.Close();
                    }
                    if (!System.IO.File.Exists(objRuta.Historic + namefile))
                    {
                        System.IO.File.Move(objRuta.Route + namefile, objRuta.Historic+ namefile);
                        trackingObjects.RegistraEvento(objParametros, namefile, null, 1, null, "Envío correcto de interfaz");
                    }
                    else
                    {
                        System.IO.File.Move(objRuta.Route + namefile, objRuta.Historic + namefile + "-" + DateTime.Now.ToString("yyyyMMddHHmmssfff"));
                        trackingObjects.RegistraEvento(objParametros, namefile, null, 1, null, "Envío correcto de datos - archivo renombrado");
                    }
                }
                catch (Exception e)
                {
                    if (!System.IO.File.Exists(objRuta.LogErr + namefile))
                    {
                        try
                        {
                            System.IO.File.Move(objRuta.Route + namefile, objRuta.LogErr + namefile);
                            trackingObjects.RegistraEvento(objParametros, namefile, null, 0, e.Message.ToString(), "Error en envío de datos");
                            Resultado = true;
                        }
                        catch (Exception y)
                        {
                            trackingObjects.RegistraEvento(objParametros, namefile, null, 0, e.Message.ToString() + " - " + y.Message.ToString(), "Error al copiar el txt a la ruta LOG");
                            Resultado = true;
                        }
                    }
                    else
                    {
                        try
                        {
                            System.IO.File.Move(objRuta.Route + namefile, objRuta.LogErr + namefile + "-" + DateTime.Now.ToString("yyyyMMddHHmmssfff"));
                            trackingObjects.RegistraEvento(objParametros, namefile, null, 0, e.Message.ToString(), "Error en envío de datos - archivo renombrado");
                            Resultado = true;
                        }
                        catch (Exception y)
                        {
                            trackingObjects.RegistraEvento(objParametros, namefile, null, 0, e.Message.ToString() + " - " + y.Message.ToString(), "Error al copiar el txt renombrado a la ruta LOG");
                            Resultado = true;
                        }
                    }
                }
            }
            return Resultado;
        }
    }
}
