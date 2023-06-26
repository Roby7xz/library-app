using LibraryApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Win32;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace LibraryApp.Helpers
{
    public class FileHelper
    {

        private static IWebHostEnvironment _webHostEnv = new HttpContextAccessor().HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();
        public static string UploadFile(string folderPath, IFormFile file)
        {
           
            string relativeFilePath = null;

            if (file == null)
            {
                return relativeFilePath; 
            }

            string uploadFolder = Path.GetFullPath(folderPath, _webHostEnv.WebRootPath);
            string fileName = string.Join("-", DateTime.Now.ToString("yyyyMMddHHmmss"), Path.GetFileName(file.FileName));

            try
            {
                var fullFilePath = Path.Combine(uploadFolder, fileName);
                using (var fileStream = new FileStream(fullFilePath, FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }

                relativeFilePath = Path.Combine(Path.DirectorySeparatorChar.ToString(), Path.GetRelativePath(_webHostEnv.WebRootPath, fullFilePath));
            }

            catch (Exception error)
            {
                Console.WriteLine("Error uploading book image: " + error.Message);
            }

            return relativeFilePath;
        }

        public static void DeleteFile(string relativeFilePath)
        {
            string filePath = Path.GetFullPath(relativeFilePath.TrimStart(Path.DirectorySeparatorChar), _webHostEnv.WebRootPath);

            if (!File.Exists(filePath))
            {
                return;
            }

            File.Delete(filePath);
        }

        public static bool IsContentTypeValid(IFormFile file)
        {
            string[] validImageTypes = { 
                "image/png", "image/jpg", 
                "image/jpeg", "image/gif", 
                "image/svg", "image/webp", 
                "image/bmp", "image/gif", 
                "image/ico", "image/tif",
                "image/tiff", null 
            };

            if(file != null)
            {
                if (!validImageTypes.Contains(file.ContentType))
                {
                    return false;
                }
            }
            
            return true;
        }
    }
}
