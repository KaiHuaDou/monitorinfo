using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace monitorinfo;

public partial class MainWindow : Window
{
    public MainWindow( )
    {
        InitializeComponent( );
        MainPanel.DataContext = info;
    }

    private List<Edid> EDIDs;
    private Info info = new( );

    private void RefreshMonitor(object o, RoutedEventArgs e)
    {
        EDIDs = Edid.Get( );
        MonitorBox.Items.Clear( );
        for (int i = 0; i < EDIDs.Count; i++)
            MonitorBox.Items.Add($"显示器{i}");
        if (MonitorBox.SelectedIndex == -1)
            MonitorBox.SelectedIndex = 0;
    }

    private void WindowLoaded(object o, RoutedEventArgs e)
    {
        RefreshMonitor(null, null);
        RefreshDiagram( );
    }

    private void MonitorBoxSelectionChanged(object o, SelectionChangedEventArgs e)
    {
        RefreshDiagram( );
    }

    private void RefreshDiagram( )
    {
        if (MonitorBox.SelectedIndex == -1)
            return;
        info.GetFrom(EDIDs[MonitorBox.SelectedIndex].monitorEDID);
        ImageGamut.Source = GamutImage.Draw(info);
    }

    private void SetClipboard(object o, MouseButtonEventArgs e)
    {
        Clipboard.SetDataObject(LblProductID.Content);
    }
}
