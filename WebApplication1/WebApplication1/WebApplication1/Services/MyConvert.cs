using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace WebApplication1.Services
{
    public class MyConvert
    {
        public static  byte[] ConvertFileToByteArray(IFormFile file)
        {
            using var memoryStream = new MemoryStream();
            file.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }
    }
}
