using Microsoft.AspNetCore.Mvc;
using tourism_api.Domain;
using tourism_api.Repositories;

namespace tourism_api.Controllers;

[Route("api/tours")]
[ApiController]
public class TourController : ControllerBase
{
    private readonly TourRepository _tourRepo;
    private readonly UserRepository _userRepo;
    private readonly ReservationRepository _reservationRepo;

    public TourController(IConfiguration configuration)
    {
        _tourRepo = new TourRepository(configuration);
        _userRepo = new UserRepository(configuration);
        _reservationRepo = new ReservationRepository(configuration);
    }

    [HttpGet]
    public ActionResult GetPaged([FromQuery] int guideId = 0, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string orderBy = "Name", [FromQuery] string orderDirection = "ASC", [FromQuery] string status = "")
    {
        if (guideId > 0)
        {
            return Ok(_tourRepo.GetByGuide(guideId));
        }

        // Validacija za orderBy i orderDirection
        List<string> validOrderByColumns = new List<string> { "Name", "Description", "DateTime", "MaxGuests" }; // Lista dozvoljenih kolona za sortiranje
        if (!validOrderByColumns.Contains(orderBy))
        {
            orderBy = "Name"; // Default vrednost
        }

        List<string> validOrderDirections = new List<string> { "ASC", "DESC" }; // Lista dozvoljenih smerova
        if (!validOrderDirections.Contains(orderDirection))
        {
            orderDirection = "ASC"; // Default vrednost
        }

        try
        {
            List<Tour> tours = _tourRepo.GetPaged(page, pageSize, orderBy, orderDirection, status);
            int totalCount = _tourRepo.CountAll();
            Object result = new
            {
                Data = tours,
                TotalCount = totalCount
            };
            return Ok(result);
        }
        catch (Exception ex)
        {
            return Problem("An error occurred while fetching tours.");
        }
    }

    [HttpGet("{id}")]
    public ActionResult<Tour> GetById(int id)
    {
        try
        {
            Tour tour = _tourRepo.GetById(id);
            if (tour == null)
            {
                return NotFound($"Tour with ID {id} not found.");
            }
            return Ok(tour);
        }
        catch (Exception ex)
        {
            return Problem("An error occurred while fetching the tour.");
        }
    }

    [HttpPost]
    public ActionResult<Tour> Create([FromBody] Tour newTour)
    {
        if (!newTour.IsValid())
        {
            return BadRequest("Invalid tour data.");
        }

        try
        {
            User user = _userRepo.GetById(newTour.GuideId);
            if (user == null)
            {
                return NotFound($"User with ID {newTour.GuideId} not found.");
            }

            Tour createdTour = _tourRepo.Create(newTour);
            return Ok(createdTour);
        }
        catch (Exception ex)
        {
            return Problem("An error occurred while creating the tour.");
        }
    }

    [HttpPut("{id}")]
    public ActionResult<Tour> Update(int id, [FromBody] Tour tour)
    {
        if (!tour.IsValid())
        {
            return BadRequest("Invalid tour data.");
        }

        try
        {
            tour.Id = id;
            Tour updatedTour = _tourRepo.Update(tour);
            if (updatedTour == null)
            {
                return NotFound();
            }
            return Ok(updatedTour);
        }
        catch (Exception ex)
        {
            return Problem("An error occurred while updating the tour.");
        }
    }

    [HttpDelete("{id}")]
    public ActionResult Delete(int id)
    {
        try
        {
            bool isDeleted = _tourRepo.Delete(id);
            if (isDeleted)
            {
                return NoContent();
            }
            return NotFound($"Tour with ID {id} not found.");
        }
        catch (Exception ex)
        {
            return Problem("An error occurred while deleting the tour.");
        }
    }

    [HttpPost("{tourId}/reservations")]
    public ActionResult CreateReservation(int tourId, [FromBody] Reservation reservationRequest, [FromQuery] int userId)
    {
        try
        {
            Reservation reservation = new Reservation
            {
                TourId = tourId,
                UserId = userId,
                NumberOfGuests = reservationRequest.NumberOfGuests,
                ReservationDate = DateTime.Now,
                Status = "Active"
            };

            Reservation created = _reservationRepo.Create(reservation);
            return Ok(created);
        }
        catch (Exception)
        {
            return Problem("An error occurred while creating the reservation.");
        }
    }

    [HttpGet("reservations")]
    public ActionResult GetTouristReservations([FromQuery] int touristId)
    {
        try
        {
            if (touristId < 0)
            {
                return BadRequest("Tourist ID is required.");
            }

            List<Reservation> reservations = _reservationRepo.GetByUserId(touristId);
            return Ok(reservations);
        }
        catch (Exception ex)
        {
            return Problem("An error occurred while fetching reservations.");
        }
    }

    [HttpDelete("reservations/{reservationId}")]
    public ActionResult CancelReservation(int reservationId, [FromQuery] int userId)
    {
        try
        {
            if(userId < 0)
            {
                return BadRequest("Valid user ID is required.");
            }

            User user = _userRepo.GetById(userId);
            if(user == null)
            {
                return NotFound($"User with ID {userId} not found.");
            }

            Reservation reservation = _reservationRepo.GetById(reservationId);
            if(reservation == null)
            {
                return NotFound($"Reservation with ID {reservationId} not found.");
            }

            if(reservation.UserId != userId)
            {
                return BadRequest("You can only cancel your own reservations.");
            }

            Tour tour = _tourRepo.GetById(reservation.TourId);
            if(tour == null)
            {
                return Problem($"Tour with ID {reservation.TourId} not found for this reservation.");
            }

            DateTime tourStart = tour.DateTime;
            DateTime now = DateTime.Now;
            TimeSpan timeUntilTour = tourStart - now;

            if(timeUntilTour.TotalHours < 24)
            {
                return BadRequest("Cannot cancel reservation less than 24 hours before tour start.");
            }

            bool isDeleted = _reservationRepo.CancelReservation(reservationId);
            if(isDeleted)
            {
                return Ok("Reservation cancelled successfully.");
            }
            else
            {
                return Problem("Failed to cancel reservation.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("CreateReservation error: " + ex.Message);
            return Problem("An error occurred while cancelling the reservation.");
        }
    }

}
