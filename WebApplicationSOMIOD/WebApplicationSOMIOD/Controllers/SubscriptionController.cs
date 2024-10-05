using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebApplicationSOMIOD.Models;

namespace WebApplicationSOMIOD.Controllers
{
    public class SubscriptionController : ApiController
    {

        string strDataConnection = $@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=
            {AppDomain.CurrentDomain.BaseDirectory}App_Data\DB_SOMIOD.mdf
            ;Integrated Security=True";

        public String PostSubscription(string name, string containerName, string eventParam, string endpoint)
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
                        return "Container not found";
                    }
                }

                sqlQuery = "INSERT INTO Subscription (name, creation_dt, parent_id, event, endpoint) VALUES (@Name, FORMAT(GETUTCDATE(), 'yyyy-MM-dd HH:mm:ss'), " + containerId + ", @Event, @Endpoint)";
                using (SqlCommand cmd = new SqlCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@Name", name);
                    cmd.Parameters.AddWithValue("@Event", eventParam);
                    cmd.Parameters.AddWithValue("@Endpoint", endpoint);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        // Successful insertion
                        return "Subscription inserted successfully.";
                    }
                    else
                    {
                        // Insertion failed
                        return "Failed to insert Subscription.";
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

        public String DeleteSubscription(String subscription)
        {
            SqlConnection conn = null;

            try
            {
                conn = new SqlConnection(strDataConnection);
                conn.Open();

                string sqlQueryDeleteData = "DELETE FROM Subscription WHERE name=@Subscription";

                using (SqlCommand cmd = new SqlCommand(sqlQueryDeleteData, conn))
                {
                    cmd.Parameters.AddWithValue("@Subscription", subscription);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        return "Subscription deleted successfully.";
                    }
                    else
                    {
                        return "No Subscription found with the specified Name.";
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

        public List<String> GetSubscriptionsNames(String applicationName = null, String containerName = null)
        {
            List<String> datas = new List<String>();
            SqlConnection conn = null;
            try
            {
                if (applicationName != null && containerName != null)
                {
                    throw new Exception("Tried to access application and container Subscription at the same time");
                }
                conn = new SqlConnection(strDataConnection);
                conn.Open();
                string sqlQuery = "SELECT s.name FROM Subscription s ";
                //Get data from a container
                if (containerName != null)
                {
                    sqlQuery += "INNER JOIN Container c ON c.Id = s.parent_id " +
                        "WHERE c.name = @Container ";
                }
                //Get data from an application 
                if (applicationName != null)
                {
                    sqlQuery = "SELECT s.name " +
                     "FROM APPLICATION a " +
                     "INNER JOIN Container c ON c.parent_id = a.Id " +
                     "INNER JOIN Subscription s ON s.parent_id = c.id " +
                     "WHERE a.name = @Application ";

                }
                else sqlQuery += "ORDER BY s.Id";



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

        public Subscription GetSubscription(string subscriptionName)
        {
            SqlConnection conn = null;
            Subscription subscription = null;
            try
            {
                conn = new SqlConnection(strDataConnection);
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT Id,content,creation_dt,parent_id,name FROM Data WHERE name=@Sub", conn);


                cmd.Parameters.AddWithValue("@Sub", subscriptionName);

                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    subscription = new Subscription()
                    {
                        Id = int.Parse(reader["Id"].ToString()),
                        name = reader["name"] as string,
                        @event = reader["event"] as string,
                        endpoint = reader ["endpoint"] as string,
                        creation_dt = reader["creation_dt"].ToString(),
                        parent_id = int.Parse(reader["parent_id"].ToString()),
                    };
                }

                reader.Close();
                conn.Close();
                return subscription;
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
    }

}