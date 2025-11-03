using System.Collections.Generic;
using System.Linq;
using DesktopUI2;
using DesktopUI2.Models.Filters;
using Speckle.ConnectorCSI.Util;
using Speckle.Core.Logging;

namespace Speckle.ConnectorCSI.UI;

public partial class ConnectorBindingsCSI : ConnectorBindings
{
  public override List<string> GetSelectedObjects()
  {
    var names = new List<string>();
    var typeNameTupleList = ConnectorCSIUtils.SelectedObjects(Model);
    if (typeNameTupleList == null)
    {
      return new List<string>() { };
    }

    foreach (var item in typeNameTupleList)
    {
      (string typeName, string name) = item;
      if (ConnectorCSIUtils.IsTypeCSIAPIUsable(typeName))
      {
        names.Add(string.Concat(typeName, ": ", name));
      }
    }
    if (names.Count == 0)
    {
      return new List<string>() { };
    }

    return names;
  }

  public override List<ISelectionFilter> GetSelectionFilters()
  {
    var filters = new List<ISelectionFilter>();
    filters.Add(
      new AllSelectionFilter
      {
        Slug = "all",
        Name = "Everything",
        Icon = "CubeScan",
        Description = "Selects all document objects."
      }
    );
    filters.Add(new ManualSelectionFilter());

    if (Model != null)
    {
      ConnectorCSIUtils.GetObjectIDsTypesAndNames(Model);
      var objectTypes = ConnectorCSIUtils.ObjectIDsTypesAndNames.Select(pair => pair.Value.Item1).Distinct().ToList();

      if (objectTypes.Any())
      {
        filters.Add(
          new ListSelectionFilter
          {
            Slug = "type",
            Name = "Categories",
            Icon = "Category",
            Values = objectTypes,
            Description = "Adds all objects belonging to the selected types."
          }
        );
      }

      string[] groupNames = System.Array.Empty<string>();
      int numNames = 0;
      Model.GroupDef.GetNameList(ref numNames, ref groupNames);
      if (groupNames.Any())
      {
        filters.Add(
          new ListSelectionFilter
          {
            Slug = "group",
            Name = "Group",
            Icon = "SelectGroup",
            Values = groupNames.ToList(),
            Description = "Add all objects belonging to CSI Group."
          }
        );
      }
    }

    return filters;
  }

  public override void SelectClientObjects(List<string> args, bool deselect = false)
  {
    // TODO!
  }

  private List<string> GetSelectionFilterObjects(ISelectionFilter filter)
  {
    var doc = Model;

    var selection = new List<string>();
    ConnectorCSIUtils.GetObjectIDsTypesAndNames(Model);

    switch (filter.Slug)
    {
      case "manual":
        return GetSelectedObjects();
      case "all":

        selection.AddRange(ConnectorCSIUtils.ObjectIDsTypesAndNames.Select(pair => pair.Key).ToList());
        return selection;

      case "type":
        var typeFilter = filter as ListSelectionFilter;

        foreach (var type in typeFilter.Selection)
        {
          selection.AddRange(
            ConnectorCSIUtils
              .ObjectIDsTypesAndNames.Where(pair => pair.Value.Item1 == type)
              .Select(pair => pair.Key)
              .ToList()
          );
        }
        return selection;

      case "group":
        //Clear objects first
        Model.SelectObj.ClearSelection();
        var groupFilter = filter as ListSelectionFilter;
        foreach (var group in groupFilter.Selection)
        {
          Model.SelectObj.Group(group);
        }

        return GetSelectedObjects();
    }

    return selection;
  }

  private List<(string name, string type)> savedSelection = new();

  private List<(string name, string type)> SaveCurrentSelection()
  {
    List<(string name, string type)> saved = new();

    // This call should give (typeName, name) tuple per selected object
    var selection = ConnectorCSIUtils.SelectedObjects(Model);
    if (selection == null)
      return saved;

    foreach (var (type, name) in selection)
    {
      if (!string.IsNullOrWhiteSpace(type) && !string.IsNullOrWhiteSpace(name))
      {
        saved.Add((name, type));
        SpeckleLog.Logger.Information($"[Connector] Saved: {name} (raw type → '{type}')");
      }
    }

    SpeckleLog.Logger.Information($"[Connector] ✅ Saved {saved.Count} selected objects.");
    return saved;
  }


  private void RestorePreviousSelection(List<(string name, string type)> saved)
  {
    Model.SelectObj.ClearSelection();

    int restoredCount = 0;

    foreach (var (name, type) in saved)
    {
      int res = 1;
      switch (type.ToLower())
      {
        case "point":
        case "joint":
          res = Model.PointObj.SetSelected(name, true);
          break;
        case "frame":
          res = Model.FrameObj.SetSelected(name, true);
          break;
        case "area":
          res = Model.AreaObj.SetSelected(name, true);
          break;
        case "solid":
        #if ETABS22 || SAP26
          SpeckleLog.Logger.Warning($"[Connector] 🚫 Skipping solid '{name}' — not supported in ETABS 22/SAP2000 26");
          continue;
        #else
          res = Model.SolidObj.SetSelected(name, true);
          break;
        #endif
        case "link":
          res = Model.LinkObj.SetSelected(name, true);
          break;
        case "tendon":
          res = Model.TendonObj.SetSelected(name, true);
          break;
        case "cable":
          SpeckleLog.Logger.Warning($"[Connector] 🚫 Skipping cable '{name}' — selection not supported");
          continue;
        default:
          SpeckleLog.Logger.Warning($"[Connector] ⚠️ Unknown type '{type}' — cannot reselect '{name}'");
          continue;
      }

      if (res == 0)
        restoredCount++;
      else
        SpeckleLog.Logger.Warning($"[Connector] ⚠️ Failed to reselect {type} '{name}'");
    }

    SpeckleLog.Logger.Information($"[Connector] ✅ Restored {restoredCount} objects to selection.");
  }

}
