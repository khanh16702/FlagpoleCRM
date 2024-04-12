using MongoDB.Bson;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace Common.Helper
{
    public class MongoDbHelper<T> where T : class
    {
        private readonly IMongoCollection<T> _collection;

        public MongoDbHelper(string connectionString, string databaseName, string collectionName)
        {
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName);
            _collection = database.GetCollection<T>(collectionName);
        }

        public async Task<List<T>> GetAll()
        {
            var result = await _collection.FindAsync(FilterDefinition<T>.Empty);
            return await result.ToListAsync();
        }

        public async Task<T> GetById(string id)
        {
            var filter = Builders<T>.Filter.Eq("_id", id);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<List<T>> Find(Expression<Func<T, bool>> filter)
        {
            var result = await _collection.FindAsync(filter);
            return await result.ToListAsync();
        }

        public async Task<List<T>> GetListSorted(SortDefinition<T> sortDefinition)
        {
            var result = await _collection.Find(FilterDefinition<T>.Empty)
                                         .Sort(sortDefinition)
                                         .ToListAsync();
            return result;
        }

        public async Task InsertAsync(T document)
        {
            await _collection.InsertOneAsync(document);
        }

        public void Insert(T document)
        {
            _collection.InsertOne(document);
        }

        public async Task UpdateAsync(ObjectId id, T document)
        {
            var filter = Builders<T>.Filter.Eq("_id", id);
            await _collection.ReplaceOneAsync(filter, document);
        }

        public void Update(ObjectId id, T document)
        {
            var filter = Builders<T>.Filter.Eq("_id", id);
            _collection.ReplaceOne(filter, document);
        }

        public async Task DeleteAsync(ObjectId id)
        {
            var filter = Builders<T>.Filter.Eq("_id", id);
            await _collection.DeleteOneAsync(filter);
        }
        
        public void Delete(ObjectId id)
        {
            var filter = Builders<T>.Filter.Eq("_id", id);
            _collection.DeleteOne(filter);
        }

        public async Task<long> CountDocumentsAsync()
        {
            return await _collection.CountDocumentsAsync(FilterDefinition<T>.Empty);
        }
    }
}
