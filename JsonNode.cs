using System.Collections.ObjectModel;
using System.Drawing;
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
}
