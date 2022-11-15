using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DiagramMaker
{
    /// <summary>
    /// Interaction logic for Node.xaml
    /// </summary>
    public partial class Node : UserControl
    {
        public Node()
        {
            InitializeComponent();
            Methods.CollectionChanged += MethodsToText;
            Variables.CollectionChanged += VariablesToText;
        }

        public enum AccessEnum
        {
            Protected = '#',
            Public = '+',
            Private = '-',
            Package = '~'
        }

        public class Variable
        {
            public AccessEnum Access { get; set; } = AccessEnum.Public;
            public string Name { get; set; } = "";
            public string Type { get; set; } = "";
        }

        public ObservableCollection<Variable> Variables = new();

        public class Method
        {
            public AccessEnum Access { get; set; } = AccessEnum.Public;
            public string Name { get; set; } = "";
            public string Type { get; set; } = "";
            public ObservableCollection<Variable> MethodVariables = new();
        }

        public ObservableCollection<Method> Methods = new();

        public void MethodsToText(object? sender, NotifyCollectionChangedEventArgs e)
        {
            StringBuilder methodsText = new();
            foreach (Method method in Methods)
            {
                string variables = "";
                foreach (Variable variable in method.MethodVariables)
                {
                    variables += $"{variable.Name} : {variable.Type}, ";
                }
                variables = variables.Length > 2 ? variables[..^2] : variables;
                _ = methodsText.Append((char)method.Access).Append(' ').Append(method.Name).Append('(').Append(variables).Append(") : ").AppendLine(method.Type);
            }
            MethodsTextBlock.Text = methodsText.ToString();
            SizeChange();
        }

        public void VariablesToText(object? sender, NotifyCollectionChangedEventArgs e)
        {
            StringBuilder variablesText = new();
            foreach (Variable variable in Variables)
            {
                _ = variablesText.Append((char)variable.Access).Append(' ').Append(variable.Name).Append(" : ").AppendLine(variable.Type);
            }
            VariablesTextBlock.Text = variablesText.ToString();
            SizeChange();
        }

        private void SizeChange()
        {
            CultureInfo culture = CultureInfo.CurrentCulture;
            const FlowDirection flowdirection = FlowDirection.LeftToRight;
            Typeface typeface = new("Segoe UI");
            FormattedText ftHeader = new(Header.Text, culture, flowdirection, typeface, Header.FontSize, Brushes.Black, 1);
            FormattedText ftVariables = new(VariablesTextBlock.Text, culture, flowdirection, typeface, VariablesTextBlock.FontSize, Brushes.Black, 1);
            FormattedText ftMethods = new(MethodsTextBlock.Text, culture, flowdirection, typeface, MethodsTextBlock.FontSize, Brushes.Black, 1);
            Width = Math.Max(ftHeader.Width, Math.Max(ftVariables.Width, ftMethods.Width)) + 25 > MinWidth ?
                Math.Max(ftHeader.Width, Math.Max(ftVariables.Width, ftMethods.Width)) + 25 : MinWidth;
            Height = ftHeader.Height + ftVariables.Height + ftMethods.Height + 40 > MinHeight
                ? ftHeader.Height + ftVariables.Height + ftMethods.Height + 40
                : MinHeight;
        }
    }
}
