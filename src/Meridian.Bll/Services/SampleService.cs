using Meridian.Domain.Repositories;

namespace Meridian.Bll.Services;

// Business logic for the sample slice. Depends on Domain repository interfaces only.
public class SampleService : ISampleService
{
    private readonly ISampleRepository _repository;

    public SampleService(ISampleRepository repository)
    {
        _repository = repository;
    }

    // TODO: implement ISampleService members per spec.
}
