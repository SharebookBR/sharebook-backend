namespace ShareBook.Service.EBook
{
    /// <summary>
    /// Configurações de armazenamento de PDFs de e-books.
    /// UseLocalStorage = true  → salva em wwwroot/EbookPdfs (desenvolvimento)
    /// UseLocalStorage = false → envia para AWS S3 (produção)
    /// </summary>
    public class EBookStorageSettings
    {
        public bool UseLocalStorage { get; set; } = true;

        public string S3BucketName { get; set; }

        public string S3Region { get; set; }

        /// <summary>
        /// Opcional. Se vazio, usa as credenciais padrão do ambiente (IAM role,
        /// variáveis AWS_ACCESS_KEY_ID / AWS_SECRET_ACCESS_KEY, etc.)
        /// </summary>
        public string S3AccessKey { get; set; }

        /// <summary>
        /// Opcional. Par de S3AccessKey.
        /// </summary>
        public string S3SecretKey { get; set; }

        /// <summary>
        /// Duração, em minutos, da URL assinada de download.
        /// </summary>
        public int DownloadUrlExpirationMinutes { get; set; } = 5;
    }
}
