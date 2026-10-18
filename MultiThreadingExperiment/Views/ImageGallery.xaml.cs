using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MultiThreadingExperiment.ViewModels;

namespace MultiThreadingExperiment.Views;

public sealed partial class ImageGallery : UserControl
{
    private ImageGalleryViewModel? _viewModel;

    public ImageGallery()
    {
        InitializeComponent();

        // lifecycle events provided by the WinUI controles
        Loaded += ImageGallery_Loaded; // Fires when ImageGallary is connected to VisualTree and ready to participate in UI 
        Unloaded += ImageGallery_Unloaded; // Fires when usercontrole is disconnected from VisualTree
    }

    /// <summary>
    /// if the _viewmodel is null instanctiate the viewmodel 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ImageGallery_Loaded(object sender, RoutedEventArgs e)
    {
        if (_viewModel != null)
            return;

        _viewModel = new ImageGalleryViewModel(DispatcherQueue);
        DataContext = _viewModel;
    }

    /// <summary>
    /// stop the background thread using Dispose and set _viewmodel to null because
    /// user control is no longer connected to the visual tree 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ImageGallery_Unloaded(object sender, RoutedEventArgs e)
    {
        _viewModel?.Dispose();
        _viewModel = null;
        DataContext = null;
    }

    private void RefreshImagesButton(object sender, RoutedEventArgs e)
    {
        _viewModel?.LoadImages();
    }
}