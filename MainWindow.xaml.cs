using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DiagramMaker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Point pointNode = new() { X = 0, Y = 0 };
        private Node selectedNode = new();
        private readonly ObservableCollection<Connection> connections = new();
        private Node? SourceNode;
        private Node? DestNode;
        private bool connectionDashed = false;
        private bool connectionFill = false;
        private Connection.ShapeEnum connectionShape = Connection.ShapeEnum.Arrow;
        private bool connect = false;
        public MainWindow()
        {
            InitializeComponent();
        }
        private void NodeButton_Click(object sender, RoutedEventArgs e)
        {
            Node node = new();
            Canvas.SetZIndex(node, 1);
            Canvas.SetLeft(node, (Window.Width / 2) - (node.Width / 2));
            Canvas.SetTop(node, (Window.Height / 2) - (node.Height / 2));
            node.PreviewMouseMove += Node_PreviewMouseMove;
            node.PreviewMouseDoubleClick += Node_PreviewMouseDoubleClick;
            node.PreviewMouseLeftButtonDown += Node_PreviewMouseLeftButtonDown;
            node.PreviewMouseRightButtonDown += Node_PreviewMouseRightButtonDown;
            _ = Canvas.Children.Add(node);
        }

        private void Node_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) { return; }
            Node node = (Node)sender;
            Point pointCanvas = e.GetPosition(Canvas);
            Canvas.SetZIndex(node, 2);
            Canvas.SetLeft(node, pointCanvas.X - pointNode.X);
            Canvas.SetTop(node, pointCanvas.Y - pointNode.Y);
            DrawConnections();
        }

        private void Node_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) { return; }
            Node_PreviewMouseLeftButtonDown(sender, e);
            Editor editor = new(selectedNode);
            _ = editor.ShowDialog();
        }

        private void Node_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            selectedNode = (Node)sender;
            pointNode = Mouse.GetPosition(selectedNode);
            foreach (Node n in Canvas.Children.OfType<Node>())
            {
                n.Background = Brushes.Beige;
                Canvas.SetZIndex(n, 1);
            }
            selectedNode.Background = Brushes.Bisque;
            if (connect)
            {
                if (SourceNode is null)
                {
                    SourceNode = selectedNode;
                }
                else
                {
                    if (SourceNode == selectedNode) { return; }
                    DestNode = selectedNode;
                    connections.Add(new Connection() { Source = SourceNode, Destination = DestNode, Dashed = connectionDashed, Fill = connectionFill, Shape = connectionShape });
                    ResetConnect();
                    DrawConnections();
                }
            }
        }

        private void Node_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            ContextMenu contextMenu = new()
            {
                PlacementTarget = (Node)sender,
                Placement = PlacementMode.Mouse,
                IsOpen = true
            };
            MenuItem removeNode = new() { Header = "Remove" };
            removeNode.Click += RemoveNode_Click;
            _ = contextMenu.Items.Add(removeNode);
        }

        private void RemoveNode_Click(object sender, RoutedEventArgs e)
        {
            Node node = (Node)((ContextMenu)((MenuItem)sender).Parent).PlacementTarget;
            Canvas.Children.Remove(node);
            if (connections.Any(c => node == c.Source || node == c.Destination))
            {
                connections.Where(c => node == c.Source || node == c.Destination).ToList().ForEach(con =>
                {
                    Canvas.Children.Remove(con.Path);
                    Canvas.Children.Remove(con.ShapePath);
                    _ = connections.Remove(con);
                });
            }
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            if (!connect)
            {
                connect = true;
                ConnectButton.Background = Brushes.LightBlue;
                ContextMenu contextMenu = new()
                {
                    PlacementTarget = (Button)sender,
                    Placement = PlacementMode.Mouse,
                    IsOpen = true
                };
                foreach (string connectType in new List<string>() { "Association", "Inheritance", "Realization", "Dependency", "Aggregation", "Composition" })
                {
                    MenuItem menuItem = new() { Header = connectType };
                    menuItem.Click += ConnectType_Click;
                    _ = contextMenu.Items.Add(menuItem);
                }
            }
            else
            {
                ResetConnect();
            }
        }

        private void ConnectType_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = (MenuItem)sender;
            switch (menuItem.Header)
            {
                case "Association":
                    connectionDashed = false;
                    connectionFill = false;
                    connectionShape = Connection.ShapeEnum.Arrow;
                    break;
                case "Inheritance":
                    connectionDashed = false;
                    connectionFill = false;
                    connectionShape = Connection.ShapeEnum.Triangle;
                    break;
                case "Realization":
                    connectionDashed = true;
                    connectionFill = false;
                    connectionShape = Connection.ShapeEnum.Triangle;
                    break;
                case "Dependency":
                    connectionDashed = true;
                    connectionFill = false;
                    connectionShape = Connection.ShapeEnum.Arrow;
                    break;
                case "Aggregation":
                    connectionDashed = false;
                    connectionFill = false;
                    connectionShape = Connection.ShapeEnum.Diamond;
                    break;
                case "Composition":
                    connectionDashed = false;
                    connectionFill = true;
                    connectionShape = Connection.ShapeEnum.Diamond;
                    break;
            }
        }

        private void ResetConnect()
        {
            connect = false;
            ConnectButton.Background = Brushes.LightGray;
            SourceNode = null;
            DestNode = null;
        }

        private void DrawConnections()
        {
            foreach (Connection c in connections)
            {
                if (c.Path is null)
                {
                    c.Path = new Path
                    {
                        Stroke = Brushes.Black,
                        StrokeThickness = 2
                    };
                    c.Path.PreviewMouseRightButtonDown += Path_PreviewMouseRightButtonDown;
                    _ = Canvas.Children.Add(c.Path);
                }
                if (c.ShapePath is null)
                {
                    c.ShapePath = new Path
                    {
                        Stroke = Brushes.Black,
                        StrokeThickness = 2
                    };
                    _ = Canvas.Children.Add(c.ShapePath);
                }
                c.ShapePath.Fill = c.Fill ? Brushes.Black : Brushes.White;
                c.Path.StrokeDashArray = c.Dashed ? new DoubleCollection() { 3, 3 } : null;
                Point SrcP = new() { X = Canvas.GetLeft(c.Source) + (c.Source.Width / 2), Y = Canvas.GetTop(c.Source) + (c.Source.Height / 2) };
                Point DstP = new() { X = Canvas.GetLeft(c.Destination) + (c.Destination.Width / 2), Y = Canvas.GetTop(c.Destination) + (c.Destination.Height / 2) };

                double distanceX = SrcP.X - DstP.X;
                double distanceY = SrcP.Y - DstP.Y;
                bool sideConnections = Math.Abs(distanceY) <= Math.Max(c.Source.Height, c.Destination.Height);
                bool notArrow = c.Shape is Connection.ShapeEnum.Diamond or Connection.ShapeEnum.Triangle;

                PathFigureCollection pathFigures = new() {
                    new PathFigure() {
                        IsClosed = false,
                        IsFilled = false,
                        StartPoint = SrcP,
                        Segments = new() {
                            new LineSegment { Point = new() { X = SrcP.X - (sideConnections ? distanceX / 2 : 0), Y = SrcP.Y - (sideConnections ? 0 : distanceY / 2) } },
                            new LineSegment { Point = new() { X = DstP.X + (sideConnections ? distanceX / 2 : 0), Y = DstP.Y + (sideConnections ? 0 : distanceY / 2) } },
                            new LineSegment { Point = DstP }
                        }
                    }
                };
                c.Path.Data = new PathGeometry() { Figures = pathFigures };
                PathFigureCollection shapePathFigures = new()
                {
                    new PathFigure()
                    {
                        IsClosed = notArrow,
                        IsFilled = true,
                        StartPoint = new() {
                            X = DstP.X + (sideConnections ? distanceX > 0 ? (c.Destination.Width / 2) + 5 : (-c.Destination.Width / 2) - 5 : 5),
                            Y = DstP.Y + (sideConnections ? 5 : distanceY > 0 ? (c.Destination.Height / 2) + 5 : (-c.Destination.Height / 2) - 5)
                        },
                        Segments = new()
                        {
                            new LineSegment { Point = new() {
                                X = DstP.X + (sideConnections ? distanceX > 0 ? c.Destination.Width / 2 : -c.Destination.Width / 2 : 0),
                                Y = DstP.Y + (sideConnections ? 0 : distanceY > 0 ? c.Destination.Height / 2 : -c.Destination.Height / 2) }
                            },
                            new LineSegment { Point = new() {
                                X = DstP.X + (sideConnections ? distanceX > 0 ? (c.Destination.Width / 2) + 5 : (-c.Destination.Width / 2) - 5 : -5),
                                Y = DstP.Y + (sideConnections ? -5 : distanceY > 0 ? (c.Destination.Height / 2) + 5 : (-c.Destination.Height / 2) - 5) }
                            },
                            c.Shape == Connection.ShapeEnum.Diamond ? new LineSegment
                            {
                                Point = new() {
                                X = DstP.X + (sideConnections ? distanceX > 0 ? (c.Destination.Width / 2) + 10 : (-c.Destination.Width / 2) - 10 : 0),
                                Y = DstP.Y + (sideConnections ? 0 : distanceY > 0 ? (c.Destination.Height / 2) + 10 : (-c.Destination.Height / 2) - 10) }
                            } : new LineSegment() { Point = new() {
                                X = DstP.X + (sideConnections ? distanceX > 0 ? (c.Destination.Width / 2) + 5 : (-c.Destination.Width / 2) - 5 : -5),
                                Y = DstP.Y + (sideConnections ? -5 : distanceY > 0 ? (c.Destination.Height / 2) + 5 : (-c.Destination.Height / 2) - 5) }
                            }
                        }
                    }
                };
                c.ShapePath.Data = new PathGeometry() { Figures = shapePathFigures };
            }
        }

        private void Path_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            ContextMenu contextMenu = new()
            {
                PlacementTarget = (Path)sender,
                Placement = PlacementMode.Mouse,
                IsOpen = true
            };
            MenuItem removePath = new() { Header = "Remove" };
            removePath.Click += RemovePath_Click;
            _ = contextMenu.Items.Add(removePath);
        }

        private void RemovePath_Click(object sender, RoutedEventArgs e)
        {
            Path path = (Path)((ContextMenu)((MenuItem)sender).Parent).PlacementTarget;
            Connection connection = connections.Single(c => c.Path == path);
            Canvas.Children.Remove(connection.Path);
            Canvas.Children.Remove(connection.ShapePath);
            _ = connections.Remove(connection);
        }

        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            PrintDialog printDialog = new();
            if (printDialog.ShowDialog() == true)
            {
                printDialog.PrintVisual(Canvas, "Diagram");
            }
        }
    }
    public class Connection
    {
        public Node Source { get; set; } = new();
        public Node Destination { get; set; } = new();
        public Path? Path { get; set; }
        public Path? ShapePath { get; set; }
        public ShapeEnum Shape { get; set; }
        public enum ShapeEnum { Arrow, Triangle, Diamond }
        public bool Dashed { get; set; } = false;
        public bool Fill { get; set; } = false;
    }
    public class JsonConnection
    {
        public Node Source { get; set; } = new();
        public Node Destination { get; set; } = new();
        public string Shape { get; set; } = "";
        public bool Dashed { get; set; } = false;
        public bool Fill { get; set; } = false;
    }
}
