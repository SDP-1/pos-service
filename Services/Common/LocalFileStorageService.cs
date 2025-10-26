namespace pos_service.Services.Common
{
    public class LocalFileStorageService : IFileStorageService
    {
        // IWebHostEnvironment is required to get the root path of the application
        private readonly IWebHostEnvironment _env;

        public LocalFileStorageService(IWebHostEnvironment env)
        {
            _env = env;
        }

        private const string BaseFolder = "uploads";

        public async Task<string> SaveFileAsync(IFormFile file, string subPath)
        {
            if (file == null || file.Length == 0) return string.Empty;

            // 1. Define folder path
            var folderPath = Path.Combine(_env.WebRootPath, BaseFolder, subPath);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            // 2. Generate a unique file name
            var extension = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(folderPath, fileName);

            // 3. Save the file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // 4. Return the relative path/URL to save in the database
            // e.g., /uploads/users/profiles/unique-id.jpg
            return $"/{BaseFolder}/{subPath}/{fileName}";
        }

        public async Task<string> CopyAndSaveFileAsync(string sourceFilePath, string subPath)
        {
            if (string.IsNullOrEmpty(sourceFilePath)) return string.Empty;

            // 1. Validate source file existence and security (CRITICAL!)
            if (!File.Exists(sourceFilePath))
            {
                // Throw an exception or handle the error
                throw new FileNotFoundException("Source file not found at the provided path.", sourceFilePath);
            }

            // IMPORTANT SECURITY NOTE: You should ideally check that the sourceFilePath 
            // is within an allowed directory structure to prevent path traversal attacks.

            // 2. Define target path
            var baseFolder = "uploads";
            var folderPath = Path.Combine(_env.WebRootPath, baseFolder, subPath);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            // 3. Generate unique file name and final path
            var extension = Path.GetExtension(sourceFilePath);
            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(folderPath, fileName);

            // 4. Copy the file (synchronous operation often fine for file copies)
            // NOTE: Copying is blocking, but file I/O is often done this way. 
            // For large files, you might stream it asynchronously.
            await Task.Run(() => File.Copy(sourceFilePath, filePath, overwrite: true));

            // 5. Return the relative path/URL
            return $"/{baseFolder}/{subPath}/{fileName}";
        }

        public void DeleteFile(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath)) return;

            // Ensure the relativePath starts with the BaseFolder to prevent deleting system files
            if (relativePath.StartsWith($"/{BaseFolder}"))
            {
                var fullPath = Path.Combine(_env.WebRootPath, relativePath.TrimStart('/'));
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
            }
        }
    }
}
