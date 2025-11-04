using Amazon.S3;
using Amazon.S3.Model;
using Amazon.Runtime;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace FamilyMemories.Services
{
    /// <summary>
    /// Cloudflare R2 的簡易實作 (S3-compatible)
    ///
    /// 說明：
    /// - 這個實作使用 AWSSDK.S3 的 S3 API 來存取 Cloudflare R2（R2 與 S3 相容）。
    /// - 預設上傳會把物件設定為 PublicRead（如果你想改成私有請移除 CannedACL 或改用 Bucket Policy）。
    /// - DeleteAsync 可接受 key 或完整 URL（會從 URL 推回 key）。
    ///
    /// 設定方式：
    /// - 在 `appsettings.json` 或 `appsettings.Development.json` 加上 CloudflareR2 的 ServiceUrl 與 Bucket 名稱。
    /// - 建議把 `AccessKeyId` / `SecretAccessKey` 放在環境變數或 secret manager，而不是直接寫在設定檔中。
    ///
    /// 範例 ServiceUrl: https://<account_id>.r2.cloudflarestorage.com
    /// Key 範例: uploads/{GUID}.jpg
    /// </summary>
    public class CloudflareR2StorageService : IStorageService
    {
        // S3 client (透過 AWSSDK.S3 與 BasicAWSCredentials 注入)
        private readonly IAmazonS3 _s3;
        // R2 bucket 名稱
        private readonly string _bucket;
        // R2 service URL，例如 https://<account>.r2.cloudflarestorage.com
        private readonly string _serviceUrl;

        public CloudflareR2StorageService(IAmazonS3 s3, IConfiguration config)
        {
            _s3 = s3;
            _bucket = config["CloudflareR2:Bucket"] ?? throw new ArgumentNullException("CloudflareR2:Bucket");
            _serviceUrl = config["CloudflareR2:ServiceUrl"]?.TrimEnd('/') ?? string.Empty;
        }

        /// <summary>
        /// 上傳檔案到 R2 並回傳物件的公開 URL（若你改成私有，請改為回傳 key 並使用 GeneratePresignedUrl）。
        /// </summary>
        public async Task<string> UploadAsync(Stream content, string key, string contentType)
        {
            // 將流讀取到內存中以避免流式簽名問題
            using var memoryStream = new MemoryStream();
            await content.CopyToAsync(memoryStream);
            var fileBytes = memoryStream.ToArray();

            var put = new PutObjectRequest
            {
                BucketName = _bucket,
                Key = key,
                InputStream = new MemoryStream(fileBytes),
                ContentType = contentType,
                DisablePayloadSigning = true  // 禁用 payload 簽名以避免 STREAMING-AWS4-HMAC-SHA256-PAYLOAD 錯誤
            };

            // 預設設為公開讀取，若想要私有請調整此處或使用 bucket policy
            put.CannedACL = S3CannedACL.PublicRead;

            var res = await _s3.PutObjectAsync(put).ConfigureAwait(false);
            if (res.HttpStatusCode == System.Net.HttpStatusCode.OK)
            {
                // 回傳可透過 CDN 訪問的 URL
                return GetUrl(key);
            }
            throw new Exception($"Upload failed: {res.HttpStatusCode}");
        }

        /// <summary>
        /// 刪除物件，接受 key 或完整 URL。
        /// 回傳 true 表示刪除成功或物件不存在。
        /// </summary>
        public async Task<bool> DeleteAsync(string keyOrUrl)
        {
            if (string.IsNullOrEmpty(keyOrUrl)) return true;

            var key = GetKeyFromKeyOrUrl(keyOrUrl);
            var req = new DeleteObjectRequest { BucketName = _bucket, Key = key };
            var res = await _s3.DeleteObjectAsync(req).ConfigureAwait(false);
            return res.HttpStatusCode == System.Net.HttpStatusCode.NoContent || res.HttpStatusCode == System.Net.HttpStatusCode.OK;
        }

        /// <summary>
        /// 取得公開 URL：回傳 path-style 的 URL（{serviceUrl}/{bucket}/{key}）。
        /// 若你使用自訂域名或 CDN，請調整回傳格式。
        /// </summary>
        public string GetUrl(string key)
        {
            if (string.IsNullOrEmpty(_serviceUrl))
                throw new InvalidOperationException("Cloudflare R2 ServiceUrl is not configured.");

            return $"{_serviceUrl}/{_bucket}/{Uri.EscapeDataString(key)}";
        }

        /// <summary>
        /// 為私有物件產生預簽名 URL，預設使用 AWSSDK 的 GetPreSignedURL 方法。
        /// </summary>
        public Task<string> GeneratePresignedUrlAsync(string key, TimeSpan expires)
        {
            var req = new GetPreSignedUrlRequest
            {
                BucketName = _bucket,
                Key = key,
                Expires = DateTime.UtcNow.Add(expires)
            };

            var url = _s3.GetPreSignedURL(req);
            return Task.FromResult(url);
        }

        // 從 key 或 URL 取得真正的 key（移除 serviceUrl 與 bucket 前置）
        private string GetKeyFromKeyOrUrl(string keyOrUrl)
        {
            if (keyOrUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || keyOrUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrEmpty(_serviceUrl) && keyOrUrl.StartsWith(_serviceUrl, StringComparison.OrdinalIgnoreCase))
                {
                    // 移除 serviceUrl + /bucket/
                    var prefix = _serviceUrl.TrimEnd('/') + "/" + _bucket + "/";
                    if (keyOrUrl.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    {
                        return Uri.UnescapeDataString(keyOrUrl.Substring(prefix.Length));
                    }
                }

                // fallback: 解析 URL 路徑當成 key
                var uri = new Uri(keyOrUrl);
                return Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/'));
            }

            return keyOrUrl;
        }
    }
}
