namespace Appointment.Api.Entities;

public class QueueNumber
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AppointmentId { get; set; }
    public Guid DoctorId { get; set; }
    public int Number { get; set; }
    public string Status { get; set; } = "Waiting";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Appointment Appointment { get; set; } = null!;
}
