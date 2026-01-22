using System.Net.Sockets;

namespace pos_service.Helpers
{
    public static class NetworkPrinter
    {
        public static async Task SendAsync(string ip, int port, byte[] bytes)
        {
            using var client = new TcpClient();
            await client.ConnectAsync(ip, port);
            using var stream = client.GetStream();
            await stream.WriteAsync(bytes, 0, bytes.Length);
            await stream.FlushAsync();
        }
    }
}
