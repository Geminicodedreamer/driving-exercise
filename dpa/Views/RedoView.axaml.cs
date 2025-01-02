using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;

namespace dpa.Views;

public partial class RedoView : UserControl
{
    public RedoView()
    {
        InitializeComponent();
        DataContext = ServiceLocator.Current.RedoViewModel;
    }
}