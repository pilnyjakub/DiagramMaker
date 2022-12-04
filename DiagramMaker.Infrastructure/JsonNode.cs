﻿using System.Collections.ObjectModel;
using System.Drawing;

namespace DiagramMaker.Infrastructure
{
    public class JsonNode
    {
        public int Uid { get; set; }
        public string Header { get; set; } = "ClassName";
        public Point Position { get; set; }
        public ObservableCollection<Variable>? Variables { get; set; }
        public ObservableCollection<Method>? Methods { get; set; }
    }
}