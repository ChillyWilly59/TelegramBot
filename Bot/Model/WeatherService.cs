using Newtonsoft.Json.Linq;
using System.Globalization;

namespace Bot.Model
{
    public class WeatherService
    {
        private readonly string geocoderApiKey = "7426d6cd-1302-49ae-a84f-55ab8a1ae00d";
        private readonly string weatherApiKey = "f0d33cd3-883b-4d94-9ab3-02dabd40b6ea";
       
        public WeatherService()
        {
        }
        public async Task<string> GetWeatherAsync(string city)
        {
            using var httpClient = new HttpClient();
            try
            {
                string geocoderApiUrl = $"https://geocode-maps.yandex.ru/1.x/?apikey={geocoderApiKey}&format=json&geocode={city}";
                HttpResponseMessage geocoderResponse = await httpClient.GetAsync(geocoderApiUrl);
                geocoderResponse.EnsureSuccessStatusCode();

                string geocoderContent = await geocoderResponse.Content.ReadAsStringAsync();
                JObject geocoderJson = JObject.Parse(geocoderContent);

                JToken coordinates = geocoderJson.SelectToken("response.GeoObjectCollection.featureMember[0].GeoObject.Point.pos");
                string[] coords = coordinates.ToString().Split(' ');
                double latitude = double.Parse(coords[1], CultureInfo.InvariantCulture);
                double longitude = double.Parse(coords[0], CultureInfo.InvariantCulture);
                await Console.Out.WriteLineAsync($"Ширина {latitude}  долгота  {longitude}");


                string weatherApiUrl = $"https://api.weather.yandex.ru/v2/forecast/?lat={latitude.ToString().Replace(',', '.')}" +
                                        $"&lon={longitude.ToString().Replace(',', '.')}&lang=ru_RU&extra=true";

                await Console.Out.WriteLineAsync($"URL запрос  {weatherApiUrl}");

                using HttpClient client = new();
                client.DefaultRequestHeaders.Add("X-Yandex-API-Key", weatherApiKey);
                HttpResponseMessage weatherResponse = await client.GetAsync(weatherApiUrl);
                weatherResponse.EnsureSuccessStatusCode();

                string weatherContent = await weatherResponse.Content.ReadAsStringAsync();
                JObject weatherJson = JObject.Parse(weatherContent);
                JObject factObject = weatherJson["fact"] as JObject;

                double temp = (double)factObject["temp"];
                double feelsLike = (double)factObject["feels_like"];
                double windSpeed = (double)factObject["wind_speed"];
                int pressureMm = (int)factObject["pressure_mm"];
                int humidity = (int)factObject["humidity"];
                string daytime = (string)factObject["daytime"];
                bool polar = (bool)factObject["polar"];
                string season = (string)factObject["season"];
                long obsTime = (long)factObject["obs_time"];

                var resultMessage = $"Погода в {city} : ({latitude.ToString().Replace(',', '.')}, {longitude.ToString().Replace(',', '.')})\n\n" +
                                    $"Температура: {temp} °C\n" +
                                    $"Ощущается как: {feelsLike} °C\n" +
                                    $"Скорость ветра: {windSpeed} м/с\n" +
                                    $"Давление: {pressureMm} мм рт.ст.\n" +
                                    $"Влажность: {humidity}%\n" +
                                    $"Сезон: {season}\n";

                return resultMessage;

            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Ошибка при запросе: {ex.Message}");
                return $"Ошибка при запросе. Подробности в журнале.";
            }
        }
    }
}