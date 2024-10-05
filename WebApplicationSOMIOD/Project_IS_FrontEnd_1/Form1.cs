using System;
using System.Collections.Generic;
using System.Windows.Forms;
using WebApplicationSOMIOD;
using WebApplicationSOMIOD.Models;
using RestSharp;
using System.Web.Script.Serialization;
using System.Xml.Linq;

using System.Net;
using System.Linq;
using System.Xml;


namespace Project_IS_FrontEnd_1
{
    public partial class Form1 : Form
    {
        string baseURI = @"http://localhost:65244/api/somiod/";
        private string selectedProduct = ""; // variavel para guardar o nome da aplicação selecionada dentro do create container
        private string selectedProduct1 = ""; // variavel para guardar o nome da aplicação selecionada da lista da esquerda
        private string selectedContainerApplication=""; // variavel para guardar o nome do container selecionada dentro da Info da App
        private string selectedData = ""; // variavel para guardar o content da data selecionada dentro da Info da App
        private string selectedSubs = ""; // variavel para guardar o nome do subs selecionada dentro da Info da App
        public Form1()
        {
            InitializeComponent();
            groupBox1.Visible = false;
            groupBox2.Visible = false;
            groupBox3.Visible = false;
            groupBox4.Visible = false;
            groupBox5.Visible = false;
            groupBox6.Visible = false;
            groupBox7.Visible = false;


        }
        //POPULAR A LISTBOX COM OS NOMES DAS APLICAÇÕES

        private void Form1_Load(object sender, EventArgs e)
        {
            
            PopulateListBox(listBox1);
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            
            if (listBox1.SelectedIndex != -1)
            {
                selectedProduct1 = listBox1.SelectedItem.ToString();

                groupBox1.Visible = false;
                groupBox2.Visible = false;
                groupBox3.Visible = true;
                groupBox4.Visible = false;
                groupBox5.Visible = false;
                groupBox6.Visible = false;
                
                PopulateListBoxContainer(listBox2, selectedProduct1);
                listBox5.Items.Clear();
                listBox6.Items.Clear();
                
            }
        }

        private void PopulateListBox(ListBox listBox)
        {
            listBox.Items.Clear(); // limpa a lista

            try
            {
                // faz pedido á API
                var client = new RestClient("http://localhost:65244/api/somiod");
                var request = new RestRequest();
                request.Method = Method.Get;
                request.AddHeader("somiod-discover", "application");

                var response = client.Execute(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string responseData = response.Content;

                    //XML
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(responseData);

                    // Get all <string> elements and add their inner text to the ListBox
                    XmlNodeList stringNodes = xmlDoc.GetElementsByTagName("name");
                    foreach (XmlNode node in stringNodes)
                    {
                        listBox.Items.Add(node.InnerText);
                    }
                }
                else
                {
                    MessageBox.Show($"Failed to fetch data from the API. Status Code: {response.StatusCode}");

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while populating ListBox1: {ex.Message}");
            }
        }
        private void PopulateListBoxContainer(ListBox listBox ,string appName) // popular a lista de containers
        {
            listBox.Items.Clear(); // limpa a lista
            listBox4.Items.Clear();// limpa a lista de datas
            

            try
            {
                // faz pedido á API
                var client = new RestClient("http://localhost:65244/api/somiod/"+appName);
                var request = new RestRequest();
                request.Method = Method.Get;
                request.AddHeader("somiod-discover", "container");

                var response = client.Execute(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string responseData = response.Content;

                    //XML
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(responseData);

                    // Get all <name> elements and add their inner text to the ListBox
                    XmlNodeList stringNodes = xmlDoc.GetElementsByTagName("name");
                    foreach (XmlNode node in stringNodes)
                    {
                        listBox.Items.Add(node.InnerText);
                    }
                }
                else
                {
                    MessageBox.Show($"There's no Container");

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while populating ListBox1: {ex.Message}");
            }
        }
        private void PopulateListBoxData(ListBox listBox, string appName , string appContainer) // popular a lista de datas
        {
            listBox.Items.Clear(); // limpa a lista


            try
            {
                // faz pedido á API
                var client = new RestClient("http://localhost:65244/api/somiod/" + appName +"/"+appContainer);
                var request = new RestRequest();
                request.Method = Method.Get;
                request.AddHeader("somiod-discover", "data");

                var response = client.Execute(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string responseData = response.Content;

                    //XML
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(responseData);

                    // Get all <string> elements and add their inner text to the ListBox
                    XmlNodeList stringNodes = xmlDoc.GetElementsByTagName("name");
                    foreach (XmlNode node in stringNodes)
                    {
                        listBox.Items.Add(node.InnerText);
                    }
                }
                else
                {
                    MessageBox.Show($"There's no Data");

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while populating ListBox1: {ex.Message}");
            }
        }
        private void PopulateListBoxSubs(ListBox listBox, string appName, string appContainer) // popular a lista de subs
        {
            listBox.Items.Clear(); // limpa a lista


            try
            {
                // faz pedido á API
                var client = new RestClient("http://localhost:65244/api/somiod/" + appName + "/" + appContainer);
                var request = new RestRequest();
                request.Method = Method.Get;
                request.AddHeader("somiod-discover", "subscription");
                

                var response = client.Execute(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string responseData = response.Content;
                   

                    //XML
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(responseData);

                    // Get all names
                    XmlNodeList stringNodes = xmlDoc.GetElementsByTagName("name");
                    foreach (XmlNode node in stringNodes)
                    {
                        listBox.Items.Add(node.InnerText);
                    }
                }
                else
                {
                    MessageBox.Show($"There's no Subs");

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while populating ListBox5: {ex.Message}");
            }
        }
        private void PopulateListBoxDataContent(ListBox listBox, string appName, string appContainer, string dataName) // popular a lista de data content com só um valor
        {
            listBox.Items.Clear(); // limpa a lista

            try
            {
                // faz pedido á API
                var client = new RestClient("http://localhost:65244/api/somiod/" + appName + "/" + appContainer +"/data/" +dataName );
                var request = new RestRequest();
                request.Method = Method.Get;
                

                var response = client.Execute(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string responseData = response.Content;
                    

                    //XML
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(responseData);

                    // Get the <content> element 
                    XmlNodeList stringNodes = xmlDoc.GetElementsByTagName("content");
                    foreach (XmlNode node in stringNodes)
                    {
                        listBox.Items.Add(node.InnerText);
                    }
                }
                else
                {
                    MessageBox.Show($"There's no Data Content");

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while populating ListBox1: {ex.Message}");
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void groupBox2_Enter(object sender, EventArgs e) // criar container group
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e) // criar Aplicacao group
        {

        }

        private void button1_Click(object sender, EventArgs e) //criar aplicacao button
        {
            groupBox1.Visible = true;
            groupBox2.Visible = false;
            groupBox3.Visible = false;
            groupBox4.Visible = false;
            groupBox5.Visible = false;
            groupBox6.Visible = false;
            
        }

        private void button2_Click(object sender, EventArgs e) // criar container button
        {
            groupBox1.Visible = false;
            groupBox2.Visible = true;
            groupBox3.Visible = false;
            groupBox4.Visible = false;
            groupBox5.Visible = false;
            groupBox6.Visible = false;
            
            PopulateListBox(listBox3);

            listBox3.Refresh();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void listBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox3.SelectedIndex != -1)
            {
                selectedProduct = listBox3.SelectedItem.ToString();

            }
        }

        private void button5_Click(object sender, EventArgs e) // criar container 
        {
            string appName = textBox2.Text; // nome da aplicação
            string appApplication = selectedProduct; // usa a aplicação selecionada na lista

            

            string xmlData = $"<container><name>{appName}</name></container>"; // data do xml

            var client = new RestClient(baseURI + appApplication); // faz o pedido á API com o nome da aplicação

            var request_ = new RestRequest();
            request_.Method = Method.Post;
            request_.AddHeader("Content-Type", "container/xml"); 
            request_.AddXmlBody(xmlData);

            var response_ = client.Execute(request_);
            if (response_.StatusCode == HttpStatusCode.OK || response_.StatusCode == HttpStatusCode.Created)
            {
                // Faz XML content
                System.Xml.XmlDocument xmlDoc = new System.Xml.XmlDocument();
                xmlDoc.LoadXml(response_.Content);

                string message = xmlDoc.InnerText; 

                if (message == "Container inserted successfully.")
                {
                    MessageBox.Show("Created successfully");
                    PopulateListBox(listBox1);
                }
                else
                {
                    MessageBox.Show($"Error: {message}");
                }
            }
            else
            {
                MessageBox.Show($"Error: {response_.Content}");
            }
        }
        private void button4_Click(object sender, EventArgs e) // Criar Aplicação
        {
            string appName = textBox3.Text; // nome da aplicação

            string xmlData = $"<application><name>{appName}</name></application>";

            var client = new RestClient(baseURI);

            var request_ = new RestRequest();
            request_.Method = Method.Post;
            request_.AddHeader("Content-Type", "application/xml"); 
            request_.AddXmlBody(xmlData); 

            var response_ = client.Execute(request_);
            if (response_.StatusCode == HttpStatusCode.OK || response_.StatusCode == HttpStatusCode.Created)
            {
                
                System.Xml.XmlDocument xmlDoc = new System.Xml.XmlDocument();
                xmlDoc.LoadXml(response_.Content);

                string message = xmlDoc.InnerText; 

                if (message == "Application inserted successfully.")
                {
                    MessageBox.Show("Created successfully");
                    PopulateListBox(listBox1); // dá refresh á lista da esquerda
                }
                else
                {
                    MessageBox.Show($"Error: {message}");
                }
            }
            else
            {
                MessageBox.Show($"Error: {response_.Content}");
            }
        }

        private void button6_Click(object sender, EventArgs e) //Delete Application
        {
            
            var client = new RestClient("http://localhost:65244/api/somiod/" + selectedProduct1);

            var request_ = new RestRequest();
            request_.Method = Method.Delete;

            var response_ = client.Execute(request_);
            if (response_.StatusCode == HttpStatusCode.OK)
            {
                MessageBox.Show("Application " + selectedProduct1+ " deleted");
                PopulateListBox(listBox1);
            }
            else
            {
                MessageBox.Show("Erro");
            }
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex != -1 && listBox2.SelectedIndex != -1)
            {
                selectedProduct1 = listBox1.SelectedItem.ToString();
                selectedContainerApplication = listBox2.SelectedItem.ToString();

                groupBox1.Visible = false;
                groupBox2.Visible = false;
                groupBox3.Visible = true;
                groupBox4.Visible = false;
                groupBox5.Visible = false;
                groupBox6.Visible = false;
                
                PopulateListBoxData(listBox4, selectedProduct1, selectedContainerApplication);
                PopulateListBoxSubs(listBox5, selectedProduct1, selectedContainerApplication);

            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            groupBox6.Visible = true;
        }

        private void button13_Click(object sender, EventArgs e) //Editar Applicacao button
        {
            groupBox4.Visible = true;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button14_Click(object sender, EventArgs e) //Editar applicacao button cancel
        {
            groupBox4.Visible = false;
        }

        private void button15_Click(object sender, EventArgs e) //Editar applicao button confirm
        {
            string appName = textBox1.Text; // nome da aplicação
           

            string xmlData = $"<application><name>{appName}</name></application>";

            var client = new RestClient(baseURI + selectedProduct1 );

            var request_ = new RestRequest();
            request_.Method = Method.Put;
            request_.AddHeader("Content-Type", "application/xml");
            request_.AddXmlBody(xmlData);

            var response_ = client.Execute(request_);
            if (response_.StatusCode == HttpStatusCode.OK || response_.StatusCode == HttpStatusCode.Created)
            {

                System.Xml.XmlDocument xmlDoc = new System.Xml.XmlDocument();
                xmlDoc.LoadXml(response_.Content);

                string message = xmlDoc.InnerText;

                if (message == "Application updated successfully.")
                {
                    MessageBox.Show("Updated successfully");
                    PopulateListBox(listBox1); // dá refresh á lista da esquerda
                    groupBox4.Visible = false;
                }
                else
                {
                    MessageBox.Show($"Error: {message}");
                }
            }
            else
            {
                MessageBox.Show($"Error: {response_.Content}");
            }
        }

        private void button16_Click(object sender, EventArgs e) // Editar button cancel
        {
            groupBox5.Visible = false;
        }

        private void button8_Click(object sender, EventArgs e) // Editar button do container
        {
            groupBox5.Visible = true;
        }

        private void button11_Click(object sender, EventArgs e) //Editar container confirm button
        {
            string appName = textBox4.Text; // nome da aplicação


            string xmlData = $"<container><name>{appName}</name></container>";

            var client = new RestClient(baseURI + selectedProduct1 +"/" + selectedContainerApplication);
            

            var request_ = new RestRequest();
            request_.Method = Method.Put;
            request_.AddHeader("Content-Type", "container/xml");
            request_.AddXmlBody(xmlData);

            var response_ = client.Execute(request_);
            if (response_.StatusCode == HttpStatusCode.OK || response_.StatusCode == HttpStatusCode.Created)
            {

                System.Xml.XmlDocument xmlDoc = new System.Xml.XmlDocument();
                xmlDoc.LoadXml(response_.Content);

                string message = xmlDoc.InnerText;

                if (message == "Container updated successfully.")
                {
                    MessageBox.Show("Updated successfully");
                    PopulateListBoxContainer(listBox2, selectedProduct1); // dá refresh á lista do container
                    groupBox5.Visible = false;
                }
                else
                {
                    MessageBox.Show($"Error: {message}");
                }
            }
            else
            {
                MessageBox.Show($"Error: {response_.Content}");
            }
        }

        private void button7_Click(object sender, EventArgs e) //Eliminar container
        {
            var client = new RestClient("http://localhost:65244/api/somiod/" + selectedProduct1 + "/" + selectedContainerApplication);

            var request_ = new RestRequest();
            request_.Method = Method.Delete;

            var response_ = client.Execute(request_);
            if (response_.StatusCode == HttpStatusCode.OK)
            {
                MessageBox.Show("Container " + selectedContainerApplication + " deleted");
                PopulateListBoxContainer(listBox2, selectedProduct1);
                listBox5.Items.Clear();
            }
            else
            {
                MessageBox.Show("Erro");
            }
        }

        private void button17_Click(object sender, EventArgs e) // adicionar data button cancel
        {
            groupBox6.Visible = false;
        }

        private void button18_Click(object sender, EventArgs e) //Adicionar Data Content confirm button
        {
            string appName = textBox5.Text; // content da data

            string xmlData = $"<data><content>{appName}</content></data>";

            var client = new RestClient(baseURI + selectedProduct1 +"/" + selectedContainerApplication);

            var request_ = new RestRequest();
            request_.Method = Method.Post;
            request_.AddHeader("Content-Type", "data/xml");
            request_.AddXmlBody(xmlData);

            var response_ = client.Execute(request_);
            if (response_.StatusCode == HttpStatusCode.OK || response_.StatusCode == HttpStatusCode.Created)
            {

                System.Xml.XmlDocument xmlDoc = new System.Xml.XmlDocument();
                xmlDoc.LoadXml(response_.Content);

                string message = xmlDoc.InnerText;

                if (message == "Data inserted successfully.")
                {
                    MessageBox.Show("Created successfully");
                    PopulateListBoxData(listBox4, selectedProduct1, selectedContainerApplication); // dá refresh á lista da data
                }
                else
                {
                    MessageBox.Show($"Error: {message}");
                }
            }
            else
            {
                MessageBox.Show($"Error: {response_.Content}");
            }
        }

        private void button10_Click(object sender, EventArgs e) //Eliminar data
        {
            var client = new RestClient("http://localhost:65244/api/somiod/" + selectedProduct1 + "/" + selectedContainerApplication +"/data/"+ selectedData);

            var request_ = new RestRequest();
            request_.Method = Method.Delete;

            var response_ = client.Execute(request_);
            if (response_.StatusCode == HttpStatusCode.OK)
            {
                MessageBox.Show("Data " + selectedData + " deleted");
                PopulateListBoxData(listBox4, selectedProduct1,selectedContainerApplication);
            }
            else
            {
                MessageBox.Show("Erro");
            }
        }

        private void listBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox4.SelectedIndex != -1)
            {
                selectedProduct1 = listBox1.SelectedItem.ToString();
                selectedContainerApplication = listBox2.SelectedItem.ToString();
                selectedData = listBox4.SelectedItem.ToString();

                groupBox1.Visible = false;
                groupBox2.Visible = false;
                groupBox3.Visible = true;
                groupBox4.Visible = false;
                groupBox5.Visible = false;
                groupBox6.Visible = false;
                PopulateListBoxDataContent(listBox6, selectedProduct1, selectedContainerApplication, selectedData);

            }
        }

        private void listBox6_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void listBox5_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex != -1 && listBox2.SelectedIndex != -1)
            {
                selectedProduct1 = listBox1.SelectedItem.ToString();
                selectedContainerApplication = listBox2.SelectedItem.ToString();

                groupBox1.Visible = false;
                groupBox2.Visible = false;
                groupBox3.Visible = true;
                groupBox4.Visible = false;
                groupBox5.Visible = false;
                groupBox6.Visible = false;

                selectedSubs = listBox5.SelectedItem.ToString();
                

            }
        }

        private void button20_Click(object sender, EventArgs e) //button cancel subs
        {
            groupBox7.Visible = false;
        }

        private void button19_Click(object sender, EventArgs e) // button subs confirm
        {
            string appName = textBox6.Text; // nome do sub
            string endpoint = textBox7.Text; // endpoint do sub
            string appApplication = selectedProduct1; // usa a aplicação selecionada na lista
            string appContainer = selectedContainerApplication; // usa o container selecionado na lista da INFO



            string xmlData = $"<subscription><name>{appName}</name><endpoint>{endpoint}</endpoint><event>creation</event></subscription>"; // data do xml

            var client = new RestClient(baseURI + appApplication+ "/"+appContainer); // faz o pedido á API com o nome da aplicação

            var request_ = new RestRequest();
            request_.Method = Method.Post;
            request_.AddHeader("Content-Type", "subscription/xml");
            request_.AddXmlBody(xmlData);

            var response_ = client.Execute(request_);
            if (response_.StatusCode == HttpStatusCode.OK || response_.StatusCode == HttpStatusCode.Created)
            {
                // Faz XML content
                System.Xml.XmlDocument xmlDoc = new System.Xml.XmlDocument();
                xmlDoc.LoadXml(response_.Content);

                string message = xmlDoc.InnerText;

                if (message == "Subscription inserted successfully.")
                {
                    MessageBox.Show("Created successfully");
                    PopulateListBoxSubs(listBox5, selectedProduct1, selectedContainerApplication);
                    groupBox7.Visible = false;
                }
                else
                {
                    MessageBox.Show($"Error: {message}");
                    PopulateListBoxSubs(listBox5, selectedProduct1, selectedContainerApplication);
                    groupBox7.Visible = false;
                }
            }
            else
            {
                MessageBox.Show($"Error: {response_.Content}");
                PopulateListBoxSubs(listBox5, selectedProduct1, selectedContainerApplication);
                groupBox7.Visible = false;
            }
        }

        private void button3_Click(object sender, EventArgs e) // button subs Adicionar
        {
            groupBox7.Visible = true;
        }

        private void button12_Click(object sender, EventArgs e) // button subs eliminar
        {
            var client = new RestClient("http://localhost:65244/api/somiod/" + selectedProduct1 + "/" + selectedContainerApplication+ "/subscription/" + selectedSubs);

            var request_ = new RestRequest();
            request_.Method = Method.Delete;

            var response_ = client.Execute(request_);
            if (response_.StatusCode == HttpStatusCode.OK)
            {
                MessageBox.Show("Subscription " + selectedSubs + " deleted");
                PopulateListBoxSubs(listBox5, selectedProduct1, selectedContainerApplication);
            }
            else
            {
                MessageBox.Show("Erro");
            }
        }
    }
}
