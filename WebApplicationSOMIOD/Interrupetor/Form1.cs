using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RestSharp;

namespace Interrupetor
{
    public partial class Form1 : Form
    {
        string baseURI = @"http://localhost:65244/api/somiod";

        public Form1()
        {
            InitializeComponent();
        }

        private void btnFanOn_Click(object sender, EventArgs e)
        {
            var client = new RestClient(baseURI + "/Ventoinhas/Vent1");
            var request = new RestRequest();
            string xmlBody = "<data><content>ON</content></data>";
            request.Method = Method.Post;
            request.AddXmlBody(xmlBody);

            var response = client.Execute(request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                MessageBox.Show("Ventoinha ligada");
            }
            else
            {
                MessageBox.Show("Erro ao ligar a ventoinha");
            }
        }

        private void btnFanOff_Click(object sender, EventArgs e)
        {
            var client = new RestClient(baseURI + "/Ventoinhas/Vent1");
            var request = new RestRequest();
            string xmlBody = "<data><content>OFF</content></data>";
            request.Method = Method.Post;
            request.AddXmlBody(xmlBody);

            var response = client.Execute(request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                MessageBox.Show("Ventoinha desligada");
            }
            else
            {
                MessageBox.Show("Erro ao desligar a ventoinha");
            }
        }
    }
}
