using Query.Commands;
using Query.Helpers;
using Query.Managers;
using Query.VMs.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Query.VMs
{
    internal class QueryVM : BaseViewModel
    {
        private DatabaseModel _databaseModel;
        private QueryBuilder _QueryBuilder;
        private ObservableCollection<Helpers.Condition> Conditions;
        public List<Helpers.Attribute> Attributes { get; }
        public List<ForeignKey> ForeignKeys { get; }
        public QueryVM()
        {
            _databaseModel = new DatabaseModel();
            Attributes = _databaseModel.Attributes;
            ForeignKeys = _databaseModel.ForeignKeys;
            _QueryBuilder = new QueryBuilder(_databaseModel);

            Conditions = new ObservableCollection<Helpers.Condition>();
            Operators = new List<string>();
            Operations = new List<string>();
            Operations.Add("AND");
            Operations.Add("OR");
            Operators.Add("<");
            Operators.Add(">");
            Operators.Add("<>");
            Operators.Add("=");

            treeCollection = new ObservableCollection<Table>();
            FillTreeCollection();
        }

        private void FillTreeCollection()
        {
            foreach (var item in Attributes)
            {
                var table = treeCollection.Where(t => t.Name == item.TableName);
                if (table.Count() == 0)
                {
                    treeCollection.Add(new Helpers.Table(item.TableName) { Columns = new List<Helpers.Attribute>() });
                    treeCollection.Last().Columns.Add(item);
                    continue;
                }
                table.FirstOrDefault().Columns.Add(item);
            }
        }

        private string selectedTable;
        private string selectedOperator;
        private string selectedOperation;
        private string valueCond;
        private Helpers.Attribute selectedAttribute;
        private Helpers.Condition selectedCondition;
        public string SelectedTable
        {
            get
            {
                OnPropertyChanged(nameof(AttributesIsEnabled));
                OnPropertyChanged(nameof(CondAttributes));
                return selectedTable;
            }
            set
            {
                selectedTable = value;
            }
        }
        public Helpers.Attribute SelectedAttribute
        {
            get
            {
                OnPropertyChanged(nameof(AddIsEnabled));
                OnPropertyChanged(nameof(DateIsEnabled));
                OnPropertyChanged(nameof(ValueIsEnabled));
                return selectedAttribute;
            }
            set
            {
                selectedAttribute = value;
            }
        }
        public string SelectedOperator
        {
            get
            {
                OnPropertyChanged(nameof(AddIsEnabled));
                return selectedOperator;
            }
            set
            {
                selectedOperator = value;
            }
        }
        public string SelectedOperation
        {
            get
            {
                OnPropertyChanged(nameof(AddIsEnabled));
                return selectedOperation;
            }
            set
            {
                selectedOperation = value;
            }
        }
        public Helpers.Condition SelectedCondition
        {
            get
            {
                OnPropertyChanged(nameof(DeleteIsEnabled));
                return selectedCondition;
            }
            set => selectedCondition = value;
        }
        public string ValueCond
        {
            get
            {
                OnPropertyChanged(nameof(AddIsEnabled));
                return valueCond;
            }
            set { valueCond = value; }
        }
        private DateTime? selectedDate;
        public DateTime? SelectedDate
        {
            get
            {
                OnPropertyChanged(nameof(AddIsEnabled));
                return selectedDate;
            }
            set
            {
                selectedDate = value;
            }
        }
        public bool AttributesIsEnabled => selectedTable != null;
        public bool AddIsEnabled => selectedAttribute != null && selectedOperator != null && (selectedOperation != null || Conditions.Count < 1) && (!string.IsNullOrEmpty(valueCond) && !string.IsNullOrEmpty(valueCond.Trim(' ')) || selectedDate != null);
        public bool DeleteIsEnabled => selectedCondition != null;
        public bool DateIsEnabled => selectedAttribute != null && selectedAttribute.Type.Contains("date");
        public bool ValueIsEnabled => selectedAttribute != null && (selectedAttribute.Type.Contains("int") || selectedAttribute.Type.Contains("char"));
        public bool OperationIsEnabled => Conditions.Count > 0;
        private TabItem tabItem;
        public TabItem SelectedTab
        {
            get
            {
                OnPropertyChanged(nameof(CondTables));
                OnPropertyChanged(nameof(CondAttributes));
                return tabItem;
            }
            set => tabItem = value;
        }
        public ICollectionView CollectionView
        {
            get
            {
                var source = CollectionViewSource.GetDefaultView(Conditions);
                return source;
            }
        }
        public List<string> CondTables
        {
            get
            {
                var attr = treeCollection.Select(x => (x.Columns).Where(y => y.IsChecked).Select(y => y.TableName).Distinct());
                List<string> result = new List<string>();
                foreach (var item in attr)
                {
                    foreach (var atr in item)
                    {
                        result.Add(atr);
                    }
                }
                return result;
            }
        }
        public List<Helpers.Attribute> CondAttributes
        {
            get
            {
                if (selectedTable != null)
                {
                    return treeCollection.Where(x => x.Name == selectedTable).FirstOrDefault().Columns.ToList();
                }
                return null;
            }
        }
        public List<string> Operators { get; }
        public List<string> Operations { get; }

        private RelayCommand addCondCmd;
        private RelayCommand deleteCondCmd;
        private RelayCommand showQueryCmd;
        private RelayCommand runQueryCmd;
        private RelayCommand checkAllCmd;
        private RelayCommand clearCheckCmd;
        public RelayCommand AddCondCmd => addCondCmd ?? new RelayCommand(obj =>
        {
            object value;
            if (selectedAttribute.Type.Contains("char"))
            {
                value = valueCond;
            }
            else if (selectedAttribute.Type.Contains("int"))
            {
                if (!int.TryParse(valueCond, out int num))
                {
                    MessageBox.Show("Введите число!");
                    return;
                }
                value = num;
            }
            else
            {
                value = selectedDate;
            }

            Conditions.Add(new Helpers.Condition() { Operation = selectedOperation, Table = selectedTable, Attribute = selectedAttribute.Name, Operator = selectedOperator, Value = selectedAttribute.Type.Contains("int") ? value.ToString() : $"'{value.ToString()}'" });
            OnPropertyChanged(nameof(CollectionView));
            CleanControls();
        });
        public RelayCommand DeleteCondCmd => deleteCondCmd ?? new RelayCommand(obj =>
         {
             Conditions.Remove(SelectedCondition);
             UpdateControls();
         });
        public RelayCommand ShowQueryCmd => showQueryCmd ?? new RelayCommand(obj =>
         {
             MessageBox.Show(_QueryBuilder.QueryBuild(Attributes.Where(a => a.IsChecked).ToList(), Conditions.ToList()));
         });
        public RelayCommand RunQueryCmd => runQueryCmd ?? new RelayCommand(obj =>
        {
            Window window = obj as Window;
            if (Attributes.Where(a => a.IsChecked).Count() == 0)
            {
                return;
            }
            List<List<string>> table = DataProvider.Instance.RunQuery(_QueryBuilder.QueryBuild(Attributes.Where(a => a.IsChecked).ToList(), Conditions.ToList()));
            dataTable = new DataTable();
            foreach (var item in table.First())
            {
                dataTable.Columns.Add(new DataColumn(item, typeof(string)));
            }
            foreach (var item in table.Skip(1))
            {
                dataTable.Rows.Add(item.ToArray());
            }
            OnPropertyChanged(nameof(DataView));
        });
        public void CleanControls()
        {
            selectedAttribute = null;
            selectedTable = null;
            selectedOperator = null;
            valueCond = null;
            selectedOperation = null;
            selectedDate = null;
            UpdateControls();
        }
        public void UpdateControls()
        {
            OnPropertyChanged(nameof(SelectedAttribute));
            OnPropertyChanged(nameof(SelectedTable));
            OnPropertyChanged(nameof(SelectedOperator));
            OnPropertyChanged(nameof(ValueCond));
            OnPropertyChanged(nameof(SelectedOperation));
            OnPropertyChanged(nameof(SelectedDate));
            OnPropertyChanged(nameof(OperationIsEnabled));
        }
        public ICollectionView TreeCollectionView
        {
            get
            {
                return CollectionViewSource.GetDefaultView(treeCollection);
            }
        }
        private ObservableCollection<Table> treeCollection;
        public List<string> Headers { get; set; }
        public List<string> Rows { get; set; }
        public List<Result> Table { get; set; }
        private DataTable dataTable;
        public DataTable DataView
        {
            get
            {
                return dataTable;
            }
            set
            {
                dataTable = value;
                OnPropertyChanged(nameof(DataView));
            }
        }
    }
}
