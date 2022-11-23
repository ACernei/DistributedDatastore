using Microsoft.AspNetCore.Mvc;

namespace DistributedDatastore.Models;

public class DataRepository : IDataRepository
{
    private Dictionary<int, Data> db = new();

    private static int id = 0;
    private object idLock = new();

    public List<Data> ToList()
    {
        return this.db.Values.ToList();
    }

    public Data Find(int id)
    {
        this.db.TryGetValue(id, out var data);
        return data;
    }

    public void Add(Data data)
    {
        this.db.Add(data.Id, data);
    }

    public void Remove(Data data)
    {
        this.db.Remove(data.Id);
    }

    public void Update(Data data)
    {
        this.db[data.Id] = data;
    }

    public int GenerateId()
    {
        lock (idLock)
        {
            return ++id;
        }
    }
}
