using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RevitTestApp.SelectParameter
{
    /// <summary>
    /// Interaction logic for SelectParameterView.xaml
    /// </summary>
    public partial class SelectParameterView : Window
    {
        public SelectParameterView(SelectParameterVM viewModel)
        {
            DataContext = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            InitializeComponent();
        }
    }

    public class SelectParameterVM
    {
        private RelayCommand okCommand;

        public SelectParameterVM(string parameterName)
        {
            ParameterName = parameterName ?? throw new ArgumentNullException(nameof(parameterName));
        }

        public string ParameterName { get; set; }

        public RelayCommand OkCommand => okCommand ?? (okCommand = new RelayCommand(
            execute: obj => {
                if (obj is Window dialogView)
                    dialogView.DialogResult = true;
            },
            canExecute: obj => !string.IsNullOrWhiteSpace(ParameterName))
        );
    }
}
