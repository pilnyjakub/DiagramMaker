using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
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
        private bool connectionDashed;
        private bool connectionFill;
        private Connection.ShapeEnum connectionShape = Connection.ShapeEnum.Arrow;
        private bool connect;
        private readonly Brush brushButton = new SolidColorBrush(Color.FromRgb(22, 27, 34));
        private readonly Brush brushDefault = new SolidColorBrush(Color.FromRgb(48, 54, 61));
        private readonly Brush brushSelected = new SolidColorBrush(Color.FromRgb(139, 148, 158));
        private readonly Brush brushBackground = new SolidColorBrush(Color.FromRgb(33, 38, 45));
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
                n.Border.BorderBrush = brushDefault;
                Canvas.SetZIndex(n, 1);
            }
            selectedNode.Border.BorderBrush = brushSelected;
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
                ConnectButton.Background = brushDefault;
                ConnectButton.BorderBrush = brushSelected;
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
            ConnectButton.Background = brushButton;
            ConnectButton.BorderBrush = brushDefault;
            SourceNode = null;
            DestNode = null;
        }

        private void DrawConnections()
        {
            for (int i = 0; i < connections.Count; i++)
            {
                Connection c = connections[i];
                double halfDestinationWidth = c.Destination.Width / 2;
                double halfDestinationHeight = c.Destination.Height / 2;
                if (c.Path is null)
                {
                    c.Path = SetPath();
                    c.Path.PreviewMouseRightButtonDown += Path_PreviewMouseRightButtonDown;
                    _ = Canvas.Children.Add(c.Path);
                }
                if (c.ShapePath is null)
                {
                    c.ShapePath = SetPath();
                    _ = Canvas.Children.Add(c.ShapePath);
                }
                c.ShapePath.Fill = c.Fill ? brushSelected : brushBackground;
                c.Path.StrokeDashArray = c.Dashed ? new DoubleCollection() { 3, 3 } : null;
                Point SrcP = new() { X = Canvas.GetLeft(c.Source) + (c.Source.Width / 2), Y = Canvas.GetTop(c.Source) + (c.Source.Height / 2) };
                Point DstP = new() { X = Canvas.GetLeft(c.Destination) + halfDestinationWidth, Y = Canvas.GetTop(c.Destination) + halfDestinationHeight };
                double distanceX = SrcP.X - DstP.X;
                double distanceY = SrcP.Y - DstP.Y;
                bool sideConnections = Math.Abs(distanceY) <= Math.Max(c.Source.Height, c.Destination.Height);
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
                PathFigureCollection shapePathFigures = ShapePath(c.Shape, sideConnections, DstP, distanceX, distanceY, halfDestinationWidth, halfDestinationHeight);
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

        private Path SetPath()
        {
            return new Path
            {
                Stroke = brushSelected,
                StrokeThickness = 2
            };
        }

        private static PathFigureCollection ShapePath(Connection.ShapeEnum shape, bool sideConnections, Point DstP, double distanceX, double distanceY, double halfDestinationWidth, double halfDestinationHeight)
        {
            return new()
                {
                    new PathFigure()
                    {
                        IsClosed = shape is Connection.ShapeEnum.Diamond or Connection.ShapeEnum.Triangle,
                        IsFilled = true,
                        StartPoint = new() {
                            X = DstP.X + (sideConnections ? distanceX > 0 ? halfDestinationWidth + 5 : (-halfDestinationWidth) - 5 : 5),
                            Y = DstP.Y + (sideConnections ? 5 : distanceY > 0 ? halfDestinationHeight + 5 : (-halfDestinationHeight) - 5)
                        },
                        Segments = new()
                        {
                            new LineSegment { Point = new() {
                                X = DstP.X + (sideConnections ? distanceX > 0 ? halfDestinationWidth : -halfDestinationWidth : 0),
                                Y = DstP.Y + (sideConnections ? 0 : distanceY > 0 ? halfDestinationHeight : -halfDestinationHeight) }
                            },
                            new LineSegment { Point = new() {
                                X = DstP.X + (sideConnections ? distanceX > 0 ? halfDestinationWidth + 5 : (-halfDestinationWidth) - 5 : -5),
                                Y = DstP.Y + (sideConnections ? -5 : distanceY > 0 ? halfDestinationHeight + 5 : (-halfDestinationHeight) - 5) }
                            },
                            shape == Connection.ShapeEnum.Diamond ? new LineSegment
                            {
                                Point = new() {
                                X = DstP.X + (sideConnections ? distanceX > 0 ? halfDestinationWidth + 10 : (-halfDestinationWidth) - 10 : 0),
                                Y = DstP.Y + (sideConnections ? 0 : distanceY > 0 ? halfDestinationHeight + 10 : (-halfDestinationHeight) - 10) }
                            } : new LineSegment() { Point = new() {
                                X = DstP.X + (sideConnections ? distanceX > 0 ? halfDestinationWidth + 5 : (-halfDestinationWidth) - 5 : -5),
                                Y = DstP.Y + (sideConnections ? -5 : distanceY > 0 ? halfDestinationHeight + 5 : (-halfDestinationHeight) - 5) }
                            }
                        }
                    }
                };
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            List<JsonConnection> jsonConnections = new();
            foreach (Connection c in connections)
            {
                JsonNode jsonNodeSource = NodeToJson(c.Source);
                JsonNode jsonNodeDest = NodeToJson(c.Destination);
                JsonConnection jsonConnection = new()
                {
                    Source = jsonNodeSource,
                    Destination = jsonNodeDest,
                    Shape = c.Shape,
                    Dashed = c.Dashed,
                    Fill = c.Fill
                };
                jsonConnections.Add(jsonConnection);
            }
            string serializedConnections = JsonSerializer.Serialize(jsonConnections, new JsonSerializerOptions() { IncludeFields = true });
            List<JsonNode> singleNodes = new();
            foreach (Node n in Canvas.Children.OfType<Node>().Where(n => !connections.Any(c => c.Source == n || c.Destination == n)))
            {
                singleNodes.Add(NodeToJson(n));
            }
            string serializedSingleNodes = JsonSerializer.Serialize(singleNodes, new JsonSerializerOptions() { IncludeFields = true });
            SaveFileDialog saveFileDialog = new() { FileName = "save", DefaultExt = ".json" };
            if (saveFileDialog.ShowDialog() == true)
            {
                string path = saveFileDialog.FileName;
                System.IO.File.WriteAllText(path, serializedConnections + '\n' + serializedSingleNodes);
                _ = MessageBox.Show("Saved.");
            }
        }

        private static JsonNode NodeToJson(Node node)
        {
            return new()
            {
                Uid = node.GetHashCode(),
                Header = node.Header.Text,
                Position = new() { X = (int)Canvas.GetLeft(node), Y = (int)Canvas.GetTop(node) },
                Methods = node.Methods,
                Variables = node.Variables
            };
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new() { FileName = "save", DefaultExt = ".json" };
            if (openFileDialog.ShowDialog() != true)
            {
                return;
            }
            Canvas.Children.Clear();
            connections.Clear();
            string fileContent = System.IO.File.ReadAllText(openFileDialog.FileName);
            string[] split = fileContent.Split('\n');
            Dictionary<int, Node> hashNodes = new();
            List<JsonConnection> jsonConnections = JsonSerializer.Deserialize<List<JsonConnection>>(split[0], new JsonSerializerOptions() { IncludeFields = true }) ?? new();
            foreach (JsonConnection jsonConnection in jsonConnections)
            {
                if (jsonConnection.Source is null || jsonConnection.Destination is null) return;
                Node sourceNode;
                Node destNode;
                if (!hashNodes.ContainsKey(jsonConnection.Source.Uid))
                {
                    sourceNode = AddJsonNode(jsonConnection.Source);
                    hashNodes.Add(jsonConnection.Source.Uid, sourceNode);
                }
                else
                {
                    sourceNode = hashNodes[jsonConnection.Source.Uid];
                }
                if (!hashNodes.ContainsKey(jsonConnection.Destination.Uid))
                {
                    destNode = AddJsonNode(jsonConnection.Destination);
                    hashNodes.Add(jsonConnection.Destination.Uid, destNode);
                }
                else
                {
                    destNode = hashNodes[jsonConnection.Destination.Uid];
                }
                connections.Add(new Connection()
                {
                    Source = sourceNode,
                    Destination = destNode,
                    Shape = jsonConnection.Shape,
                    Dashed = jsonConnection.Dashed,
                    Fill = jsonConnection.Fill
                });
            }
            List<JsonNode> jsonNodes = JsonSerializer.Deserialize<List<JsonNode>>(split[1], new JsonSerializerOptions() { IncludeFields = true }) ?? new();
            foreach (JsonNode jsonNode in jsonNodes)
            {
                _ = AddJsonNode(jsonNode);
            }
            DrawConnections();
            _ = MessageBox.Show("Loaded.");
        }

        private Node AddJsonNode(JsonNode jsonNode)
        {
            Node node = JsonToNode(jsonNode);
            _ = Canvas.Children.Add(node);
            Canvas.SetLeft(node, jsonNode.Position.X);
            Canvas.SetTop(node, jsonNode.Position.Y);
            Canvas.SetZIndex(node, 2);
            return node;
        }

        private Node JsonToNode(JsonNode jsonNode)
        {
            Node node = new()
            {
                Variables = jsonNode.Variables ?? new(),
                Methods = jsonNode.Methods ?? new()
            };
            node.MethodsToText(null, null!);
            node.VariablesToText(null, null!);
            node.Header.Text = jsonNode.Header;
            node.PreviewMouseMove += Node_PreviewMouseMove;
            node.PreviewMouseDoubleClick += Node_PreviewMouseDoubleClick;
            node.PreviewMouseLeftButtonDown += Node_PreviewMouseLeftButtonDown;
            node.PreviewMouseRightButtonDown += Node_PreviewMouseRightButtonDown;
            return node;
        }
    }
}
