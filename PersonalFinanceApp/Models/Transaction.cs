using System;

namespace PersonalFinanceApp.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Type { get; set; } // "Income" или "Expense"
        public decimal Amount { get; set; }
        public int CategoryId { get; set; }
        public DateTime Date { get; set; } // Поле для хранения даты и времени
        public string Description { get; set; }
    }
}