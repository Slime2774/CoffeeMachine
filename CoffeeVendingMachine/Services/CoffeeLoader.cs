using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using CoffeeVendingMachine.Models;

namespace CoffeeVendingMachine.Services
{
    public static class CoffeeLoader
    {
        // Загрузка меню
        public static List<Drink> LoadMenu()
        {
            // Формируем путь к файлу в папке, где лежит .exe
            var path = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Data/menu.json");

            // Защита: если файла нет, возвращаем пустой список (программа не упадет)
            if (!File.Exists(path)) return new List<Drink>();

            var json = File.ReadAllText(path);

            // Настройки парсера
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true, // Игнорировать регистр (Name = name)
                AllowTrailingCommas = true,        // Разрешить запятую в конце списка
                ReadCommentHandling = JsonCommentHandling.Skip // Пропускать комментарии
            };

            var drinks = new List<Drink>();

            // Читаем JSON вручную, чтобы определить тип класса по полю "type"
            using var doc = JsonDocument.Parse(json);

            foreach (var element in doc.RootElement.EnumerateArray())
            {
                if (element.TryGetProperty("type", out var typeProp))
                {
                    var type = typeProp.GetString();

                    // Если в JSON написано "CoffeeDrink", создаем CoffeeDrink
                    if (type == "CoffeeDrink")
                    {
                        var coffee = JsonSerializer.Deserialize<CoffeeDrink>(element.GetRawText(), options);
                        if (coffee != null) drinks.Add(coffee);
                    }
                }
            }
            return drinks;
        }

        // Загрузка настроек автомата
        public static Config LoadConfig()
        {
            var path = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Data/config.json");
            if (!File.Exists(path)) return new Config(); // Если файла нет — пустой конфиг

            var json = File.ReadAllText(path);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            return JsonSerializer.Deserialize<Config>(json, options) ?? new Config();
        }
    }
}