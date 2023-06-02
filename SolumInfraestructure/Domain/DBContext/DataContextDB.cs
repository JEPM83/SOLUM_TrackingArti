using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using SolumInfraestructure.Domain.Entities;
using SolumInfraestructure.Interface;
using System.Data;
using System.Data.SqlClient;
using static System.Net.WebRequestMethods;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Reflection;

namespace SolumInfraestructure.Domain.DBContext
{
    public class DataContextDB:IPrintSpool
    {
        protected string cnxStringCRM = ConfigurationManager.ConnectionStrings["conexionCRM"].ToString();

        public bool cargar_configuracion(eParametros parametros)
        {
            using (SqlConnection conn = new SqlConnection(cnxStringCRM))
            using (SqlCommand cmd = new SqlCommand("select Code from [@EX_PROCESO] where Code = '" + parametros.proceso + "' and state = 0", conn))
            {
                SqlDataReader rd = null;
                cmd.CommandType = CommandType.Text;
                conn.Open();
                int i = 0;
                try
                {
                    rd = cmd.ExecuteReader();
                    while (rd.Read())
                    {
                        i++;
                    }
                    conn.Close();
                    rd.Close();
                    if (i == 0)
                    {
                        return false;
                    }
                    else
                    {
                        Console.WriteLine("Entro a configuracion [@EX_PROCESO]!!: " + parametros.proceso + "");
                        return true;
                    }
                }
                finally
                {
                    conn.Close();
                    rd.Close();
                }
            }
            
        }
        public List<eRuta> cargar_ruta(eParametros parametros)
        {
            var eFtpListDetail = new List<eRuta>();
            using (SqlConnection conn = new SqlConnection(cnxStringCRM))
            using (SqlCommand cmd = new SqlCommand("select Code,Server,Port,Route,Sftp,Historic,UserSap,Password,Email,Attached,U_EX_MODELO,Zip,Subject,Environment,LogErr from [@EX_RUTA] where U_EX_CLIENTE = '" + parametros.cliente + "' and U_EX_PROCESO = '" + parametros.proceso + "' and state = 0", conn))
            {
                SqlDataReader rd = null;
                cmd.CommandType = CommandType.Text;
                conn.Open();
                int i = 0;
                try
                {
                    rd = cmd.ExecuteReader();
                    while (rd.Read())
                    {
                        i++;
                        var eFtpDetail =  new eRuta();
                        eFtpDetail.Code = int.Parse(rd.GetValue(0).ToString());
                        eFtpDetail.Server = rd.GetValue(1).ToString();
                        eFtpDetail.Port = string.IsNullOrEmpty(rd.GetValue(2).ToString()) ? 0 : int.Parse(rd.GetValue(2).ToString());
                        eFtpDetail.Route = rd.GetValue(3).ToString();
                        eFtpDetail.Sftp = rd.GetBoolean(4);
                        eFtpDetail.Historic = rd.GetValue(5).ToString();
                        eFtpDetail.UserSap = rd.GetValue(6).ToString();
                        eFtpDetail.Password = rd.GetValue(7).ToString();
                        eFtpDetail.Email = rd.GetBoolean(8);
                        eFtpDetail.Attached = rd.GetBoolean(9);
                        eFtpDetail.LogErr = rd.GetValue(14).ToString();
                        eFtpDetail.Subject = rd.GetValue(12).ToString();
                        eFtpListDetail.Add(eFtpDetail);
                    }

                    if (i == 0)
                    {
                        //MessageBox.Show("RUTA no configurada o desactivada en proceso de INVENTORY");
                    }
                    rd.Close();
                    conn.Close();
                }
                finally
                {
                    rd.Close();
                    conn.Close();
                }
                return eFtpListDetail;
            }
        }
        public List<eFile> cargar_file(eParametros parametros,eRuta ruta)
        {
            var eFileListDetail = new List<eFile>();
            using (SqlConnection conn = new SqlConnection(cnxStringCRM))
            using (SqlCommand cmd = new SqlCommand("select Prefix,Extent,Separator,Destino,[Order] from [@EX_FILE] where U_EX_RUTA = " + ruta.Code + " and state = 0 order by [Order]", conn))
            {
                SqlDataReader rd = null;
                cmd.CommandType = CommandType.Text;
                conn.Open();
                int i = 0;
                Console.WriteLine("Entrando a obtener archivo");
                try
                {
                    rd = cmd.ExecuteReader();
                    List<List<string>> lista = new List<List<string>>();
                    while (rd.Read())
                    {
                        i++;
                        var eFileDetail = new eFile();
                        eFileDetail.Prefix = rd.GetValue(0).ToString();
                        eFileDetail.Extent = rd.GetValue(1).ToString();
                        eFileDetail.Separator = rd.GetValue(2).ToString();
                        eFileDetail.Destino = rd.GetValue(3).ToString();
                        eFileDetail.Ordenamiento = rd.GetValue(4).ToString();
                        eFileListDetail.Add(eFileDetail);
                        Console.WriteLine("Archivo " + eFileDetail.Prefix);
                    }
                    rd.Close();
                    conn.Close();
                    return eFileListDetail;
                }
                finally
                {
                    rd.Close();
                    conn.Close();
                }
            }
        }
        public string cargar_company(eParametros parametros)
        {
            using (SqlConnection conn = new SqlConnection(cnxStringCRM))
            using (SqlCommand cmd = new SqlCommand("select Code, Class, Name,Company from [@EX_CLIENTE] where state = 0 and code = '" + parametros.cliente + "'", conn))
            {
                SqlDataReader rd = null;
                cmd.CommandType = CommandType.Text;
                conn.Open();
                string Ccompany = null;
                try
                {
                    rd = cmd.ExecuteReader();
                    while (rd.Read())
                    {
                        Ccompany = rd.GetValue(3).ToString();
                    }
                    rd.Close();
                    conn.Close() ;
                    return Ccompany;
                }
                finally
                {
                    rd.Close();
                    conn.Close();
                }
            }
        }
        public bool sendemail(eParametros objParametros, eRuta objRuta)
        {
            bool Resultado = false;
            if (objRuta.Email == true)
            {
                using (SqlConnection conn = new SqlConnection(cnxStringCRM))
                using (SqlCommand cmd = new SqlCommand("select Email,Password,Type from [@EX_CONTACT] where U_EX_RUTA = " + objRuta.Code + " and state = 0", conn))
                {
                    SqlDataReader rd = null;
                    SqlDataReader rd_log = null;
                    cmd.CommandType = CommandType.Text;
                    conn.Open();
                    System.Data.DataTable dtSap;
                    int i = 0;
                    int j = 0;
                    try
                    {
                        rd = cmd.ExecuteReader();
                        dtSap = new System.Data.DataTable() { TableName = "Contact" };
                        System.Net.Mail.MailMessage mmsg = new System.Net.Mail.MailMessage();
                        System.Net.Mail.SmtpClient clienteC = new System.Net.Mail.SmtpClient();
                        mmsg.Subject = "OPERADOR SOLUM: Tablas intermedias - Proceso: (" + objParametros.proceso + ") - " + objRuta.Subject + " " + cargar_company(objParametros) + " " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        mmsg.SubjectEncoding = System.Text.Encoding.UTF8;
                        while (rd.Read())
                        {
                            i++;
                            if (int.Parse(rd.GetValue(2).ToString()) == 0)
                            {
                                j++;
                                //Correo emisor
                                mmsg.From = new System.Net.Mail.MailAddress(rd.GetValue(0).ToString());
                                //Credenciales
                                clienteC.Credentials = new System.Net.NetworkCredential(rd.GetValue(0).ToString(), rd.GetValue(1).ToString());
                                //Si es gmail
                                clienteC.Port = 587;
                                clienteC.EnableSsl = true;
                                clienteC.Host = "smtp.gmail.com";
                            }
                            else if (int.Parse(rd.GetValue(2).ToString()) == 1)
                            {
                                mmsg.To.Add(rd.GetValue(0).ToString());
                            }
                            else if (int.Parse(rd.GetValue(2).ToString()) == 2)
                            {
                                mmsg.CC.Add(rd.GetValue(0).ToString());
                            }
                            else if (int.Parse(rd.GetValue(2).ToString()) == 3)
                            {
                                mmsg.Bcc.Add(rd.GetValue(0).ToString());
                            }
                        }
                        rd.Close();
                        conn.Close();
                        //Lista de interfaces
                        using (SqlCommand cmd_log = new SqlCommand("select isnull([LogErr] ,'') , isnull(FileName,''),MessageSystem  from [@EX_LOG] where HostGroupId = '" + objParametros.hostgroupid + "' and State = 0", conn))
                        {
                            rd_log = cmd_log.ExecuteReader();
                            cmd_log.CommandType = CommandType.Text;
                            conn.Open();
                            string mensaje = null;
                            while (rd_log.Read())
                            {
                                //Environment.NewLine
                                if (objRuta.Attached == true)
                                {
                                    mensaje = mensaje + "ARCHIVO: " + rd_log.GetString(0) + rd_log.GetString(1) + "  ERROR: " + rd_log.GetString(2) + Environment.NewLine;
                                    System.Net.Mail.Attachment attachment = new System.Net.Mail.Attachment(rd_log.GetString(0) + rd_log.GetString(1));
                                    mmsg.Attachments.Add(attachment);
                                }
                            }
                            //
                            mmsg.Body = "Se ha encontrado errores en los siguientes archivos: " + Environment.NewLine + Environment.NewLine + mensaje + Environment.NewLine + "Identificador: " + objParametros.hostgroupid;
                            mmsg.BodyEncoding = System.Text.Encoding.UTF8;
                            mmsg.IsBodyHtml = false;
                            mmsg.Priority = System.Net.Mail.MailPriority.Normal;

                            if (j == 1)
                            {
                                try
                                {
                                    clienteC.Send(mmsg);
                                    Resultado = true;
                                }
                                catch (System.Net.Mail.SmtpException ex)
                                {
                                    //MessageBox.Show(ex.Message.ToString());
                                    //Inscribir error
                                    Resultado = false;
                                }
                                if (i == 0)
                                {
                                    //MessageBox.Show("DESTINATARIOS no configurados o desactivados en proceso de INVENTORY");
                                    //Inscribir error
                                }
                            }
                            else
                            {
                                //MessageBox.Show("Correo emisor no configurado en proceso INVENTORY");
                                //Inscribir error
                            }
                            ////
                            rd.Close();
                            conn.Close();
                        }
                    }
                    catch (Exception e)
                    {
                        rd.Close();
                        conn.Close();
                    }
                }
               
            }
            return Resultado;
        }
        public bool EjecutarScript(string strSql, string cliente)
        {
            bool Resultado = false;
            using (SqlConnection conn = new SqlConnection(cnxStringCRM))
            using (SqlCommand cmd = new SqlCommand("begin tran C" + cliente + " " + strSql + " commit tran C" + cliente, conn))
            {
                conn.Open();
                cmd.CommandTimeout = 1000;
                cmd.ExecuteNonQuery();
                Resultado = true;
                conn.Close();
            }
            return Resultado;
        }
        public bool RegistraEvento(eParametros objParametros, string fileName, string referencia, int status, string messageSystem, string message)
        {
            bool Resultado = false;
            try
            {
                using (SqlConnection conn = new SqlConnection(cnxStringCRM))
                using (SqlCommand cmd = new SqlCommand("Sp_registareventosolum", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@HostGroupId", SqlDbType.VarChar).Value = objParametros.hostgroupid == null ? string.Empty : objParametros.hostgroupid; ;
                    cmd.Parameters.Add("@Cliente", SqlDbType.NVarChar).Value = objParametros.cliente;
                    cmd.Parameters.Add("@ProcessName", SqlDbType.NVarChar).Value = objParametros.proceso;
                    cmd.Parameters.Add("@FileName", SqlDbType.VarChar).Value = fileName == null ? System.Data.SqlTypes.SqlString.Null : fileName;
                    cmd.Parameters.Add("@Reference", SqlDbType.VarChar).Value = referencia == null ? string.Empty : referencia;
                    cmd.Parameters.Add("@Status", SqlDbType.Int).Value = status;
                    cmd.Parameters.Add("@MessageSystem", SqlDbType.VarChar).Value = messageSystem == null ? System.Data.SqlTypes.SqlString.Null : messageSystem;
                    cmd.Parameters.Add("@Message", SqlDbType.VarChar).Value = message == null ? System.Data.SqlTypes.SqlString.Null : message;
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    Resultado = true;
                    conn.Close();
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.ToString());
            }
            return Resultado;
        }
        public bool EjecutarTracking(eParametros objParametros)
        {
            bool Resultado = false;
            try
            {
                using (SqlConnection conn = new SqlConnection(cnxStringCRM))
                using (SqlCommand cmd = new SqlCommand(ObjectsDA.EjecutarTracking, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@HostGroupId", SqlDbType.VarChar).Value = objParametros.hostgroupid == null ? string.Empty : objParametros.hostgroupid; ;
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    Resultado = true;
                    conn.Close();
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.ToString());
            }
            return Resultado;
        }
    }
    }
