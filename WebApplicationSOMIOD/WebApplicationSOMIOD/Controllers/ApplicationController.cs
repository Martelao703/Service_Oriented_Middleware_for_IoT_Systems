using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebApplicationSOMIOD.Models;

namespace WebApplicationSOMIOD.Controllers
{
    public class ApplicationController : ApiController
    {
        
        string strDataConnection = $@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=
            {AppDomain.CurrentDomain.BaseDirectory}App_Data\DB_SOMIOD.mdf
            ;Integrated Security=True";

        public List<String> GetApplicationsName()
        {
            List<String> applications = new List<String>();
            SqlConnection conn = null;
            try
            {
                conn = new SqlConnection(strDataConnection);
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT name FROM application ORDER BY Id", conn);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    applications.Add((string)reader["name"]);   
                }
                reader.Close();
                conn.Close();
                return applications;
            }
            catch (Exception ex)
            {
                if (conn.State == System.Data.ConnectionState.Open)
                {
                    conn.Close();
                }
                throw;
            }
        }

        //Orlando
        public Application GetApplication(string applicationName)
        {
            SqlConnection conn = null;
            Application application = null;
            try
            {
                conn = new SqlConnection(strDataConnection);
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT * FROM Application WHERE name=@Application", conn);

                cmd.Parameters.AddWithValue("@Application", applicationName);


                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    application = new Application()
                    {
                        Id = (int)reader["Id"],
                        name = reader["name"] as string, // Use "as" to handle possible null values
                        creation_dt = reader["creation_dt"] as string // Use "as" to handle possible null values
                    };
                }
                reader.Close();
                conn.Close();
                return application;
            }
            catch (Exception ex)
            {
                if (conn.State == System.Data.ConnectionState.Open)
                {
                    conn.Close();
                }
                return null;
            }
        }

        //Pedro
        /* 
        public List<String> GetAllApplicationInfo(String applicationName)
        {
            List<String> applicationInfo = new List<String>();
            SqlConnection conn = null;
            try
            {
                using (ContainerController containerController = new ContainerController())
                {
                    List<String> containers = containerController.GetContainersName(applicationName);
                    foreach (String container in containers)
                    {
                        List<String> containerInfo = containerController.GetAllContainerInfo(container);

                        applicationInfo.AddRange(containerInfo);
                    }
                };
                return applicationInfo;
            }
            catch (Exception ex)
            {
                if (conn.State == System.Data.ConnectionState.Open)
                {
                    conn.Close();
                }
                throw;
            }
        }
        */

        public String DeleteApplication(String applicationName)
        {
            SqlConnection conn = null;
            try
            {
            conn = new SqlConnection(strDataConnection);
            conn.Open();

            List<String> listContainers = new ContainerController().GetContainersId(applicationName);
            List<String> listDatas = new List<String>();
            List<String> listSubscriptions = new List<String>();

            string sqlQueryDeleteContainers = "DELETE FROM Container WHERE Id IN (";
            string sqlQueryDeleteData = "DELETE FROM Data WHERE Id IN (";
            string sqlQueryDeleteSubs = "DELETE FROM Subscription WHERE Id IN (";
            for (int i = 0; i < listContainers.Count; i++)
            {
                sqlQueryDeleteContainers += $"'{listContainers[i]}'";
                
                if (i < listContainers.Count - 1)
                {
                    sqlQueryDeleteContainers += ",";
                }
                string sqlQueryAux = $"SELECT Id FROM Data WHERE parent_id='{listContainers[i]}'";

                SqlCommand cmd = new SqlCommand(sqlQueryAux, conn);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    listDatas.Add($"'{reader["Id"].ToString()}'");
                    //sqlQueryDeleteData += $"'{reader["Id"].ToString()}',";
                }
                reader.Close();

                sqlQueryAux = $"SELECT Id FROM Subscription WHERE parent_id='{listContainers[i]}'";
                cmd = new SqlCommand(sqlQueryAux, conn);
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    listSubscriptions.Add($"'{reader["Id"].ToString()}'");
                    //sqlQueryDeleteSubs += $"'{reader["Id"].ToString()}',";
                }
                reader.Close();
            }

            for (int i = 0; i < listDatas.Count; i++)
            {
                sqlQueryDeleteData += listDatas[i];
                if (i < listDatas.Count - 1)
                {
                    sqlQueryDeleteData += ",";
                }
            }
            for (int i = 0; i < listSubscriptions.Count; i++)
            {
                sqlQueryDeleteSubs += listSubscriptions[i];
                if (i < listSubscriptions.Count - 1)
                {
                    sqlQueryDeleteSubs += ",";
                }
            }
            sqlQueryDeleteContainers += ")";
            sqlQueryDeleteData += ")";
            sqlQueryDeleteSubs += ")";
            //DELETE DATA
            if(listDatas.Count > 0)
            {
                using (SqlCommand cmd = new SqlCommand(sqlQueryDeleteData, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
            //DELETE SUBS
            if (listSubscriptions.Count > 0)
            {
                using (SqlCommand cmd = new SqlCommand(sqlQueryDeleteSubs, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }

            //DELETE CONTAINERS
            if (listContainers.Count > 0)
            { 
                using (SqlCommand cmd = new SqlCommand(sqlQueryDeleteContainers, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
                
            //DELETE APPLICATION
            String sqlQueryDeleteApp = "DELETE FROM Application WHERE name=@Application";
            using (SqlCommand cmd = new SqlCommand(sqlQueryDeleteApp, conn))
                {
                cmd.Parameters.AddWithValue("@Application", applicationName);

                int rowsAffected = cmd.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    return "Application deleted successfully.";
                }

                return "Application not found.";
                }
            }

            catch (Exception ex)
            {
                if (conn != null && conn.State == System.Data.ConnectionState.Open)
                {
                    conn.Close();
                }
                // Handle exceptions here
                return "error";
            }
        }

        public String PostApplication(string applicationName)
        {
            SqlConnection conn = null;

            try
            {
                conn = new SqlConnection(strDataConnection);
                conn.Open();
                string sqlQuery = "SELECT Id FROM Application WHERE name=@Application";
                using (SqlCommand cmd = new SqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@Application", applicationName);

                    object result = cmd.ExecuteScalar();

                    int rowCount = result as int? ?? 0;

                    if (rowCount > 0)
                    {
                        // Name already in use
                        return "Name already in use";
                    }
                }

                sqlQuery = "INSERT INTO Application (name, creation_dt) VALUES (@Application, FORMAT(GETUTCDATE(), 'yyyy-MM-dd HH:mm:ss'))";

                using (SqlCommand cmd = new SqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@Application", applicationName);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {  
                        // Successful insertion
                        return "Application inserted successfully.";
                    }
                    else
                    {
                        // Insertion failed
                        return "Failed to insert application.";
                    }
                }
            }
            catch (Exception ex)
            {
                if (conn != null && conn.State == System.Data.ConnectionState.Open)
                {
                    conn.Close();
                }
                // Handle exceptions here
                return "error";
            }
        }

        public String UpdateApplication(string application, string newApplication)
        {
            SqlConnection conn = null;
            try
            {
                conn = new SqlConnection(strDataConnection);
                conn.Open();

                string sqlQuery = "SELECT Id FROM Application WHERE name=@Application";
                using (SqlCommand cmd = new SqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@Application", application);

                    object result = cmd.ExecuteScalar();

                    int rowCount = result as int? ?? 0;

                    if (rowCount == 0)
                    {
                        return "Application not found";
                    }
                }

                sqlQuery = "SELECT Id FROM Application WHERE name=@NewApplication";
                using (SqlCommand cmd = new SqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@NewApplication", newApplication);

                    object result = cmd.ExecuteScalar();

                    int rowCount = result as int? ?? 0;

                    if (rowCount > 0)
                    {
                        return "Application name already in use";
                    }
                }

                sqlQuery = "UPDATE Application SET name=@NewApplication WHERE name=@Application";
                using (SqlCommand cmd = new SqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@Application", application);
                    cmd.Parameters.AddWithValue("@NewApplication", newApplication);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        return "Application updated successfully.";
                    }
                    else
                    {
                        return "Failed to update application.";
                    }
                }
            }
            catch (Exception ex)
            {
                if (conn != null && conn.State == System.Data.ConnectionState.Open)
                {
                    conn.Close();
                }
                return "error";
            }
        }
    }
}