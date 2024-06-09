﻿using Newtonsoft.Json;
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
        private Dictionary<string, string> queryDictionary = new Dictionary<string, string>
        {
            { "InsertUser","https://initdb.azurewebsites.net/api/insertIntoDBUser?code=TBbHph8oLYfvtS4bT_-DKEan1sWi0LjPDp7H9USwD8bxAzFu_ez-DQ%3D%3D" },
            { "GetAllUser","https://initdb.azurewebsites.net/api/getAllUser?code=TBbHph8oLYfvtS4bT_-DKEan1sWi0LjPDp7H9USwD8bxAzFu_ez-DQ%3D%3D" },
            { "GetFilteredUser","https://initdb.azurewebsites.net/api/filteredGetUser?code=TBbHph8oLYfvtS4bT_-DKEan1sWi0LjPDp7H9USwD8bxAzFu_ez-DQ%3D%3D" },
            { "DeleteUser","https://initdb.azurewebsites.net/api/deleteUser?code=TBbHph8oLYfvtS4bT_-DKEan1sWi0LjPDp7H9USwD8bxAzFu_ez-DQ%3D%3D" },
            { "UpdateUser","https://initdb.azurewebsites.net/api/updateUser?code=TBbHph8oLYfvtS4bT_-DKEan1sWi0LjPDp7H9USwD8bxAzFu_ez-DQ%3D%3D" },

            { "InsertHouse","https://initdb.azurewebsites.net/api/insertIntoDBHouse?code=TBbHph8oLYfvtS4bT_-DKEan1sWi0LjPDp7H9USwD8bxAzFu_ez-DQ%3D%3D" },
            { "GetAllHouse","https://initdb.azurewebsites.net/api/getAllHouse?code=TBbHph8oLYfvtS4bT_-DKEan1sWi0LjPDp7H9USwD8bxAzFu_ez-DQ%3D%3D" },
            { "GetFilteredHouse","https://initdb.azurewebsites.net/api/filteredGetHouse?code=TBbHph8oLYfvtS4bT_-DKEan1sWi0LjPDp7H9USwD8bxAzFu_ez-DQ%3D%3D" },
            { "DeleteHouse","https://initdb.azurewebsites.net/api/deleteHouse?code=TBbHph8oLYfvtS4bT_-DKEan1sWi0LjPDp7H9USwD8bxAzFu_ez-DQ%3D%3D" },
            { "UpdateHouse","https://initdb.azurewebsites.net/api/updateHouse?code=TBbHph8oLYfvtS4bT_-DKEan1sWi0LjPDp7H9USwD8bxAzFu_ez-DQ%3D%3D" },

            { "InsertFurniture","https://initdb.azurewebsites.net/api/insertIntoDBFurniture?code=TBbHph8oLYfvtS4bT_-DKEan1sWi0LjPDp7H9USwD8bxAzFu_ez-DQ%3D%3D" },
            { "GetAllFurniture","https://initdb.azurewebsites.net/api/getAllFurniture?code=TBbHph8oLYfvtS4bT_-DKEan1sWi0LjPDp7H9USwD8bxAzFu_ez-DQ%3D%3D" },
            { "GetFilteredFurniture","https://initdb.azurewebsites.net/api/filteredGetFurniture?code=TBbHph8oLYfvtS4bT_-DKEan1sWi0LjPDp7H9USwD8bxAzFu_ez-DQ%3D%3D" },
            { "DeleteFurniture","https://initdb.azurewebsites.net/api/deleteFurniture?code=TBbHph8oLYfvtS4bT_-DKEan1sWi0LjPDp7H9USwD8bxAzFu_ez-DQ%3D%3D" },
            { "UpdateFurniture","https://initdb.azurewebsites.net/api/updateFurniture?code=TBbHph8oLYfvtS4bT_-DKEan1sWi0LjPDp7H9USwD8bxAzFu_ez-DQ%3D%3D" },

            { "InsertRoom","https://initdb.azurewebsites.net/api/insertIntoDBRoom?code=TBbHph8oLYfvtS4bT_-DKEan1sWi0LjPDp7H9USwD8bxAzFu_ez-DQ%3D%3D" },
            { "GetAllRoom","https://initdb.azurewebsites.net/api/getAllRoom?code=TBbHph8oLYfvtS4bT_-DKEan1sWi0LjPDp7H9USwD8bxAzFu_ez-DQ%3D%3D" },
            { "GetFilteredRoom","https://initdb.azurewebsites.net/api/filteredGetRoom?code=TBbHph8oLYfvtS4bT_-DKEan1sWi0LjPDp7H9USwD8bxAzFu_ez-DQ%3D%3D" },
            { "DeleteRoom","https://initdb.azurewebsites.net/api/deleteRoom?code=TBbHph8oLYfvtS4bT_-DKEan1sWi0LjPDp7H9USwD8bxAzFu_ez-DQ%3D%3D" },
            { "UpdateRoom","https://initdb.azurewebsites.net/api/updateRoom?code=TBbHph8oLYfvtS4bT_-DKEan1sWi0LjPDp7H9USwD8bxAzFu_ez-DQ%3D%3D" },
            
            {"InsertItem", "https://initdb.azurewebsites.net/api/insertIntoDBItem?code=e3hxmCUEwdgLGJbM_fJWQvXogM5j6MimlvIo34lzAVruAzFu-cX2QQ%3D%3D" },
            {"GetAllItem", "https://initdb.azurewebsites.net/api/getAllItem?code=e3hxmCUEwdgLGJbM_fJWQvXogM5j6MimlvIo34lzAVruAzFu-cX2QQ%3D%3D" },
            {"GetFilteredItem", "https://initdb.azurewebsites.net/api/filteredGetItem?code=e3hxmCUEwdgLGJbM_fJWQvXogM5j6MimlvIo34lzAVruAzFu-cX2QQ%3D%3D" },
            {"DeleteItem", "https://initdb.azurewebsites.net/api/deleteItem?code=e3hxmCUEwdgLGJbM_fJWQvXogM5j6MimlvIo34lzAVruAzFu-cX2QQ%3D%3D" },
            {"UpdateItem", "https://initdb.azurewebsites.net/api/updateItem?code=e3hxmCUEwdgLGJbM_fJWQvXogM5j6MimlvIo34lzAVruAzFu-cX2QQ%3D%3D" }
        };


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
            var operation = "Insert";
            var type = typeof(T);
            var dexOperation = operation + type.Name;
            var jsonObject = JsonConvert.SerializeObject(model);
            var content = new StringContent(jsonObject.ToString(), Encoding.UTF8, "application/json");
            var postLink = URL + dexOperation + code;
            using (HttpClient client = new HttpClient())
            {
                var tot = await client.PostAsync(queryDictionary[dexOperation], content);
                tot.EnsureSuccessStatusCode();
                if (tot.IsSuccessStatusCode)
                {
                    try
                    {
                        var response = await tot.Content.ReadAsStringAsync();
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
            var operation = "GetAll";
            var type = typeof(T);
            var dexOperation = operation + type.Name;
            using (HttpClient client = new HttpClient())
            {

                var tot = await client.GetAsync(queryDictionary[dexOperation]);
                tot.EnsureSuccessStatusCode();
                if (tot.IsSuccessStatusCode)
                {
                    try
                    {
                        var json = await tot.Content.ReadAsStringAsync();
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
            var operation = "GetFiltered";
            var type = typeof(T);
            var dexOperation = operation + type.Name;
            //columnName = EnsureCorrectColumnFormat(columnName);
            using (HttpClient client = new HttpClient())
            {
                var tot = await client.GetAsync(queryDictionary[dexOperation] + $"&columnName={columnName}&columnValue={value}");
                tot.EnsureSuccessStatusCode();
                if (tot.IsSuccessStatusCode)
                {
                    try
                    {
                        var json = await tot.Content.ReadAsStringAsync();
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
            var operation = "Update";
            var type = typeof(T);
            var dexOperation = operation + type.Name;
            var jsonObject = JsonConvert.SerializeObject(model);
            var content = new StringContent(jsonObject.ToString(), Encoding.UTF8, "application/json");
            using (HttpClient client = new HttpClient())
            {
                var tot = await client.PostAsync(queryDictionary[dexOperation], content);
                tot.EnsureSuccessStatusCode();
                if (tot.IsSuccessStatusCode)
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
            var operation = "Delete";
            var type = typeof(T);
            var dexOperation = operation + type.Name;
            var jsonObject = JsonConvert.SerializeObject(model);
            var content = new StringContent(jsonObject.ToString(), Encoding.UTF8, "application/json");
            using (HttpClient client = new HttpClient())
            {
                var tot = await client.PostAsync(queryDictionary[dexOperation], content);
                tot.EnsureSuccessStatusCode();
                if (tot.IsSuccessStatusCode)
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