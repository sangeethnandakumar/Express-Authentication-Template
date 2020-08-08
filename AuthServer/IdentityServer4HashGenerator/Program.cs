using System;
using System.Security.Cryptography;
using System.Text;

namespace IdentityServer4HashGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("IDENTITY SERVER 4 - Password Hashing Utility");
            Console.WriteLine("--------------------------------------------");
            Console.WriteLine("");
            Console.Write("Enter password : ");
            var password = Console.ReadLine();
            var result = Sha256(password);
            Console.WriteLine("");
            Console.WriteLine("--------------------------------------------");
            Console.WriteLine("SHA256 Hashed And Encoded Password is - ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(result);
            Console.Read();
        }

        public static string Sha256(string input)
        {
            using (var sha = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(input);
                var hash = sha.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
    }
}
