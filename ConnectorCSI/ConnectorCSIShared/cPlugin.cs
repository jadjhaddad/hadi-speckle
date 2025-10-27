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
      // Log diagnostics about Application.Current state
      SpeckleLog.Logger.Information("🔍 CreateOrFocusSpeckle called");
      SpeckleLog.Logger.Information("🔍 Application.Current is null? {IsNull}", Application.Current == null);
      SpeckleLog.Logger.Information("🔍 _avaloniaInitialized: {Initialized}", _avaloniaInitialized);
      SpeckleLog.Logger.Information("🔍 MainWindow is null? {IsNull}", MainWindow == null);

      if (Application.Current != null)
      {
        SpeckleLog.Logger.Information("🔍 Application.Current type: {Type}", Application.Current.GetType().FullName);
      }

      // Check if Avalonia is ALREADY initialized in this PROCESS (not just this plugin instance)
      // Application.Current persists across plugin reloads within the same ETABS process
      if (Application.Current != null)
      {
        SpeckleLog.Logger.Information("⚠️ Avalonia already initialized in this process - skipping initialization");
        _avaloniaInitialized = true;
      }
      else if (!_avaloniaInitialized)
      {
        SpeckleLog.Logger.Information("✅ Initializing Avalonia for the first time in this process");

        try
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

          SpeckleLog.Logger.Information("✅ Avalonia initialized successfully");
        }
        catch (Exception ex)
        {
          SpeckleLog.Logger.Error(ex, "❌ Failed to initialize Avalonia");
          throw;
        }
      }

      // If window exists, show it (whether visible or hidden)
      if (MainWindow != null)
      {
        if (MainWindow.IsVisible)
        {
          SpeckleLog.Logger.Information("📍 Window already visible - focusing");
          MainWindow.Activate();
        }
        else
        {
          SpeckleLog.Logger.Information("📍 Window exists but hidden - showing it");
          // Refresh the data in case streams changed
          if (MainWindow.DataContext is MainViewModel vm)
          {
            var streams = Bindings.GetStreamsInFile();
            streams = streams ?? new List<DesktopUI2.Models.StreamState>();
            Bindings.UpdateSavedStreams?.Invoke(streams);
          }
          MainWindow.Show();
          MainWindow.Activate();
          SpeckleLog.Logger.Information("✅ MainWindow shown");
        }
        return;
      }

      SpeckleLog.Logger.Information("📍 Creating new MainWindow");

      // Create the window for the first time
      var viewModel = new MainViewModel(Bindings);

      var streams = Bindings.GetStreamsInFile();
      streams = streams ?? new List<DesktopUI2.Models.StreamState>();
      Bindings.UpdateSavedStreams?.Invoke(streams);

      MainWindow = new MainWindow { DataContext = viewModel };
      MainWindow.Closed += SpeckleWindowClosed;
      MainWindow.Closing += SpeckleWindowClosed;
      MainWindow.Show();

      SpeckleLog.Logger.Information("✅ MainWindow created and shown");
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
    SpeckleLog.Logger.Information("🚪 SpeckleWindowClosed called");
    SpeckleLog.Logger.Information("🔍 Application.Current is null? {IsNull}", Application.Current == null);

    if (Application.Current != null)
    {
      SpeckleLog.Logger.Information("🔍 Application.Current type: {Type}", Application.Current.GetType().FullName);
      SpeckleLog.Logger.Information("🔍 Application.Current.Styles count: {Count}", Application.Current.Styles.Count);
    }

    isSpeckleClosed = true;

    // Clean up window reference - it can be recreated
    if (MainWindow != null)
    {
      MainWindow.Closed -= SpeckleWindowClosed;
      MainWindow.Closing -= SpeckleWindowClosed;

      // CRITICAL: Hide the window instead of setting to null
      // This keeps Avalonia's Application.Current alive
      MainWindow.Hide();

      // DON'T set MainWindow = null yet - that might trigger Avalonia shutdown
      // MainWindow = null;
    }

    SpeckleLog.Logger.Information("✅ Window hidden - ready for next open");
    SpeckleLog.Logger.Information("🔍 Application.Current after hide - is null? {IsNull}", Application.Current == null);

    // DON'T call pluginCallback.Finish(0) here!
    // That unloads the entire plugin and destroys Application.Current
    // Just hide the window and leave the plugin loaded so it can be reopened

    // Only exit if we're in the standalone driver process
    Process[] processCollection = Process.GetProcesses();
    foreach (Process p in processCollection)
    {
      if (p.ProcessName == "DriverCSharp")
      {
        Environment.Exit(0);
      }
    }

    // Removed: pluginCallback.Finish(0);
    // This was causing ETABS to unload the plugin DLL, destroying all static state
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
