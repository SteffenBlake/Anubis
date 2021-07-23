using System.Collections.Generic;
using System.Text.Json;

namespace Anubis.Models
{
    public class DeviceInfo
    {
        public string name { get; set; }
        public string type { get; set; }
        public string assignee { get; set; }
        public List<ParentRelationInfo> parentRelations { get; set; }

        public Dictionary<string, Dictionary<string, JsonElement>> traits { get; set; }
    }

    public class ParentRelationInfo
    {
        public string parent { get; set; }

        public string displayName { get; set; }
    }
}
