using Appointment.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration
    .GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Database connection string 'DefaultConnection' is missing.");
}

builder.Services.AddDbContext<MedicalQueueDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddOpenApi();

var app = builder.Build();

// Seed sample data.
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider
        .GetRequiredService<MedicalQueueDbContext>();

    await DbSeeder.SeedAsync(dbContext);
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Root endpoint.
app.MapGet("/", () => Results.Ok(new
{
    Service = "Appointment API",
    Status = "Running"
}));

// Database health check.
app.MapGet("/health/db", async (
    MedicalQueueDbContext dbContext,
    CancellationToken cancellationToken) =>
{
    var canConnect = await dbContext.Database
        .CanConnectAsync(cancellationToken);

    return canConnect
        ? Results.Ok(new { Database = "Connected" })
        : Results.Problem("Cannot connect to PostgreSQL.");
});

// GET /doctors
app.MapGet("/doctors", async (
    MedicalQueueDbContext dbContext,
    CancellationToken cancellationToken) =>
{
    var doctors = await dbContext.Doctors
        .AsNoTracking()
        .OrderBy(d => d.FullName)
        .Select(d => new
        {
            d.Id,
            d.FullName,
            d.Specialty,
            d.ClinicName
        })
        .ToListAsync(cancellationToken);

    return Results.Ok(doctors);
});

// GET /doctors/{id}/slots
app.MapGet("/doctors/{id:guid}/slots", async (
    Guid id,
    MedicalQueueDbContext dbContext,
    CancellationToken cancellationToken) =>
{
    var doctorExists = await dbContext.Doctors
        .AnyAsync(d => d.Id == id, cancellationToken);

    if (!doctorExists)
    {
        return Results.NotFound(new
        {
            Message = "Doctor not found."
        });
    }

    var now = DateTime.UtcNow;

    var slots = await dbContext.Slots
        .AsNoTracking()
        .Where(s =>
            s.DoctorId == id &&
            !s.IsBooked &&
            s.StartTime > now)
        .OrderBy(s => s.StartTime)
        .Select(s => new
        {
            s.Id,
            s.DoctorId,
            s.StartTime,
            s.EndTime,
            s.IsBooked
        })
        .ToListAsync(cancellationToken);

    return Results.Ok(slots);
});


// POST /appointments
app.MapPost("/appointments", async (
    CreateAppointmentRequest request,
    MedicalQueueDbContext dbContext,
    CancellationToken cancellationToken) =>
{
    // Check patient.
    var patientExists = await dbContext.Patients
        .AnyAsync(p => p.Id == request.PatientId, cancellationToken);

    if (!patientExists)
    {
        return Results.NotFound(new
        {
            Message = "Patient not found."
        });
    }

    // Find an available slot belonging to the selected doctor.
    var slot = await dbContext.Slots
        .FirstOrDefaultAsync(
            s => s.Id == request.SlotId
                && s.DoctorId == request.DoctorId,
            cancellationToken);

    if (slot is null)
    {
        return Results.NotFound(new
        {
            Message = "Slot not found for this doctor."
        });
    }

    if (slot.IsBooked)
    {
        return Results.Conflict(new
        {
            Message = "This slot has already been booked."
        });
    }

    if (slot.StartTime <= DateTime.UtcNow)
    {
        return Results.BadRequest(new
        {
            Message = "Cannot book a slot in the past."
        });
    }

    // Create appointment and mark slot as booked atomically.
    await using var transaction =
        await dbContext.Database.BeginTransactionAsync(cancellationToken);

    var appointment = new Appointment.Api.Entities.Appointment
    {
        PatientId = request.PatientId,
        DoctorId = request.DoctorId,
        SlotId = request.SlotId,
        Status = "Pending",
        CreatedAt = DateTime.UtcNow
    };

    slot.IsBooked = true;

    dbContext.Appointments.Add(appointment);

    try
    {
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Results.Created(
            $"/appointments/{appointment.Id}",
            new
            {
                appointment.Id,
                appointment.PatientId,
                appointment.DoctorId,
                appointment.SlotId,
                appointment.Status,
                appointment.CreatedAt
            });
    }
    catch (DbUpdateException)
    {
        await transaction.RollbackAsync(cancellationToken);

        return Results.Conflict(new
        {
            Message = "This slot may have been booked by another request."
        });
    }
});


// GET /appointments/{id}
app.MapGet("/appointments/{id:guid}", async (
    Guid id,
    MedicalQueueDbContext dbContext,
    CancellationToken cancellationToken) =>
{
    var appointment = await dbContext.Appointments
        .AsNoTracking()
        .Where(a => a.Id == id)
        .Select(a => new
        {
            a.Id,
            a.Status,
            a.CreatedAt,

            Patient = new
            {
                a.Patient.Id,
                a.Patient.FullName,
                a.Patient.PhoneNumber,
                a.Patient.Email
            },

            Doctor = new
            {
                a.Doctor.Id,
                a.Doctor.FullName,
                a.Doctor.Specialty,
                a.Doctor.ClinicName
            },

            Slot = new
            {
                a.Slot.Id,
                a.Slot.StartTime,
                a.Slot.EndTime
            },

            QueueNumber = a.QueueNumber == null
                ? null
                : new
                {
                    a.QueueNumber.Number,
                    a.QueueNumber.Status
                }
        })
        .FirstOrDefaultAsync(cancellationToken);

    if (appointment is null)
    {
        return Results.NotFound(new
        {
            Message = "Appointment not found."
        });
    }

    return Results.Ok(appointment);
});


// DELETE /appointments/{id}
app.MapDelete("/appointments/{id:guid}", async (
    Guid id,
    MedicalQueueDbContext dbContext,
    CancellationToken cancellationToken) =>
{
    var appointment = await dbContext.Appointments
        .Include(a => a.Slot)
        .FirstOrDefaultAsync(
            a => a.Id == id,
            cancellationToken);

    if (appointment is null)
    {
        return Results.NotFound(new
        {
            Message = "Appointment not found."
        });
    }

    if (appointment.Status == "Cancelled")
    {
        return Results.Conflict(new
        {
            Message = "Appointment has already been cancelled."
        });
    }

    appointment.Status = "Cancelled";
    appointment.Slot.IsBooked = false;

    await dbContext.SaveChangesAsync(cancellationToken);

    return Results.Ok(new
    {
        appointment.Id,
        appointment.Status,
        Message = "Appointment cancelled successfully."
    });
});


// GET /queues/{doctorId}
app.MapGet("/queues/{doctorId:guid}", async (
    Guid doctorId,
    MedicalQueueDbContext dbContext,
    CancellationToken cancellationToken) =>
{
    var doctorExists = await dbContext.Doctors
        .AnyAsync(d => d.Id == doctorId, cancellationToken);

    if (!doctorExists)
    {
        return Results.NotFound(new
        {
            Message = "Doctor not found."
        });
    }

    var queues = await dbContext.QueueNumbers
        .AsNoTracking()
        .Where(q => q.DoctorId == doctorId && q.Status != "Cancelled")
        .OrderBy(q => q.Number)
        .Select(q => new
        {
            q.Id,
            q.AppointmentId,
            q.Number,
            q.Status,
            q.CreatedAt
        })
        .ToListAsync(cancellationToken);

    return Results.Ok(queues);
});

app.Run();

public record CreateAppointmentRequest(
    Guid PatientId,
    Guid DoctorId,
    Guid SlotId
);