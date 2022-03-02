using System.Collections.Generic;

namespace Query.Helpers
{
    internal class Table
    {
        public string Name { get; set; }
        public List<Helpers.Attribute> Columns { get; set; }
        public Table(string name)
        {
            Name = name;
        }
    }
}
