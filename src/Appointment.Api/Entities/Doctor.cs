namespace Appointment.Api.Entities;

public class Doctor
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FullName { get; set; } = string.Empty;
    public string Specialty { get; set; } = string.Empty;
    public string ClinicName { get; set; } = string.Empty;

    public ICollection<Slot> Slots { get; set; }
        = new List<Slot>();
}
