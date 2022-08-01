using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlfaMap.Common
{
    public static class StringUtils
    {
        public static async Task<string> ConvertoToZipBase64(string value)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value);

            using (MemoryStream input = new MemoryStream(bytes))
            using (MemoryStream output = new MemoryStream())
            {
                
                using (var zipStream = new GZipStream(output, CompressionMode.Compress))
                {
                    await input.CopyToAsync(zipStream);
                }
                
                return Convert.ToBase64String(output.ToArray());
            }
        }

        public static async Task<string> FromZipBase64(string value)
        {
            byte[] bytes = Convert.FromBase64String(value);

            using (MemoryStream input = new MemoryStream(bytes))
            using (MemoryStream output = new MemoryStream())
            {
                using (var zipStream = new GZipStream(input, CompressionMode.Decompress))
                {
                    await zipStream.CopyToAsync(output);
                }

                return Encoding.UTF8.GetString(output.ToArray());
            }
        }
    }
}
