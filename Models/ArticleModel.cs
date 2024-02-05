namespace Alimentations.Models
{
    public class ArticleModel
    {
        
            public int ID { get; set; }
            public string? Designation { get; set; }
            public string? Marque { get; set; }
            public decimal Prix { get; set; }
            public DateTime Date { get; set; }
    }
}
