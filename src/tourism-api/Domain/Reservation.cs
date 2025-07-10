namespace tourism_api.Domain
{
    public class Reservation
    {
        public int Id { get; set; }
        public int TourId { get; set; }

        public int UserId { get; set; }
        public int NumberOfGuests { get; set; }
        public DateTime ReservationDate { get; set; }
        public string Status { get; set; } = "Active";

        public Tour? Tour { get; set; }
        public User? User { get; set; }

        public bool IsValid()
        {
            return TourId > 0 && UserId > 0 && NumberOfGuests > 0 && NumberOfGuests <= 80 && !string.IsNullOrEmpty(Status) && (Status == "Active" || Status == "Cancelled");
        }
    }
}
