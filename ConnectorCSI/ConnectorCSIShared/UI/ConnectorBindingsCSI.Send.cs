using System;
using System.IO;
using System.Reflection;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DesktopUI2;
using DesktopUI2.Models;
using DesktopUI2.Models.Settings;
using DesktopUI2.ViewModels;
using Serilog;
using Serilog.Context;
using Speckle.ConnectorCSI.Util;
using Speckle.Core.Api;
using Speckle.Core.Kits;
using Speckle.Core.Logging;
using Speckle.Core.Models;
using SCT = Speckle.Core.Transports;

namespace Speckle.ConnectorCSI.UI;

public partial class ConnectorBindingsCSI : ConnectorBindings
{
  public override bool CanPreviewSend => false;

  public override void PreviewSend(StreamState state, ProgressViewModel progress)
  {
    // TODO!
  }

  public override async Task<string> SendStream(StreamState state, ProgressViewModel progress)
  {
    SpeckleLog.Logger.Information("🧪 Available kits before GetDefaultKit():");

    foreach (var loadedKit in KitManager.Kits)
    {
      SpeckleLog.Logger.Information($"🔍 Kit registered: {loadedKit.GetType().FullName}, Name: {loadedKit.Name}");
    }

    foreach (var loadedKit in KitManager.Kits)
    {
      SpeckleLog.Logger.Information($"🔍 Kit Name: {loadedKit.Name}, Converters: {string.Join(", ", loadedKit.Converters)}");
    }
    foreach (var loadedKit in KitManager.Kits)
    {
      SpeckleLog.Logger.Information("🔍 Kit found: {kitName} with Converters: {converters}", loadedKit.Name, string.Join(", ", loadedKit.Converters));
    }

    var kitsForEtabs = KitManager.GetKitsWithConvertersForApp("ETABS22").ToList();
    SpeckleLog.Logger.Information("✅ Kits with ETABS22: {count}", kitsForEtabs.Count);

    foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
    {
      if (asm.FullName.Contains("Objects"))
        SpeckleLog.Logger.Information("🔧 Loaded: {name} from {path}", asm.FullName, asm.Location);
    }

#if ETABS22
    SpeckleLog.Logger.Information("🚧 Manually loading ObjectsKit and ETABS22 converter.");

    // Log connector assembly location and its dependencies
    var connectorAssembly = Assembly.GetExecutingAssembly();
    SpeckleLog.Logger.Information("📍 Connector assembly location: {Location}", connectorAssembly.Location);

    // Check which Objects assembly is loaded
    var objectsAssembly = typeof(Objects.ObjectsKit).Assembly;
    SpeckleLog.Logger.Information("📍 Objects assembly location: {Location}", objectsAssembly.Location);
    SpeckleLog.Logger.Information("📍 Objects assembly version: {Version}", objectsAssembly.GetName().Version);

    // Check which Core assembly is loaded
    var coreAssembly = typeof(Speckle.Core.Models.Base).Assembly;
    SpeckleLog.Logger.Information("📍 Core assembly location: {Location}", coreAssembly.Location);
    SpeckleLog.Logger.Information("📍 Core assembly version: {Version}", coreAssembly.GetName().Version);

    var kit = new Objects.ObjectsKit();
    SpeckleLog.Logger.Information("✅ Loaded Objects Kit manually.");

    var basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    var converterPath = Path.Combine(basePath!, "Objects.Converter.ETABS22.dll");

    // Fallback to shared objects folder if not found
    if (!File.Exists(converterPath))
    {
      converterPath = Path.Combine(Objects.ObjectsKit.ObjectsFolder, "Objects.Converter.ETABS22.dll");
    }

    if (!File.Exists(converterPath))
    {
      throw new FileNotFoundException("❌ Could not find ETABS22 converter DLL", converterPath);
    }

    SpeckleLog.Logger.Information("📍 Loading converter from: {Path}", converterPath);
    var assembly = Assembly.LoadFrom(converterPath);
    SpeckleLog.Logger.Information("📍 Converter assembly version: {Version}", assembly.GetName().Version);

    // Log converter's dependencies
    foreach (var refAssembly in assembly.GetReferencedAssemblies())
    {
      if (refAssembly.Name.Contains("Objects") || refAssembly.Name.Contains("Core"))
      {
        SpeckleLog.Logger.Information("📍 Converter references: {Name} v{Version}", refAssembly.Name, refAssembly.Version);
      }
    }

    SpeckleLog.Logger.Information("🔍 Scanning loaded assembly for ISpeckleConverter implementations...");

    Type[] types;
    try
    {
      types = assembly.GetTypes();
    }
    catch (ReflectionTypeLoadException ex)
    {
      SpeckleLog.Logger.Fatal("❌ Could not load types from ETABS22 DLL: {Message}", ex.Message);
      foreach (var loaderEx in ex.LoaderExceptions)
        SpeckleLog.Logger.Fatal("⛔ Loader exception: {Error}", loaderEx?.Message ?? "Unknown error");
      throw;
    }

    SpeckleLog.Logger.Information("📦 Found {Count} types in ETABS22 assembly.", types.Length);

    Type converterType = null;
    foreach (var type in types)
    {
      SpeckleLog.Logger.Information("🔍 Type: {Type}", type.FullName);

      // Match by interface name to avoid type identity issues
      bool implementsConverter = type.GetInterfaces()
        .Any(i => i.FullName == "Speckle.Core.Kits.ISpeckleConverter");

      if (implementsConverter && !type.IsAbstract)
      {
        SpeckleLog.Logger.Information("✅ ISpeckleConverter candidate found: {Type}", type.FullName);
        converterType = type;
        break;
      }
    }

    if (converterType == null)
    {
      throw new Exception("❌ Could not find a suitable ISpeckleConverter in ETABS22 DLL");
    }

    SpeckleLog.Logger.Information("🔬 ISpeckleConverter interface is from: {Assembly}", typeof(ISpeckleConverter).Assembly.Location);
    SpeckleLog.Logger.Information("🔬 Converter type is from: {Assembly}", converterType.Assembly.Location);

    // ✅ Use dynamic to bypass type identity mismatch
    dynamic converter = Activator.CreateInstance(converterType)!;

    SpeckleLog.Logger.Information("✅ Created instance of {Type}", converterType.FullName);
    SpeckleLog.Logger.Information("🔬 Created object type: {Type}", converter.GetType().FullName);
    SpeckleLog.Logger.Information("🔬 ISpeckleConverter loaded from: {Path}", typeof(ISpeckleConverter).Assembly.Location);
    SpeckleLog.Logger.Information("🔬 rawInstance loaded from: {Path}", converter.GetType().Assembly.Location);

    // Defensive runtime check
    Type actualType = ((object)converter).GetType();
    bool runtimeCheck = actualType.GetInterfaces()
      .Any(i => i.FullName == "Speckle.Core.Kits.ISpeckleConverter");


    if (!runtimeCheck)
    {
      SpeckleLog.Logger.Warning("⚠️ Dynamic converter does not appear to implement ISpeckleConverter at runtime.");
    }

    var supportedApps = converter.GetServicedApplications();
    SpeckleLog.Logger.Information("🔗 {Type} supports apps: {Apps}", converterType.FullName, string.Join(", ", supportedApps));
#else
    SpeckleLog.Logger.Information("✅ Using default kit manager.");
    var kit = KitManager.GetDefaultKit();
    SpeckleLog.Logger.Information("✅ Loaded Objects Kit");

    var appName = GetHostAppVersion(Model);
    SpeckleLog.Logger.Warning("🔍 Requested converter for app: {App}", appName);

    var converter = kit.LoadConverter(appName);
    SpeckleLog.Logger.Information("✅ Loaded converter: {Type}", converter.GetType().FullName);
    SpeckleLog.Logger.Information("🔬 ISpeckleConverter interface is from: {Path}", typeof(ISpeckleConverter).Assembly.Location);
    SpeckleLog.Logger.Information("🔬 Loaded converter type is from: {Path}", converter.GetType().Assembly.Location);

#endif
    var savedSelection = SaveCurrentSelection();
    // Reflectively call SetSendAsExtruded if available
    var extrudeSetting = state.Settings.FirstOrDefault(s => s.Slug == "send-extruded-view") as CheckBoxSetting;
    bool sendExtruded = extrudeSetting?.IsChecked == true;

    var method = converter.GetType().GetMethod("SetSendAsExtruded");
    if (method != null)
    {
      method.Invoke(converter, new object[] { sendExtruded });
    }
    else
    {
      SpeckleLog.Logger.Warning("Converter does not implement SetSendAsExtruded(bool).");
    }

    // set converter settings as tuples (setting slug, setting selection)
    // for csi, these must go before the SetContextDocument method.
    var settings = new Dictionary<string, string>();
    foreach (var setting in state.Settings)
    {
      settings.Add(setting.Slug, setting.Selection);
    }

    settings.Add("operation", "send");
    converter.SetConverterSettings(settings);

    bool sendExtrudedView = settings.TryGetValue("send-extruded-view", out string value) && value == "true";

    converter.SetContextDocument(Model);
    RestorePreviousSelection(savedSelection);


    try
    {
      var json = Newtonsoft.Json.JsonConvert.SerializeObject(state.ReceivedObjects);
      var setPreviousMethod = converter.GetType().GetMethod("SetPreviousContextObjects");

      if (setPreviousMethod != null)
      {
        var paramType = setPreviousMethod.GetParameters()[0].ParameterType;
        var deserializedObjects = Newtonsoft.Json.JsonConvert.DeserializeObject(json, paramType);
        setPreviousMethod.Invoke(converter, new object[] { deserializedObjects });
      }
    }
    catch (Exception ex)
    {
      SpeckleLog.Logger.Warning(ex, "Could not set previous context objects");
    }

    Exceptions.Clear();

    int objCount = 0;

    if (state.Filter != null)
    {
      state.SelectedObjectIds = GetSelectionFilterObjects(state.Filter);
    }

    var totalObjectCount = state.SelectedObjectIds.Count;

    if (totalObjectCount == 0)
    {
      throw new InvalidOperationException(
        "Zero objects selected; send stopped. Please select some objects, or check that your filter can actually select something."
      );
    }

    var conversionProgressDict = new ConcurrentDictionary<string, int>();
    progress.Max = totalObjectCount;
    conversionProgressDict["Conversion"] = 0;
    progress.Update(conversionProgressDict);

    using var d0 = LogContext.PushProperty("converterName", converter.Name);
    using var d1 = LogContext.PushProperty("converterAuthor", converter.Author);
    using var d2 = LogContext.PushProperty("conversionDirection", nameof(ISpeckleConverter.ConvertToSpeckle));
    using var d3 = LogContext.PushProperty("converterSettings", settings);

    var commitObjects = new List<object>();
    BuildSendCommitObj(converter, state.SelectedObjectIds, ref progress, ref conversionProgressDict, commitObjects);

    var commitObj = GetCommitObj(converter, commitObjects, progress, conversionProgressDict);

    return await SendCommitObj(state, progress, commitObj, conversionProgressDict);
  }

  public void BuildSendCommitObj(
    dynamic converter,
    List<string> selectedObjIds,
    ref ProgressViewModel progress,
    ref ConcurrentDictionary<string, int> conversionProgressDict,
    List<object> commitObjects // 👈 NEW
  )
  {
    foreach (var applicationId in selectedObjIds)
    {
      progress.CancellationToken.ThrowIfCancellationRequested();

      dynamic converted = null;
      string containerName = string.Empty;

      var selectedObjectType = ConnectorCSIUtils
        .ObjectIDsTypesAndNames.Where(pair => pair.Key == applicationId)
        .Select(pair => pair.Value.Item1)
        .FirstOrDefault();

      var reportObj = new ApplicationObject(applicationId, selectedObjectType) { applicationId = applicationId };

      if (!converter.CanConvertToSpeckle(selectedObjectType))
      {
        progress.Report.Log($"Skipped not supported type:  ${selectedObjectType} are not supported");
        continue;
      }

      var typeAndName = ConnectorCSIUtils
        .ObjectIDsTypesAndNames.Where(pair => pair.Key == applicationId)
        .Select(pair => pair.Value)
        .FirstOrDefault();

      using var _0 = LogContext.PushProperty("fromType", typeAndName.typeName);

      SpeckleLog.Logger.ForContext<ConnectorBindingsCSI>().Information($"[Send] Converting: {typeAndName.typeName}:{typeAndName.name}");

      try
      {
        converted = converter.ConvertToSpeckle(typeAndName);
        if (converted == null)
        {
          throw new ConversionException("Conversion Returned Null");
        }

        if (converted != null)
        {
          if (!commitObjects.Contains(converted))
          {
            commitObjects.Add(converted);
          }
          SpeckleLog.Logger.ForContext<ConnectorBindingsCSI>().Information($"[Send] Added {converted.GetType().Name} to commit: {typeAndName.name}");
          SpeckleLog.Logger.ForContext<ConnectorBindingsCSI>().Information($"📦 Added {converted.GetType().Name} with displayValue? {(converted["displayValue"] != null)}");
        }

        reportObj.Update(
          status: ApplicationObject.State.Created,
          logItem: $"Sent as {ConnectorCSIUtils.SimplifySpeckleType(converted.speckle_type)}"
        );
      }
      catch (Exception ex) when (!ex.IsFatal())
      {
        ConnectorHelpers.LogConversionException(ex);

        var failureStatus = ConnectorHelpers.GetAppObjectFailureState(ex);
        reportObj.Update(status: failureStatus, logItem: ex.Message);
      }

      progress.Report.Log(reportObj);

      if (converted == null)
      {
        Log.Warning($"[Send] Conversion returned null for: {typeAndName.name}");
      }
      else
      {
        SpeckleLog.Logger.ForContext<ConnectorBindingsCSI>().Information($"[Send] Successfully converted: {converted.GetType().Name} for {typeAndName.name}");
      }

      conversionProgressDict["Conversion"]++;
      progress.Update(conversionProgressDict);
    }
  }

  public Base GetCommitObj(
    dynamic converter,
    List<object> objects,
    ProgressViewModel progress,
    ConcurrentDictionary<string, int> conversionProgressDict
  )
  {
    var commitObj = new Base();
    // Add model-level info if needed
    var modelObj = converter.ConvertToSpeckle(("Model", "CSI"));

    // Diagnostic logging for assembly mismatch debugging
    if (modelObj != null)
    {
      Type modelType = ((object)modelObj).GetType();
      SpeckleLog.Logger.Information("🔍 Model object type: {Type}", modelType.FullName);
      SpeckleLog.Logger.Information("🔍 Model assembly: {Assembly}", modelType.Assembly.Location);
      SpeckleLog.Logger.Information("🔍 Model assembly version: {Version}", modelType.Assembly.GetName().Version);
      SpeckleLog.Logger.Information("🔍 Model is Base? {IsBase}", modelObj is Speckle.Core.Models.Base);

      // Check the Base type from the serializer's perspective
      var baseType = typeof(Speckle.Core.Models.Base);
      SpeckleLog.Logger.Information("🔍 Expected Base type assembly: {Assembly}", baseType.Assembly.Location);
      SpeckleLog.Logger.Information("🔍 Expected Base type version: {Version}", baseType.Assembly.GetName().Version);

      // Check if types are assignable
      bool isAssignable = baseType.IsAssignableFrom(modelType);
      SpeckleLog.Logger.Information("🔍 Is Model assignable to Base? {IsAssignable}", isAssignable);
    }

    commitObj["@Model"] = modelObj;

    // Add converted objects to the commit
    commitObj["elements"] = objects;

    // Optionally include analysis results
    commitObj["AnalysisResults"] = converter.ConvertToSpeckle(("AnalysisResults", "CSI"));
    var reportObj = new ApplicationObject("model", "ModelInfo");
    if (commitObj["@Model"] == null)
    {
      try
      {
        commitObj["@Model"] = converter.ConvertToSpeckle(("Model", "CSI"));
        reportObj.Update(status: ApplicationObject.State.Created);
      }
      catch (Exception ex) when (!ex.IsFatal())
      {
        SpeckleLog.Logger.Error(ex, "Error when attempting to retreive commit object");
        reportObj.Update(status: ApplicationObject.State.Failed, logItem: ex.Message);
      }
      progress.Report.Log(reportObj);
    }

    reportObj = new ApplicationObject("results", "AnalysisResults");
    if (commitObj["AnalysisResults"] == null)
    {
      try
      {
        commitObj["AnalysisResults"] = converter.ConvertToSpeckle(("AnalysisResults", "CSI"));
        reportObj.Update(status: ApplicationObject.State.Created);
      }
      catch (Exception ex) when (!ex.IsFatal())
      {
        SpeckleLog.Logger.Error(ex, "Error when attempting to retreive analysis results");
        reportObj.Update(status: ApplicationObject.State.Failed, logItem: ex.Message);
      }
      progress.Report.Log(reportObj);
    }

    try
    {
      progress.Report.Merge(converter.Report);
    }
    catch (Exception ex)
    {
      SpeckleLog.Logger.Warning(ex, "Could not merge converter report - assembly version mismatch");
    }

    //if (conversionProgressDict["Conversion"] == 0)
    //{
    //  throw new SpeckleException("Zero objects converted successfully. Send stopped.");
    //}

    progress.CancellationToken.ThrowIfCancellationRequested();

    return commitObj;
  }

  public async Task<string> SendCommitObj(
    StreamState state,
    ProgressViewModel progress,
    Base commitObj,
    ConcurrentDictionary<string, int> conversionProgressDict,
    string branchName = null
  )
  {
    var streamId = state.StreamId;
    var client = state.Client;

    var transports = new List<SCT.ITransport>() { new SCT.ServerTransport(client.Account, streamId) };
    progress.Max = conversionProgressDict["Conversion"];

    var objectId = await Operations.Send(
      @object: commitObj,
      cancellationToken: progress.CancellationToken,
      transports: transports,
      onProgressAction: dict =>
      {
        progress.Update(dict);
      },
      onErrorAction: ConnectorHelpers.DefaultSendErrorHandler,
      disposeTransports: true
    );

    if (branchName != null)
    {
      var branchesSplit = state.BranchName.Split('/');
      branchesSplit[branchesSplit.Count() - 1] = branchName;
      branchName = string.Join("", branchesSplit);

      var branchInput = new BranchCreateInput()
      {
        streamId = streamId,
        name = branchName,
        description = "This branch holds the comprehensive reports generated by Speckle"
      };
      var branch = await client.BranchGet(streamId, branchName, 0);
      if (branch == null)
      {
        await client.BranchCreate(branchInput);
      }
    }
    else
    {
      branchName = state.BranchName;
    }

    var actualCommit = new CommitCreateInput
    {
      streamId = streamId,
      objectId = objectId,
      branchName = branchName,
      message =
        state.CommitMessage != null
          ? state.CommitMessage
          : $"Pushed {conversionProgressDict["Conversion"]} elements from CSI.",
      sourceApplication = GetHostAppVersion(Model)
    };

    if (state.PreviousCommitId != null)
    {
      actualCommit.parents = new List<string>() { state.PreviousCommitId };
    }

    return await ConnectorHelpers.CreateCommit(client, actualCommit, progress.CancellationToken);
  }
}
