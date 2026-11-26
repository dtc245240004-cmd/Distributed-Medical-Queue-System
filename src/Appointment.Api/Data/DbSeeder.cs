using Appointment.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Appointment.Api.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(
        MedicalQueueDbContext dbContext)
    {
        if (await dbContext.Doctors.AnyAsync())
        {
            return;
        }

        var doctors = new List<Doctor>
        {
            new()
            {
                FullName = "Nguyen Van An",
                Specialty = "Cardiology",
                ClinicName = "Ha Noi Medical Clinic"
            },
            new()
            {
                FullName = "Tran Thi Binh",
                Specialty = "Dermatology",
                ClinicName = "Ha Noi Medical Clinic"
            },
            new()
            {
                FullName = "Le Van Cuong",
                Specialty = "Pediatrics",
                ClinicName = "Children Medical Clinic"
            }
        };

        var patients = new List<Patient>
        {
            new()
            {
                FullName = "Pham Minh Duc",
                PhoneNumber = "0900000001",
                Email = "duc@example.com"
            },
            new()
            {
                FullName = "Hoang Thu Ha",
                PhoneNumber = "0900000002",
                Email = "ha@example.com"
            },
            new()
            {
                FullName = "Do Quang Huy",
                PhoneNumber = "0900000003",
                Email = "huy@example.com"
            }
        };

        var slots = new List<Slot>();

        foreach (var doctor in doctors)
        {
            var start = DateTime.UtcNow.Date.AddDays(1).AddHours(1);

            slots.Add(new Slot
            {
                DoctorId = doctor.Id,
                StartTime = start,
                EndTime = start.AddMinutes(30),
                IsBooked = false
            });

            slots.Add(new Slot
            {
                DoctorId = doctor.Id,
                StartTime = start.AddMinutes(30),
                EndTime = start.AddHours(1),
                IsBooked = false
            });
        }

        await dbContext.Doctors.AddRangeAsync(doctors);
        await dbContext.Patients.AddRangeAsync(patients);
        await dbContext.Slots.AddRangeAsync(slots);

        await dbContext.SaveChangesAsync();
    }
}
