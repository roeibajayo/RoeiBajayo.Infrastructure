//using RoeiBajayo.Infrastructure.Repositires.Persistent;
//using System;
//using System.Linq;

//namespace RoeiBajayo.Infrastructure.Text.Analyzers.FrequencyAnalyzerStores
//{
//    public class JsonStore : IFrequencyAnalyzerStore
//    {
//        private readonly PersistentCollectionConnection<Tuple<string, int>> _connection;

//        public JsonStore(string path, int intervalMiliseconds = 1000) :
//            this(path, "FrequencyAnalzyer", intervalMiliseconds)
//        { }

//        public JsonStore(string path, string name, int intervalMiliseconds = 1000)
//        {
//            _connection = new JsonPersistentCollectionConnection<Tuple<string, int>>(path, 
//                name, intervalMiliseconds: intervalMiliseconds);
//        }

//        public void Clear() =>
//            _connection.Collection.Clear();

//        public void CountOne(string key)
//        {
//            var item = _connection.Collection.FirstOrDefault(x => x.Item1 == key);
//            if (item.Item1 != null) _connection.Collection.Remove(item);
//            _connection.Collection.Add(new Tuple<string, int>(key, item.Item2 + 1));
//        }

//        public Tuple<string, int>[] Get(int minimumCounter = 3) =>
//            _connection.Collection.Where(x => x.Item2 >= minimumCounter).ToArray();

//        public void Dispose() =>
//            _connection.Dispose();

//        public bool TryCountOne(string key)
//        {
//            throw new NotImplementedException();
//        }

//        public bool Exists(string key)
//        {
//            return _connection.Collection.Any(x => x.Item1 == key);
//        }

//        public void CountIfExists(string key)
//        {
//            throw new NotImplementedException();
//        }
//    }
//}