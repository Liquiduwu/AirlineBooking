using JazeeraBooking.Application.Common.Exceptions;
using JazeeraBooking.Application.Common.Interfaces;

namespace JazeeraBooking.Application.Features.Aircraft;

public class AircraftService : IAircraftService
{
    private readonly IAircraftRepository _aircraftRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AircraftService(IAircraftRepository aircraftRepository, IUnitOfWork unitOfWork)
    {
        _aircraftRepository = aircraftRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<AircraftDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var aircraft = await _aircraftRepository.ListAsync(cancellationToken);
        return aircraft.Select(ToDto).ToList();
    }

    public async Task<AircraftDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var aircraft = await _aircraftRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Aircraft), id);
        return ToDto(aircraft);
    }

    public async Task<AircraftDto> CreateAsync(CreateAircraftRequest request, CancellationToken cancellationToken = default)
    {
        var existing = await _aircraftRepository.GetByRegistrationAsync(request.RegistrationNumber, cancellationToken);
        if (existing is not null)
        {
            throw new ConflictException($"An aircraft with registration '{request.RegistrationNumber}' already exists.");
        }

        var aircraft = new Domain.Entities.Aircraft
        {
            RegistrationNumber = request.RegistrationNumber,
            Model = request.Model,
            TotalSeats = request.TotalSeats
        };

        await _aircraftRepository.AddAsync(aircraft, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToDto(aircraft);
    }

    private static AircraftDto ToDto(Domain.Entities.Aircraft aircraft) =>
        new(aircraft.Id, aircraft.RegistrationNumber, aircraft.Model, aircraft.TotalSeats);
}
