using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Rapimed
{
    public partial class MainPage : ContentPage
    {
        public List<string> ids { get; set; }
        public string id { get; set; }
        public List<string> opciones { get; set; }
        public List<string> opcionesConfigurarDispositivo { get; set; }
        public List<string> opcionesEjecutarFuncion { get; set; }
        public string cadenaConcatenada { get; set; }

        static HttpClient _client = new HttpClient();
        public MainPage()
        {
            InitializeComponent();
            FetchStuffAsync();
        }
        private async Task<string> FetchStuffAsync()
        {
            using (var response = await _client.GetAsync(CONSTANTES.URL_BASE+"/api/Rapimed"))
            {
                if (response.IsSuccessStatusCode)
                {
                    // Horray it went well!
                    var page = await response.Content.ReadAsStringAsync();
                    inicializarPickers(page);
                    return page;
                }
            }
            return null;
        }
        private void inicializarPickers(string idsJson)
        {
            ids = new List<string>();
            //lleno los ids con lo que recibo del json
            var jsonData = JArray.Parse(idsJson);
            //inicio el for en 1 para omitir el Test1
            for (int i = 1; i < jsonData.Count; i++)
            {
                ids.Add(jsonData[i].ToString());
            }
            idsPicker.ItemsSource = ids;
            //opciones
            opciones = new List<string>()
            {
                "Setear PIN",
                "Configurar dispositivo",
                "Ejecutar Funcion"
            };
            opcionPicker.ItemsSource = opciones;
            //opciones segun opcion
            opcionesConfigurarDispositivo = new List<string>()
            {
                "Ultrasonido",
                "Pin"
            };
            opcionesEjecutarFuncion= new List<string>()
            {
                "Encender y apagar 3 luces juntas",
                "Encender y apagar 3 luces en sucecion",
                "Medir distancia con presicion",
                "Medir distancia"
            };
            //setea los picker al primer valor
            idsPicker.SelectedIndex = 0;
            opcionPicker.SelectedIndex = 0;
            cambiarTercerPckerSegunSeleccion(0);
        }
        private void sendCommandButton_Clicked(object sender, EventArgs e)
        {
            enviar();
        }
        private void enviar()
        {
            //todo envio de cadena al backend
            actualizarCadena();
            postSendRawCommand();
        }

        private async Task<string> postSendRawCommand()
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri(CONSTANTES.URL_BASE);
            var jsonData = JsonConvert.SerializeObject(
            new
            {
                controllerId = id,
                cmd = cadenaConcatenada
            });

            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PostAsync("/SendRawCommand", content);
            var result = await response.Content.ReadAsStringAsync();
            string mensaje = "Enviado:\n" + id + "\n" + cadenaConcatenada + "\nResultado:\n" + result;
            await DisplayAlert("", mensaje, "Ok");
            return result;
        }

        private void opcionPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            cambiarTercerPckerSegunSeleccion(opcionPicker.SelectedIndex);
        }
        void cambiarTercerPckerSegunSeleccion(int i)
        {
            switch (i)
            {
                case 0:
                    accionPicker.ItemsSource = null;
                    accionPicker.IsVisible = false;
                    comandosEntry.Placeholder = "Set PIN Here";
                    break;
                case 1:
                    accionPicker.ItemsSource = opcionesConfigurarDispositivo;
                    accionPicker.SelectedIndex = 0;
                    accionPicker.IsVisible = true;
                    comandosEntry.Placeholder = "Comandos";
                    break;
                case 2:                    
                    accionPicker.ItemsSource = opcionesEjecutarFuncion;
                    accionPicker.SelectedIndex = 0;
                    accionPicker.IsVisible = true;
                    comandosEntry.Placeholder = "Comandos";
                    break;
            }
        }
        void actualizarCadena()
        {
            id = limpiarID(ids[idsPicker.SelectedIndex]);
            cadenaConcatenada += id;
            string segundaOp = opcionPicker.SelectedIndex == 0 ? "S" :
                                    opcionPicker.SelectedIndex == 1 ? "C" : "F";
            cadenaConcatenada += segundaOp+"|";
            if (segundaOp == "F")
                cadenaConcatenada += accionPicker.ItemsSource != null ? (accionPicker.SelectedIndex + 1)+"" : "";
            else if (segundaOp == "C")
            {
                if (accionPicker.ItemsSource != null)
                    cadenaConcatenada += accionPicker.SelectedIndex == 0  ?  "U": "P";
            }
            //valores de la entrada de comandos
            if (!comandosEntry.Text.Contains(","))
                return;
            if (comandosEntry.Text.Split(',').Length == 0)
            {
                cadenaConcatenada += "|" + comandosEntry.Text;
            }
            else if (comandosEntry.Text.Split(',').Length >= 1)
            {
                var comands = comandosEntry.Text.Split(',');
                foreach (var c in comands)
                {
                    cadenaConcatenada += "|" + c;
                }
            }
            
        }
        string limpiarID(string ID)
        {
            Regex soloNumeros = new Regex(@"[^\d]");
            return soloNumeros.Replace(ID, "");
        }
    }
}
