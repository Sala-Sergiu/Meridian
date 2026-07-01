using Meridian.Domain.Repositories;
using NSubstitute;

namespace Meridian.UnitTests.Services;

// Unit tests for BLL services. Mock repository interfaces (NSubstitute).
public class SampleServiceTests
{
    private readonly ISampleRepository _repository = Substitute.For<ISampleRepository>();

    // TODO: add tests for SampleService behaviour per spec.
}
