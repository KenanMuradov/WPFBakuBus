﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Maps.MapControl.WPF;
using WPFBakuBus.Models;
using System.Text.Json;
using System.IO;
using System.Windows;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using System.Net.Http;
using System.Windows.Input;
using WPFBakuBus.Commands;
using System.Security.Policy;
using System.Runtime.Serialization.Json;

namespace WPFBakuBus.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string prorertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prorertyName));

    private List<string> comboBoxBuses;

    public List<string> ComboBoxBuses
    {
        get { return comboBoxBuses; }
        set
        {
            comboBoxBuses = value;
            OnPropertyChanged(nameof(ComboBoxBuses));
        }
    }

    public ICommand BusSelectCommand { get; set; }

    public int CurrentIndex { get; set; }

    private BakuBus? bakuBus;

    public BakuBus? BakuBus
    {
        get { return bakuBus; }
        set
        {
            bakuBus = value;
            OnPropertyChanged(nameof(BakuBus));
        }
    }

    private ApplicationIdCredentialsProvider? mapKey;

    public ApplicationIdCredentialsProvider? MapKey
    {
        get { return mapKey; }
        set
        {
            mapKey = value;
            OnPropertyChanged(nameof(MapKey));
        }
    }


    public MainViewModel()
    {
        var key = new ConfigurationBuilder().AddJsonFile("C:\\Users\\User\\source\\repos\\WPFBakuBus\\WPFBakuBus\\appsettings.json").Build()["mapKey"];

        MapKey = new ApplicationIdCredentialsProvider(key);

        comboBoxBuses = new();
        ComboBoxBuses.Add("View All");

        UpdateBakuBusStatus();
        

        DispatcherTimer timer = new();
        timer.Interval = new TimeSpan(10000);
        timer.Tick += Timer_Tick;
        timer.Start();

        BusSelectCommand = new RelayCommand(ExecuteBusSelectCommand);
    }

    private void ExecuteBusSelectCommand(object? parametr)
    {
        //if (parametr is MapItemsControl map)
        //{
        //    var busName = ComboBoxBuses[CurrentIndex];

        //    foreach (var bus in map.Items.OfType<Bus>())
        //    {

        //        if (bus.Attributes.DISPLAY_ROUTE_CODE != busName)
        //            bus.Attributes.VISIBILITY = Visibility.Collapsed;
        //        else
        //            bus.Attributes.VISIBILITY = Visibility.Visible;
        //    }

        //}
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        UpdateBakuBusStatus();
    }

    private async void UpdateBakuBusStatus()
    {
        try
        {
            var client = new HttpClient();
            var jsonStr = await client.GetStringAsync("https://www.bakubus.az/az/ajax/apiNew1");
            BakuBus = JsonSerializer.Deserialize<BakuBus>(jsonStr);
        }
        catch (Exception)
        {
            //BakuBus = JsonSerializer.Deserialize<BakuBus>(File.ReadAllText("../../../bakubusApi.json"));
        }

        if (BakuBus != null)
        {
            double latitude, longitude;

            foreach (var bus in BakuBus.Buses)
            {
                latitude = double.Parse(bus.Attributes.LATITUDE);
                longitude = double.Parse(bus.Attributes.LONGITUDE);

                bus.Attributes.LOCATION = new Location(latitude, longitude);
            }

            foreach (var bus in BakuBus.Buses)
            {
                if (!ComboBoxBuses.Contains(bus.Attributes.DISPLAY_ROUTE_CODE))
                    ComboBoxBuses.Add(bus.Attributes.DISPLAY_ROUTE_CODE);
            }
        }

    }

}
