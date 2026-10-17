using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.Json;
using CoffeeVendingMachine.Models;
using System.Windows.Navigation;


namespace CoffeeVendingMachine.Services
{
    public static class CoffeeLoader
    {
        private const string FileName = "Data/coffee.json"; // загружаем список напитков из JSON

        public static List<Coffee> Load()
        {
            var path = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, FileName);

            // Если файла coffee.json нет, то программа вернёт пустой список
            if (!File.Exists(path))
                return new List<Coffee>();

            var json = File.ReadAllText(path); // Читем текст в строку

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true, // Игнорируем регистр букв
                ReadCommentHandling = JsonCommentHandling.Skip, // Пропускаем комы
                AllowTrailingCommas = true // Разрешить "висячие" запятые. Например, если после последнего элемента массива
            };

            //Берём строку json, превращаем её в лист
            //JsonSerializer.Deserialize - преобразуем JSON в объект заданного типа
            return JsonSerializer.Deserialize<List<Coffee>>(json, options) ?? new List<Coffee>();
        }
    }
}
