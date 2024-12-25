using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using System.Linq;

class Program
{
    static async Task Main(string[] args)
    {
        Console.Write("Введите поисковый запрос: ");
        string query = Console.ReadLine();

        try
        {
            string encodedQuery = Uri.EscapeDataString(query);
            string apiUrl = $"https://ru.wikipedia.org/w/api.php?action=query&list=search&utf8=&format=json&srsearch={encodedQuery}";

            // Выполняем запрос к API
            string jsonResponse = await SendGetRequest(apiUrl);
            await ProcessResponse(jsonResponse);
        }
        catch (Exception e)
        {
            Console.WriteLine("Произошла ошибка: " + e.Message);
            Console.ReadLine();
        }
    }

    private static async Task<string> SendGetRequest(string apiUrl)
    {
        using (HttpClient client = new HttpClient())
        {
            return await client.GetStringAsync(apiUrl);
        }
    }

    private static async Task ProcessResponse(string jsonResponse)
    {
        var jsonObject = JObject.Parse(jsonResponse);
        var searchResults = jsonObject["query"]["search"];

        if (searchResults.HasValues == false)
        {
            Console.WriteLine("Результаты не найдены.");
            Console.ReadLine();
            return;
        }

        Console.WriteLine("Результаты поиска:");
        for (int i = 0; i < searchResults.Count(); i++)
        {
            string title = searchResults[i]["title"].ToString();
            int pageId = (int)searchResults[i]["pageid"];
            Console.WriteLine($"{i + 1}. {title} (pageId: {pageId})");
        }

        Console.Write("Введите номер статьи для открытия (или 'exit' для выхода): ");
        string choice = Console.ReadLine();

        if (choice.Equals("exit", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("Выход из программы.");
            Console.ReadLine();
            return;
        }

        try
        {
            int choiceIndex = int.Parse(choice) - 1;
            if (choiceIndex >= 0 && choiceIndex < searchResults.Count())
            {
                int pageId = (int)searchResults[choiceIndex]["pageid"];
                string articleUrl = $"https://ru.wikipedia.org/w/index.php?curid={pageId}";
                OpenBrowser(articleUrl);
            }
            else
            {
                Console.WriteLine("Некорректный номер статьи.");
                Console.ReadLine();
            }
        }
        catch (FormatException)
        {
            Console.WriteLine("Некорректный ввод.");
            Console.ReadLine();
        }
    }

    private static void OpenBrowser(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        catch (Exception e)
        {
            Console.WriteLine("Не удалось открыть браузер: " + e.Message);
            Console.ReadLine();
        }
    }
}


