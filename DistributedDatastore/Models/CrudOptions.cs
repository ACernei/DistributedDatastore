namespace DistributedDatastore.Models;

public class CrudOptions
{
    public bool IsLeader { get; set; }
    public string Self { get; set; }
    public List<string> Servers { get; set; }

    public List<string> GetOtherServers()
        => Servers.Where(x => x != Self).ToList();
}
