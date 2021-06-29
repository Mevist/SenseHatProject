﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

namespace MultiWPFApp.ViewModel
{
    using Model;
    public class ConfigViewModel : BaseViewModel
    {
        ConfigParams config;

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

        private int maxSamples;
        public int MaxSamples
        {
            get { return maxSamples; }
            set
            {
                if (maxSamples != value)
                {
                    maxSamples = value;
                    OnPropertyChanged("MaxSamples");
                }
            }
        }

        public ButtonCommand SetParams { get; set; }

        public ButtonCommand LoadDefault { get; set; }

        public ConfigViewModel()
        {
            config = new ConfigParams();
            MaxSamples = config.MaxSample;
            SampleTime = config.SampleTime.ToString();
            Url = config.Url;
            SetParams = new ButtonCommand(SaveParams);
            LoadDefault = new ButtonCommand(DefaultParams);
        }
        // Saving parameters to JSON file
        private void SaveParams()
        {
            var data = new Dictionary<string, string>();
            data.Add("sample_time", SampleTime);
            data.Add("sample_amount", maxSamples.ToString());  
            data.Add("url", Url);
            string json = JsonConvert.SerializeObject(data);
            File.WriteAllText("config.json", json);
        }

        private void DefaultParams()
        {
            var data = new Dictionary<string, string>();
            MaxSamples = config.maxSampleDefault;
            SampleTime = config.sampleTimeDefault.ToString();
            Url = config.urlDefault;
            data.Add("sample_time", SampleTime.ToString());
            data.Add("sample_amount", MaxSamples.ToString());
            data.Add("url", Url.ToString());
            string json = JsonConvert.SerializeObject(data);
            File.WriteAllText("config.json", json);
        }
    }
}
