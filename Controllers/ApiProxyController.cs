using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;
using System.Net.Http.Headers;

namespace API_CLIENT.Controllers
{
    public class ProxyRequestDto
    {
        public string Method { get; set; } = "GET";
        public string Url { get; set; } = string.Empty;
        public string? HeadersJson { get; set; }
        public string BodyType { get; set; } = "none";
        public string? ContentType { get; set; }
        public string? Body { get; set; }
    }

    [Route("[controller]")]
    [ApiController]
    public class ApiProxyController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ApiProxyController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpPost("Execute")]
        public async Task<IActionResult> Execute([FromBody] ProxyRequestDto requestDto)
        {
            if (string.IsNullOrWhiteSpace(requestDto.Url))
            {
                return BadRequest("URL cannot be empty.");
            }

            try
            {
                var client = _httpClientFactory.CreateClient();
                var reqMessage = new HttpRequestMessage(new HttpMethod(requestDto.Method.ToUpper()), requestDto.Url);

                // Add headers
                if (!string.IsNullOrWhiteSpace(requestDto.HeadersJson))
                {
                    var headers = JsonSerializer.Deserialize<Dictionary<string, string>>(requestDto.HeadersJson);
                    if (headers != null)
                    {
                        foreach (var header in headers)
                        {
                            // Avoid setting Content-Type directly in default headers as HttpContent handles it.
                            if (!header.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
                            {
                                reqMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
                            }
                        }
                    }
                }

                // Add body
                if ((requestDto.Method.Equals("POST", StringComparison.OrdinalIgnoreCase) || 
                     requestDto.Method.Equals("PUT", StringComparison.OrdinalIgnoreCase) || 
                     requestDto.Method.Equals("PATCH", StringComparison.OrdinalIgnoreCase)) &&
                    !string.IsNullOrWhiteSpace(requestDto.Body))
                {
                    if (requestDto.BodyType == "raw")
                    {
                        reqMessage.Content = new StringContent(requestDto.Body, Encoding.UTF8, requestDto.ContentType ?? "application/json");
                    }
                    else if (requestDto.BodyType == "formdata" || requestDto.BodyType == "urlencoded")
                    {
                        var formData = JsonSerializer.Deserialize<Dictionary<string, string>>(requestDto.Body);
                        if (formData != null)
                        {
                            if (requestDto.BodyType == "formdata")
                            {
                                var multipartContent = new MultipartFormDataContent();
                                foreach(var kvp in formData)
                                {
                                    multipartContent.Add(new StringContent(kvp.Value), kvp.Key);
                                }
                                reqMessage.Content = multipartContent;
                            }
                            else
                            {
                                reqMessage.Content = new FormUrlEncodedContent(formData);
                            }
                        }
                    }
                }

                var response = await client.SendAsync(reqMessage);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return Ok(responseContent);
                }
                else
                {
                    return StatusCode((int)response.StatusCode, responseContent);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }
    }
}
