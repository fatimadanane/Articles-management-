using Alimentations.Models;
using Alimentations.Pages;
using Microsoft.JSInterop;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq.Expressions;


namespace Alimentations.Repositories
{
    public class AlimentationRepository
    {
        private readonly DatabaseService _databaseService;

        public AlimentationRepository(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }


        //Modifier la quantity d'un article

        public async Task UpdateArticleQuantity(int alimentationId, int articleId, int newQuantity)
        {
            try
            {
                string updateQuery = "UPDATE Alimentation_Articles SET Quantity = @Quantity " +
                                     "WHERE AlimentationID = @AlimentationID AND ArticleID = @ArticleID";

                await _databaseService.ExecuteNonQueryAsync(updateQuery, new
                {
                    AlimentationID = alimentationId,
                    ArticleID = articleId,
                    Quantity = newQuantity
                });

                Debug.WriteLine($"Article quantity updated (AlimentationID: {alimentationId}, ArticleID: {articleId}, Quantity: {newQuantity})");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating article quantity: {ex.Message}");
                throw;
            }
        }
        //Modifier une alimentation

        public async Task UpdateAlimentation(int alimentationId, DateTime? date, int depotId)
        {
            try
            {
                string updateQuery = "UPDATE Alimentation SET DepotID = @DepotID, Date = COALESCE(@Date, GetDate())  WHERE ID = @ID";

                await _databaseService.ExecuteNonQueryAsync(updateQuery, new
                {
                    ID = alimentationId,
                    Date = date,
                    DepotID = depotId
                });

                Debug.WriteLine($"Alimentation updated (ID: {alimentationId}, Date: {date}, DepotID: {depotId})");

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating Alimentation: {ex.Message}");
                throw;
            }
        }

        //Avoir une alimentation
        public async Task<List<AlimentationModel>> GetAlimentations()
        {
            try
            {
                string query = "SELECT a.ID, a.Date, a.TotalPrice, d.Designation, d.Ville " +
                                "FROM Alimentation AS a " +
                                "INNER JOIN Depot AS d ON a.DepotID = d.ID";

                List<Dictionary<string, object>> result = await _databaseService.ExecuteQueryAsync(query);

                List<AlimentationModel> alimentations = new List<AlimentationModel>();

                foreach (var row in result)
                {
                    AlimentationModel alimentation = new AlimentationModel
                    {
                        ID = (int)row["ID"],
                        Date = row["Date"] != DBNull.Value ? (DateTime)row["Date"] : DateTime.MinValue,
                        TotalPrice = (decimal)row["TotalPrice"],
                        ShowArticles = false,
                        Articles = new List<ArticleModel>(),
                        Depot = new DepotModel
                        {

                            Designation = (string)row["Designation"],
                            Ville = (string)row["Ville"]

                        }
                    };
                 



                    int alimentationId = (int)row["ID"];
                    List<AlimentationArticleModel> alimentationArticles = await GetArticlesForAlimentation(alimentationId);


                    List<ArticleModel> articles = alimentationArticles.Select(aa => aa.Article).ToList();
                    alimentation.Articles = articles;

                    alimentations.Add(alimentation);
                }
                return alimentations;
            }
            catch (Exception ex)
            {
               
                throw;
            }
        }


        //Afficher les articles de chaque alimentation
        public async Task<List<AlimentationArticleModel>> GetArticlesForAlimentation(int AlimentationId)
        {
            try
            {
                string query = "SELECT aa.AlimentationID, aa.ArticleID, aa.Quantity,a.Designation,a.Marque, a.ID AS ArticleID, a.Prix " +
                                "FROM Alimentation_Articles AS aa " +
                                "INNER JOIN Article AS a ON aa.ArticleID = a.ID " +
                                "WHERE aa.AlimentationID = @AlimentationID";  

                var parameters = new
                {
                    AlimentationID = AlimentationId
                };

                List<Dictionary<string, object>> result = await _databaseService.ExecuteQueryAsync(query, parameters);

                List<AlimentationArticleModel> alimentationArticles = new List<AlimentationArticleModel>();

                foreach (var row in result)
                {
                    AlimentationArticleModel alimentationArticle = new AlimentationArticleModel
                    {
                        AlimentationID = (int)row["AlimentationID"],
                        ArticleID = (int)row["ArticleID"],
                        Quantity = (int)row["Quantity"],
                        
                        Alimentation = new AlimentationModel
                        {
                            // Assign AlimentationModel properties if needed
                        },
                        Article = new ArticleModel
                        {
                            ID = (int)row["ArticleID"],
                            Designation = (string)row["Designation"],
                            Marque = (string)row["Marque"],
                            Prix = (decimal)row["Prix"]  
                        }
                    };

                    alimentationArticles.Add(alimentationArticle);
                }

                return alimentationArticles;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        //Ajouter une alimentation
        public async Task<int> InsertAlimentationAsync(DateTime date, decimal totalPrice, int depotId, List<AlimentationArticleModel> alimentationArticles)
        {
            try
            {
                // Get the current maximum ID from the Alimentation table
                string getMaxIdQuery = "SELECT MAX(ID) FROM Alimentation;";
                int maxId = await _databaseService.ExecuteScalarAsync<int>(getMaxIdQuery);

                // Increment the ID for the new Alimentation record
                int newAlimentationId = maxId + 1;

                // Insert the new Alimentation record
                string insertAlimentationQuery = "INSERT INTO Alimentation (ID, DepotID, Date, TotalPrice) VALUES (@ID, @DepotID, @Date, @TotalPrice);";

                var alimentationParameters = new
                {
                    ID = newAlimentationId,
                    DepotID = depotId,
                    Date = date,
                    TotalPrice = totalPrice
                };

                await _databaseService.ExecuteQueryAsync(insertAlimentationQuery, alimentationParameters);

                // Call the method to insert Alimentation_Articles
                await InsertAlimentationArticles(newAlimentationId, alimentationArticles);

                return newAlimentationId;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error inserting Alimentation: {ex.Message}");
                throw;
            }
        }
        


        //Supprimer les articles pour une alimentation

             public async Task DeleteAlimentationArticle(int alimentationId, int articleId)
        {
            try
            {
                string deleteQuery = "DELETE FROM Alimentation_Articles WHERE AlimentationID = @AlimentationID AND ArticleID = @ArticleID";
                // Include ArticleID in the parameters for accurate deletion
                var parameters = new { AlimentationID = alimentationId, ArticleID = articleId };
                await _databaseService.ExecuteNonQueryAsync(deleteQuery, parameters);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting Alimentation: {ex.Message}");
                throw;
            }
        }





     //Ajouter un article pour une alimentation

        public async Task InsertAlimentationArticles(int alimentationId, List<AlimentationArticleModel> alimentationArticles)
        {
            try
            {
             
          

                string insertAlimentationArticlesQuery = "INSERT INTO Alimentation_Articles (AlimentationID, ArticleID, Quantity) VALUES (@AlimentationID, @ArticleID, @Quantity);";

                foreach (var alimentationArticle in alimentationArticles)
                {
                    var articleParameters = new
                    {
                        AlimentationID = alimentationId,
                        ArticleID = alimentationArticle.ArticleID,
                        Quantity = alimentationArticle.Quantity
                    };

                    Debug.WriteLine($"Inserting Alimentation_Article - AlimentationID: {articleParameters.AlimentationID}, ArticleID: {articleParameters.ArticleID}, Quantity: {articleParameters.Quantity}");

                    await _databaseService.ExecuteQueryAsync(insertAlimentationArticlesQuery, articleParameters);
                }

                Debug.WriteLine($"Insertion of Alimentation_Articles successful.");


            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error inserting Alimentation_Articles: {ex.Message}");
                throw;
            }
        }

       //Supprimer une Alimentation
        public async Task DeleteAlimentationAsync(int alimentationId)
        {
            try
            {
                // Delete from Alimentation_Articles first
                string deleteArticlesQuery = "DELETE FROM Alimentation_Articles WHERE AlimentationID = @AlimentationID;";
                var articlesParameters = new { AlimentationID = alimentationId };
                await _databaseService.ExecuteNonQueryAsync(deleteArticlesQuery, articlesParameters);

                // Delete from Alimentation
                string deleteAlimentationQuery = "DELETE FROM Alimentation WHERE ID = @ID;";
                var alimentationParameters = new { ID = alimentationId };
                await _databaseService.ExecuteNonQueryAsync(deleteAlimentationQuery, alimentationParameters);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting Alimentation: {ex.Message}");
                throw;
            }
        }
        //Selectionner les alimentations
        public async Task<List<AlimentationModel>> GetAlimentationsSync(int AlimentationId)
        {
            try
            {
                string query = "SELECT a.ID, a.Date, a.TotalPrice, d.Designation, d.Ville " +
                 "FROM Alimentation AS a " +
                 "INNER JOIN Depot AS d ON a.DepotID = d.ID " + 
                 "WHERE a.ID = @AlimentationID";

                var parameters = new
                {
                    AlimentationID = AlimentationId
                };

                List<Dictionary<string, object>> result = await _databaseService.ExecuteQueryAsync(query, parameters);

                List<AlimentationModel> alimentations = new List<AlimentationModel>();

                foreach (var row in result)
                {
                    AlimentationModel alimentation = new AlimentationModel
                    {
                        ID = (int)row["ID"],
                        Date = row["Date"] != DBNull.Value ? (DateTime)row["Date"] : DateTime.MinValue,
                        TotalPrice = (decimal)row["TotalPrice"],
                        ShowArticles = false,
                        Articles = new List<ArticleModel>(),
                        Depot = new DepotModel
                        {

                            Designation = (string)row["Designation"],
                            Ville = (string)row["Ville"]

                        }
                    };




                    int alimentationId = (int)row["ID"];
                    List<AlimentationArticleModel> alimentationArticles = await GetArticlesForAlimentation(alimentationId);


                    List<ArticleModel> articles = alimentationArticles.Select(aa => aa.Article).ToList();
                    alimentation.Articles = articles;

                    alimentations.Add(alimentation);
                }
                return alimentations;
            }
            catch (Exception ex)
            {

                throw;
            }
        }




    }
}