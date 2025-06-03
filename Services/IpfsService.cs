using System.Net.Http.Headers;
using System.Text.RegularExpressions;

namespace BlockShare.Services
{
    public class IpfsService
    {
        private readonly HttpClient _http;

        public IpfsService()
        {
            _http = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:5001") // або свій IPFS нод
            };

            // Якщо ти реєструвалась на Infura, встав сюди ключі:
            // _http.DefaultRequestHeaders.Authorization =
            //     new AuthenticationHeaderValue("Basic", "base64(apiKey:apiSecret)");
        }

		/*   public async Task<string> UploadFileAsync(IFormFile file)
		   {
			   using var content = new MultipartFormDataContent();
			   using var stream = file.OpenReadStream();

			   var fileContent = new StreamContent(stream);
			   fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(file.ContentType);

			   content.Add(fileContent, "file", file.FileName);

			   var response = await _http.PostAsync("/api/v0/add", content);
			   var result = await response.Content.ReadAsStringAsync();


			   var hash = System.Text.RegularExpressions.Regex.Match(result, "\"Hash\":\"(.*?)\"").Groups[1].Value;

			   return hash;
		   }*/

		/*		public async Task<string> UploadFileAsync(IFormFile file)
                {
                    using var content = new MultipartFormDataContent();
                    using var stream = file.OpenReadStream();

                    var fileContent = new StreamContent(stream);
                    fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(file.ContentType);

                    content.Add(fileContent, "file", file.FileName);

                    // 1. Завантаження файлу
                    var response = await _http.PostAsync("/api/v0/add?pin=true", content); // 🟢 pin=true одразу закріплює
                    var result = await response.Content.ReadAsStringAsync();

                    // Витягуємо IPFS-хеш
                    var match = Regex.Match(result, "\"Hash\":\"(.*?)\"");
                    var hash = match.Success ? match.Groups[1].Value : null;

                    if (string.IsNullOrWhiteSpace(hash))
                        throw new Exception("Не вдалося отримати IPFS-хеш з відповіді: " + result);

                    return hash;
                }*/


		public async Task<string> UploadFileAsync(IFormFile file)
		{
			using var content = new MultipartFormDataContent();
			using var stream = file.OpenReadStream();

			var fileContent = new StreamContent(stream);
			fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(file.ContentType);

			content.Add(fileContent, "file", file.FileName);

			// 1. Завантаження файлу в IPFS
			var response = await _http.PostAsync("/api/v0/add?pin=true", content);
			var result = await response.Content.ReadAsStringAsync();

			var match = Regex.Match(result, "\"Hash\":\"(.*?)\"");
			var hash = match.Success ? match.Groups[1].Value : null;

			if (string.IsNullOrWhiteSpace(hash))
				throw new Exception("Не вдалося отримати IPFS-хеш з відповіді: " + result);

			// 2. Створення директорії в MFS, якщо потрібно
			var mkdirResponse = await _http.PostAsync("/api/v0/files/mkdir?arg=/myfiles&parents=true", null);
			if (!mkdirResponse.IsSuccessStatusCode)
			{
				var mkdirError = await mkdirResponse.Content.ReadAsStringAsync();
				Console.WriteLine("Помилка створення директорії MFS: " + mkdirError);
			}

			// 3. Додавання у MFS
			var mfsPath = $"/myfiles/{file.FileName}";
			var cpUrl = $"/api/v0/files/cp?arg=/ipfs/{hash}&arg={Uri.EscapeDataString(mfsPath)}";

			var cpResponse = await _http.PostAsync(cpUrl, null);
			if (!cpResponse.IsSuccessStatusCode)
			{
				var cpError = await cpResponse.Content.ReadAsStringAsync();
				Console.WriteLine("Помилка при додаванні до MFS: " + cpError);
			}

			return hash;
		}

	}
}
