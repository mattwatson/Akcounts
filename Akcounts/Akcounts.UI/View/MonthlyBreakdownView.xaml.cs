using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Akcounts.UI.Util;

namespace Akcounts.UI.View
{
    public partial class MonthlyBreakdownView
    {
        public MonthlyBreakdownView()
        {
            InitializeComponent();
        }

        private void FilterDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            AccountBalanceGrid.Visibility = Visibility.Hidden;
        }

        private void ToggleButtonChecked(object sender, RoutedEventArgs e)
        {
            AccountBalanceGrid.Visibility = Visibility.Hidden;
        }

        private void AccountSelectorSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AccountBalanceGrid.Visibility = Visibility.Hidden;
        }

        private void UpdateClicked(object sender, RoutedEventArgs e)
        {
            SetUpColumns();
            RefreshRows();
            AccountBalanceGrid.Visibility = Visibility.Visible;
        }

        public void SetUpColumns()
        {
            AccountBalanceGrid.Columns.Clear();

            AddStandardColumns();
            ValidateDatesAndAddDateColumns();
        }

        private void AddStandardColumns()
        {
            var nameColumn = new DataGridTextColumn
            {
                Header = "Account",
                Binding = new Binding("AccountName"),
            };
            AccountBalanceGrid.Columns.Add(nameColumn);
        }

        private void ValidateDatesAndAddDateColumns()
        {
            if (!FromDatePicker.SelectedDate.HasValue) return;
            if (!ToDatePicker.SelectedDate.HasValue) return;
            var fromDate = FromDatePicker.SelectedDate.Value;
            var toDate = ToDatePicker.SelectedDate.Value;

            var dateRange = DateUtil.GenerateDateTimeRange(fromDate, toDate);

            AddDateColumns(dateRange);
        }

        private void AddDateColumns(IList<DateTime> dateRange)
        {
            var bindingNames = GenerateBindingNames(dateRange);

            var columns = Enumerable.Range(0, dateRange.Count())
                .Select(i => new DataGridTextColumn
                {
                    Header = dateRange[i].ToString("dd-MMM"),
                    Binding = new Binding(bindingNames[i]),
                });

            foreach (var col in columns)
            {
                AccountBalanceGrid.Columns.Add(col);
            }
        }

        private static IList<string> GenerateBindingNames(IEnumerable<DateTime> dateRange)
        {
            return Enumerable.Range(0, dateRange.Count())
                .Select(i => "BalanceHistories[" + i + "]").ToList();
        }

        private void RefreshRows()
        {
            var bindingExpression = AccountBalanceGrid.GetBindingExpression(ItemsControl.ItemsSourceProperty);
            if (bindingExpression != null)
            {
                bindingExpression.UpdateTarget();
            }
        }
    }
}


