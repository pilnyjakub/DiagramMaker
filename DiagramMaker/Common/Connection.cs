using DiagramMaker.Infrastructure;
using System.Windows.Shapes;

namespace DiagramMaker
{
    public class Connection
    {
        public Node Source { get; set; } = new();
        public Node Destination { get; set; } = new();
        public Path? Path { get; set; }
        public Path? ShapePath { get; set; }
        public ShapeEnum Shape { get; set; }
        public bool Dashed { get; set; }
        public bool Fill { get; set; }
    }
}