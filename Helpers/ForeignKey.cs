namespace Query.Helpers
{
    internal class ForeignKey
    {
        public string TableFrom { get; set; }
        public string TableTo { get; set; }
        public string AttributeFrom { get; set; }
        public string AttributeTo { get; set; }
    }
}
