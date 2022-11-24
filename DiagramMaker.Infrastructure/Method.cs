using System.Collections.ObjectModel;

namespace DiagramMaker.Infrastructure
{
    public class Method
    {
        public AccessEnum Access { get; set; } = AccessEnum.Public;
        public string Name { get; set; } = "";
        public string Type { get; set; } = "";
        public ObservableCollection<Variable> MethodVariables = new();
    }
}