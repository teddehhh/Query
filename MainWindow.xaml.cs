using Query.VMs;
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
using Query.Managers;

namespace Query
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            DataContext = new QueryVM();
            InitializeComponent();
        }
        protected override void OnClosed(EventArgs e)
        {
            DataProvider.Instance.ConnectionClose();
            base.OnClosed(e);
        }

        private void Grid_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var cols = (IEnumerable<string>)((FrameworkElement)sender).DataContext;
            var rows = (IEnumerable<string>)((FrameworkElement)sender).DataContext;
        }
    }
}
