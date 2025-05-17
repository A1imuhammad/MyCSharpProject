# PersonalFinanceApp

**PersonalFinanceApp** — это настольное приложение для управления личными финансами, разработанное на C# с использованием WPF. Оно предоставляет пользователю возможность отслеживать доходы и расходы, анализировать финансовые данные с помощью графиков и эффективно управлять бюджетом.

---

## 📂 Структура проекта

- **PersonalFinanceApp.sln** — основной файл решения Visual Studio.
- **PersonalFinanceApp/** — основной проект приложения:
  - `App.xaml` / `App.xaml.cs` — точка входа приложения.
  - `MainWindow.xaml` / `MainWindow.xaml.cs` — главное окно приложения.
  - `DashboardWindow.xaml` / `DashboardWindow.xaml.cs` — окно с аналитикой и графиками.
  - `packages.config` — список используемых NuGet-пакетов.
  - `App.config` — конфигурационный файл приложения.
- **Models/** — модели данных:
  - `User.cs` — модель пользователя.
  - `Category.cs` — модель категории расходов/доходов.
  - `Transaction.cs` — модель финансовой транзакции.
- **DataAccess/** — доступ к базе данных:
  - `DatabaseContext.cs` — контекст базы данных SQLite.
- **ViewModels/** — модели представлений (MVVM).
- **Properties/** — метаданные проекта и ресурсы.
- **packages/** — сторонние библиотеки:
  - `LiveCharts` и `LiveCharts.Wpf` — для визуализации данных.
  - `System.Data.SQLite.Core` — для работы с SQLite.

---

## 🛠️ Используемые технологии

- **.NET Framework 4.7.2**
- **WPF (Windows Presentation Foundation)**
- **SQLite** — встроенная база данных.
- **LiveCharts** — библиотека для построения графиков.

---

## 🚀 Как запустить проект

1. Открой `PersonalFinanceApp.sln` в Visual Studio.
2. Установи недостающие NuGet-пакеты через `Tools > NuGet Package Manager > Manage NuGet Packages for Solution...`.
3. Построй и запусти проект (`Ctrl + F5`).
4. При первом запуске будет создана база данных `finance.db` в папке `bin/Debug`.

---

## 📊 Возможности

- Добавление, редактирование и удаление транзакций.
- Категоризация доходов и расходов.
- Визуализация финансовых данных с помощью графиков.
- Поддержка нескольких пользователей.

---

## 📈 Пример визуализации

![Пример графика](https://raw.githubusercontent.com/A1imuhammad/MyCSharpProject/master/PersonalFinanceApp/Assets/chart-example.png)

---

## 📄 Лицензия

[MIT License](LICENSE)

---

## 🤝 Контакты

Разработчик: [Алимагомед](https://github.com/A1imuhammad)
