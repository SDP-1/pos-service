using System.Threading.Tasks;

namespace pos_service.Services
{
    /// <summary>
    /// Service interface for rendering HTML pages into PDF binary documents.
    /// </summary>
    public interface IPdfService
    {
        /// <summary>
        /// Converts a raw HTML string into a PDF document file stream.
        /// </summary>
        /// <param name="htmlContent">The raw HTML content to convert.</param>
        /// <returns>A byte array containing the generated PDF document binary data.</returns>
        Task<byte[]> ConvertHtmlToPdfAsync(string htmlContent);
    }
}
