using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

public class Product
{
    public string  Name { get; set; }
    public string Picture { get; set; }
    public string Description { get; set; } 
    public float Price { get; set; }
    public int Stock { get; set; }
    public int SoldQuantity { get; set; }
    public string SubCategoryId { get; set; }
    public string CategoryId { get; set; }
    
    // Foreign Keys
    [ForeignKey("SubCategoryId")]
    public SubCategory SubCategory { get; set; }
    [ForeignKey("CategoryId")]
    public Category Category { get; set; }
}