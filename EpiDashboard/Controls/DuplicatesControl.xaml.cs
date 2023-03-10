using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EpiDashboard.Controls
{
    /// <summary>
    /// Interaction logic for DuplicatesControl.xaml
    /// </summary>
    public partial class DuplicatesControl : UserControl
    {
        private DataView dv;

        public DuplicatesControl()
        {
            InitializeComponent();
        }

        public DuplicatesControl(DataView dv)
        {
            InitializeComponent();
            this.dv = dv;
        }

        public void SetDataView(DataView dv)
        {
            this.dv = dv;
        }

        public void Refresh()
        {
            dataGridMain.DataContext = dv;

            List<DataGridColumn> columnsToRemove = new List<DataGridColumn>();

            foreach (DataGridColumn dgc in dataGridMain.Columns)
            {
                if (dgc.Header.ToString().Equals("SYSTEMDATE"))
                {
                    columnsToRemove.Add(dgc);
                }
            }

            foreach (DataGridColumn dgc in columnsToRemove)
            {
                dataGridMain.Columns.Remove(dgc);
            }
        }

        private void dataGridMain_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
        }

        private void dataGridMain_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (e.Column.Header.ToString().Equals("GlobalRecordId") ||
                e.Column.Header.ToString().Equals("RECSTATUS") ||
                e.Column.Header.ToString().Equals("RecStatus") ||
                e.Column.Header.ToString().Equals("UniqueKey") ||
                e.Column.Header.ToString().Equals("FKEY"))
            {
                e.Cancel = true;
            }
        }

        private void dataGridMain_AutoGeneratedColumns(object sender, EventArgs e)
        {
            List<DataGridColumn> columnsToRemove = new List<DataGridColumn>();

            foreach (DataGridColumn dgc in dataGridMain.Columns)
            {
                if (dgc.Header.ToString().Equals("SYSTEMDATE"))
                {
                    columnsToRemove.Add(dgc);
                }
            }

            foreach (DataGridColumn dgc in columnsToRemove)
            {
                dataGridMain.Columns.Remove(dgc);
            }
        }
    }
}
