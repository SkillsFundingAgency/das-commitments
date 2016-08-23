using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SFA.DAS.ProviderApprenticeshipsService.Infrastructure.Data
{
    public abstract class FileSystemRepository
    {
        protected readonly string Directory;

        protected FileSystemRepository(string appDataFolderName)
        {
            var appData = !string.IsNullOrEmpty((string)AppDomain.CurrentDomain.GetData("DataDirectory"))
                ? (string)AppDomain.CurrentDomain.GetData("DataDirectory")
                : Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "App_Data");

            Directory = Path.Combine(appData, appDataFolderName);
        }

        protected string[] GetDataFiles()
        {
            if (!System.IO.Directory.Exists(Directory))
            {
                return new string[0];
            }
            return System.IO.Directory.GetFiles(Directory, "*.json");
        }

        protected async Task<T> ReadFileById<T>(string id)
        {
            var path = Path.Combine(Directory, id + ".json");
            return await ReadFile<T>(path);
        }
        protected async Task<T> ReadFile<T>(string path)
        {
            if (!File.Exists(path))
            {
                return default(T);
            }

            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            using (var reader = new StreamReader(stream))
            {
                var json = await reader.ReadToEndAsync();
                reader.Close();

                return JsonConvert.DeserializeObject<T>(json);
            }
        }

        protected async Task CreateFile<T>(T item, string id)
        {
            if (!System.IO.Directory.Exists(Directory))
            {
                System.IO.Directory.CreateDirectory(Directory);
            }

            var path = Path.Combine(Directory, id + ".json");
            using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write))
            using (var writer = new StreamWriter(stream))
            {
                var json = JsonConvert.SerializeObject(item, Formatting.Indented);
                await writer.WriteAsync(json);
                await writer.FlushAsync();
                writer.Close();
            }
        }
    }
}