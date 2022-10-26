using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace DiagramMaker
{
    /// <summary>
    /// Interaction logic for Editor.xaml
    /// </summary>
    public partial class Editor : Window
    {
        public Node Node;
        public Editor(Node node)
        {
            Node = node;
            InitializeComponent();
            Header.Text = Node.Header.Text;
            MethodDataGrid.ItemsSource = Node.Methods;
            VariableDataGrid.ItemsSource = Node.Variables;
            MethodComboBoxAccess.ItemsSource = Enum.GetNames(typeof(Node.AccessEnum));
            VariableComboBoxAccess.ItemsSource = Enum.GetNames(typeof(Node.AccessEnum));
        }

        private void SaveExitButton_Click(object sender, RoutedEventArgs e)
        {
            Node.Header.Text = Header.Text;
            Close();
        }

        #region Variables

        private void VariableAddEditButton_Click(object sender, RoutedEventArgs e)
        {
            Node.Variable variable = new() { Access = (Node.AccessEnum)Enum.Parse(typeof(Node.AccessEnum), VariableComboBoxAccess.SelectedItem.ToString() ?? ""), Name = VariableTextBoxName.Text, Type = VariableTextBoxType.Text };
            if (VariableDataGrid.SelectedIndex == -1)
            {
                Node.Variables.Add(variable);
            }
            else
            {
                Node.Variables[VariableDataGrid.SelectedIndex] = variable;
            }
        }

        private void VariableRemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (VariableDataGrid.SelectedIndex == -1) { return; }
            Node.Variables.RemoveAt(VariableDataGrid.SelectedIndex);
            VariableDeselect();
        }

        private void VariableDeselectButton_Click(object sender, RoutedEventArgs e)
        {
            VariableDeselect();
        }

        private void VariableDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (VariableDataGrid.SelectedIndex == -1) { return; }
            VariableAddEditButton.Content = "Edit";
            VariableDeselectButton.IsEnabled = VariableRemoveButton.IsEnabled = true;
            Node.Variable variable = Node.Variables[VariableDataGrid.SelectedIndex];
            VariableComboBoxAccess.SelectedItem = variable.Access.ToString();
            VariableTextBoxName.Text = variable.Name;
            VariableTextBoxType.Text = variable.Type;
        }

        private void VariableDeselect()
        {
            VariableDataGrid.SelectedIndex = -1;
            VariableAddEditButton.Content = "Add";
            VariableDeselectButton.IsEnabled = VariableRemoveButton.IsEnabled = false;
        }

        #endregion

        #region Methods

        private void MethodAddButton_Click(object sender, RoutedEventArgs e)
        {
            if (MethodDataGrid.SelectedIndex == -1)
            {
                Node.Methods.Add(new Node.Method { Access = (Node.AccessEnum)Enum.Parse(typeof(Node.AccessEnum), MethodComboBoxAccess.SelectedItem.ToString() ?? ""), Name = MethodTextBoxName.Text, Type = MethodTextBoxType.Text });
            }
            else
            {
                Node.Methods[MethodDataGrid.SelectedIndex].MethodVariables.Add(new Node.Variable { Name = MethodTextBoxName.Text, Type = MethodTextBoxType.Text });
                Node.MethodsToText(null, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        private void MethodEditButton_Click(object sender, RoutedEventArgs e)
        {
            int selected = MethodDataGrid.SelectedIndex;
            if (MethodDataGrid.SelectedIndex != -1 && MethodVariablesDataGrid.SelectedIndex == -1)
            {
                Node.Method method = Node.Methods[MethodDataGrid.SelectedIndex];
                method.Access = (Node.AccessEnum)Enum.Parse(typeof(Node.AccessEnum), MethodComboBoxAccess.SelectedItem.ToString() ?? "");
                method.Name = MethodTextBoxName.Text;
                method.Type = MethodTextBoxType.Text;
                Node.Methods[MethodDataGrid.SelectedIndex] = method;
                MethodDataGrid.ItemsSource = null;
                MethodDataGrid.ItemsSource = Node.Methods;
                MethodDataGrid.SelectedIndex = selected;
                MethodVariablesDataGrid.ItemsSource = null;
                MethodVariablesDataGrid.ItemsSource = Node.Methods[MethodDataGrid.SelectedIndex].MethodVariables;
            }
            else if (MethodDataGrid.SelectedIndex != -1 && MethodVariablesDataGrid.SelectedIndex != -1)
            {
                Node.Methods[MethodDataGrid.SelectedIndex].MethodVariables[MethodVariablesDataGrid.SelectedIndex] = new Node.Variable { Name = MethodTextBoxName.Text, Type = MethodTextBoxType.Text };
                Node.MethodsToText(null, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        private void MethodRemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (MethodDataGrid.SelectedIndex != -1 && MethodVariablesDataGrid.SelectedIndex == -1)
            {
                Node.Methods.RemoveAt(MethodDataGrid.SelectedIndex);
                MethodDataGrid.SelectedIndex = -1;
            }
            else if (MethodDataGrid.SelectedIndex != -1 && MethodVariablesDataGrid.SelectedIndex != -1)
            {
                Node.Methods[MethodDataGrid.SelectedIndex].MethodVariables.RemoveAt(MethodVariablesDataGrid.SelectedIndex);
                Node.MethodsToText(null, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                MethodVariablesDataGrid.SelectedIndex = -1;
            }
        }

        private void MethodDeselectButton_Click(object sender, RoutedEventArgs e)
        {
            MethodDataGrid.SelectedIndex = -1;
            MethodVariablesDataGrid.SelectedIndex = -1;
            MethodVariablesDataGrid.ItemsSource = null;
            MethodDeselectButton.IsEnabled = MethodRemoveButton.IsEnabled = MethodEditButton.IsEnabled = false;
        }

        private void MethodDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MethodDataGrid.SelectedIndex == -1) { return; }
            Node.Method method = Node.Methods[MethodDataGrid.SelectedIndex];
            MethodComboBoxAccess.SelectedItem = method.Access.ToString();
            MethodTextBoxName.Text = method.Name;
            MethodTextBoxType.Text = method.Type;
            MethodVariablesDataGrid.ItemsSource = method.MethodVariables;
            MethodDeselectButton.IsEnabled = MethodRemoveButton.IsEnabled = MethodEditButton.IsEnabled = true;
        }

        private void MethodVariablesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MethodVariablesDataGrid.SelectedIndex == -1) { return; }
            Node.Variable variable = Node.Methods[MethodDataGrid.SelectedIndex].MethodVariables[MethodVariablesDataGrid.SelectedIndex];
            MethodTextBoxName.Text = variable.Name;
            MethodTextBoxType.Text = variable.Type;
            MethodDeselectButton.IsEnabled = MethodRemoveButton.IsEnabled = MethodEditButton.IsEnabled = true;
        }

        #endregion
    }
}
