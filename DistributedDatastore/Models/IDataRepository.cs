using Microsoft.AspNetCore.Mvc;

namespace DistributedDatastore.Models;

public interface IDataRepository
{
    List<Data> ToList();
    Data Find(int id);
    void Add(Data data);
    void Remove(Data data);
    void Update(Data data);
}
