using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

//librerias
using YoutubeExplode;
using YoutubeExplode.Models.MediaStreams;

namespace Youtube
{
    public partial class Form1 : Form
    {

        //Son necesarias las siguientes librerias Nuget:
        //YoutubeExplode: dotnet add package YoutubeExplode
        //VideoConverter:  Install-Package NReco.VideoConverter -Version 1.1.3 
        // https://www.nuget.org/packages/NReco.VideoConverter/ 


        public Form1()
        {
            InitializeComponent();
        }

        //Normaliza los niveles de salida del audio
        private static string NormalizeVideoId(string input)
        {
            return YoutubeClient.TryParseVideoId(input, out var videoId)
                ? videoId
                : input;
        }


        //el async permite que sea un proceso asincrono, es decir que el proceso de conversión siga corriendo
        //independientemente de los demás procesos, y que no parezca que el programa se congeló                
        private async void button1_ClickAsync(object sender, EventArgs e)
        {
            
            //nuevo cliente de Youtube
            var client = new YoutubeClient();            
            //lee la dirección de youtube que le escribimos en el textbox
            var videoId = NormalizeVideoId(txtURL.Text);
            var video = await client.GetVideoAsync(videoId);
            var streamInfoSet = await client.GetVideoMediaStreamInfosAsync(videoId);

            // Busca la mejor resolución en la que está disponible el video
            var streamInfo = streamInfoSet.Muxed.WithHighestVideoQuality();
            
            // Compone el nombre que tendrá el video en base a su título y extensión
            var fileExtension = streamInfo.Container.GetFileExtension();
            var fileName = $"{video.Title}.{fileExtension}";

            //TODO: Reemplazar los caractéres ilegales del nombre
            //fileName = RemoveIllegalFileNameChars(fileName);

            //Activa el timer para que el proceso funcione de forma asincrona
            tmrVideo.Enabled = true;

            // mensajes indicando que el video se está descargando
            txtMensaje.Text = "Descargando el video ... ";

            //TODO: se pude usar una barra de progreso para ver el avance
            //using (var progress = new ProgressBar())

            //Empieza la descarga
            await client.DownloadMediaStreamAsync(streamInfo, fileName);

            //Ya descargado se inicia la conversión a MP3
            var Convert = new NReco.VideoConverter.FFMpegConverter();
            //Especificar la carpeta donde se van a guardar los archivos, recordar la \ del final
            String SaveMP3File = @"E:\MP3\" + fileName.Replace(".mp4", ".mp3");
            //Guarda el archivo convertido en la ubicación indicada
            Convert.ConvertMedia(fileName, SaveMP3File, "mp3");

            //Si el checkbox de solo audio está chequeado, borrar el mp4 despues de la conversión
            if (ckbAudio.Checked)
                File.Delete(fileName);

            
            //Indicar que se terminó la conversion
            txtMensaje.Text = "Archivo Convertido en MP3";
            tmrVideo.Enabled = false;
            txtMensaje.BackColor = Color.White;

            //TODO: Cargar el MP3 al reproductor o a la lista de reproducción
            //CargarMP3s();
            //Se puede incluir un checkbox para indicar que de una vez se reproduzca el MP3
            //if (ckbAutoPlay.Checked) 
            //  ReproducirMP3(SaveMP3File);
            return;
            }


        //Para agregar el control Windows Media Player a la barra de herramientas
        //https://docs.microsoft.com/en-us/windows/desktop/wmp/using-the-windows-media-player-control-with-microsoft-visual-studio

        //Al cargar el formulario ponemos el reproductor como invisible
        private void Form1_Load(object sender, EventArgs e)
        {
            //Modos de poner invisible al reproductor, puede usar la que mas se ajuste a su diseño de interfaz
            //Aquí puede ver los modos: https://docs.microsoft.com/en-us/windows/desktop/wmp/player-uimode
            Reproductor.uiMode = "invisible";

            //O simplemente hacerlo invisible
            //Reproductor.Visible = false;

            //O hacerlo de tamaño 0 de alto por 0 de ancho
            //Reproductor.Width = 0;
            //Reproductor.Height = 0;


        }


        //Reproducir A través de Windows Player
        private void btnPlay_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //Aquí se le indica al reproductor el nombre del archivo que va a reproducir
                Reproductor.URL = openFileDialog1.FileName;
            }
            Reproductor.Ctlcontrols.play();

        }


        //Detener la reproducción en Windows Player
        private void btnStop_Click(object sender, EventArgs e)
        {
            Reproductor.Ctlcontrols.stop();
        }
    }
    }

