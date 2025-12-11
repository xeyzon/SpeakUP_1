using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SpeakUP_1
{
    public class GigaChatService
    {
        // ... (Константы и поля остаются без изменений)
        private const string AuthData = "MDE5YWZkZDUtYmM5Ni03ZTI5LWE1NzItYTg3YjRiOGVlODU2OjYzMzI2M2Q4LWRiYzAtNGYwYS1iMmNkLTQ4YWJhNzI5MzcwYw==";
        private const string Scope = "GIGACHAT_API_PERS";
        private const string AuthUrl = "https://ngw.devices.sberbank.ru:9443/api/v2/oauth";
        private const string ChatUrl = "https://gigachat.devices.sberbank.ru/api/v1/chat/completions";

        private string _accessToken;
        private DateTime _tokenExpiration;
        private string _userRole; // Поле для хранения роли

        public string UserRole
        {
            get => _userRole;
            set => _userRole = value;
        }


        private readonly HttpClientHandler _handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
        };

        // =========================================================================
        // НОВЫЙ МЕТОД (Перегрузка с двумя аргументами)
        // Этот метод будет вызываться из MainWindow, чтобы не менять код вызова.
        // Он обновляет внутреннюю роль и вызывает основной метод.
        // =========================================================================
        public async Task<string> SendRequestAsync(string userRole, string speechText)
        {
            // 1. Обновляем внутреннее поле _userRole на основе переданного аргумента
            // Это гарантирует, что промпт будет использовать самую актуальную роль.
            _userRole = userRole;

            // 2. Вызываем основной метод с одним аргументом
            return await SendRequestAsync(speechText);
        }

        // =========================================================================
        // ОСНОВНОЙ МЕТОД (с одним аргументом)
        // Этот метод выполняет всю логику и использует внутреннее поле _userRole.
        // =========================================================================
        public async Task<string> SendRequestAsync(string speechText)
        {
            if (string.IsNullOrEmpty(_accessToken) || DateTime.Now >= _tokenExpiration)
            {
                await GetTokenAsync().ConfigureAwait(false);
            }

            // 2. Формируем запрос к нейросети
            // Используем сохраненную переменную _userRole
            var prompt = $"Привет, сейчас я расскажу о себе: \"{_userRole}\" . Ты — эксперт по ораторскому мастерству.  мой Текст речи: \"{speechText}\". Дай краткий анализ (стиль, ошибки) и совет, самое важное, чтобы в начале совета ты обратился ко мне по имени без кавычек. Также важным условием, чтобы в тексте не было решеток '#'  ";
                         

            using (var client = new HttpClient(_handler, disposeHandler: false))
            {
                client.Timeout = TimeSpan.FromSeconds(30);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

                var requestData = new
                {
                    model = "GigaChat",
                    messages = new[]
                    {
                        new { role = "user", content = prompt }
                    },
                    temperature = 0.7
                };

                var jsonContent = JsonConvert.SerializeObject(requestData);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                try
                {
                    var response = await client.PostAsync(ChatUrl, content).ConfigureAwait(false);

                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        await GetTokenAsync().ConfigureAwait(false);
                        // Рекурсивный вызов
                        return await SendRequestAsync(speechText);
                    }

                    response.EnsureSuccessStatusCode();
                    var jsonResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var parsed = JObject.Parse(jsonResponse);

                    return parsed["choices"]?[0]?["message"]?["content"]?.ToString() ?? "Нет ответа.";
                }
                catch (Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException)
                {
                    return $"Ошибка запроса или таймаут: {ex.Message}";
                }
                catch (Exception ex)
                {
                    return $"Неизвестная ошибка: {ex.Message}";
                }
            }
        }

        private async Task GetTokenAsync()
        {
            using (var client = new HttpClient(_handler, disposeHandler: false))
            {
                client.Timeout = TimeSpan.FromSeconds(15);

                string rquid = Guid.NewGuid().ToString();

                var request = new HttpRequestMessage(HttpMethod.Post, AuthUrl);

                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", AuthData);
                request.Headers.Add("RqUID", rquid);

                request.Content = new StringContent($"scope={Scope}", Encoding.UTF8, "application/x-www-form-urlencoded");

                var response = await client.SendAsync(request).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var data = JObject.Parse(json);

                _accessToken = data["access_token"].ToString();
                _tokenExpiration = DateTime.Now.AddMinutes(29);
            }
        }
    }
}