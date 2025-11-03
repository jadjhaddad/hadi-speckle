using System;
using System.Collections.Generic;
#if ETABS22
using ETABSv1;
#elif SAP26
using CSiAPIv1;
#else
using CSiAPIv1;
#endif

namespace ConnectorCSIShared.Util;

internal static class ResultUtils
{
  public static List<string> GetNamesOfAllLoadCasesAndCombos(cSapModel sapModel)
  {
    List<string> names = new();

    int numberOfLoadCombinations = 0;
    string[] loadCombinationNames = Array.Empty<string>();
    sapModel.RespCombo.GetNameList(ref numberOfLoadCombinations, ref loadCombinationNames);
    names.AddRange(loadCombinationNames);

    sapModel.LoadCases.GetNameList(ref numberOfLoadCombinations, ref loadCombinationNames);
    names.AddRange(loadCombinationNames);

    return names;
  }
}
