using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Http;

using WebApplicationSOMIOD.Models;
using WebApplicationSOMIOD.Controllers;
using System.Net.Http;

using System.Xml.Linq;
using System.Web.Http.Results;
using System.Xml;
using WebApplicationSOMIOD.Utils;
using System.Security.Cryptography;

namespace WebApplicationSOMIOD.Controllers
{
    public class SOMIODController : ApiController
    {
        string strDataConnection = $@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=
            {AppDomain.CurrentDomain.BaseDirectory}App_Data\DB_SOMIOD.mdf
            ;Integrated Security=True";

        //------------------------POSTS------------------------

        [HttpPost]
        [Route("api/somiod")]
        public IHttpActionResult CreateNewApplication()
        {
            try
            {
                // Read the request body as a string
                string requestBody = Request.Content.ReadAsStringAsync().Result;

                // Parse the XML content into an XDocument
                XDocument xmlDoc = XDocument.Parse(requestBody);

                string appName = xmlDoc.Root.Element("name").Value;
                String result = new ApplicationController().PostApplication(appName);

                if (result != "Application inserted successfully.")
                {
                    return BadRequest(result);
                }

                
                return Ok(new XMLResponses().GetMessageXMLType(result));
            }
            catch (Exception ex)
            {
                return BadRequest($"Error creating a new application: {ex.Message}");
            }
        }

        [HttpPost]
        [Route("api/somiod/{application}")]
        public IHttpActionResult CreateNewContainer(String application)
        {
            try
            {
                // Read the request body as a string
                string requestBody = Request.Content.ReadAsStringAsync().Result;

                // Parse the XML content into an XDocument
                XDocument xmlDoc = XDocument.Parse(requestBody);

                string containerName = xmlDoc.Root.Element("name").Value;
                String result = new ContainerController().PostContainer(application, containerName);

                if (result != "Container inserted successfully.")
                {
                    return BadRequest(result);
                }

                return Ok(new XMLResponses().GetMessageXMLType(result));
            }
            catch (Exception ex)
            {
                return BadRequest($"Error creating a new Container: {ex.Message}");
            }
        }

        [HttpPost]
        [Route("api/somiod/{application}/{container}")]
        public IHttpActionResult CreateNewDataOrSubscrition(String application, String container)
        {
            try
            {
                // Read the request body as a string
                string requestBody = Request.Content.ReadAsStringAsync().Result;

                // Parse the XML content into an XDocument
                XDocument xmlDoc = XDocument.Parse(requestBody);

                string type = xmlDoc.Root.Name.LocalName;
                string result = "";
                if (type == "subscription")
                {
                    string name = xmlDoc.Root.Element("name").Value;
                    string eventParam = xmlDoc.Root.Element("event").Value;
                    string endpoint = xmlDoc.Root.Element("endpoint").Value;
                    result = new SubscriptionController().PostSubscription(name, container, eventParam, endpoint);
                } 
                else if (type == "data")
                {
                    string data = xmlDoc.Root.Element("content").Value;
                    //string name = xmlDoc.Root.Element("name").Value;
                    result = new DataController().PostData(container, data);
                }

                if (result != "Data inserted successfully.")
                {
                    return BadRequest(result);
                }

                return Ok(new XMLResponses().GetMessageXMLType(result));
            }
            catch (Exception ex)
            {
                return BadRequest($"Error creating a new Container: {ex.Message}");
            }
        }


        //------------------------DELETES------------------------

        [HttpDelete]
        [Route("api/somiod/{application}")]
        public IHttpActionResult DeleteApplication(String application)
        {
            
            String result = new ApplicationController().DeleteApplication(application);

            if (result != "Application deleted successfully.")
            {
                return BadRequest(result);
            }

            return Ok(new XMLResponses().GetMessageXMLType(result));
        }

        [HttpDelete]
        [Route("api/somiod/{application}/{container}")]
        public IHttpActionResult DeleteContainer(String application, String container)
        {
            String result = new ContainerController().DeleteContainer(container);

            if (result != "Container deleted successfully.")
            {
                return BadRequest(result);
            }

            return Ok(new XMLResponses().GetMessageXMLType(result));
        }

        [HttpDelete]
        [Route("api/somiod/{application}/{container}/data/{data}")]
        public IHttpActionResult DeleteData(String application, String container, String data)
        {
            String result = new DataController().DeleteData(data);
            if (result != "Data deleted successfully.")
            {
                return BadRequest(result);
            }

            return Ok(new XMLResponses().GetMessageXMLType(result));
        }

        [HttpDelete]
        [Route("api/somiod/{application}/{container}/subscription/{subscription}")]
        public IHttpActionResult DeleteSubscription(String application, String container, String subscription)
        {
            String result = new SubscriptionController().DeleteSubscription(subscription);
            if (result != "Subscription deleted successfully.")
            {
                return BadRequest(result);
            }

            return Ok(new XMLResponses().GetMessageXMLType(result));
        }

        //------------------------GETS------------------------

        //Returns all names of apps, containers and datas registered to SOMIOD using discover
        [HttpGet]
        [Route("api/somiod")]
        public IHttpActionResult DiscoverNames()
        {
            //Check if header has the somiod discover
            if (Request.Headers.Contains("somiod-discover"))
            {
                string discoverType = Request.Headers.GetValues("somiod-discover").FirstOrDefault();

                switch (discoverType)
                {
                    case "application":
                        List<string> names = new ApplicationController().GetApplicationsName();
                        return Ok(new XMLResponses().GetDiscoverXMLType(names, "ApplicationNames"));
                    case "container":
                        List<string> containerNames = new ContainerController().GetContainersName();
                        return Ok(new XMLResponses().GetDiscoverXMLType(containerNames, "ContainerNames"));
                    case "data":
                        List<string> dataNames = new DataController().GetDatasNames();
                        return Ok(new XMLResponses().GetDiscoverXMLType(dataNames, "DataNames"));
                    case "subscription":
                        List<string> subsNames = new SubscriptionController().GetSubscriptionsNames();
                        return Ok(new XMLResponses().GetDiscoverXMLType(subsNames, "SubscriptionNames"));
                    default:
                        return BadRequest("Invalid discover type");
                }
            }
            else return BadRequest("somiod-discover header required");

        }

        //Get all names of containers or data of an application or just one application info
        [HttpGet]
        [Route("api/somiod/{application}")]
        public IHttpActionResult DiscoverAppNames(String application)
        {
            if (Request.Headers.Contains("somiod-discover"))
            {
                string discoverType = Request.Headers.GetValues("somiod-discover").FirstOrDefault();
                List<String> result = new List<String>();
                switch (discoverType)
                {
                    case "container":
                        result = new ContainerController().GetContainersName(application);
                        if (result.Count > 0)
                            return Ok(new XMLResponses().GetDiscoverXMLType(result, "ContainerNames"));
                        return NotFound();
                    case "data":
                        result = new DataController().GetDatasNames(application);
                        if (result.Count > 0)
                            return Ok(new XMLResponses().GetDiscoverXMLType(result, "DataNames"));
                        return NotFound();
                    case "subscription":
                        List<string> subsNames = new SubscriptionController().GetSubscriptionsNames(application);
                        return Ok(new XMLResponses().GetDiscoverXMLType(subsNames, "SubscriptionNames"));
                    default:
                        return BadRequest("Invalid discover type");
                }
            }
            else
            {
                Application result = new ApplicationController().GetApplication(application);

                return Ok(result);
            }
        }


        //Get all names of data of a container or just one container info
        [HttpGet]
        [Route("api/somiod/{application}/{container}")]
        public IHttpActionResult DiscoverContainerNames(String application, String container)
        {
            if (Request.Headers.Contains("somiod-discover"))
            {
                string discoverType = Request.Headers.GetValues("somiod-discover").FirstOrDefault();

                switch (discoverType)
                {
                    case "data":
                        List<String> result = new DataController().GetDatasNames(null, container);       
                        return Ok(new XMLResponses().GetDiscoverXMLType(result, "DataNames"));
                    case "subscription":
                        List<string> subsNames = new SubscriptionController().GetSubscriptionsNames(null, container);
                        return Ok(new XMLResponses().GetDiscoverXMLType(subsNames, "SubscriptionNames"));
                    default:
                        return BadRequest("Invalid discover type");
                }
            }
            else
            {
                return Ok(new ContainerController().GetContainer(container));
            }

        }

        [HttpGet]
        [Route("api/somiod/{application}/{container}/data/{data}")]
        public IHttpActionResult GetData(String application, String container, string data)
        { 
               return Ok(new DataController().GetData(data));
        }

        [HttpGet]
        [Route("api/somiod/{application}/{container}/subscription/{subscription}")]
        public IHttpActionResult GetSubscription(String application, String container, string subscription)
        {
            return Ok(new SubscriptionController().GetSubscription(subscription));
        }

        //------------------------UPDATES------------------------
        [HttpPut]
        [Route("api/somiod/{application}")]
        public IHttpActionResult UpdateApplication(String application)
        {
            try
            {
                // Read the request body as a string
                string requestBody = Request.Content.ReadAsStringAsync().Result;

                // Parse the XML content into an XDocument
                XDocument xmlDoc = XDocument.Parse(requestBody);

                string newAppName = xmlDoc.Root.Element("name").Value;

                String result = new ApplicationController().UpdateApplication(application, newAppName);

                if (result != "Application updated successfully.")
                {
                    return BadRequest(result);
                }

                return Ok(new XMLResponses().GetMessageXMLType(result));
            }
            catch (Exception ex)
            {
                return BadRequest($"Error updating application: {ex.Message}");
            }
        }

        
        [HttpPut]
        [Route("api/somiod/{application}/{container}")]
        public IHttpActionResult UpdateContainer(String application, String container)
        {
            try
            {
                // Read the request body as a string
                string requestBody = Request.Content.ReadAsStringAsync().Result;

                // Parse the XML content into an XDocument
                XDocument xmlDoc = XDocument.Parse(requestBody);

                string newContainerName = xmlDoc.Root.Element("name").Value;

                String result = new ContainerController().UpdateContainer(application, container, newContainerName);

                if (result != "Container updated successfully.")
                {
                    return BadRequest(result);
                }

                return Ok(new XMLResponses().GetMessageXMLType(result));
            }
            catch (Exception ex)
            {
                return BadRequest($"Error updating container: {ex.Message}");
            }
        }
    }
}