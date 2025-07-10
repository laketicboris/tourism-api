using Microsoft.Data.Sqlite;
using tourism_api.Domain;

namespace tourism_api.Repositories;

public class KeyPointRepository
{
    private readonly string _connectionString;

    public KeyPointRepository(IConfiguration configuration)
    {
        _connectionString = configuration["ConnectionString:SQLiteConnection"];
    }

    public KeyPoint Create(KeyPoint keyPoint)
    {
        try
        {
            using SqliteConnection connection = new SqliteConnection(_connectionString);
            connection.Open();

            string query = @"
                    INSERT INTO KeyPoints (OrderPosition, Name, Description, ImageUrl, Latitude, Longitude, TourId)
                    VALUES (@Order, @Name, @Description, @ImageUrl, @Latitude, @Longitude, @TourId);
                    SELECT LAST_INSERT_ROWID();";
            using SqliteCommand command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@Order", keyPoint.Order);
            command.Parameters.AddWithValue("@Name", keyPoint.Name);
            command.Parameters.AddWithValue("@Description", keyPoint.Description);
            command.Parameters.AddWithValue("@ImageUrl", keyPoint.ImageUrl);
            command.Parameters.AddWithValue("@Latitude", keyPoint.Latitude);
            command.Parameters.AddWithValue("@Longitude", keyPoint.Longitude);
            command.Parameters.AddWithValue("@TourId", keyPoint.TourId);

            keyPoint.Id = Convert.ToInt32(command.ExecuteScalar());

            return keyPoint;
        }
        catch (SqliteException ex)
        {
            Console.WriteLine($"Greška pri konekciji ili izvršavanju neispravnih SQL upita: {ex.Message}");
            throw;
        }
        catch (FormatException ex)
        {
            Console.WriteLine($"Greška u konverziji podataka iz baze: {ex.Message}");
            throw;
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"Konekcija nije otvorena ili je otvorena više puta: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Neočekivana greška: {ex.Message}");
            throw;
        }
    }

    public bool Delete(int id)
    {
        try
        {
            using SqliteConnection connection = new SqliteConnection(_connectionString);
            connection.Open();

            string query = "DELETE FROM KeyPoints WHERE Id = @Id";
            using SqliteCommand command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@Id", id);

            int rowsAffected = command.ExecuteNonQuery();

            return rowsAffected > 0;
        }
        catch (SqliteException ex)
        {
            Console.WriteLine($"Greška pri konekciji ili izvršavanju neispravnih SQL upita: {ex.Message}");
            throw;
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"Konekcija nije otvorena ili je otvorena više puta: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Neočekivana greška: {ex.Message}");
            throw;
        }
    }
}
