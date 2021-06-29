﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Timers;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using Newtonsoft.Json;

namespace MultiWPFApp.ViewModel
{
    using Model;
    public class JoystickViewModel : BaseViewModel
    {
        private string url;
        public string Url
        {
            get { return url; }
            set
            {
                if (url != value)
                {
                    url = value;
                    OnPropertyChanged("Url");
                }
            }
        }

        private int sampleTime;
        public string SampleTime
        {
            get { return sampleTime.ToString(); }
            set
            {
                if (Int32.TryParse(value, out int st))
                {
                    if (sampleTime != st)
                    {
                        sampleTime = st;
                        OnPropertyChanged("SampleTime");
                    }
                }
            }
        }

        private string xPoint;
        public string XPoint
        {
            get { return xPoint; }
            set
            {
                if (xPoint != value)
                {
                    xPoint = value;
                    OnPropertyChanged("XPoint");
                }
            }
        }

        private string yPoint;
        public string YPoint
        {
            get { return yPoint; }
            set
            {
                if (yPoint != value)
                {
                    yPoint = value;
                    OnPropertyChanged("YPoint");
                }
            }
        }

        //Initialize variables and models
        public PlotModel DataPlotModel { get; set; }
        public ButtonCommand StartButton { get; set; }
        public ButtonCommand StopButton { get; set; }
        public ButtonCommand UpdateConfigButton { get; set; }
        public ButtonCommand DefaultConfigButton { get; set; }

        private int timeStamp = 0;
        private ConfigParams config = new ConfigParams();
        private Timer RequestTimer;
        private ServerIoT server;


        public JoystickViewModel()
        {
            DataPlotModel = new PlotModel { Title = "Joystick position data" };

            //Adding X and Y axis
            DataPlotModel.Axes.Add(new LinearAxis()
            {
                Position = AxisPosition.Bottom,
                Minimum = -25,
                Maximum = 25,
                Key = "Horizontal",
                Unit = "-",
                Title = "Coordinate x"
            });
            DataPlotModel.Axes.Add(new LinearAxis()
            {
                Position = AxisPosition.Left,
                Minimum = -25,
                Maximum = 25,
                Key = "Vertical",
                Unit = "-",
                Title = "Coordinate y"
            });

            // Adding series for each orientation
            DataPlotModel.Series.Add(new LineSeries() { Title = "Point position", Color = OxyColor.Parse("#FFFF0000") });

            // Initializing buttons
            StartButton = new ButtonCommand(StartTimer);
            StopButton = new ButtonCommand(StopTimer);
            UpdateConfigButton = new ButtonCommand(UpdateConfig);
            DefaultConfigButton = new ButtonCommand(DefaultConfig);

            url = config.Url;
            sampleTime = config.SampleTime;

            server = new ServerIoT(Url);
        }

        // Update plot with new data points
        private void UpdatePlot(double t, double x, double y)
        {
            LineSeries xLineSeries = DataPlotModel.Series[0] as LineSeries;


            xLineSeries.Points.Add(new DataPoint(x, y));

            //if (xLineSeries.Points.Count > config.maxSampleDefault)
            //{
            //    xLineSeries.Points.RemoveAt(0);
            //    yLineSeries.Points.RemoveAt(0);
            //}

            XPoint = x.ToString();
            YPoint = y.ToString();
            if (t >= config.XAxisMax)
            {
                DataPlotModel.Axes[0].Minimum = (t - config.XAxisMax);
                DataPlotModel.Axes[0].Maximum = t + config.SampleTime / 1000.0; ;
            }

            DataPlotModel.InvalidatePlot(true);
        }

        // Asynchrous chart update with parsing json data from server
        private async void UpdatePlot()
        {

            string responseText = await server.POSTwithClient("joystick");

            try
            {
                JoystickData responseJson = JsonConvert.DeserializeObject<JoystickData>(responseText);
                UpdatePlot(timeStamp / 1000.0, responseJson.x, responseJson.y);
            }
            catch (Exception e)
            {
                Debug.WriteLine("JSON DATA ERROR");
                Debug.WriteLine(responseText);
                Debug.WriteLine(e);
            }

            timeStamp += config.SampleTime;
        }
        private void RequestTimerElapsed(object sender, ElapsedEventArgs e)
        {
            UpdatePlot();
        }


        private void StartTimer()
        {
            if (RequestTimer == null)
            {
                RequestTimer = new Timer(config.SampleTime);
                RequestTimer.Elapsed += new ElapsedEventHandler(RequestTimerElapsed);
                RequestTimer.Enabled = true;

                DataPlotModel.ResetAllAxes();
            }
        }

        private void StopTimer()
        {
            if (RequestTimer != null)
            {
                RequestTimer.Enabled = false;
                RequestTimer = null;
            }
        }

        // Update parameters when Update Config button is clicked
        private void UpdateConfig()
        {
            bool restartTimer = (RequestTimer != null);

            if (restartTimer)
                StopTimer();

            config = new ConfigParams(url,sampleTime, 100);
            server = new ServerIoT(Url);

            if (restartTimer)
                StartTimer();
        }

        // Loads default config
        private void DefaultConfig()
        {
            bool restartTimer = (RequestTimer != null);

            if (restartTimer)
                StopTimer();

            config = new ConfigParams();
            Url = config.Url;
            SampleTime = config.SampleTime.ToString();
            server = new ServerIoT(Url);

            if (restartTimer)
                StartTimer();
        }

    }
}