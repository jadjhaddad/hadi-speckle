using Objects.Converter.CSI; // or whatever your base namespace is
using Speckle.Core.Kits;

namespace Objects.Converter.SAP2000;

public class ConverterSAP2000 : ConverterCSI, ISpeckleConverter
{
  // Empty class: this is just to expose the shared converter to the loader.
}
