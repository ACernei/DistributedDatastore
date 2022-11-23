namespace Broker.Models;

public class CrudOptions
{
    public string Leader { get; set; }
    public List<string> Servers { get; set; }
}
