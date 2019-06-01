using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using YoutubeExplode;
using YoutubeExplode.Models.MediaStreams;

namespace YouTube
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private static string NormalizeVideoId(string input)
        {
            string videoId = string.Empty;

            return YoutubeClient.TryParseVideoId(input, out videoId)
                ? videoId
                : input;
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            //nuevo cliente de Youtube
            var client = new YoutubeClient();
            //lee la dirección de youtube que le escribimos en el textbox
            var videoId = NormalizeVideoId(txtURL.Text);
            var video = await client.GetVideoAsync(videoId); //Esta descarga el video
            var streamInfoSet = await client.GetVideoMediaStreamInfosAsync(videoId); //Esta descarga la info sobre el video como nombre y eso

            // Busca la mejor resolución en la que está disponible el video
            var streamInfo = streamInfoSet.Muxed.WithHighestVideoQuality(); //Descargar video con la alta calidad

            // Compone el nombre que tendrá el video en base a su título y extensión
            var fileExtension = streamInfo.Container.GetFileExtension(); //Mira que extension tiene por defecto Mp4
            var fileName = $"{video.Title}.{fileExtension}"; //Pone nombre al video con la extension MP4
            //var fileName = "E:\\"+$"{video.Title}.{fileExtension}";

            //TODO: Reemplazar los caractéres ilegales del nombre
            //fileName = RemoveIllegalFileNameChars(fileName); //Elimina caracteres que no son legales como ñ o por el estilo
            //Activa el timer para que el proceso funcione de forma asincrona
            tmrVideo.Enabled = true;

            // mensajes indicando que el video se está descargando
            txtMensaje.Text = "Descargando el video ... ";

            //TODO: se pude usar una barra de progreso para ver el avance
            //using (var progress = new ProgressBar())

            //Empieza la descarga
            await client.DownloadMediaStreamAsync(streamInfo, fileName);//Aqui empieza la descarga

            //Ya descargado se inicia la conversión a MP3
            var Convert = new NReco.VideoConverter.FFMpegConverter();//Ya descargado se convierte
            //Especificar la carpeta donde se van a guardar los archivos, recordar la \ del final
            String SaveMP3File = @"C:\Users\CruzAguilar\Documents\Visual Studio 2015\Projects\Programación III\YouTube\YouTube\bin\Descargas\" + fileName.Replace(".mp4", ".mp3");//Aqui se indica en donde guardara el archivo
            //Guarda el archivo convertido en la ubicación indicada
            Convert.ConvertMedia(fileName, SaveMP3File, "mp3");//Convierte a MP3 y se dice que formato se quiere usar

            //Si el checkbox de solo audio está chequeado, borrar el mp4 despues de la conversión
            if (ckbAudio.Checked)//Si el chekbox esta habilitado
                File.Delete(fileName);//Guarda solo el audio


            //Indicar que se terminó la conversion
            txtMensaje.Text = "Archivo Convertido en MP3";
            tmrVideo.Enabled = false;
            txtMensaje.BackColor = Color.White;
            label2.Text = fileName;
            //TODO: Cargar el MP3 al reproductor o a la lista de reproducción
            //CargarMP3s();
            //Se puede incluir un checkbox para indicar que de una vez se reproduzca el MP3
            //if (ckbAutoPlay.Checked) 
            //  ReproducirMP3(SaveMP3File);
            return;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Reproductor.uiMode = "invisible"; //para ocultar el reproductor
        }

        private void btnPlay_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //Aqui se le indica al reproductor el nombre del archivo
                Reproductor.URL = openFileDialog1.FileName;
                label3.Text = openFileDialog1.SafeFileName;
            }
            Reproductor.Ctlcontrols.play();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            Reproductor.Ctlcontrols.stop();
        }

        private void btnPause_Click(object sender, EventArgs e)
        {
            Reproductor.Ctlcontrols.pause();
        }
    }
}
