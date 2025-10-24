using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Timers;
using Avalonia;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
#if ETABS22
using ETABSv1;
#else
using CSiAPIv1;
#endif
using DesktopUI2.ViewModels;
using DesktopUI2.Views;
using Speckle.ConnectorCSI.UI;
using Speckle.Core.Logging;

namespace SpeckleConnectorCSI;

public class cPlugin
{
  public static cPluginCallback pluginCallback { get; set; }
  public static bool isSpeckleClosed { get; set; } = false;
  public Timer SelectionTimer;
  public static cSapModel model { get; set; }

  public static Window MainWindow { get; private set; }

  public static ConnectorBindingsCSI Bindings { get; set; }

  private static bool _avaloniaInitialized = false;
  private static object _initLock = new object();

  public static void CreateOrFocusSpeckle()
  {
    lock (_initLock)
    {
      // Initialize Avalonia once
      if (!_avaloniaInitialized)
      {
        // Build and initialize Avalonia on this thread
        var builder = AppBuilder
          .Configure<DesktopUI2.App>()
          .UsePlatformDetect()
          .With(new SkiaOptions { MaxGpuResourceSizeBytes = 8096000 })
          .With(new Win32PlatformOptions { AllowEglInitialization = true, EnableMultitouch = false })
          .LogToTrace()
          .UseReactiveUI();

        // Initialize but don't start the main loop yet
        builder.SetupWithoutStarting();
        _avaloniaInitialized = true;
      }

      // If window exists and is visible, just focus it
      if (MainWindow != null && MainWindow.IsVisible)
      {
        MainWindow.Activate();
        return;
      }

      // Create or recreate the window
      var viewModel = new MainViewModel(Bindings);

      var streams = Bindings.GetStreamsInFile();
      streams = streams ?? new List<DesktopUI2.Models.StreamState>();
      Bindings.UpdateSavedStreams?.Invoke(streams);

      MainWindow = new MainWindow { DataContext = viewModel };
      MainWindow.Closed += SpeckleWindowClosed;
      MainWindow.Closing += SpeckleWindowClosed;
      MainWindow.Show();
    }
  }

  public static void OpenOrFocusSpeckle(cSapModel model)
  {
    Bindings = new ConnectorBindingsCSI(model);
    Setup.Init(Bindings.GetHostAppNameVersion(), Bindings.GetHostAppName());
    CreateOrFocusSpeckle();
  }

  private static void SpeckleWindowClosed(object sender, EventArgs e)
  {
    isSpeckleClosed = true;

    // Clean up window reference - it can be recreated
    if (MainWindow != null)
    {
      MainWindow.Closed -= SpeckleWindowClosed;
      MainWindow.Closing -= SpeckleWindowClosed;
      MainWindow = null;
    }

    // Don't exit - just clean up the window
    // User can reopen the connector without restarting ETABS
    Process[] processCollection = Process.GetProcesses();
    foreach (Process p in processCollection)
    {
      if (p.ProcessName == "DriverCSharp")
      {
        Environment.Exit(0);
      }
    }

    pluginCallback.Finish(0);
  }

  public int Info(ref string Text)
  {
    Text = "This is a Speckle plugin for CSI Products";
    return 0;
  }

  public static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
  {
    Assembly a = null;
    var name = args.Name.Split(',')[0];
    string path = Path.GetDirectoryName(typeof(cPlugin).Assembly.Location);

    string assemblyFile = Path.Combine(path, name + ".dll");

    if (File.Exists(assemblyFile))
    {
      a = Assembly.LoadFrom(assemblyFile);
    }

    return a;
  }

  public void Main(ref cSapModel SapModel, ref cPluginCallback ISapPlugin)
  {
    cSapModel model;
    pluginCallback = ISapPlugin;
    AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(OnAssemblyResolve);
    model = SapModel;
    AppDomain domain = null;

    try
    {
      OpenOrFocusSpeckle(model);
    }
    catch (Exception e)
    {
      throw;
      ISapPlugin.Finish(0);
      //return;
    }

    return;
  }
}
