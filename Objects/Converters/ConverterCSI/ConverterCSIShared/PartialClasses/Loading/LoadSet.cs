using Speckle.Core.Models;
using System.Collections.Generic;

namespace Objects.Structural.Loading
{
  public class LoadSet : Base
  {
    public string name { get; set; }
    public List<string> loadCases { get; set; } = new();
    public string type { get; set; } = "Linear"; // Optional, placeholder

    public LoadSet() { }

    public string speckle_type => "Objects.Structural.Loading.LoadSet";

    public LoadSet(string name, List<string> loadCases, string type = "Linear")
    {
      this.name = name;
      this.loadCases = loadCases;
      this.type = type;
    }
  }
}
