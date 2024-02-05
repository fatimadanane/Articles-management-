namespace Alimentations.Models
{
    public class AlimentationArticleModel
    {
        public int AlimentationID { get; set; }
        public int ArticleID { get; set; }
        public int Quantity { get; set; }
    

        public AlimentationModel Alimentation { get; set; }
        public ArticleModel Article { get; set; }
    }
} 
