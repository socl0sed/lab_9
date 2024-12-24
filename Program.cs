using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Laba9;

class Program
{
	private static readonly HttpClient httpClient = new HttpClient();
	private static readonly string apiToken = "dE9nWHNsaXk0bzUwWkJVVWlwWHIwaGRlUWc5RjhjcXBfTkw1Vmp4VVBxdz0";

	static async Task Main(string[] args)
	{
		var tickers = await File.ReadAllLinesAsync("ticker.txt");
		var tasks = new List<Task>();

		foreach (var ticker in tickers)
		{
			tasks.Add(ProcessTickerAsync(ticker));
		}

		await Task.WhenAll(tasks);
	}

	static async Task ProcessTickerAsync(string ticker)
	{
		var url = $"https://api.marketdata.app/v1/stocks/candles/D/{ticker}/?from=2024-01-01&to=2024-09-30&token={apiToken}";
		var response = await httpClient.GetAsync(url);
		if (response.IsSuccessStatusCode == false)
		{
			return;
		}
		var json = await response.Content.ReadAsStringAsync();
		var data = JsonSerializer.Deserialize<CandleData>(json);

		// Проверка на null
		if (data == null || data.c == null || data.h == null || data.l == null)
		{
			Console.WriteLine($"Error: Data for ticker {ticker} is null or incomplete.");
			return;
		}

		double sum = 0;
		int count = data.c.Length;

		for (int i = 0; i < count; i++)
		{
			sum += (data.h[i] + data.l[i]) / 2;
		}

		double averagePrice = sum / count;

		lock (Console.Out)
		{
			File.AppendAllText("average_prices.txt", $"{ticker}:{averagePrice}\n");
		}
	}

	class CandleData
	{
		public string s { get; set; }
		public double[] c { get; set; }
		public double[] h { get; set; }
		public double[] l { get; set; }
	}
}