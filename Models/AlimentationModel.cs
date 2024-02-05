 using Alimentations.Pages;

namespace Alimentations.Models
{
   
        public class AlimentationModel
        {
            public int ID { get; set; }
        public DateTime? Date { get; set; } = DateTime.Now;
        public decimal TotalPrice { get; set; }

        public bool IsSelected { get; set; }

        public bool ShowArticles { get; set; }
            public List<ArticleModel> Articles { get; set; }

        public DepotModel Depot { get; set; }
        }
}
