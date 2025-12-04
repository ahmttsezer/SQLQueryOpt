using System.Collections.Generic;

namespace QueryDebugger.Core.Models
{
    public class ParsedQuery
    {
        public TableNode MainTable { get; set; }
        public List<JoinNode> Joins { get; set; } = new List<JoinNode>();
        public List<FilterNode> Filters { get; set; } = new List<FilterNode>();
    }

    public class TableNode
    {
        public string Name { get; set; }
        public string Alias { get; set; }
        public string Schema { get; set; } = "dbo";
    }

    public class JoinNode
    {
        public TableNode Table { get; set; }
        public string JoinType { get; set; } // Inner, Left, etc.
        public string Condition { get; set; } // The raw SQL condition for now
    }

    public class FilterNode
    {
        public string Condition { get; set; }
        public string TableAlias { get; set; } // Try to attribute to a specific table if possible
    }
}

