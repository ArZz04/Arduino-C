using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Arduino_C
{
    public sealed partial class MainPage : Page
    {
        SerialDevice serialPort;
        DataWriter dataWriter;
        DataReader dataReader;

        public MainPage()
        {


            this.InitializeComponent();
            _ = MostrarPuertosDisponibles();
            ConnectArduino();   // Conectar automáticamente al iniciar
        }

        private async void ConnectArduino()
        {
            try
            {
                string selector = SerialDevice.GetDeviceSelector();
                var devices = await DeviceInformation.FindAllAsync(selector);

                DeviceInformation com9 = null;

                foreach (var d in devices)
                {
                    if (d.Name.Contains("(COM8)"))   // <-- Aquí coincide EXACTO con tu captura
                    {
                        com9 = d;
                        break;
                    }
                }

                if (com9 == null)
                {
                    StatusIndicator.Text = "Estado: USB Serial Device (COM9) no encontrado";
                    return;
                }

                await Task.Delay(1500);

                serialPort = await SerialDevice.FromIdAsync(com9.Id);

                if (serialPort == null)
                {
                    StatusIndicator.Text = "Estado: Error al abrir COM9";
                    return;
                }

                serialPort.BaudRate = 9600;
                serialPort.DataBits = 8;
                serialPort.StopBits = SerialStopBitCount.One;
                serialPort.Parity = SerialParity.None;

                dataWriter = new DataWriter(serialPort.OutputStream);
                dataReader = new DataReader(serialPort.InputStream);

                StatusIndicator.Text = $"Estado: Conectado a {com9.Name}";
            }
            catch (Exception ex)
            {
                StatusIndicator.Text = $"Estado: Error → {ex.Message}";
            }
        }

        private async Task MostrarPuertosDisponibles()
        {
            string selector = SerialDevice.GetDeviceSelector();
            var devices = await DeviceInformation.FindAllAsync(selector);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Puertos COM detectados:");

            if (devices.Count == 0)
            {
                sb.AppendLine("Ninguno");
            }
            else
            {
                foreach (var d in devices)
                {
                    sb.AppendLine($"• {d.Name}");
                }
            }

            StatusIndicator.Text = sb.ToString();
        }


        private async void SendToArduino(string msg)
        {
            if (serialPort == null)
            {
                StatusIndicator.Text = "Estado: Desconectado";
                return;
            }

            dataWriter.WriteString(msg + "\n");
            await dataWriter.StoreAsync();
        }

        private void BtnOn_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Botón ON presionado");
            SendToArduino("ON");
        }

        private void BtnOff_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Botón OFF presionado");
            SendToArduino("OFF");
        }

    }
}
