using Newtonsoft.Json;
using System.Text.Json.Nodes;
using System.Text;
using Prism.Events;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.IO;

namespace DBManager
{
    public class DbManagerService
    {

        private string URL = "https://initdb.azurewebsites.net/api/";
        private string code = "?code=e3hxmCUEwdgLGJbM_fJWQvXogM5j6MimlvIo34lzAVruAzFu-cX2QQ%3D%3D";

        private static string ConnectionString = "DefaultEndpointsProtocol=https;AccountName=testingfirstazuredemo;AccountKey=O3/UjkVlO63CK8HnuuCTp+TldwPjexDUhwYzXYdEEQfDBjKuc0cLginUXVhaMbiO0SLxupMkMgx6+ASt5lgfmg==;EndpointSuffix=core.windows.net";
        private static CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConnectionString);
        private static CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
        private static CloudBlobContainer container = blobClient.GetContainerReference("images");
        public DbManagerService(IEventAggregator ea)
        {

        }



        public async Task SaveImageToBlob(string path)
        {
            var picture = Path.GetFileName(path);
            await container.CreateIfNotExistsAsync();
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(picture);
            using (var fileStream = File.OpenRead(path))
            {
                await blockBlob.UploadFromStreamAsync(fileStream);
            }
        }

        public async Task DownloadPicture(string picture)
        {
            await container.CreateIfNotExistsAsync();
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(picture);
            try
            {
                await blockBlob.DownloadToFileAsync(picture, FileMode.CreateNew);
            }
            catch (Exception) { }
        }

        public async Task DeleteFromBlob(string picture)
        {
            await container.CreateIfNotExistsAsync();
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(picture);
            await blockBlob.DeleteIfExistsAsync();
        }


        public async Task<int> Insert<T>(T model)
        {
            var operation = "insertIntoDB";
            var type = typeof(T);
            var jsonObject = JsonConvert.SerializeObject(model);
            var content = new StringContent(jsonObject.ToString(), Encoding.UTF8, "application/json");
            var postLink = URL + operation + type.Name + code;
            using (HttpClient client = new HttpClient())
            {
                var messageResponse = await client.PostAsync(postLink, content);
                messageResponse.EnsureSuccessStatusCode();
                if (messageResponse.IsSuccessStatusCode)
                {
                    try
                    {
                        var response = await messageResponse.Content.ReadAsStringAsync();
                        int id = JsonConvert.DeserializeObject<int>(response);
                        return id;
                    }
                    catch (Exception ex)
                    {
                        return -1;
                    }
                }
            }
            return -1;
        }

        public async Task<List<T>> GetAll<T>()
        {
            var operation = "getAll";
            var type = typeof(T);
            var postLink = URL + operation + type.Name + code;

            using (HttpClient client = new HttpClient())
            {

                var messageResponse = await client.GetAsync(postLink);
                messageResponse.EnsureSuccessStatusCode();
                if (messageResponse.IsSuccessStatusCode)
                {
                    try
                    {
                        var json = await messageResponse.Content.ReadAsStringAsync();
                        var lista = JsonConvert.DeserializeObject<List<T>>(json);
                        return lista;
                    }
                    catch (Exception ex)
                    {
                        return null;
                    }
                }
                return null;
            }
        }


        public async Task<List<T>> GetFiltered<T>(string columnName, string value)
        {
            var operation = "filteredGet";
            var type = typeof(T);
            var postLink = URL + operation + type.Name + code;
            using (HttpClient client = new HttpClient())
            {
                var messageResponse = await client.GetAsync(postLink + $"&columnName={columnName}&columnValue={value}");
                messageResponse.EnsureSuccessStatusCode();
                if (messageResponse.IsSuccessStatusCode)
                {
                    try
                    {
                        var json = await messageResponse.Content.ReadAsStringAsync();
                        var lista = JsonConvert.DeserializeObject<List<T>>(json);
                        return lista;
                    }
                    catch (Exception ex)
                    {
                        return null;
                    }
                }
                return null;
            }
        }

        public async Task<bool> Update<T>(T model)
        {
            var operation = "update";
            var type = typeof(T);
            var jsonObject = JsonConvert.SerializeObject(model);
            var postLink = URL + operation + type.Name + code;

            var content = new StringContent(jsonObject.ToString(), Encoding.UTF8, "application/json");
            using (HttpClient client = new HttpClient())
            {
                var messageResponse = await client.PostAsync(postLink, content);
                messageResponse.EnsureSuccessStatusCode();
                if (messageResponse.IsSuccessStatusCode)
                {
                    try
                    {
                        return true;
                    }
                    catch (Exception ex)
                    {
                        return false;
                    }
                }
            }
            return false;
        }

        public async Task<bool> Delete<T>(T model)
        {
            var operation = "delete";
            var type = typeof(T);
            var jsonObject = JsonConvert.SerializeObject(model);
            var content = new StringContent(jsonObject.ToString(), Encoding.UTF8, "application/json");
            var postLink = URL + operation + type.Name + code;

            using (HttpClient client = new HttpClient())
            {
                var messageResponse = await client.PostAsync(postLink, content);
                messageResponse.EnsureSuccessStatusCode();
                if (messageResponse.IsSuccessStatusCode)
                {
                    try
                    {
                        return true;
                    }
                    catch (Exception ex)
                    {
                        return false;
                    }
                }

            }
            return false;
        }
    }
}