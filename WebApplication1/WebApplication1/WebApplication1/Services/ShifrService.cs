using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace WebApplication1.Services
{
    public class ShifrService
    {
        private static int shift = 25;

        public static string HashPassword(string inputPassword)
        {
            string result = "";

            foreach (char c in inputPassword)
            {
                int charIndex = Convert.ToInt32(c) + shift;
                charIndex = charIndex > char.MaxValue ? charIndex - char.MaxValue : charIndex;
                charIndex = charIndex < char.MinValue ? charIndex + char.MaxValue : charIndex;
                result += Convert.ToChar(charIndex);
            }
            return result;
        }

        public static bool DeHashPassword(string serverPassword, string inputPassword)
        {
            string result = "";
            foreach (char c in inputPassword)
            {
                int charIndex = Convert.ToInt32(c) +shift;
                charIndex = charIndex > char.MaxValue ? charIndex - char.MaxValue : charIndex;
                charIndex = charIndex < char.MinValue ? charIndex + char.MaxValue : charIndex;
                result += Convert.ToChar(charIndex);
            }
            return serverPassword== result;
        }
    }
}
