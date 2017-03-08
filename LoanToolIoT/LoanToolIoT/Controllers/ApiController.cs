using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LoanToolIoT.Controllers
{
    public class ApiController
    {
        private string API_KEY = "98955B3577FD4642C68A84913F73E732";
        private string API_SECRET = "ED3C7856E63ED77FA9FF8D8F82B12B8F";
        public ApiErrorResponse ValidateAPI(string token, string apikey, string url)
        {
            if (apikey != API_KEY)
            {
                return new ApiErrorResponse { Code = 1, Reason = "Invalid API Key" };
            }
            var totalMd5 = string.Format("{0}-{1}-{2}", API_KEY, url, API_SECRET);
            MD5 hash = MD5.Create();
            var computedHash = GetMd5Hash(hash, totalMd5);
            if (computedHash != token)
                return new ApiErrorResponse { Code = 2, Reason = "Invalid Token" };
            return new ApiErrorResponse { Code = 0, Reason = "OK" };
        }
        static string GetMd5Hash(MD5 md5Hash, string input)
        {

            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }
    }
    public sealed class ApiErrorResponse
    {
        public int Code { get; set; }
        public string Reason { get; set; }
    }
}
