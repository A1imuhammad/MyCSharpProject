using PersonalFinanceApp.DataAccess;
using PersonalFinanceApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using LiveCharts;
using LiveCharts.Wpf;

namespace PersonalFinanceApp
{
    public partial class DashboardWindow : Window
    {
        private readonly DatabaseContext _dbContext;
        private readonly int _userId;
        private string _currentSortColumn = null;
        private bool _isSortAscending = true;

        public DashboardWindow(int userId)
        {
            InitializeComponent();
            _dbContext = new DatabaseContext();
            _userId = userId;
            LoadData();
        }

        private void LoadData()
        {
            // Загрузка категорий
            CategoryComboBox.ItemsSource = _dbContext.GetCategories(_userId);
            CategoryComboBox.DisplayMemberPath = "Name";
            CategoriesListView.ItemsSource = _dbContext.GetCategories(_userId);

            // Загрузка транзакций
            LoadTransactions();

            // Загрузка валюты
            CurrencyComboBox.SelectedItem = CurrencyComboBox.Items
                .Cast<ComboBoxItem>()
                .FirstOrDefault(i => i.Content.ToString() == GetDisplayCurrency(_dbContext.GetCurrency(_userId)));
        }

        private void LoadTransactions(string searchQuery = null)
        {
            var transactions = _dbContext.GetTransactions(_userId);

            // Фильтрация по поисковому запросу
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                searchQuery = searchQuery.ToLower();
                transactions = transactions.Where(t =>
                    t.Type.ToLower().Contains(searchQuery) ||
                    t.Amount.ToString().Contains(searchQuery) ||
                    t.Description.ToLower().Contains(searchQuery)
                ).ToList();
            }

            // Сортировка
            if (!string.IsNullOrEmpty(_currentSortColumn))
            {
                switch (_currentSortColumn)
                {
                    case "Тип":
                        transactions = _isSortAscending
                            ? transactions.OrderBy(t => t.Type.ToLower()).ToList()
                            : transactions.OrderByDescending(t => t.Type.ToLower()).ToList();
                        break;
                    case "Сумма":
                        transactions = _isSortAscending
                            ? transactions.OrderBy(t => t.Amount).ToList()
                            : transactions.OrderByDescending(t => t.Amount).ToList();
                        break;
                    case "Дата":
                        transactions = _isSortAscending
                            ? transactions.OrderBy(t => t.Date).ToList()
                            : transactions.OrderByDescending(t => t.Date).ToList();
                        break;
                    case "Описание":
                        transactions = _isSortAscending
                            ? transactions.OrderBy(t => t.Description.ToLower()).ToList()
                            : transactions.OrderByDescending(t => t.Description.ToLower()).ToList();
                        break;
                }
            }

            // Отображаем транзакции
            TransactionsListView.ItemsSource = transactions.Select(t => new
            {
                t.Id,
                Type = t.Type == "Income" ? "Доход" : "Расход",
                Amount = t.Amount,
                Date = t.Date,
                Description = t.Description
            });
        }

        private string GetDisplayCurrency(string currencyCode)
        {
            switch (currencyCode)
            {
                case "USD":
                    return "долл.";
                case "EUR":
                    return "евро";
                case "RUB":
                    return "руб.";
                default:
                    return "руб.";
            }
        }

        private string GetCurrencyCode(string displayCurrency)
        {
            switch (displayCurrency)
            {
                case "долл.":
                    return "USD";
                case "евро":
                    return "EUR";
                case "руб.":
                    return "RUB";
                default:
                    return "RUB";
            }
        }

        private void AddTransactionButton_Click(object sender, RoutedEventArgs e)
        {
            if (decimal.TryParse(TransactionAmountTextBox.Text, out decimal amount) &&
                TransactionTypeComboBox.SelectedItem is ComboBoxItem typeItem &&
                CategoryComboBox.SelectedItem is Category category)
            {
                // Получаем текущее время по МСК (UTC+3)
                DateTime moscowTime = DateTime.UtcNow.AddHours(3);

                var transaction = new Transaction
                {
                    UserId = _userId,
                    Type = typeItem.Content.ToString() == "Доход" ? "Income" : "Expense",
                    Amount = amount,
                    CategoryId = category.Id,
                    Date = moscowTime,
                    Description = TransactionDescriptionTextBox.Text
                };
                _dbContext.AddTransaction(transaction);

                // Проверяем, что время сохранилось
                var addedTransaction = _dbContext.GetTransactions(_userId).Last();
                System.Diagnostics.Debug.WriteLine($"Добавлена транзакция: ID {addedTransaction.Id}, Date: {addedTransaction.Date:dd/MM/yyyy HH:mm}");

                LoadData();
            }
            else
            {
                MessageBox.Show("Неверный ввод. Проверьте сумму и выберите категорию.");
            }
        }

        private void EditTransactionButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && int.TryParse(button.Tag.ToString(), out int transactionId))
            {
                var transaction = _dbContext.GetTransactions(_userId).FirstOrDefault(t => t.Id == transactionId);
                if (transaction != null)
                {
                    if (decimal.TryParse(TransactionAmountTextBox.Text, out decimal amount))
                    {
                        transaction.Amount = amount;
                        _dbContext.UpdateTransaction(transaction);
                        LoadData();
                    }
                    else
                    {
                        MessageBox.Show("Неверная сумма для редактирования.");
                    }
                }
            }
        }

        private void DeleteTransactionButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && int.TryParse(button.Tag.ToString(), out int transactionId))
            {
                _dbContext.DeleteTransaction(transactionId);
                LoadData();
            }
        }

        private void AddCategoryButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(CategoryNameTextBox.Text))
            {
                var category = new Category
                {
                    UserId = _userId,
                    Name = CategoryNameTextBox.Text
                };
                _dbContext.AddCategory(category);
                LoadData();
            }
            else
            {
                MessageBox.Show("Введите название категории.");
            }
        }

        private void ViewExpensesByCategoryButton_Click(object sender, RoutedEventArgs e)
        {
            var transactions = _dbContext.GetTransactions(_userId);
            var categories = _dbContext.GetCategories(_userId);
            var report = transactions
                .Where(t => t.Type == "Expense")
                .GroupBy(t => t.CategoryId)
                .Select(g => new
                {
                    Category = categories.FirstOrDefault(c => c.Id == g.Key)?.Name ?? "Без категории",
                    Total = g.Sum(t => t.Amount)
                })
                .Where(r => r.Total > 0);

            // Текстовый отчет
            ReportTextBlock.Text = string.Join("\n", report.Select(r => $"{r.Category}: {r.Total} {GetDisplayCurrency(_dbContext.GetCurrency(_userId))}"));

            // Построение круговой диаграммы
            var series = new SeriesCollection();
            foreach (var item in report)
            {
                series.Add(new PieSeries
                {
                    Title = item.Category,
                    Values = new ChartValues<double> { (double)item.Total },
                    DataLabels = true,
                    LabelPoint = chartPoint => $"{chartPoint.Y} {GetDisplayCurrency(_dbContext.GetCurrency(_userId))}"
                });
            }

            ExpensesPieChart.Series = series;
        }

        private void ViewSummaryReportButton_Click(object sender, RoutedEventArgs e)
        {
            var transactions = _dbContext.GetTransactions(_userId);

            // Подсчитываем доходы и расходы
            decimal totalIncome = transactions
                .Where(t => t.Type == "Income")
                .Sum(t => t.Amount);
            decimal totalExpenses = transactions
                .Where(t => t.Type == "Expense")
                .Sum(t => t.Amount);

            // Формируем текстовый отчет
            string currency = GetDisplayCurrency(_dbContext.GetCurrency(_userId));
            ReportTextBlock.Text = $"Доходы: {totalIncome} {currency}\n" +
                                   $"Расходы: {totalExpenses} {currency}\n" +
                                   $"Баланс: {totalIncome - totalExpenses} {currency}";

            // Построение круговой диаграммы
            var series = new SeriesCollection();
            if (totalIncome > 0)
            {
                series.Add(new PieSeries
                {
                    Title = "Доходы",
                    Values = new ChartValues<double> { (double)totalIncome },
                    DataLabels = true,
                    LabelPoint = chartPoint => $"{chartPoint.Y} {currency}"
                });
            }
            if (totalExpenses > 0)
            {
                series.Add(new PieSeries
                {
                    Title = "Расходы",
                    Values = new ChartValues<double> { (double)totalExpenses },
                    DataLabels = true,
                    LabelPoint = chartPoint => $"{chartPoint.Y} {currency}"
                });
            }

            ExpensesPieChart.Series = series;
        }

        private void SaveCurrencyButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrencyComboBox.SelectedItem is ComboBoxItem currencyItem)
            {
                _dbContext.SetCurrency(_userId, GetCurrencyCode(currencyItem.Content.ToString()));
                LoadData();
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string searchQuery = SearchTextBox.Text;
            LoadTransactions(searchQuery);
        }

        private void ResetSearchButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = string.Empty;
            LoadTransactions();
        }

        private void SortButton_Click(object sender, RoutedEventArgs e)
        {
            if (SortColumnComboBox.SelectedItem is ComboBoxItem sortColumnItem &&
                SortDirectionComboBox.SelectedItem is ComboBoxItem sortDirectionItem)
            {
                _currentSortColumn = sortColumnItem.Content.ToString();
                _isSortAscending = sortDirectionItem.Content.ToString() == "По возрастанию";
                LoadTransactions(SearchTextBox.Text);
            }
            else
            {
                MessageBox.Show("Выберите столбец и направление сортировки.");
            }
        }
    }
}