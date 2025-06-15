using System.Windows;
using System.Windows.Controls;
using SportVAR.Models;
using SportVAR.ViewModels;

namespace SportVAR;

public partial class Window1
{
    public Window1(MainViewModel2 viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private MainViewModel2 Vm => (MainViewModel2)DataContext;

    private void cmbCameraNames_Loaded(object sender, RoutedEventArgs e)
    {
        cmbCamera1Names.ItemsSource = Vm.Camera1Options;
        cmbCamera2Names.ItemsSource = Vm.Camera2Options;
    }

    private void cmbCamera1Names_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (cmbCamera1Names.SelectedItem is CameraModel model)
            Vm.Camera1Selected(model);
    }

    private void cmbCamera1Resolutions_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (cmbCamera1Resolutions.SelectedItem is CameraDetail detail)
            Vm.Camera1ResolutionSelected(detail);
    }

    private void cmbCamera2Names_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (cmbCamera2Names.SelectedItem is CameraModel model)
            Vm.Camera2Selected(model);
    }

    private void cmbCamera2Resolutions_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (cmbCamera2Resolutions.SelectedItem is CameraDetail detail)
            Vm.Camera2ResolutionSelected(detail);
    }
}