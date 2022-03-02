namespace Query.Helpers
{
    class Attribute
    {
        public string Name { get; set; }
        public string TableName { get; set; }
        public string Type { get; set; }
        public bool IsChecked { get; set; } = false;
        public override string ToString()
        {
            return $"{Name} <{Type}>";
        }
    }
}
