using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System.IO;
using Newtonsoft.Json;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Net.Http;
using Azure.Storage.Blobs;
using Azure.Storage;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Linq;
using WpfApp1;

namespace FunctionApp1
{
    public static class Functions
    {
        #region --CONNECTION STRINGS--

        private static string url = "mongodb://azure-db-test1:X22wlg8tHkuQBlswPqiASt9VaP0XM2I7Uo0EiobpVF4zzy8lkGvLEzZWgEREZjLA0hSrbqQNLVa2ACDbfnQ5Gw==@azure-db-test1.mongo.cosmos.azure.com:10255/?ssl=true&replicaSet=globaldb&retrywrites=false&maxIdleTimeMS=120000&appName=@azure-db-test1@";
        private static MongoClient dbClient = new MongoClient(url);
        #endregion

        #region --USER--

        [FunctionName("InsertUser")]
        public static async Task<long> InsertUser(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "insertIntoDBUser")] HttpRequestMessage req,
            ILogger log)
        {
            var db = dbClient.GetDatabase("HousePlanner");
            var data = await req.Content.ReadAsAsync<User>();
            var collection = db.GetCollection<User>("User");
            long lastId = 0;
            if (collection.AsQueryable().Count() != 0)
            {
                try
                {
                    var orderedCollection = collection.AsQueryable().OrderByDescending(u => u.Id);
                    var id = orderedCollection.First().Id + 1;
                    lastId = id;
                }
                catch (Exception ex)
                {
                    log.LogError(ex.ToString());
                    return -1;
                }
            }
            data.Id = lastId;
            collection.InsertOne(data);
            log.LogInformation($"Inserted {data} from HousePlanner DB");
            return lastId;
        }


        [FunctionName("GetAllUser")]
        public static async Task<List<User>> GetAllUser(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "getAllUser")] HttpRequest req,
            ILogger log)
        {

            var db = dbClient.GetDatabase("HousePlanner");

            var collection = db.GetCollection<User>("User");

            return collection.AsQueryable().ToList();
        }


        [FunctionName("DeleteUser")]
        public static async Task<IActionResult> DeleteUser(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "deleteUser")] HttpRequestMessage req,
            ILogger log)
        {

            var db = dbClient.GetDatabase("HousePlanner");

            var data = await req.Content.ReadAsAsync<User>();

            var collection = db.GetCollection<User>("User");

            var filter = Builders<User>.Filter.Eq("Id", data.Id);


            collection.DeleteOne(filter);

            log.LogInformation($"Deleted {data} from HousePlanner DB");

            return new OkResult();
        }


        [FunctionName("FilteredGetUser")]
        public static async Task<List<User>> FilteredGetUser(
         [HttpTrigger(AuthorizationLevel.Function, "get", Route = "filteredGetUser")] HttpRequest req,
         ILogger log)
        {
            string columnName = req.Query["columnName"];
            string columnValue = req.Query["columnValue"];

            FilterDefinition<User> filter = Builders<User>.Filter.Eq(columnName, columnValue);

            var db = dbClient.GetDatabase("HousePlanner");

            var collection = db.GetCollection<User>("User");

            var filteredData = await collection.Find(filter).ToListAsync();

            return filteredData;
        }

        [FunctionName("UpdateUser")]
        public static async Task<IActionResult> UpdateUser(
          [HttpTrigger(AuthorizationLevel.Function, "post", Route = "updateUser")] HttpRequestMessage req,
          ILogger log)
        {

            var db = dbClient.GetDatabase("HousePlanner");
            var data = await req.Content.ReadAsAsync<User>();
            var collection = db.GetCollection<User>("User");
            var filter = Builders<User>.Filter.Eq("Id", data.Id);
            UpdateDefinition<User> update = Builders<User>.Update
                .Set(u => u.Email, data.Email)
                .Set(u => u.Password, data.Password);
            collection.UpdateOne(filter, update);
            log.LogInformation($"Updated user with Id {data.Id} from HousePlanner DB");
            return new OkResult();
        }


        #endregion

        #region --ROOM--

        [FunctionName("InsertRoom")]
        public static async Task<long> InsertRoom(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "insertIntoDBRoom")] HttpRequestMessage req,
            ILogger log)
        {

            var db = dbClient.GetDatabase("HousePlanner");

            var data = await req.Content.ReadAsAsync<Room>();

            var collection = db.GetCollection<Room>("Room");

            long lastId = 0;


            if (collection.AsQueryable().Count() != 0)
            {
                try
                {
                    var orderedCollection = collection.AsQueryable().OrderByDescending(u => u.Id);
                    var id = orderedCollection.First().Id + 1;
                    lastId = id;
                }
                catch (Exception ex)
                {
                    log.LogError(ex.ToString());
                    return -1;
                }
            }

            data.Id = lastId;

            collection.InsertOne(data);

            log.LogInformation($"Inserted {data} from HousePlanner DB");


            return lastId;
        }


        [FunctionName("GetAllRoom")]
        public static async Task<List<Room>> GetAllRoom(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "getAllRoom")] HttpRequest req,
            ILogger log)
        {

            List<Room> list = new List<Room>();

            var db = dbClient.GetDatabase("HousePlanner");

            var collection = db.GetCollection<Room>("Room");

            return collection.AsQueryable().ToList();
        }


        [FunctionName("DeleteRoom")]
        public static async Task<IActionResult> DeleteRoom(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "deleteRoom")] HttpRequestMessage req,
            ILogger log)
        {

            var db = dbClient.GetDatabase("HousePlanner");

            var data = await req.Content.ReadAsAsync<Room>();

            var collection = db.GetCollection<Room>("Room");

            var filter = Builders<Room>.Filter.Eq("Id", data.Id);


            collection.DeleteOne(filter);

            log.LogInformation($"Deleted {data} from HousePlanner DB");

            return new OkResult();
        }


        [FunctionName("FilteredGetRoom")]
        public static async Task<List<Room>> GetFiltered(
         [HttpTrigger(AuthorizationLevel.Function, "get", Route = "filteredGetRoom")] HttpRequest req,
         ILogger log)
        {
            string columnName = req.Query["columnName"];
            string columnValue = req.Query["columnValue"];

            FilterDefinition<Room> filter = Builders<Room>.Filter.Eq(columnName, columnValue);

            var db = dbClient.GetDatabase("HousePlanner");

            var collection = db.GetCollection<Room>("Room");

            // Retrieve filtered data
            var filteredData = await collection.Find(filter).ToListAsync();

            return filteredData;
        }

        [FunctionName("UpdateRoom")]
        public static async Task<IActionResult> UpdateRoom(
          [HttpTrigger(AuthorizationLevel.Function, "post", Route = "updateRoom")] HttpRequestMessage req,
          ILogger log)
        {

            var db = dbClient.GetDatabase("HousePlanner");

            var data = await req.Content.ReadAsAsync<Room>();

            var collection = db.GetCollection<Room>("Room");

            var filter = Builders<Room>.Filter.Eq("Id", data.Id);

            UpdateDefinition<Room> update = Builders<Room>.Update
                .Set(r => r.Name, data.Name)
                .Set(r => r.HouseId, data.HouseId)
                .Set(r => r.Width, data.Width)
                .Set(r => r.Length, data.Length)
                .Set(r => r.Floor, data.Floor)
                .Set(r => r.PositionInHouse, data.PositionInHouse);

            collection.UpdateOne(filter, update);

            log.LogInformation($"Updated Room with Id {data.Id} from HousePlanner DB");

            return new OkResult();
        }


        #endregion

        #region --FURNITURE--

        [FunctionName("InsertFurniture")]
        public static async Task<long> InsertFurniture(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "insertIntoDBFurniture")] HttpRequestMessage req,
            ILogger log)
        {

            var db = dbClient.GetDatabase("HousePlanner");

            var data = await req.Content.ReadAsAsync<Furniture>();

            var collection = db.GetCollection<Furniture>("Furniture");

            long lastId = 0;


            if (collection.AsQueryable().Count() != 0)
            {
                try
                {
                    var orderedCollection = collection.AsQueryable().OrderByDescending(u => u.Id);
                    var id = orderedCollection.First().Id + 1;
                    lastId = id;
                }
                catch (Exception ex)
                {
                    log.LogError(ex.ToString());
                    return -1;
                }
            }

            data.Id = lastId;

            collection.InsertOne(data);

            log.LogInformation($"Inserted {data} from HousePlanner DB");


            return lastId;
        }

        [FunctionName("GetAllFurniture")]
        public static async Task<List<Furniture>> GetAllFurniture(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "getAllFurniture")] HttpRequest req,
            ILogger log)
        {

            List<Furniture> list = new List<Furniture>();

            var db = dbClient.GetDatabase("HousePlanner");

            var collection = db.GetCollection<Furniture>("Furniture");

            return collection.AsQueryable().ToList();
        }


        [FunctionName("DeleteFurniture")]
        public static async Task<IActionResult> DeleteFurniture(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "deleteFurniture")] HttpRequestMessage req,
            ILogger log)
        {

            var db = dbClient.GetDatabase("HousePlanner");

            var data = await req.Content.ReadAsAsync<Furniture>();

            var collection = db.GetCollection<Furniture>("Furniture");

            var filter = Builders<Furniture>.Filter.Eq("Id", data.Id);


            collection.DeleteOne(filter);

            log.LogInformation($"Deleted {data} from HousePlanner DB");

            return new OkResult();
        }

        [FunctionName("FilteredGetFurniture")]
        public static async Task<List<Furniture>> GetFilteredFurniture(
         [HttpTrigger(AuthorizationLevel.Function, "get", Route = "filteredGetFurniture")] HttpRequest req,
         ILogger log)
        {
            string columnName = req.Query["columnName"];
            string columnValue = req.Query["columnValue"];

            FilterDefinition<Furniture> filter = Builders<Furniture>.Filter.Eq(columnName, columnValue);

            var db = dbClient.GetDatabase("HousePlanner");

            var collection = db.GetCollection<Furniture>("Furniture");

            // Retrieve filtered data
            var filteredData = await collection.Find(filter).ToListAsync();

            return filteredData;
        }

        [FunctionName("UpdateFurniture")]
        public static async Task<IActionResult> UpdateFurniture(
          [HttpTrigger(AuthorizationLevel.Function, "post", Route = "updateFurniture")] HttpRequestMessage req,
          ILogger log)
        {

            var db = dbClient.GetDatabase("HousePlanner");

            var data = await req.Content.ReadAsAsync<Furniture>();

            var collection = db.GetCollection<Furniture>("Furniture");

            var filter = Builders<Furniture>.Filter.Eq("Id", data.Id);

            UpdateDefinition<Furniture> update = Builders<Furniture>.Update
                .Set(f => f.Name, data.Name)
                .Set(f => f.RoomId, data.RoomId)
                .Set(f => f.Width, data.Width)
                .Set(f => f.Length, data.Length)
                .Set(f => f.PositionInRoom, data.PositionInRoom)
                .Set(f => f.Picture, data.Picture)
                .Set(f => f.ItemsInFurniture, data.ItemsInFurniture);


            collection.UpdateOne(filter, update);

            log.LogInformation($"Updated Furniture with Id {data.Id} from HousePlanner DB");

            return new OkResult();
        }

        #endregion

        #region --HOUSE--

        [FunctionName("InsertHouse")]
        public static async Task<long> InsertHouse(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "insertIntoDBHouse")] HttpRequestMessage req,
            ILogger log)
        {

            var db = dbClient.GetDatabase("HousePlanner");

            var data = await req.Content.ReadAsAsync<House>();

            var collection = db.GetCollection<House>("House");

            long lastId = 0;


            if (collection.AsQueryable().Count() != 0)
            {
                try
                {
                    var orderedCollection = collection.AsQueryable().OrderByDescending(u => u.Id);
                    var id = orderedCollection.First().Id + 1;
                    lastId = id;
                }
                catch (Exception ex)
                {
                    log.LogError(ex.ToString());
                    return -1;
                }
            }

            data.Id = lastId;

            collection.InsertOne(data);

            log.LogInformation($"Inserted {data} from HousePlanner DB");
            return lastId;
        }


        [FunctionName("GetAllHouse")]
        public static async Task<List<House>> GetAllHouse(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "getAllHouse")] HttpRequest req,
            ILogger log)
        {

            List<House> list = new List<House>();

            var db = dbClient.GetDatabase("HousePlanner");

            var collection = db.GetCollection<House>("House");

            return collection.AsQueryable().ToList();
        }

        [FunctionName("DeleteHouse")]
        public static async Task<IActionResult> DeleteHouse(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "deleteHouse")] HttpRequestMessage req,
            ILogger log)
        {

            var db = dbClient.GetDatabase("HousePlanner");

            var data = await req.Content.ReadAsAsync<House>();

            var collection = db.GetCollection<House>("House");

            var filter = Builders<House>.Filter.Eq("Id", data.Id);


            collection.DeleteOne(filter);

            log.LogInformation($"Deleted {data} from HousePlanner DB");

            return new OkResult();
        }

        [FunctionName("FilteredGetHouse")]
        public static async Task<List<House>> GetFilteredHouse(
         [HttpTrigger(AuthorizationLevel.Function, "get", Route = "filteredGetHouse")] HttpRequest req,
         ILogger log)
        {
            string columnName = req.Query["columnName"];
            string columnValue = req.Query["columnValue"];

            FilterDefinition<House> filter = Builders<House>.Filter.Eq(columnName, columnValue);

            var db = dbClient.GetDatabase("HousePlanner");

            var collection = db.GetCollection<House>("House");

            // Retrieve filtered data
            var filteredData = await collection.Find(filter).ToListAsync();

            return filteredData;
        }

        [FunctionName("UpdateHouse")]
        public static async Task<IActionResult> UpdateHouse(
          [HttpTrigger(AuthorizationLevel.Function, "post", Route = "updateHouse")] HttpRequestMessage req,
          ILogger log)
        {

            var db = dbClient.GetDatabase("HousePlanner");

            var data = await req.Content.ReadAsAsync<House>();

            var collection = db.GetCollection<House>("House");

            var filter = Builders<House>.Filter.Eq("Id", data.Id);

            UpdateDefinition<House> update = Builders<House>.Update
                .Set(h => h.Name, data.Name)
                .Set(h => h.NumberOfFloors, data.NumberOfFloors)
                .Set(h => h.OwnerEmail, data.OwnerEmail);

            collection.UpdateOne(filter, update);

            log.LogInformation($"Updated House with Id {data.Id} from HousePlanner DB");

            return new OkResult();
        }

        #endregion

        #region --IMAGE--


        [FunctionName("InsertItem")]
        public static async Task<long> InsertItem(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "insertIntoDBItem")] HttpRequestMessage req,
            ILogger log)
        {

            var db = dbClient.GetDatabase("HousePlanner");

            var data = await req.Content.ReadAsAsync<Item>();

            var collection = db.GetCollection<Item>("Item");

            long lastId = 0;


            if (collection.AsQueryable().Count() != 0)
            {
                try
                {
                    var orderedCollection = collection.AsQueryable().OrderByDescending(u => u.Id);
                    var id = orderedCollection.First().Id + 1;
                    lastId = id;
                }
                catch (Exception ex)
                {
                    log.LogError(ex.ToString());
                    return -1;
                }
            }

            data.Id = lastId;

            collection.InsertOne(data);

            log.LogInformation($"Inserted {data} from HousePlanner DB");
            return lastId;
        }


        [FunctionName("GetAllItem")]
        public static async Task<List<Item>> GetAllItem(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "getAllItem")] HttpRequest req,
            ILogger log)
        {

            List<Item> list = new List<Item>();

            var db = dbClient.GetDatabase("HousePlanner");

            var collection = db.GetCollection<Item>("Item");

            return collection.AsQueryable().ToList();
        }

        [FunctionName("DeleteItem")]
        public static async Task<IActionResult> DeleteItem(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "deleteItem")] HttpRequestMessage req,
            ILogger log)
        {

            var db = dbClient.GetDatabase("HousePlanner");

            var data = await req.Content.ReadAsAsync<Item>();

            var collection = db.GetCollection<Item>("Item");

            var filter = Builders<Item>.Filter.Eq("Id", data.Id);


            collection.DeleteOne(filter);

            log.LogInformation($"Deleted {data} from HousePlanner DB");

            return new OkResult();
        }

        [FunctionName("FilteredGetItem")]
        public static async Task<List<Item>> GetFilteredItem(
         [HttpTrigger(AuthorizationLevel.Function, "get", Route = "filteredGetItem")] HttpRequest req,
         ILogger log)
        {
            string columnName = req.Query["columnName"];
            string columnValue = req.Query["columnValue"];

            FilterDefinition<Item> filter = Builders<Item>.Filter.Eq(columnName, columnValue);

            var db = dbClient.GetDatabase("HousePlanner");

            var collection = db.GetCollection<Item>("Item");

            // Retrieve filtered data
            var filteredData = await collection.Find(filter).ToListAsync();

            return filteredData;
        }

        [FunctionName("UpdateItem")]
        public static async Task<IActionResult> UpdateItem(
          [HttpTrigger(AuthorizationLevel.Function, "post", Route = "updateItem")] HttpRequestMessage req,
          ILogger log)
        {

            var db = dbClient.GetDatabase("HousePlanner");

            var data = await req.Content.ReadAsAsync<Item>();

            var collection = db.GetCollection<Item>("Item");

            var filter = Builders<Item>.Filter.Eq("Id", data.Id);

            UpdateDefinition<Item> update = Builders<Item>.Update
                .Set(i => i.Name, data.Name)
                .Set(i => i.FurnitureId, data.FurnitureId);

            collection.UpdateOne(filter, update);

            log.LogInformation($"Updated Item with Id {data.Id} from HousePlanner DB");

            return new OkResult();
        }



        #endregion
    }
}
