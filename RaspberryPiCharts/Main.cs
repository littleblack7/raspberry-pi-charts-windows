﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using Microsoft.VisualBasic;

namespace RaspberryPiCharts
{
    public partial class Main : Form
    {
        public class ResaponseObject
        {
            public float cpu;
            public float temp;
        }

        public string Endpoint = "";
        public string EndpointFileName = "endpoint.config";
        public System.Windows.Forms.DataVisualization.Charting.ChartColorPalette OldPalette; 

        public Main()
        {
            InitializeComponent();
            if (File.Exists(EndpointFileName))
            {
                Endpoint = File.ReadAllText(EndpointFileName);
            }
            else
            {
                SetEndpointDialog();
            }
        }

        public void SetEndpointDialog()
        {
            string OldEndpoint = Endpoint;
            Endpoint = "";
            Endpoint = Interaction.InputBox("Set endpoint", "Set endpoint", OldEndpoint);
            File.WriteAllText(EndpointFileName, Endpoint);
        }

        private void MainTimer_Tick(object sender, EventArgs e)
        {
            if (Endpoint != "")
            {
                try
                {
                    WebClient Http = new WebClient();
                    Http.DownloadStringAsync(new Uri(Endpoint));
                    Http.DownloadStringCompleted += new DownloadStringCompletedEventHandler(ResponseListener);
                }
                catch (Exception ex)
                {
                    SetEndpointDialog();
                }
            }

        }


        public void ResponseListener(Object sender, DownloadStringCompletedEventArgs e)
        {
            var webException = e.Error as WebException;
            if (webException != null && webException.Status == WebExceptionStatus.NameResolutionFailure)
                return;
            try
            {
                string Response = (string)e.Result;
                ResaponseObject Object = JsonConvert.DeserializeObject<ResaponseObject>(Response);

                CpuChart.ChartAreas[0].AxisY.Minimum = 0;
                CpuChart.ChartAreas[0].AxisY.Maximum = 100;
                CpuChart.Series[0].Points.AddY(Object.cpu);
                TemperatureChart.ChartAreas[0].AxisY.Minimum = 0;
                TemperatureChart.ChartAreas[0].AxisY.Maximum = 100;
                TemperatureChart.Series[0].Points.AddY(Object.temp);
            }
            catch (Exception)
            {

            }
        }

        private void setEndpointToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetEndpointDialog();
        }

        private void ResetMenuItem_Click(object sender, EventArgs e)
        {
            CpuChart.Series[0].Points.Clear();
            TemperatureChart.Series[0].Points.Clear();
        }

        private void FancifyMenuItem_Click(object sender, EventArgs e)
        {
            if (CpuChart.Series[0].Palette != System.Windows.Forms.DataVisualization.Charting.ChartColorPalette.Pastel)
            {
                OldPalette = CpuChart.Series[0].Palette;
                CpuChart.Series[0].Palette = System.Windows.Forms.DataVisualization.Charting.ChartColorPalette.Pastel;
                TemperatureChart.Series[0].Palette = System.Windows.Forms.DataVisualization.Charting.ChartColorPalette.Pastel;
                FancifyMenuItem.Text = "Borify!";
            }
            else
            {
                CpuChart.Series[0].Palette = OldPalette;
                TemperatureChart.Series[0].Palette = OldPalette;
                FancifyMenuItem.Text = "Fancify!";
            }
        }
    }
}
