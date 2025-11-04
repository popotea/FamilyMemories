using System;
using System.IO;
using System.Threading.Tasks;

namespace FamilyMemories.Services
{
    /// <summary>
    /// 檔案儲存抽象介面 (Storage Abstraction)
    ///
    /// 說明：
    /// 這個介面用於抽離實際的檔案儲存實作（本地磁碟、Cloudflare R2、S3 或其他雲端儲存）。
    /// 在程式其他地方（例如上傳、刪除、產生預簽名 URL）都透過這個介面呼叫，便於未來更換儲存後端。
    ///
    /// 使用建議：
    /// - 在本機開發可不設定 Cloudflare，會回落到本地 `wwwroot/uploads`。
    /// - 在生產環境請使用環境變數或 Secret 管理解敏感金鑰，並在 `appsettings` 只放非敏感設定。
    /// </summary>
    public interface IStorageService
    {
    /// <summary>
    /// 上傳一個資料流並回傳公開可存取的 URL（或物件識別鍵）
    /// </summary>
        Task<string> UploadAsync(Stream content, string key, string contentType);

    /// <summary>
    /// 刪除指定的物件，可接受 key 或完整 URL。若物件不存在也視為成功（回傳 true）。
    /// </summary>
        Task<bool> DeleteAsync(string keyOrUrl);

    /// <summary>
    /// 取得給定 key 的公開 URL（若儲存為私有，請改用 GeneratePresignedUrlAsync）。
    /// </summary>
        string GetUrl(string key);

        /// <summary>
        /// 為私有物件產生預簽名 URL，通常用於短期授權存取。
        /// </summary>
        Task<string> GeneratePresignedUrlAsync(string key, TimeSpan expires);
    }
}
