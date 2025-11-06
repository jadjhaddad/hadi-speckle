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

  public static AppBuilder BuildAvaloniaApp() =>
    AppBuilder
      .Configure<DesktopUI2.App>()
      .UsePlatformDetect()
      .With(new SkiaOptions { MaxGpuResourceSizeBytes = 8096000 })
#if CSIBRIDGE
      // CSIBridge: Disable GPU rendering to avoid conflicts with CSIBridge's own rendering
      .With(new Win32PlatformOptions { AllowEglInitialization = false, EnableMultitouch = false, UseWgl = false })
#else
      // ETABS and others: Use standard GPU rendering
      .With(new Win32PlatformOptions { AllowEglInitialization = true, EnableMultitouch = false })
#endif
      .LogToTrace()
      .UseReactiveUI();

  public static void CreateOrFocusSpeckle()
  {
    try
    {
      SpeckleLog.Logger.Information("🪟 CreateOrFocusSpeckle: Starting");

      if (MainWindow == null)
      {
        SpeckleLog.Logger.Information("🔧 MainWindow is null, building Avalonia app...");
        BuildAvaloniaApp().Start(AppMain, null);
        SpeckleLog.Logger.Information("✅ Avalonia app.Start(AppMain) completed");
      }
      else
      {
        SpeckleLog.Logger.Information("ℹ️ MainWindow already exists, reusing");
      }

      SpeckleLog.Logger.Information("🔧 About to call MainWindow.Show()...");
      SpeckleLog.Logger.Information("   Window properties: Width={Width}, Height={Height}, IsVisible={IsVisible}",
        MainWindow.Width, MainWindow.Height, MainWindow.IsVisible);

      try
      {
        MainWindow.Show();
        SpeckleLog.Logger.Information("✅ MainWindow.Show() completed successfully");
      }
      catch (Exception showEx)
      {
        SpeckleLog.Logger.Fatal(showEx, "❌ CRASH during MainWindow.Show()");
        SpeckleLog.Logger.Error("   Window state at crash: Width={Width}, Height={Height}",
          MainWindow.Width, MainWindow.Height);
        throw;
      }

      SpeckleLog.Logger.Information("🔧 About to call MainWindow.Activate()...");
      MainWindow.Activate();
      SpeckleLog.Logger.Information("✅ CreateOrFocusSpeckle completed successfully");
    }
    catch (Exception ex)
    {
      SpeckleLog.Logger.Fatal(ex, "❌ FATAL ERROR in CreateOrFocusSpeckle");
      throw;
    }
  }

  private static void AppMain(Application app, string[] args)
  {
    try
    {
      SpeckleLog.Logger.Information("🎯 AppMain: Starting");

      SpeckleLog.Logger.Information("🔧 Creating MainViewModel...");
      var viewModel = new MainViewModel(Bindings);
      SpeckleLog.Logger.Information("✅ MainViewModel created");

      SpeckleLog.Logger.Information("🔧 Getting streams in file...");
      var streams = Bindings.GetStreamsInFile();
      streams = streams ?? new List<DesktopUI2.Models.StreamState>();
      SpeckleLog.Logger.Information("✅ Retrieved {Count} streams", streams.Count);

      SpeckleLog.Logger.Information("🔧 Updating saved streams...");
      Bindings.UpdateSavedStreams?.Invoke(streams);
      SpeckleLog.Logger.Information("✅ Saved streams updated");

      SpeckleLog.Logger.Information("🔧 Creating MainWindow...");
      MainWindow = new MainWindow { DataContext = viewModel };
      SpeckleLog.Logger.Information("✅ MainWindow created: Type={Type}", MainWindow.GetType().FullName);

      // Add event handlers for diagnostics
      SpeckleLog.Logger.Information("🔧 Adding window event handlers for diagnostics...");
      MainWindow.Opened += (s, e) => SpeckleLog.Logger.Information("📋 Window.Opened event fired");
      MainWindow.Closed += SpeckleWindowClosed;
      MainWindow.Closing += SpeckleWindowClosed;

      // Log layout events to see what's happening
      MainWindow.LayoutUpdated += (s, e) => SpeckleLog.Logger.Information("📐 Window.LayoutUpdated event fired");
      MainWindow.PropertyChanged += (s, e) =>
      {
        var propName = e.Property?.Name;
        if (propName == "Bounds" || propName == "ClientSize")
        {
          SpeckleLog.Logger.Information("📏 Window.{PropertyName} changed", propName);
        }
      };
      SpeckleLog.Logger.Information("✅ Event handlers attached");

#if CSIBRIDGE
      SpeckleLog.Logger.Information("🔧 CSIBridge mode: Using MainWindow.Show() instead of app.Run()");
      SpeckleLog.Logger.Information("   Window initial state: Width={Width}, Height={Height}, WindowState={WindowState}",
        MainWindow.Width, MainWindow.Height, MainWindow.WindowState);

      try
      {
        SpeckleLog.Logger.Information("🔧 Calling MainWindow.Show()...");
        MainWindow.Show();
        SpeckleLog.Logger.Information("✅ MainWindow.Show() returned successfully");
      }
      catch (Exception showEx)
      {
        SpeckleLog.Logger.Fatal(showEx, "❌ CRASH in MainWindow.Show() within AppMain");
        SpeckleLog.Logger.Error("   Exception type: {Type}", showEx.GetType().FullName);
        SpeckleLog.Logger.Error("   Inner exception: {InnerException}", showEx.InnerException?.Message);
        throw;
      }
#else
      // ETABS and others: Use the standard Avalonia app.Run() pattern
      SpeckleLog.Logger.Information("🔧 ETABS mode: Using app.Run(MainWindow)");
      app.Run(MainWindow);
#endif

      SpeckleLog.Logger.Information("✅ AppMain completed");
    }
    catch (Exception ex)
    {
      SpeckleLog.Logger.Fatal(ex, "❌ FATAL ERROR in AppMain");
      throw;
    }
  }

  public static void OpenOrFocusSpeckle(cSapModel model)
  {
    try
    {
      SpeckleLog.Logger.Information("🚀 OpenOrFocusSpeckle: Starting");

      SpeckleLog.Logger.Information("🔧 Creating ConnectorBindingsCSI...");
      Bindings = new ConnectorBindingsCSI(model);
      SpeckleLog.Logger.Information("✅ ConnectorBindingsCSI created successfully");

      var appNameVer = Bindings.GetHostAppNameVersion();
      var appName = Bindings.GetHostAppName();
      SpeckleLog.Logger.Information("📋 App info: Name={AppName}, NameVersion={AppNameVersion}", appName, appNameVer);

      SpeckleLog.Logger.Information("🔧 Calling Setup.Init...");
      Setup.Init(appNameVer, appName);
      SpeckleLog.Logger.Information("✅ Setup.Init completed successfully");

      SpeckleLog.Logger.Information("🔧 Creating or focusing Speckle window...");
      CreateOrFocusSpeckle();
      SpeckleLog.Logger.Information("✅ OpenOrFocusSpeckle completed successfully");
    }
    catch (Exception ex)
    {
      SpeckleLog.Logger.Fatal(ex, "❌ FATAL ERROR in OpenOrFocusSpeckle");
      throw;
    }
  }

  private static void SpeckleWindowClosed(object sender, EventArgs e)
  {
    isSpeckleClosed = true;
    Process[] processCollection = Process.GetProcesses();
    foreach (Process p in processCollection)
    {
      if (p.ProcessName == "DriverCSharp")
      {
        Environment.Exit(0);
      }
    }
    //Environment.Exit(0);
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
    try
    {
      SpeckleLog.Logger.Information("═══════════════════════════════════════════════════════");
      SpeckleLog.Logger.Information("🚀 cPlugin.Main: STARTING PLUGIN INITIALIZATION");
      SpeckleLog.Logger.Information("═══════════════════════════════════════════════════════");

      cSapModel model;
      pluginCallback = ISapPlugin;

      SpeckleLog.Logger.Information("🔧 Setting up AssemblyResolve handler...");
      AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(OnAssemblyResolve);
      SpeckleLog.Logger.Information("✅ AssemblyResolve handler set up");

      // Preload System.Numerics.Vectors and Avalonia assemblies to ensure correct versions are loaded
      SpeckleLog.Logger.Information("🔧 Preloading critical assemblies...");
      string pluginPath = Path.GetDirectoryName(typeof(cPlugin).Assembly.Location);

      // Preload System.Numerics.Vectors
      string vectorsPath = Path.Combine(pluginPath, "System.Numerics.Vectors.dll");
      if (File.Exists(vectorsPath))
      {
        byte[] vectorsBytes = File.ReadAllBytes(vectorsPath);
        var vectorsAssembly = Assembly.Load(vectorsBytes);
        SpeckleLog.Logger.Information("✅ Preloaded System.Numerics.Vectors version {Version}", vectorsAssembly.GetName().Version);
      }

      // Preload Avalonia assemblies to prevent version conflicts
      string[] avaloniaAssemblies = { "Avalonia.Base.dll", "Avalonia.Controls.dll", "Avalonia.Visuals.dll",
                                       "Avalonia.Markup.Xaml.dll", "Avalonia.Markup.dll", "Avalonia.dll" };
      foreach (var asmName in avaloniaAssemblies)
      {
        string asmPath = Path.Combine(pluginPath, asmName);
        if (File.Exists(asmPath))
        {
          try
          {
            byte[] asmBytes = File.ReadAllBytes(asmPath);
            var asm = Assembly.Load(asmBytes);
            SpeckleLog.Logger.Information("✅ Preloaded {Assembly} version {Version}", asmName, asm.GetName().Version);
          }
          catch (Exception ex)
          {
            SpeckleLog.Logger.Warning("⚠️ Could not preload {Assembly}: {Message}", asmName, ex.Message);
          }
        }
      }

      model = SapModel;
      AppDomain domain = null;

      SpeckleLog.Logger.Information("🔧 Calling OpenOrFocusSpeckle...");
      OpenOrFocusSpeckle(model);

      SpeckleLog.Logger.Information("═══════════════════════════════════════════════════════");
      SpeckleLog.Logger.Information("✅ cPlugin.Main: PLUGIN INITIALIZATION COMPLETED");
      SpeckleLog.Logger.Information("═══════════════════════════════════════════════════════");
    }
    catch (Exception e)
    {
      SpeckleLog.Logger.Fatal(e, "❌❌❌ FATAL ERROR in cPlugin.Main ❌❌❌");
      SpeckleLog.Logger.Error("Exception Type: {ExceptionType}", e.GetType().FullName);
      SpeckleLog.Logger.Error("Exception Message: {Message}", e.Message);
      SpeckleLog.Logger.Error("Stack Trace: {StackTrace}", e.StackTrace);
      if (e.InnerException != null)
      {
        SpeckleLog.Logger.Error("Inner Exception: {InnerException}", e.InnerException);
      }

      // Call Finish to properly clean up
      ISapPlugin.Finish(1); // 1 indicates error

      // Re-throw so CSI knows the plugin failed
      throw;
    }

    return;
  }
}
