using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebApplicationSOMIOD.Models;

namespace WebApplicationSOMIOD.Controllers
{

    public class ContainerController : ApiController
    {

        string strDataConnection = $@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=
            {AppDomain.CurrentDomain.BaseDirectory}App_Data\DB_SOMIOD.mdf
            ;Integrated Security=True";


        public String PostContainer(string applicationName, string containerName)
        {
            SqlConnection conn = null;
            try
            {
                conn = new SqlConnection(strDataConnection);
                conn.Open();
                string sqlQuery = "SELECT Id FROM Container WHERE name=@Container";
                using (SqlCommand cmd = new SqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@Container", containerName);

                    object result = cmd.ExecuteScalar();

                    int rowCount = result as int? ?? 0;

                    if (rowCount > 0)
                    {
                        // Name already in use
                        return "Name already in use";
                    }
                }
                int applicationId = -1;
               sqlQuery = "SELECT Id FROM Application WHERE name=@Application";
                using (SqlCommand cmd = new SqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@Application", applicationName);

                    object result = cmd.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        // The query returned an Id
                        applicationId = (int)result;
                    }
                    else
                    {
                        return "Application does not exist";
                    }
                }

                sqlQuery = "INSERT INTO Container (name, creation_dt, parent_id) VALUES (@Container, FORMAT(GETUTCDATE(), 'yyyy-MM-dd HH:mm:ss'), "+applicationId+")";
                using (SqlCommand cmd = new SqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@Container", containerName);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        // Successful insertion
                        return "Container inserted successfully.";
                    }
                    else
                    {
                        // Insertion failed
                        return "Failed to insert Container.";
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

        public Container GetContainer(string containerName)
        {
            SqlConnection conn = null;
            Container container = null;
            try
            {
                conn = new SqlConnection(strDataConnection);
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT * FROM Container WHERE name=@Container",  conn);

                cmd.Parameters.AddWithValue("@Container", containerName);


                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    container = new Container()
                    {
                        Id = (int)reader["Id"],
                        name = reader["name"] as string, // Use "as" to handle possible null values
                        creation_dt = reader["creation_dt"] as string, // Use "as" to handle possible null values
                        parent_id = (int)reader["parent_id"]    
                    };
                }
                reader.Close();
                conn.Close();
                return container;
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

        public List<String> GetContainersName(String application = null)
        {
            List<String> containers = new List<String>();
            SqlConnection conn = null;
            try
            {
                conn = new SqlConnection(strDataConnection);
                conn.Open();
                string sqlQuery = "SELECT c.name FROM Container c";
                if (application != null)
                {
                    sqlQuery += " INNER JOIN Application a ON c.parent_id = a.Id WHERE a.name = @Application";
                }
                sqlQuery += " ORDER BY c.Id";

                SqlCommand cmd = new SqlCommand(sqlQuery, conn);

                if (application != null)
                {
                    cmd.Parameters.AddWithValue("@Application", application);
                }

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    containers.Add((string)reader["name"]);
                }
                reader.Close();
                conn.Close();
                return containers;
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

        public List<String> GetContainersId(String application)
        {
            List<String> containers = new List<String>();
            SqlConnection conn = null;
            try
            {
                conn = new SqlConnection(strDataConnection);
                conn.Open();
                string sqlQuery = "SELECT c.Id FROM Container c INNER JOIN Application a ON c.parent_id = a.Id WHERE a.name = @Application";     

                SqlCommand cmd = new SqlCommand(sqlQuery, conn);
       
                cmd.Parameters.AddWithValue("@Application", application);
                

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    containers.Add(reader["Id"].ToString());
                }
                reader.Close();
                conn.Close();
                return containers;
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

        public List<String> GetAllContainerInfo(String container)
        {
            List<String> containerInfo = new List<String>();
            SqlConnection conn = null;

            try
            {
                conn = new SqlConnection(strDataConnection);
                conn.Open();

                string sqlQuery = @"
                    SELECT 
                        c.name AS ContainerName, 
                        d.content AS DataContent, 
                        s.name AS SubscriptionName
                    FROM 
                        Container c
                    LEFT JOIN 
                        Data d ON c.Id = d.parent_id
                    LEFT JOIN 
                        Subscription s ON c.Id = s.parent_id
                    WHERE 
                        c.name = @Container
                    ORDER BY 
                        c.Id
                ";

                SqlCommand cmd = new SqlCommand(sqlQuery, conn);
                cmd.Parameters.AddWithValue("@Container", container);

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    string containerName = (string)reader["ContainerName"];
                    string dataContent = reader["DataContent"] is DBNull ? null : (string)reader["DataContent"];
                    string subscriptionName = reader["SubscriptionName"] is DBNull ? null : (string)reader["SubscriptionName"];

                    // Build a string or object to represent the container info as needed
                    string containerInfoString = $"{containerName}, Data: {dataContent}, Subscription: {subscriptionName}";

                    containerInfo.Add(containerInfoString);
                }

                reader.Close();
                conn.Close();
                return containerInfo;
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

        public String DeleteContainer(String containerName)
        {
            SqlConnection conn = null;

            try
            {
                conn = new SqlConnection(strDataConnection);
                conn.Open();

                List<String> listDatas = new List<String>();
                List<String> listSubscriptions = new List<String>();

                string sqlQueryGetContainer = "SELECT Id from Container WHERE name=@Container";
                int id = -1;

                using (SqlCommand cmd = new SqlCommand(sqlQueryGetContainer, conn))
                {
                    cmd.Parameters.AddWithValue("@Container", containerName);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Retrieve the 'Id' value from the result set
                            id = (int)reader["Id"];
                        }
                    }

                    // Container not found
                    if(id == -1) return "Container not found.";
                    
                }

                string sqlQueryDeleteData = $"DELETE FROM Data WHERE parent_id='{id}'";
                string sqlQueryDeleteSubs = $"DELETE FROM Subscription WHERE parent_id='{id}'";

                //DELETE DATA
                using (SqlCommand cmd = new SqlCommand(sqlQueryDeleteData, conn))
                {
                    cmd.ExecuteNonQuery();
                }
                //DELETE SUBS
                using (SqlCommand cmd = new SqlCommand(sqlQueryDeleteSubs, conn))
                {
                    cmd.ExecuteNonQuery();
                }

                //DELETE CONTAINER
                String sqlQueryDeleteApp = "DELETE FROM Container WHERE name=@Container";
                using (SqlCommand cmd = new SqlCommand(sqlQueryDeleteApp, conn))
                {
                    cmd.Parameters.AddWithValue("@Container", containerName);

                    cmd.ExecuteNonQuery();
                    
                }
                return "Container deleted successfully.";
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

        public String UpdateContainer(String application, String container, String newContainer)
        {
            SqlConnection conn = null;
            try
            {
                conn = new SqlConnection(strDataConnection);
                conn.Open();

                int applicationId = -1;
                string sqlQuery = "SELECT Id FROM Application WHERE name=@Application";
                using (SqlCommand cmd = new SqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@Application", application);

                    object result = cmd.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        // The query returned an Id
                        applicationId = (int)result;
                    }
                    else
                    {
                        return "Application not found";
                    }
                }

                sqlQuery = "SELECT Id FROM Container WHERE name=@Container";
                using (SqlCommand cmd = new SqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@Container", container);

                    object result = cmd.ExecuteScalar();

                    int rowCount = result as int? ?? 0;

                    if (rowCount == 0)
                    {
                        return "Container not found";
                    }
                }

                sqlQuery = "SELECT Id FROM Container WHERE name=@Container";
                using (SqlCommand cmd = new SqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@Container", newContainer);

                    object result = cmd.ExecuteScalar();

                    int rowCount = result as int? ?? 0;

                    if (rowCount > 0)
                    {
                        return "Container name already in use";
                    }
                }

                sqlQuery = "UPDATE Container SET name=@NewContainer WHERE name=@Container AND parent_id=@ApplicationId";
                using (SqlCommand cmd = new SqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@Container", container);
                    cmd.Parameters.AddWithValue("@NewContainer", newContainer);
                    cmd.Parameters.AddWithValue("@ApplicationId", applicationId);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        return "Container updated successfully.";
                    }
                    else
                    {
                        return "Failed to update Container.";
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