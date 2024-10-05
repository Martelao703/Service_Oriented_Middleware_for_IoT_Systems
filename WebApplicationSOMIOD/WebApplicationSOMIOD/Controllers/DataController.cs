using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebApplicationSOMIOD.Models;
using static System.Net.Mime.MediaTypeNames;

using uPLibrary.Networking.M2Mqtt;
using System.Text;
using System.Threading.Tasks;

using System.Threading;
using System.Net;

namespace WebApplicationSOMIOD.Controllers
{
    public class DataController : ApiController
    {
        string strDataConnection = $@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=
            {AppDomain.CurrentDomain.BaseDirectory}App_Data\DB_SOMIOD.mdf
            ;Integrated Security=True";

        public List<String> GetDatasNames(String applicationName = null, String containerName = null)
        {
            List<String> datas = new List<String>();
            SqlConnection conn = null;
            try
            {
                if(applicationName != null && containerName != null) {
                    throw new Exception("Tried to access application and container DATA at the same time");
                }
                conn = new SqlConnection(strDataConnection);
                conn.Open();
                string sqlQuery = "SELECT d.name FROM Data d ";
                //Get data from a container
                if (containerName != null)
                {
                    sqlQuery += "INNER JOIN Container c ON c.Id = d.parent_id " +
                        "WHERE c.name = @Container ";
                }
                //Get data from an application 
                if (applicationName != null) 
                {
                    sqlQuery = "SELECT d.name " +
                     "FROM APPLICATION a " +
                     "INNER JOIN Container c ON c.parent_id = a.Id " +
                     "INNER JOIN Data d ON d.parent_id = c.id " +
                     "WHERE a.name = @Application ";

                }
                else sqlQuery += "ORDER BY d.Id";
                
                

                SqlCommand cmd = new SqlCommand(sqlQuery, conn);

                if (applicationName != null)
                {
                    cmd.Parameters.AddWithValue("@Application", applicationName);
                }
                else if (containerName != null)
                {
                    cmd.Parameters.AddWithValue("@Container", containerName);
                }

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    datas.Add((string)reader["name"]);
                }
                reader.Close();
                conn.Close();
                return datas;
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

        public Data GetData(string dataName)
        {
            SqlConnection conn = null;
            Data data = null;
            try
            {
                conn = new SqlConnection(strDataConnection);
                conn.Open();
                SqlCommand cmd = new SqlCommand( "SELECT Id,content,creation_dt,parent_id,name FROM Data WHERE name=@Data", conn);


                cmd.Parameters.AddWithValue("@Data", dataName);

                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    data = new Data()
                    {
                        Id = int.Parse(reader["Id"].ToString()),
                        name = reader["name"] as string,
                        content = reader["content"] as string,
                        creation_dt = reader["creation_dt"].ToString(),
                        parent_id = int.Parse(reader["parent_id"].ToString()),
                    };
                }

                reader.Close();
                conn.Close();
                return data;
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

        public String PostData(string containerName, string data)
        {
            SqlConnection conn = null;
            try
            {
                conn = new SqlConnection(strDataConnection);
                conn.Open();

                string sqlQuery = "SELECT Id FROM Container WHERE name=@Container";
                int containerId = -1;

                using (SqlCommand cmd = new SqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@Container", containerName);

                    object result = cmd.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        // The query returned an Id
                        containerId = (int)result;
                    }
                    else
                    {
                        return "Container does not exist";
                    }
                }
                conn.Close();
                conn.Open();

                //Verify if there is a subscription that has creation or both on event
                List<string> endpoints = new List<string>();
                sqlQuery = "SELECT endpoint FROM Subscription WHERE parent_id = @containerId AND event IN ('both', 'creation')";
                using (SqlCommand cmd = new SqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@containerId", containerId);

                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        endpoints.Add((string)reader["endpoint"]);
                    }

                    //if (subs.Count > 0)
                    //{
                    //    Manda uma notificação com a mensagem que está no content do data
                    //    Console.WriteLine(subs);
                    //    flag = true;
                    //}
                }
                conn.Close();
                conn.Open();

                //Generate a unique name for the data based on container name
                string uniqueDataName = GenerateUniqueDataName(containerName);

                //Insert data in table
                sqlQuery = "INSERT INTO Data (content, name, creation_dt, parent_id) VALUES (@Data, @Name, FORMAT(GETUTCDATE(), 'yyyy-MM-dd HH:mm:ss'), @ContainerId)";
                using (SqlCommand cmd = new SqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@Data", data);
                    cmd.Parameters.AddWithValue("@Name", uniqueDataName);

                    cmd.Parameters.AddWithValue("@ContainerId", containerId);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        // Successful insertion
                        string xmlData = $"<content>{data}</content>";
                        foreach (string endpoint in endpoints)
                        {
                            Uri uri = new Uri(endpoint);
                            string ipAddress = uri.Host;

                            // Use ipAddress to publish to the MQTT endpoint
                            MqttClient mcClient = new MqttClient(ipAddress);
                            mcClient.Connect(Guid.NewGuid().ToString());
                            if (mcClient.IsConnected)
                            {
                                //Publish on channel that is the name of the container
                                mcClient.Publish(containerName, Encoding.UTF8.GetBytes(xmlData));
                                Thread.Sleep(10);
                                mcClient.Disconnect();
                            }
                        }
                        
                        return "Data inserted successfully.";
                    }
                    else
                    {
                        // Insertion failed
                        return "Failed to insert Data.";
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

        public String DeleteData(String data)
        {
            SqlConnection conn = null;

            try
            {
                conn = new SqlConnection(strDataConnection);
                conn.Open();
     
                string sqlQueryDeleteData = "DELETE FROM Data WHERE name=@Data";

                using (SqlCommand cmd = new SqlCommand(sqlQueryDeleteData, conn))
                {
                    cmd.Parameters.AddWithValue("@Data", data);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        return "Data deleted successfully.";
                    }
                    else
                    {
                        return "No data found with the specified Id.";
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

        // Method to generate a unique name for the data based on container name and an incrementing number
        private string GenerateUniqueDataName(string containerName)
        {
            SqlConnection conn = new SqlConnection(strDataConnection);
            conn.Open();

            string sqlQuery = "SELECT MAX(Id) FROM Data";
            int maxId = 0;

            using (SqlCommand cmd = new SqlCommand(sqlQuery, conn))
            {
                maxId = (int)cmd.ExecuteScalar();
            }

            conn.Close();

            // Increment the count to get the next unique data name
            maxId++;

            return $"{containerName}_data{maxId}";
        }
    }
}