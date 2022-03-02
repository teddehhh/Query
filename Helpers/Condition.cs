namespace Query.Helpers
{
    internal class Condition
    {
        public string Table { get; set; }
        public string Attribute { get; set; }
        public string Operator { get; set; }
        public string Operation { get; set; }
        public string Value { get; set; }
    }
}
