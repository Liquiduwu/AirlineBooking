using JazeeraBooking.Application.Common.Exceptions;
using JazeeraBooking.Application.Common.Interfaces;

namespace JazeeraBooking.Application.Features.FareClasses;

public class FareClassService : IFareClassService
{
    private readonly IFareClassRepository _fareClassRepository;
    private readonly IUnitOfWork _unitOfWork;

    public FareClassService(IFareClassRepository fareClassRepository, IUnitOfWork unitOfWork)
    {
        _fareClassRepository = fareClassRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<FareClassDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var fareClasses = await _fareClassRepository.ListAsync(cancellationToken);
        return fareClasses.Select(ToDto).ToList();
    }

    public async Task<FareClassDto> CreateAsync(CreateFareClassRequest request, CancellationToken cancellationToken = default)
    {
        var existing = await _fareClassRepository.GetByCodeAsync(request.Code, cancellationToken);
        if (existing is not null)
        {
            throw new ConflictException($"A fare class with code '{request.Code}' already exists.");
        }

        var fareClass = new Domain.Entities.FareClass
        {
            Code = request.Code,
            Name = request.Name,
            CabinClass = request.CabinClass,
            PriceMultiplier = request.PriceMultiplier,
            IsRefundable = request.IsRefundable
        };

        await _fareClassRepository.AddAsync(fareClass, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToDto(fareClass);
    }

    private static FareClassDto ToDto(Domain.Entities.FareClass f) =>
        new(f.Id, f.Code, f.Name, f.CabinClass, f.PriceMultiplier, f.IsRefundable);
}
