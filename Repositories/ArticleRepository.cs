using Alimentations.Models;
using Alimentations.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Diagnostics;
using System.Data;
namespace Alimentations.Repositories
{
    public class ArticleRepository
    {
        private readonly DatabaseService _databaseService;

        public ArticleRepository(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }
  //Afficher les articles lors de l'ajout d'un nouveau article pour une alimentation qui ne sont déja présents dans cette alimentation
             public async Task<List<ArticleModel>> GetArticlesNot(int alimentationId)
        {
            List<ArticleModel> articles = new List<ArticleModel>();

            string query = "SELECT * FROM Article WHERE ID NOT IN (SELECT ArticleID FROM Alimentation_Articles WHERE AlimentationID =@Id)";
            var parameters = new { Id = alimentationId };
            List<Dictionary<string, object>> queryResult = await _databaseService.ExecuteQueryAsync(query,parameters);

            foreach (Dictionary<string, object> row in queryResult)
            {
                ArticleModel article = new ArticleModel();
                article.ID = Convert.ToInt32(row["ID"]);
                article.Designation = row["Designation"].ToString();
                article.Marque = row["Marque"].ToString();
                article.Prix = Convert.ToDecimal(row["Prix"]);
                article.Date = Convert.ToDateTime(row["Date"]);
                articles.Add(article);
            }

            return articles;
        }

        //Afficher tous les articles
        public async Task<List<ArticleModel>> GetArticles()
        {
            List<ArticleModel> articles = new List<ArticleModel>();

            string query = "SELECT ID, Designation, Marque, Prix, Date FROM Article";
            List<Dictionary<string, object>> queryResult = await _databaseService.ExecuteQueryAsync(query);

            foreach (Dictionary<string, object> row in queryResult)
            {
                ArticleModel article = new ArticleModel();
                article.ID = Convert.ToInt32(row["ID"]);
                article.Designation = row["Designation"].ToString();
                article.Marque = row["Marque"].ToString();
                article.Prix = Convert.ToDecimal(row["Prix"]);
                article.Date = Convert.ToDateTime(row["Date"]);
                articles.Add(article);
            }

            return articles;
        }
     //Ajouter un article====================================================================
        public async Task<int> AddArticle(int id, string designation, string marque, decimal prix, DateTime date)
        {
           
            string query = "INSERT INTO Article (ID, Designation, Marque, Prix, Date) " +
                "VALUES (@ID, @Designation, @Marque, @Prix, @Date);";

            var parameters = new
            {
                ID = id,
                Designation = designation,
                Marque = marque,
                Prix = prix,
                Date = date
            };

            // Execute the query and return the ID
            await _databaseService.ExecuteQueryAsync(query, parameters);



            // Return the generated ID
            return id;
        }
        //Modifier un article ======================================
        public async Task<int> UpdateArticle(int id, string designation, string marque, decimal prix, DateTime date)
        {
            // Your implementation to update the article in the database
            // This is a simplified example, adapt it based on your actual database logic
            string query = "UPDATE Article " +
                           "SET Designation = @Designation, Marque = @Marque, " +
                           "Prix = @Prix, Date = @Date " +
                           "WHERE ID = @ID;";

            var parameters = new
            {
                ID = id,
                Designation = designation,
                Marque = marque,
                Prix = prix,
                Date = date
            };

            // Execute the query to update the row
            await _databaseService.ExecuteQueryAsync(query, parameters);

            // Return the updated ID
            return id;
        }
        private string errorMessage;
        //Supprimer un article //////////////=================
        public async Task DeleteArticle(int articleId)
        {
            string query = @"
        DECLARE @FKCount INT;
        SELECT @FKCount = COUNT(*) FROM Alimentation_Articles WHERE ArticleID = @ArticleId;

        IF (@FKCount = 0)
        BEGIN
            DELETE FROM Article WHERE ID = @ArticleId;
        END
        ELSE
        BEGIN
            THROW 51000, 'Foreign key constraint violation', 1;
        END ";

            var parameters = new { ArticleId = articleId };

            try
            {
                // Execute the query to delete the article or throw an exception if a foreign key constraint is violated
                await _databaseService.ExecuteQueryAsync(query, parameters);
            }
            catch (SqlException ex)
            {
                if (ex.Number == 51000)
                {
                    // Handle the case where a foreign key constraint is violated
                    throw new InvalidOperationException("Foreign key constraint violation");
                }
                else
                {
                    // Handle other database exceptions
                    throw;
                }
            }
        }

        //Avoir le ID d'article MAx pour l'incrementer lors de l'ajout d'un nouveau article

        public async Task<int> GetMaxArticleId()
        {
            string query = "SELECT MAX(ID) AS MaxID FROM Article";

            // Call the non-generic ExecuteQueryAsync
            List<Dictionary<string, object>> queryResult = await _databaseService.ExecuteQueryAsync(query);

            // Assuming the query returns a single row with the maximum ID
            if (queryResult.Count > 0)
            {
                if (queryResult[0].TryGetValue("MaxID", out object maxIdObject) && maxIdObject != null)
                {
                    if (int.TryParse(maxIdObject.ToString(), out int maxId))
                    {
                        return maxId;
                    }
                }
            }

            // Handle the case where the result is unexpected
            throw new InvalidOperationException("Failed to retrieve the maximum article ID.");
        }

    }
}
