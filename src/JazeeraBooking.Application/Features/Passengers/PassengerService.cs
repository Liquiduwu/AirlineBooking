using JazeeraBooking.Application.Common.Exceptions;
using JazeeraBooking.Application.Common.Interfaces;
using JazeeraBooking.Domain.Entities;

namespace JazeeraBooking.Application.Features.Passengers;

public class PassengerService : IPassengerService
{
    private readonly IPassengerRepository _passengerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PassengerService(IPassengerRepository passengerRepository, IUnitOfWork unitOfWork)
    {
        _passengerRepository = passengerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<PassengerDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var passengers = await _passengerRepository.ListAsync(cancellationToken);
        return passengers.Select(ToDto).ToList();
    }

    public async Task<PassengerDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var passenger = await _passengerRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Passenger), id);
        return ToDto(passenger);
    }

    public async Task<PassengerDto> CreateAsync(CreatePassengerRequest request, CancellationToken cancellationToken = default)
    {
        var existing = await _passengerRepository.GetByPassportNumberAsync(request.PassportNumber, cancellationToken);
        if (existing is not null)
        {
            throw new ConflictException($"A passenger with passport number '{request.PassportNumber}' already exists.");
        }

        var passenger = new Passenger
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            PassportNumber = request.PassportNumber,
            DateOfBirth = request.DateOfBirth
        };

        await _passengerRepository.AddAsync(passenger, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToDto(passenger);
    }

    public async Task<PassengerDto> UpdateAsync(int id, UpdatePassengerRequest request, CancellationToken cancellationToken = default)
    {
        var passenger = await _passengerRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Passenger), id);

        passenger.FirstName = request.FirstName;
        passenger.LastName = request.LastName;
        passenger.Email = request.Email;
        passenger.PhoneNumber = request.PhoneNumber;

        _passengerRepository.Update(passenger);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToDto(passenger);
    }

    private static PassengerDto ToDto(Passenger p) =>
        new(p.Id, p.FirstName, p.LastName, p.Email, p.PhoneNumber, p.PassportNumber, p.DateOfBirth);
}
