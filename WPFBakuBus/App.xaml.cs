using Microsoft.Extensions.Configuration;
using Microsoft.Maps.MapControl.WPF;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using WPFBakuBus.ViewModels;
using WPFBakuBus.Views;

namespace WPFBakuBus;

public partial class App : Application
{
    private void Application_Startup(object sender, StartupEventArgs e)
    {
        var key = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build()["mapKey"];

        var mainViewModel = new MainViewModel(key);
        var mainView = new MainView();

        mainView.DataContext = mainViewModel;

        mainView.ShowDialog();
    }
}
