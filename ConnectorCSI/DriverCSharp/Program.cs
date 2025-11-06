using System;
using System.Windows.Forms;
#if ETABS22
using ETABSv1;
#else
using CSiAPIv1;
#endif
using Speckle.Core.Logging;
using SpeckleConnectorCSI;
#if DEBUG
using System.Diagnostics;
#endif

namespace DriverCSharp;

class Program
{
  private const string ProgID_SAP2000 = "CSI.SAP2000.API.SapObject";
  private const string ProgID_ETABS = "CSI.ETABS.API.ETABSObject";
  private const string ProgID_CSiBridge = "CSI.CSiBridge.API.SapObject";
  private const string ProgID_SAFE = "CSI.SAFE.API.SAFEObject";

  static int Main(string[] args)
  {
    try
    {
      SpeckleLog.Logger.Information("═══════════════════════════════════════════════════════");
      SpeckleLog.Logger.Information("🚀 DriverCSharp.Main: STARTING");
      SpeckleLog.Logger.Information("═══════════════════════════════════════════════════════");

#if DEBUG
      Debugger.Launch();
#endif
      //MessageBox.Show("Starting DriverCSharp");

      SpeckleLog.Logger.Information("📋 Command line args: {Args}", string.Join(", ", args));

      // dimension the SapObject as cOAPI type
      cOAPI mySapObject = null;

      // Use ret to check if functions return successfully (ret = 0) or fail (ret = nonzero)
      int ret = -1;

      // create API helper object
      cHelper myHelper = null;

      SpeckleLog.Logger.Information("🔧 Creating Helper object...");
      myHelper = new Helper();
      SpeckleLog.Logger.Information("✅ Helper object created");

      // attach to a running program instance

      // get the active SapObject
      // determine program type
      string progID = null;
      string[] arguments = Environment.GetCommandLineArgs();

      SpeckleLog.Logger.Information("🔍 Determining program type from arguments...");
      if (arguments.Length > 1)
      {
        string arg = arguments[1];
        SpeckleLog.Logger.Information("📋 Argument provided: {Arg}", arg);

        if (string.Equals(arg, "SAP2000", StringComparison.CurrentCultureIgnoreCase))
        {
          progID = ProgID_SAP2000;
          SpeckleLog.Logger.Information("✅ Program type: SAP2000");
        }
        else if (string.Equals(arg, "ETABS", StringComparison.CurrentCultureIgnoreCase))
        {
          progID = ProgID_ETABS;
          SpeckleLog.Logger.Information("✅ Program type: ETABS");
        }
        else if (string.Equals(arg, "SAFE", StringComparison.CurrentCultureIgnoreCase))
        {
          progID = ProgID_SAFE;
          SpeckleLog.Logger.Information("✅ Program type: SAFE");
        }
        else if (string.Equals(arg, "CSiBridge", StringComparison.CurrentCultureIgnoreCase))
        {
          progID = ProgID_CSiBridge;
          SpeckleLog.Logger.Information("✅ Program type: CSiBridge");
        }
        else
        {
          SpeckleLog.Logger.Warning("⚠️ Unknown program type argument: {Arg}", arg);
        }
      }
      else
      {
        SpeckleLog.Logger.Information("ℹ️ No program type argument provided, will try all");
      }

      if (progID != null)
      {
        SpeckleLog.Logger.Information("🔧 Getting {ProgID} object...", progID);
        mySapObject = myHelper.GetObject(progID);
        if (mySapObject != null)
        {
          SpeckleLog.Logger.Information("✅ Successfully got {ProgID} object", progID);
        }
        else
        {
          SpeckleLog.Logger.Warning("❌ Failed to get {ProgID} object", progID);
        }
      }
      else
      {
        // missing/unknown program type, try one by one
        SpeckleLog.Logger.Information("🔍 Trying SAP2000...");
        progID = ProgID_SAP2000;
        mySapObject = myHelper.GetObject(progID);

        if (mySapObject == null)
        {
          SpeckleLog.Logger.Information("❌ SAP2000 not found, trying ETABS...");
          progID = ProgID_ETABS;
          mySapObject = myHelper.GetObject(progID);
        }
        if (mySapObject == null)
        {
          SpeckleLog.Logger.Information("❌ ETABS not found, trying CSiBridge...");
          progID = ProgID_CSiBridge;
          mySapObject = myHelper.GetObject(progID);
        }

        if (mySapObject != null)
        {
          SpeckleLog.Logger.Information("✅ Successfully connected to {ProgID}", progID);
        }
      }

      if (mySapObject is null)
      {
        SpeckleLog.Logger.Error("❌ No running instance of the program found");
        MessageBox.Show("No running instance of the program found");

        ret = -2;
        return ret;
      }

      // Get a reference to cSapModel to access all API classes and functions
      SpeckleLog.Logger.Information("🔧 Getting SapModel reference...");
      cSapModel mySapModel = mySapObject.SapModel;
      SpeckleLog.Logger.Information("✅ SapModel reference obtained");

      // call Speckle plugin
      SpeckleLog.Logger.Information("🔧 Creating cPlugin and cPluginCallback...");
      cPlugin p = new();
      cPluginCallback cb = new PluginCallback();
      SpeckleLog.Logger.Information("✅ Plugin objects created");

      // DO NOT return from SpeckleConnectorETABS.cPlugin.Main() until all work is done.
      SpeckleLog.Logger.Information("🔧 Calling cPlugin.Main...");
      p.Main(ref mySapModel, ref cb);
      SpeckleLog.Logger.Information("✅ cPlugin.Main returned");

      if (cb.Finished == true)
      {
        SpeckleLog.Logger.Information("✅ Plugin callback finished successfully");
        Environment.Exit(0);
      }

      SpeckleLog.Logger.Information("🔚 Returning with error flag: {ErrorFlag}", cb.ErrorFlag);
      return cb.ErrorFlag;
    }
    catch (Exception ex) when (!ex.IsFatal())
    {
      SpeckleLog.Logger.Fatal(ex, "❌❌❌ FATAL ERROR in DriverCSharp.Main ❌❌❌");
      SpeckleLog.Logger.Error("Exception Type: {ExceptionType}", ex.GetType().FullName);
      SpeckleLog.Logger.Error("Exception Message: {Message}", ex.Message);
      SpeckleLog.Logger.Error("Stack Trace: {StackTrace}", ex.StackTrace);
      if (ex.InnerException != null)
      {
        SpeckleLog.Logger.Error("Inner Exception: {InnerException}", ex.InnerException);
      }
      MessageBox.Show("Failed to initialize plugin: " + ex.Message + "\n\nSee logs for details.");
      return -3;
    }
  }
}
