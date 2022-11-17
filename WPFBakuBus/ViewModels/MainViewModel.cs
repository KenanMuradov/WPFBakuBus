using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using Microsoft.Maps.MapControl.WPF;
using WPFBakuBus.Models;
using System.Text.Json;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using System.Net.Http;
using System.Windows.Input;
using WPFBakuBus.Commands;
using System.Windows.Controls;
using System.Collections.ObjectModel;

namespace WPFBakuBus.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string prorertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prorertyName));

    public ObservableCollection<string> ComboBoxBuses { get; set; }
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


    public MainViewModel(string key)
    {
        MapKey = new ApplicationIdCredentialsProvider(key);

        ComboBoxBuses = new();
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
        if (parametr is MapItemsControl map)
        {
            var busName = ComboBoxBuses[CurrentIndex];

            if (busName == "View All")
                foreach (var bus in map.Items.OfType<Bus>())
                    bus.Attributes.VISIBILITY = Visibility.Visible;

            foreach (var bus in map.Items.OfType<Bus>())
            {

                if (bus.Attributes.DISPLAY_ROUTE_CODE != busName)
                    bus.Attributes.VISIBILITY = Visibility.Collapsed;
                else
                    bus.Attributes.VISIBILITY = Visibility.Visible;
            }

        }

    }

    private void Timer_Tick(object? sender, EventArgs e) => UpdateBakuBusStatus();

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
