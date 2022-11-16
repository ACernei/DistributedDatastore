namespace DistributedDatastore.Models;

public class CrudOptions
{
    public bool IsLeader { get; set; }
    public List<string> Servers { get; set; }
}
