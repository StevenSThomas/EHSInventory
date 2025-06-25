namespace EHSInventory.Models;

public class Change
{
    public string FieldChanged { get; set; } = String.Empty;
    public string Before { get; set; } = String.Empty;
    public string After { get; set; } = String.Empty;
}