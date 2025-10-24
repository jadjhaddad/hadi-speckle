using System.Collections.Generic;
using System.Linq;
using CSiAPIv1;
using Objects.Structural.Loading;
using Speckle.Core.Logging;

namespace Objects.Converter.CSI;

public partial class ConverterCSI
{
  public List<LoadSet> GetLoadSetsFromTable()
  {
    var loadSets = new List<LoadSet>();
    var loadSetMap = new Dictionary<string, List<string>>();

    var dbTables = Model.DatabaseTables;

    string[] tableVersion = null;
    string tableXml = ""; // Not passed as ref
    int numberFields = 0;
    string[] fieldKeysIncluded = null;
    int numberRecords = 0;
    string[] tableDataFlat = null;

    int result = dbTables.GetTableForDisplayArray(
      "Load Sets",
      ref tableVersion,
      tableXml,
      ref numberFields,
      ref fieldKeysIncluded,
      ref numberRecords,
      ref tableDataFlat
    );

    if (result != 0 || tableDataFlat == null || numberFields == 0) return loadSets;

    // Optional: log field names for confirmation
    // Console.WriteLine("Fields: " + string.Join(", ", fieldKeysIncluded));

    for (int i = 0; i < numberRecords; i++)
    {
      int rowStart = i * numberFields;
      string setName = tableDataFlat[rowStart];       // column 0
      string patternName = tableDataFlat[rowStart + 1]; // column 1

      if (!loadSetMap.ContainsKey(setName))
        loadSetMap[setName] = new List<string>();

      loadSetMap[setName].Add(patternName);
    }

    foreach (var kvp in loadSetMap)
    {
      var loadSetName = kvp.Key;
      var patterns = string.Join(", ", kvp.Value);
      SpeckleLog.Logger
        .ForContext<ConverterCSI>()
        .Information($"🧩 Load Set '{loadSetName}' includes patterns: {patterns}");
    }

    return loadSetMap.Select(kvp => new LoadSet
    {
      name = kvp.Key,
      loadCases = kvp.Value
    }).ToList();
  }

  public void AddSingleLoadPattern(string name = "LL REDUCIBLE")
  {
    var loadCase = LoadPatternToSpeckle(name);
    if (SpeckleModel.loads.Contains(loadCase))
    {
      SpeckleLog.Logger
        .ForContext<ConverterCSI>()
        .Information($"📦 LoadCase '{loadCase.name}' successfully added to SpeckleModel.loads");
    }
    else
    {
      SpeckleLog.Logger
        .ForContext<ConverterCSI>()
        .Warning($"⚠️ LoadCase '{loadCase.name}' was NOT added to SpeckleModel.loads");
    }

    SpeckleLog.Logger
      .ForContext<ConverterCSI>()
      .Information($"✅ Sent LoadPattern '{name}' as LoadCase: {loadCase.loadType}");
  }
}
