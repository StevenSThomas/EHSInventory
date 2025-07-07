using System.ComponentModel.DataAnnotations.Schema;

namespace EHSInventory.Models;

public class ProductHistory
{
    public enum changeType
    {
        update,
        delete,
        setQuantity,
        setDisplayOrder
    }

    public long? ProductHistoryId { get; set; }
    public DateTime CreatedDt { get; set; }
    public string CreatedBy { get; set; } = String.Empty;
    public long? ProductId { get; set; }
    public changeType ChangeType { get; set; }
    public string ChangeJson { get; set; } = String.Empty;

    [NotMapped]
    public List<Change>? Changes { get; set; }

    public string Comment { get; set; } = String.Empty;
}