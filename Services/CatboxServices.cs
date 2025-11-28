using System.Net.Http.Headers;

namespace Vex_E_commerce.Services
{
    public class CatboxServices
    {
        private const string CatboxApiUrl = "https://catbox.moe/user/api.php";
        private readonly HttpClient _httpClient;

        public CatboxServices(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> UploadImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return null;

            try
            {
                using (var content = new MultipartFormDataContent())
                {
                    // 1. reqtype
                    content.Add(new StringContent("fileupload"), "reqtype");

                    // 2. แปลงไฟล์เป็น MemoryStream ก่อนส่ง (เสถียรกว่า)
                    using (var ms = new MemoryStream())
                    {
                        await file.CopyToAsync(ms);
                        ms.Position = 0; // รีเซ็ตตำแหน่งอ่านไปที่จุดเริ่มต้น

                        using (var fileContent = new StreamContent(ms))
                        {
                            // ต้องระบุ Content-Type ให้ชัดเจน
                            fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

                            // "fileToUpload" และชื่อไฟล์
                            content.Add(fileContent, "fileToUpload", file.FileName);

                            // 3. ส่ง Request
                            var response = await _httpClient.PostAsync(CatboxApiUrl, content);

                            if (response.IsSuccessStatusCode)
                            {
                                return await response.Content.ReadAsStringAsync();
                            }
                            else
                            {
                                // อ่าน Error message จาก Server (ถ้ามี)
                                var errorMsg = await response.Content.ReadAsStringAsync();
                                throw new Exception($"Catbox Error: {response.StatusCode} - {errorMsg}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error หรือ throw ออกไป
                throw new Exception($"Upload failed: {ex.Message}");
            }
        }
    }
}