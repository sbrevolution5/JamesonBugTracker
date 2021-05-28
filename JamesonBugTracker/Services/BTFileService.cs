using JamesonBugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace JamesonBugTracker.Services
{
    public class BTFileService : IBTFileService
    {
        public string ConvertByteArrayToFile(byte[] fileData, string extension)
        {
            if(fileData == null || extension == null)
            {
                return null;
            }
            var filestring = Convert.ToBase64String(fileData);
            //TODO allow to handle things other than images
            return $"data:image/{extension};base64,{filestring}";
        }

        public async Task<byte[]> ConvertFileToByteArrayAsync(IFormFile file)
        {
            if (file == null)
            {
                return null;
            }
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            return ms.ToArray();
        }

        public string FormatFileSize(long bytes)
        {
            throw new NotImplementedException();
        }

        public string GetFileIcon(string file)
        {
            throw new NotImplementedException();
        }
    }
}
