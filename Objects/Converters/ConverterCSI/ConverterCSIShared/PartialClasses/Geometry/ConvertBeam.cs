using Objects.Structural.Geometry;
using Serilog;
using Speckle.Core.Logging;

namespace Objects.Converter.CSI;

public partial class ConverterCSI
{
  public Element1D BeamToSpeckle(string name)
  {
    SpeckleLog.Logger
  .ForContext<ConverterCSI>()
  .Information("✅ FrameToSpeckle called for {name}", name);
    return FrameToSpeckle(name);
  }
}
