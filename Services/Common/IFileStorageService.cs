namespace pos_service.Services.Common
{
    public interface IFileStorageService
    {
        /// <summary>
        /// Saves a file to the configured file system path.
        /// </summary>
        /// <param name="file">The file content.</param>
        /// <param name="subPath">The subfolder path (e.g., "users/profiles").</param>
        /// <returns>The relative URL/path where the file was saved.</returns>
        Task<string> SaveFileAsync(IFormFile file, string subPath);

        /// <summary>
        /// Copies a file from a source path to the configured file system path.
        /// </summary>
        /// <param name="sourceFilePath">The full local path provided by the client (e.g., C:\Users\...).</param>
        /// <param name="subPath">The subfolder path where the file should be saved.</param>
        /// <returns>The relative URL/path where the file was saved.</returns>
        Task<string> CopyAndSaveFileAsync(string sourceFilePath, string subPath);

        void DeleteFile(string relativePath);
    }
}
