namespace DiagramMaker.Infrastructure
{
    public class Variable
    {
        public AccessEnum Access { get; set; } = AccessEnum.Public;
        public string Name { get; set; } = "";
        public string Type { get; set; } = "";
    }
}