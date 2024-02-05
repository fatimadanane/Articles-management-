using Alimentations.Models;
using System.Data.SqlClient;

namespace Alimentations.Repositories
{
    public class DepotRepository
    {
        private readonly DatabaseService _databaseService;

        public DepotRepository(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }
        //Afficher les autres dépots pour chaque alimentation
              public async Task<List<DepotModel>> GetDepotsNot(int alimentationId)
        {
            List<DepotModel> depots = new List<DepotModel>();
 string query = "SELECT ID, Designation, Ville, Date  FROM Depot WHERE ID NOT IN (SELECT DepotID FROM Alimentation WHERE ID = @Id)";
            var parameters = new { Id = alimentationId };
            List<Dictionary<string, object>> queryResult = await _databaseService.ExecuteQueryAsync(query, parameters);
            foreach (Dictionary<string, object> row in queryResult)
            {
                DepotModel depot = new DepotModel();
                depot.ID = Convert.ToInt32(row["ID"]);
                depot.Designation = row["Designation"].ToString();
                depot.Ville = row["Ville"].ToString();

                depot.Date = Convert.ToDateTime(row["Date"]);
                depots.Add(depot);
            }

            return depots;
        }

        //Avoir touts les dépots
        public async Task<List<DepotModel>> GetDepots()
        {
            List<DepotModel> depots = new List<DepotModel>();

            string query = "SELECT ID, Designation, Ville, Date FROM Depot";
            List<Dictionary<string, object>> queryResult = await _databaseService.ExecuteQueryAsync(query);

            foreach (Dictionary<string, object> row in queryResult)
            {
                DepotModel depot = new DepotModel();
                depot.ID = Convert.ToInt32(row["ID"]);
                depot.Designation = row["Designation"].ToString();
                depot.Ville = row["Ville"].ToString();
               
                depot.Date = Convert.ToDateTime(row["Date"]);
                depots.Add(depot);
            }

            return depots;
        }
        //Ajouter un dépot

        public async Task<int> AddDepot(int id, string designation, string ville,  DateTime date)
        {

            string query = "INSERT INTO Depot (ID, Designation, Ville,  Date) " +
                "VALUES (@ID, @Designation, @Ville, @Date);";

            var parameters = new
            {
                ID = id,
                Designation = designation,
                Ville = ville,
               
                Date = date,
            };

            // Execute the query and return the ID
            await _databaseService.ExecuteQueryAsync(query, parameters);



            // Return the generated ID
            return id;
        }
   //Modifier un dépot 
        public async Task<int> UpdateDepot(int id, string designation, string ville, DateTime date)
        {
            // Your implementation to update the article in the database
            // This is a simplified example, adapt it based on your actual database logic
            string query = "UPDATE Depot " +
                           "SET Designation = @Designation, Ville = @ville, " +
                           " Date = @Date " +
                           "WHERE ID = @ID;";

            var parameters = new
            {
                ID = id,
                Designation = designation,
                Ville = ville,
                Date = date,
                
            };

            // Execute the query to update the row
            await _databaseService.ExecuteQueryAsync(query, parameters);

            // Return the updated ID
            return id;
        }


       //Supprimmer un dépot 
        public async Task DeleteDepot(int depotId)
        {
            string query = @"
        DECLARE @FKCount INT;
        SELECT @FKCount = COUNT(*) FROM Alimentation WHERE DepotID = @DepotId;

        IF (@FKCount = 0)
        BEGIN
            DELETE FROM Depot WHERE ID = @DepotId;
        END
        ELSE
        BEGIN
            THROW 51000, 'Foreign key constraint violation', 1;
        END ";

            var parameters = new { DepotId = depotId };

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
        //same as article
        public async Task<int> GetMaxDepotId()
        {
            string query = "SELECT MAX(ID) AS MaxID FROM Depot";

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
