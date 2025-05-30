using Microsoft.Playwright;

string vacancySearchUrl = "https://hh.ru/search/vacancy?search_field=description&text=C%23+.NET&enable_snippets=false";
string sessionFilePath = Path.Join(Directory.GetCurrentDirectory(), "hh_session.json");

using IPlaywright playwright = await Playwright.CreateAsync();
IBrowser browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = false });

BrowserNewContextOptions contextOptions = new() { ViewportSize = new ViewportSize { Width = 1280, Height = 720 } };

if (File.Exists(sessionFilePath)) contextOptions.StorageStatePath = sessionFilePath;

var context = await browser.NewContextAsync(contextOptions);
var mainPage = await context.NewPageAsync();

await mainPage.GotoAsync(vacancySearchUrl, new PageGotoOptions { Timeout = 30000 });

if (!File.Exists(sessionFilePath))
{
    Console.WriteLine("Пожалуйста, войдите в свой аккаунт на hh.ru. После входа в аккаунт, нажмите enter");
    await context.StorageStateAsync(new BrowserContextStorageStateOptions { Path = sessionFilePath });
    Console.WriteLine("Сессия сохранена. Перезапустите программу");
}

List<string> vacancyLinks = [];
await mainPage.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
var links = await mainPage.QuerySelectorAllAsync("a[data-qa='serp-item__title']");

foreach (var link in links)
{
    var href = await link.GetAttributeAsync("href");
    if (!string.IsNullOrEmpty(href)) vacancyLinks.Add(href);
}

Console.WriteLine($"Найдено {vacancyLinks.Count} вакансий на главной странице");

foreach (var link in vacancyLinks)
{
    try
    {
        IPage vacancyPage = await context.NewPageAsync();
        await vacancyPage.GotoAsync(link, new PageGotoOptions { Timeout = 3000000 });
        Console.WriteLine($"Открыта вакансия: {link}");

        // Ожидание закрытия страницы
        var tcs = new TaskCompletionSource<bool>();
        vacancyPage.Close += (_, _) =>
        {
            Console.WriteLine($"Вакансия {link} закрыта");
            tcs.SetResult(true);
        };

        // Ждем, пока страница не будет закрыта
        await tcs.Task;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Ошибка при открытии вакансии {link}: {ex.Message}");
        continue;
    }
}

Console.WriteLine("Все вакансии обработаны");
await browser.CloseAsync();