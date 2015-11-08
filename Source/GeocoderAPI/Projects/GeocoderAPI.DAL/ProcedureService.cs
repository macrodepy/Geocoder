using GeocoderAPI.Model;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data;
namespace GeocoderAPI.DAL
{
    public class ProcedureService
    {
        OracleConnection oraConn = null;
        OracleDataAdapter oraDa = null;
        OracleCommand oraCmd = null;

        public ProcedureService()
        {
            //var connection =
            //    System.Configuration.ConfigurationManager.ConnectionStrings["LocalOracleDbConnection"].ConnectionString;

            oraConn = new OracleConnection("Data Source=LOCALDB;User ID=MUSTAFALOCAL;Password=mustafa;");
            oraCmd = new OracleCommand(); 
        }

        private void checkConn()
        {
            if (oraConn.State == ConnectionState.Closed)
            {
                oraConn.Open();
            }
        }

        public DataSet GetUnitSearchDataByIlAndIlceId(string search, long ilceId)
        {
            checkConn();
            DataSet ds = new DataSet();

            string name = search.Replace(" ", "").Replace(".", "");

            oraCmd = new OracleCommand();
            oraCmd.CommandText = "select * from v_unit_search  where ILCE_ID =" + ilceId + " and name like '" + name + "%'";
            oraCmd.CommandType = CommandType.Text;
            oraCmd.Connection = oraConn;
           
            oraDa = new OracleDataAdapter();
            oraDa.SelectCommand = oraCmd;
            oraDa.Fill(ds);

            if (oraConn.State == ConnectionState.Open)
            {
                oraConn.Close();
            }

            return ds;
        }

        public DataSet GetUnitSearchDataByIlId(string search, long ilId)
        {
            checkConn();
            DataSet ds = new DataSet();

            string name = search.Replace(" ", "").Replace(".", "");

            oraCmd = new OracleCommand();
            oraCmd.CommandText = "select * from v_unit_search  where IL_ID =" + ilId + " and name like '" + name + "%'";
            oraCmd.CommandType = CommandType.Text;
            oraCmd.Connection = oraConn;

            oraDa = new OracleDataAdapter();
            oraDa.SelectCommand = oraCmd;
            oraDa.Fill(ds);

            if (oraConn.State == ConnectionState.Open)
            {
                oraConn.Close();
            }

            return ds;
        }


    }
}
