namespace DiagramMaker.Infrastructure
{
    public class JsonConnection
    {
        public JsonNode? Source { get; set; }
        public JsonNode? Destination { get; set; }
        public ShapeEnum Shape { get; set; }
        public bool Dashed { get; set; }
        public bool Fill { get; set; }
    }
}