using System.Collections.ObjectModel;
using System.Drawing;
using static DiagramMaker.Connection;
using static DiagramMaker.Node;

namespace DiagramMaker
{
    public class JsonNode
    {
        public int Uid { get; set; }
        public Point Position { get; set; }
        public ObservableCollection<Variable>? Variables { get; set; }
        public ObservableCollection<Method>? Methods { get; set; }
    }

    public class JsonConnection
    {
        public JsonNode? Source { get; set; }
        public JsonNode? Destination { get; set; }
        public ShapeEnum Shape { get; set; }
        public bool Dashed { get; set; }
        public bool Fill { get; set; }
    }
}
