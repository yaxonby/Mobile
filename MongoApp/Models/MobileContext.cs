using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
 
namespace MongoApp.Models
{
    public class MobileContext
    { 
        IMongoDatabase database; // база данных
        IGridFSBucket gridFS;   // файловое хранилище
 
        public MobileContext()
        {
            // строка подключения
            string connectionString = "mongodb://localhost:27017/mobilestore";
            var connection = new MongoUrlBuilder(connectionString);
            // получаем клиента для взаимодействия с базой данных
            MongoClient client = new MongoClient(connectionString);
            // получаем доступ к самой базе данных
            database = client.GetDatabase(connection.DatabaseName);
            // получаем доступ к файловому хранилищу
            gridFS = new GridFSBucket(database);
        }
        // обращаемся к коллекции Phones
        private IMongoCollection<Phone> Phones
        {
            get { return database.GetCollection<Phone>("Phones"); }
        }
        // получаем все документы, используя критерии фальтрации
        public async Task<IEnumerable<Phone>> GetPhones(int? minPrice, int? maxPrice, string name)
        {
            // строитель фильтров
            var builder = new FilterDefinitionBuilder<Phone>();
            var filter = builder.Empty; // фильтр для выборки всех документов
            // фильтр по имени
            if (!String.IsNullOrWhiteSpace(name))
            {
                filter = filter & builder.Regex("Name", new BsonRegularExpression(name));
            }
            if (minPrice.HasValue)  // фильтр по минимальной цене
            {
                filter = filter & builder.Gte("Price", minPrice.Value);
            }
            if (maxPrice.HasValue)  // фильтр по максимальной цене
            {
                filter = filter & builder.Lte("Price", maxPrice.Value);
            }
             
            return await Phones.Find(filter).ToListAsync();
        }
 
        // получаем один документ по id
        public async Task<Phone> GetPhone(string id)
        {
            return await Phones.Find(new BsonDocument("_id", new ObjectId(id))).FirstOrDefaultAsync();
        }
        // добавление документа
        public async Task Create(Phone p)
        {
            await Phones.InsertOneAsync(p);
        }
        // обновление документа
        public async Task Update(Phone p)
        {
            await Phones.ReplaceOneAsync(new BsonDocument("_id", new ObjectId(p.Id)), p);
        }
        // удаление документа
        public async Task Remove(string id)
        {
            await Phones.DeleteOneAsync(new BsonDocument("_id", new ObjectId(id)));
        }
        // получение изображения
        public async Task<byte[]> GetImage(string id)
        {
            return await gridFS.DownloadAsBytesAsync(new ObjectId(id));
        }
        // сохранение изображения
        public async Task StoreImage(string id, Stream imageStream, string imageName)
        {
            Phone p = await GetPhone(id);
            if (p.HasImage())
            {
                // если ранее уже была прикреплена картинка, удаляем ее
                await gridFS.DeleteAsync(new ObjectId(p.ImageId));
            }
            // сохраняем изображение
            ObjectId imageId = await gridFS.UploadFromStreamAsync(imageName, imageStream);
            // обновляем данные по документу
            p.ImageId = imageId.ToString();
            var filter = Builders<Phone>.Filter.Eq("_id", new ObjectId(p.Id));
            var update = Builders<Phone>.Update.Set("ImageId", p.ImageId);
            await Phones.UpdateOneAsync(filter, update);
        }
    }
}