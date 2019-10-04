using System;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoApp.Models
{
    public class Phone
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        [Display(Name = "Модель")]
        public string Name { get; set; }
        [Display(Name = "Производитель")]
        public string Company { get; set; }
        [Display(Name = "Цена")]
        public int Price { get; set; }
 
        public string ImageId { get; set; } // ссылка на изображение
 
        public bool HasImage()
        {
            return !String.IsNullOrWhiteSpace(ImageId);
        }
    }
}