using System.Data.SQLite;
using PersonalFinanceApp.Models;
using System;
using System.Collections.Generic;

namespace PersonalFinanceApp.DataAccess
{
    public class DatabaseContext
    {
        private readonly string _connectionString = "Data Source=finance.db;Version=3;";

        public DatabaseContext()
        {
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();

                // Таблица пользователей
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Users (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Username TEXT NOT NULL UNIQUE,
                        Password TEXT NOT NULL
                    )";
                command.ExecuteNonQuery();

                // Таблица транзакций
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Transactions (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        UserId INTEGER,
                        Type TEXT,
                        Amount REAL,
                        CategoryId INTEGER,
                        Date TEXT,
                        Description TEXT,
                        FOREIGN KEY(UserId) REFERENCES Users(Id),
                        FOREIGN KEY(CategoryId) REFERENCES Categories(Id)
                    )";
                command.ExecuteNonQuery();

                // Миграция: добавляем время для старых записей
                command.CommandText = @"
                    UPDATE Transactions 
                    SET Date = Date || ' 12:00:00' 
                    WHERE Date NOT LIKE '%:%'";
                command.ExecuteNonQuery();

                // Таблица категорий
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Categories (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        UserId INTEGER,
                        Name TEXT,
                        FOREIGN KEY(UserId) REFERENCES Users(Id)
                    )";
                command.ExecuteNonQuery();

                // Таблица настроек (валюта)
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Settings (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        UserId INTEGER,
                        Currency TEXT,
                        FOREIGN KEY(UserId) REFERENCES Users(Id)
                    )";
                command.ExecuteNonQuery();
            }
        }

        public bool RegisterUser(string username, string password)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "INSERT INTO Users (Username, Password) VALUES (@username, @password)";
                command.Parameters.AddWithValue("@username", username);
                command.Parameters.AddWithValue("@password", password);
                try
                {
                    command.ExecuteNonQuery();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public User LoginUser(string username, string password)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT Id, Username FROM Users WHERE Username = @username AND Password = @password";
                command.Parameters.AddWithValue("@username", username);
                command.Parameters.AddWithValue("@password", password);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new User
                        {
                            Id = reader.GetInt32(0),
                            Username = reader.GetString(1)
                        };
                    }
                    return null;
                }
            }
        }

        // Методы для транзакций
        public void AddTransaction(Transaction transaction)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO Transactions (UserId, Type, Amount, CategoryId, Date, Description)
                    VALUES (@userId, @type, @amount, @categoryId, @date, @description)";
                command.Parameters.AddWithValue("@userId", transaction.UserId);
                command.Parameters.AddWithValue("@type", transaction.Type);
                command.Parameters.AddWithValue("@amount", transaction.Amount);
                command.Parameters.AddWithValue("@categoryId", transaction.CategoryId);
                command.Parameters.AddWithValue("@date", transaction.Date.ToString("yyyy-MM-dd HH:mm:ss")); // Сохраняем дату и время
                command.Parameters.AddWithValue("@description", transaction.Description);
                command.ExecuteNonQuery();
            }
        }

        public void UpdateTransaction(Transaction transaction)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    UPDATE Transactions
                    SET Type = @type, Amount = @amount, CategoryId = @categoryId, Date = @date, Description = @description
                    WHERE Id = @id";
                command.Parameters.AddWithValue("@id", transaction.Id);
                command.Parameters.AddWithValue("@type", transaction.Type);
                command.Parameters.AddWithValue("@amount", transaction.Amount);
                command.Parameters.AddWithValue("@categoryId", transaction.CategoryId);
                command.Parameters.AddWithValue("@date", transaction.Date.ToString("yyyy-MM-dd HH:mm:ss")); // Сохраняем дату и время
                command.Parameters.AddWithValue("@description", transaction.Description);
                command.ExecuteNonQuery();
            }
        }

        public void DeleteTransaction(int transactionId)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM Transactions WHERE Id = @id";
                command.Parameters.AddWithValue("@id", transactionId);
                command.ExecuteNonQuery();
            }
        }

        public List<Transaction> GetTransactions(int userId)
        {
            var transactions = new List<Transaction>();
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM Transactions WHERE UserId = @userId";
                command.Parameters.AddWithValue("@userId", userId);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        transactions.Add(new Transaction
                        {
                            Id = reader.GetInt32(0),
                            UserId = reader.GetInt32(1),
                            Type = reader.GetString(2),
                            Amount = reader.GetDecimal(3),
                            CategoryId = reader.GetInt32(4),
                            Date = DateTime.Parse(reader.GetString(5)), // Загружаем дату и время
                            Description = reader.GetString(6)
                        });
                    }
                }
            }
            return transactions;
        }

        // Методы для категорий
        public void AddCategory(Category category)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "INSERT INTO Categories (UserId, Name) VALUES (@userId, @name)";
                command.Parameters.AddWithValue("@userId", category.UserId);
                command.Parameters.AddWithValue("@name", category.Name);
                command.ExecuteNonQuery();
            }
        }

        public List<Category> GetCategories(int userId)
        {
            var categories = new List<Category>();
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM Categories WHERE UserId = @userId";
                command.Parameters.AddWithValue("@userId", userId);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        categories.Add(new Category
                        {
                            Id = reader.GetInt32(0),
                            UserId = reader.GetInt32(1),
                            Name = reader.GetString(2)
                        });
                    }
                }
            }
            return categories;
        }

        // Методы для настроек
        public void SetCurrency(int userId, string currency)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT OR REPLACE INTO Settings (Id, UserId, Currency)
                    VALUES ((SELECT Id FROM Settings WHERE UserId = @userId), @userId, @currency)";
                command.Parameters.AddWithValue("@userId", userId);
                command.Parameters.AddWithValue("@currency", currency);
                command.ExecuteNonQuery();
            }
        }

        public string GetCurrency(int userId)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT Currency FROM Settings WHERE UserId = @userId";
                command.Parameters.AddWithValue("@userId", userId);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return reader.GetString(0);
                    }
                    return "RUB"; // Валюта по умолчанию — рубли
                }
            }
        }
    }
}