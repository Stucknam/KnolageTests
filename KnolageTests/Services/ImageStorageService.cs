using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace KnolageTests.Services
{
    public class ImageStorageService
    {
        private readonly string _folder = FileSystem.AppDataDirectory;
        public void DeleteImageIfExists(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        public string CopyToAppFolder(string sourcePath)
        {
            var extention = Path.GetExtension(sourcePath);
            var fileName = $"{Guid.NewGuid}{extention}";
            var destinationPath = Path.Combine(_folder, fileName);

            File.Copy(sourcePath, destinationPath, overwrite: true );
            return destinationPath;
        }

    }
}
