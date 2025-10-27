using Objects.Geometry;
using Speckle.Core.Kits;
using Speckle.Core.Logging;

namespace Objects.Converter.CSI;

public partial class ConverterCSI
{
  public string LineToNative(Line line)
  {
    string newFrame = "";
    Point lineStart = line.start;
    Point end2Node = line.end;

    var modelUnits = ModelUnits();
    var end1NodeSf = Units.GetConversionFactor(lineStart.units, modelUnits);
    var end2NodeSf = Units.GetConversionFactor(end2Node.units, modelUnits);

    // Diagnostic logging to identify API failure cause
    SpeckleLog.Logger.Information("🔍 LineToNative called:");
    SpeckleLog.Logger.Information("   Start: ({X}, {Y}, {Z}) {Units}",
      lineStart.x, lineStart.y, lineStart.z, lineStart.units);
    SpeckleLog.Logger.Information("   End: ({X}, {Y}, {Z}) {Units}",
      end2Node.x, end2Node.y, end2Node.z, end2Node.units);
    SpeckleLog.Logger.Information("   Model units: {Units}", modelUnits);
    SpeckleLog.Logger.Information("   Conversion factors: start={Start}, end={End}",
      end1NodeSf, end2NodeSf);
    SpeckleLog.Logger.Information("   Converted start: ({X}, {Y}, {Z})",
      lineStart.x * end1NodeSf, lineStart.y * end1NodeSf, lineStart.z * end1NodeSf);
    SpeckleLog.Logger.Information("   Converted end: ({X}, {Y}, {Z})",
      end2Node.x * end2NodeSf, end2Node.y * end2NodeSf, end2Node.z * end2NodeSf);

    var success = Model.FrameObj.AddByCoord(
      lineStart.x * end1NodeSf,
      lineStart.y * end1NodeSf,
      lineStart.z * end1NodeSf,
      end2Node.x * end2NodeSf,
      end2Node.y * end2NodeSf,
      end2Node.z * end2NodeSf,
      ref newFrame
    );

    SpeckleLog.Logger.Information("   API result: success={Code}, frame={Name}", success, newFrame);

    if (success != 0)
    {
      throw new ConversionException($"Failed to add new frame (error code: {success})");
    }

    return newFrame;
  }
}
