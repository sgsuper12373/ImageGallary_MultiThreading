
using Microsoft.UI.Dispatching;
using MultiThreadingExperiment.Models;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;

namespace MultiThreadingExperiment.ViewModels;

/// <summary>
/// In the Following ImageGalleryViewModel class we do following
///     -> read the folder and populate the observable collections Images, this images are later used by the views
///     -> We have a background thread which will update the images ImageGallery usercontrol every 2000 ms 
///     -> The thread is managed using the Manual event
/// </summary>
public class ImageGalleryViewModel : IDisposable
{
    private readonly string _folder = @"C:\Users\Sumit\Pictures\Screenshots";
    private readonly DispatcherQueue _dispatcherQueue;

    private Thread? _backgroundThread;
    private readonly ManualResetEventSlim _stopEvent = new(false);
    private bool _disposed; // By defaults it will be set to false by c#

    public ObservableCollection<ImageItem> Images { get; } = new();

    /// <summary>
    /// Initializes a new ImageGalleryViewModel that uses the provided DispatcherQueue, initiates image loading, and
    /// starts background processing.
    /// </summary>
    /// <param name="dispatcherQueue">DispatcherQueue used to marshal work to the UI thread.</param>
    public ImageGalleryViewModel(DispatcherQueue dispatcherQueue)
    {
        _dispatcherQueue = dispatcherQueue;

        LoadImages();
        StartBackgroundThread();
    }

    /// <summary>
    /// Scan the _folder directory and return the stringa array containing path of image files 
    /// </summary>
    /// <returns></returns>
    private string[] GetImageFiles()
    {
        if (!Directory.Exists(_folder))
            return Array.Empty<string>();

        return Directory.GetFiles(_folder)
            .Where(file =>
                file.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                file.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                file.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                file.EndsWith(".webp", StringComparison.OrdinalIgnoreCase))
            .OrderBy(file => file, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    /// <summary>
    /// Clear the current Images collection and then load images again from _folder directory 
    /// </summary>
    public void LoadImages()
    {
        string[] files = GetImageFiles();

        Images.Clear();

        foreach (string file in files)
        {
            Images.Add(new ImageItem
            {
                ImagePath = file
            });
        }
    }

    /// <summary>
    /// Run the current background thread which checks if there are some changes in 
    /// _folder directory using the MoniterFolders functions, 
    /// if something is changed then cal the LoadImages() using dispatcherqueue and update the display elements
    /// </summary>
    private void StartBackgroundThread()
    {
        _backgroundThread = new Thread(MonitorFolder)
        {
            IsBackground = true,
            Name = "Image Folder Monitor"
        };

        _backgroundThread.Start();
    }

    /// <summary>
    /// Background thread will run this functions
    /// </summary>
    private void MonitorFolder()
    {
        string[] previousFiles = GetImageFiles();

        while (!_stopEvent.Wait(2000))
        {
            try
            {
                string[] currentFiles = GetImageFiles();

                bool changed = !previousFiles.SequenceEqual( currentFiles, StringComparer.OrdinalIgnoreCase);

                if (changed)
                {
                    previousFiles = currentFiles;

                    _dispatcherQueue.TryEnqueue(() =>
                    {
                        if (!_disposed)
                            LoadImages();
                    });
                }
            }
            catch (IOException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }
    }

    /// <summary>
    /// Releases resources used by the instance, signals the stop event, and waits for the background thread to exit.
    /// Called by the viewModel when user controle in unloaded. 
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _stopEvent.Set();

        if (_backgroundThread != null &&
            _backgroundThread != Thread.CurrentThread)
        {
            _backgroundThread.Join();
        }

        _stopEvent.Dispose();
    }
}
