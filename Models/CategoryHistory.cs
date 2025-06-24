namespace EHSInventory.Models;

public class CategoryHistory
{
    public enum changeType
    {
        update,
        delete
    }

    public long? CategoryHistoryId { get; set; }
    public DateTime CreatedDt { get; set; }
    public string CreatedBy { get; set; } = String.Empty;
    public long? CategoryId { get; set; }
    public changeType ChangeType { get; set; }
    public string Comment { get; set; } = String.Empty;
}