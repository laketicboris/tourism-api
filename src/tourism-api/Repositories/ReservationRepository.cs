using Microsoft.Data.Sqlite;
using tourism_api.Domain;

namespace tourism_api.Repositories
{
    public class ReservationRepository
    {
        private readonly string _connectionString;

        public ReservationRepository(IConfiguration configuration)
        {
            _connectionString = configuration["ConnectionString:SQLiteConnection"];
        }

        public Reservation Create (Reservation reservation)
        {
            try
            {
                using SqliteConnection connection = new SqliteConnection(_connectionString);
                connection.Open();

                string query = @"
                    INSERT INTO Reservations (TourId,UserId, NumberOfGuests, ReservationDate, Status)
                    VALUES (@TourId, @UserId, @NumberOfGuests, @ReservationDate, @Status);
                    SELECT LAST_INSERT_ROWID();";

                using SqliteCommand command = new SqliteCommand(query, connection);
                command.Parameters.AddWithValue("@TourId", reservation.TourId);
                command.Parameters.AddWithValue("@UserId", reservation.UserId);
                command.Parameters.AddWithValue("@NumberOfGuests", reservation.NumberOfGuests);
                command.Parameters.AddWithValue("@ReservationDate", reservation.ReservationDate.ToString("yyyy-MM-dd HH:mm:ss"));
                command.Parameters.AddWithValue("@Status", reservation.Status);

                reservation.Id = Convert.ToInt32(command.ExecuteScalar());
                return reservation;
            }
            catch (SqliteException ex)
            {
                Console.WriteLine($"Greška pri konekciji ili izvršavanju neispravnih SQL upita: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Neočekivana greška: {ex.Message}");
                throw;
            }
        }

        public List<Reservation> GetByUserId(int userId)
        {
            List<Reservation> reservations = new List<Reservation>();

            try
            {
                using SqliteConnection connection = new SqliteConnection(_connectionString);
                connection.Open();

                string query = @"
                SELECT r.Id, r.TourId, r.UserId, r.NumberOfGuests, r.ReservationDate, r.Status,
                       t.Name AS TourName, t.DateTime AS TourDateTime, t.MaxGuests,
                       u.Username
                FROM Reservations r
                INNER JOIN Tours t ON r.TourId = t.Id
                INNER JOIN Users u ON r.UserId = u.Id
                WHERE r.UserId = @UserId
                ORDER BY r.ReservationDate DESC";

                using SqliteCommand command = new SqliteCommand(query, connection);
                command.Parameters.AddWithValue("@UserId", userId);

                using SqliteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    reservations.Add(new Reservation
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        TourId = Convert.ToInt32(reader["TourId"]),
                        UserId = Convert.ToInt32(reader["UserId"]),
                        NumberOfGuests = Convert.ToInt32(reader["NumberOfGuests"]),
                        ReservationDate = Convert.ToDateTime(reader["ReservationDate"]),
                        Status = reader["Status"].ToString(),
                        Tour = new Tour
                        {
                            Id = Convert.ToInt32(reader["TourId"]),
                            Name = reader["TourName"].ToString(),
                            DateTime = Convert.ToDateTime(reader["TourDateTime"]),
                            MaxGuests = Convert.ToInt32(reader["MaxGuests"])
                        },
                        User = new User
                        {
                            Id = Convert.ToInt32(reader["UserId"]),
                            Username = reader["Username"].ToString()
                        }
                    });
                }

                return reservations;
            }

            catch (SqliteException ex)
            {
                Console.WriteLine($"Greška pri konekciji ili izvršavanju neispravnih SQL upita: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Neočekivana greška: {ex.Message}");
                throw;
            }
        }

        public List<Reservation> GetTourById (int tourId)
        {
            List<Reservation> reservations = new List<Reservation>();

            try
            {
                using SqliteConnection connection = new SqliteConnection(_connectionString);
                connection.Open();

                string query = @"
                    SELECT Id, TourId, UserId, NumberOfGuests, ReservationDate, Status
                    FROM Reservations
                    WHERE TourId = @TourId";

                using SqliteCommand command = new SqliteCommand(query, connection);
                command.Parameters.AddWithValue("@TourId", tourId);

                using SqliteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    reservations.Add(new Reservation
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        TourId = Convert.ToInt32(reader["TourId"]),
                        UserId = Convert.ToInt32(reader["UserId"]),
                        NumberOfGuests = Convert.ToInt32(reader["NumberOfGuests"]),
                        ReservationDate = Convert.ToDateTime(reader["ReservationDate"]),
                        Status = reader["Status"].ToString()
                    });
                }
                return reservations;
            }
            catch (SqliteException ex)
            {
                Console.WriteLine($"Greška pri konekciji ili izvršavanju neispravnih SQL upita: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Neočekivana greška: {ex.Message}");
                throw;
            }
        }

        public Reservation GetById (int id)
        {
            try
            {
                using SqliteConnection connection = new SqliteConnection(_connectionString);
                connection.Open();

                string query = @"
                    SELECT r.Id, r.TourId, r.UserId, r.NumberOfGuests, r.ReservationDate, r.Status,
                       t.Name AS TourName, t.DateTime AS TourDateTime, t.MaxGuests
                    FROM Reservations r
                    INNER JOIN Tours t ON r.TourId = t.Id
                    WHERE r.Id = @Id";

                using SqliteCommand command = new SqliteCommand(query, connection);
                command.Parameters.AddWithValue("@Id", id);

                using SqliteDataReader reader = command.ExecuteReader();


                if (reader.Read())
                {
                    return new Reservation
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        TourId = Convert.ToInt32(reader["TourId"]),
                        UserId = Convert.ToInt32(reader["UserId"]),
                        NumberOfGuests = Convert.ToInt32(reader["NumberOfGuests"]),
                        ReservationDate = Convert.ToDateTime(reader["ReservationDate"]),
                        Status = reader["Status"].ToString(),
                        Tour = new Tour
                        {
                            Id = Convert.ToInt32(reader["TourId"]),
                            Name = reader["TourName"].ToString(),
                            DateTime = Convert.ToDateTime(reader["TourDateTime"]),
                            MaxGuests = Convert.ToInt32(reader["MaxGuests"])
                        }
                    };
                }
                return null;
            }
            catch (SqliteException ex)
            {
                Console.WriteLine($"Greška pri konekciji ili izvršavanju neispravnih SQL upita: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Neočekivana greška: {ex.Message}");
                throw;
            }
        }

        public bool CancelReservation (int id)
        {
            try
            {
                using SqliteConnection connection = new SqliteConnection(_connectionString);
                connection.Open();

                string query = "DELETE FROM Reservations WHERE Id = @Id";

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
            catch (Exception ex)
            {
                Console.WriteLine($"Neočekivana greška: {ex.Message}");
                throw;
            }
        }

        public int GetTotalGuestsForTour (int tourId)
        {
            try
            {
                using SqliteConnection connection = new SqliteConnection(_connectionString);
                connection.Open();

                string query = @"
                    SELECT COALESCE(SUM(NumberOfGuests), 0)
                    FROM Reservations
                    WHERE TourId = @TourId AND Status = 'Active'";

                using SqliteCommand command = new SqliteCommand(query, connection);
                command.Parameters.AddWithValue("@TourId", tourId);

                return Convert.ToInt32(command.ExecuteScalar());
            }
            catch (SqliteException ex)
            {
                Console.WriteLine($"Greška pri konekciji ili izvršavanju neispravnih SQL upita: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Neočekivana greška: {ex.Message}");
                throw;
            }
        }
    }
}
