using BAVES.Api.Modules.Scenario.Models.DTOs;
using BAVES.Api.Modules.Scenario.Models.Enums;
using BAVES.Api.Modules.Scenario.Services.Interfaces;
using BAVES.Api.Modules.Simulation.Models.Entities;
using BAVES.Api.Modules.Simulation.Models.Enums;
using BAVES.Api.Modules.Simulation.Models.Requests;
using BAVES.Api.Modules.Simulation.Repositories.Interfaces;
using BAVES.Api.Modules.Simulation.Services;
using BAVES.Api.Shared.Exceptions;
using FluentAssertions;
using Moq;
using Xunit;

namespace BAVES.UnitTests.Simulation;

public class SimulationSessionServiceTests
{
    private readonly Mock<ISimulationSessionRepository> _simulationRepoMock = new();
    private readonly Mock<IScenarioCatalogService> _scenarioCatalogServiceMock = new();
    private readonly Mock<IAiRuntimeClient> _aiRuntimeClientMock = new();

    private readonly SimulationSessionService _sut;

    public SimulationSessionServiceTests()
    {
        _sut = new SimulationSessionService(
            _simulationRepoMock.Object,
            _scenarioCatalogServiceMock.Object,
            _aiRuntimeClientMock.Object
        );
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task StartSessionAsync_InvalidScenarioVersionId_ThrowsRequestValidationException(int invalidVersionId)
    {
        // Arrange
        var request = new StartSessionRequest
        {
            ScenarioVersionId = invalidVersionId
        };

        // Act
        var act = () => _sut.StartSessionAsync(1, request);

        // Assert
        var ex = await act.Should().ThrowAsync<RequestValidationException>();
        ex.Which.Errors.Should().ContainKey("ScenarioVersionId");
    }

    [Fact]
    public async Task StartSessionAsync_ScenarioVersionDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var request = new StartSessionRequest { ScenarioVersionId = 999 };

        _scenarioCatalogServiceMock
            .Setup(s => s.GetVersionInfoAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ScenarioVersionInfoDto?)null);

        // Act
        var act = () => _sut.StartSessionAsync(1, request);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*was not found or is not available*");
    }

    [Fact]
    public async Task StartSessionAsync_ScenarioVersionNotPublished_ThrowsNotFoundException()
    {
        // Arrange
        var request = new StartSessionRequest { ScenarioVersionId = 15 };

        _scenarioCatalogServiceMock
            .Setup(s => s.GetVersionInfoAsync(15, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ScenarioVersionInfoDto(15, 1, 1, ScenarioDifficulty.Beginner, IsPublished: false));

        // Act
        var act = () => _sut.StartSessionAsync(1, request);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*was not found or is not available*");
    }

    [Fact]
    public async Task StartSessionAsync_ScenarioVersionHasNoActivePersonas_ThrowsConflictException()
    {
        // Arrange
        var request = new StartSessionRequest { ScenarioVersionId = 15 };

        _scenarioCatalogServiceMock
            .Setup(s => s.GetVersionInfoAsync(15, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ScenarioVersionInfoDto(15, 1, 1, ScenarioDifficulty.Intermediate, IsPublished: true));

        _scenarioCatalogServiceMock
            .Setup(s => s.GetActivePersonasAsync(15, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ScenarioPersonaInfoDto>());

        // Act
        var act = () => _sut.StartSessionAsync(1, request);

        // Assert
        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*has no active stakeholder personas*");
    }

    [Fact]
    public async Task StartSessionAsync_ValidRequest_CreatesSessionAndSessionAgentsSuccessfully()
    {
        // Arrange
        var request = new StartSessionRequest { ScenarioVersionId = 10 };
        const int learnerId = 42;

        _scenarioCatalogServiceMock
            .Setup(s => s.GetVersionInfoAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ScenarioVersionInfoDto(10, 1, 1, ScenarioDifficulty.Advanced, IsPublished: true));

        var personas = new List<ScenarioPersonaInfoDto>
        {
            new(Id: 101, Name: "John Doe", StakeholderRole: "Product Owner", DisplayOrder: 1),
            new(Id: 102, Name: "Jane Smith", StakeholderRole: "Tech Lead", DisplayOrder: 2)
        };

        _scenarioCatalogServiceMock
            .Setup(s => s.GetActivePersonasAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(personas);

        _simulationRepoMock
            .Setup(r => r.GetNextAttemptNoAsync(learnerId, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        // Act
        var result = await _sut.StartSessionAsync(learnerId, request);

        // Assert
        result.Should().NotBeNull();
        result.ScenarioVersionId.Should().Be(10);
        result.AttemptNo.Should().Be(2);
        result.Status.Should().Be(SessionStatus.Active);
        result.Agents.Should().HaveCount(2);

        result.Agents.Should().Contain(a => a.PersonaId == 101 && a.Name == "John Doe" && a.StakeholderRole == "Product Owner");
        result.Agents.Should().Contain(a => a.PersonaId == 102 && a.Name == "Jane Smith" && a.StakeholderRole == "Tech Lead");

        _simulationRepoMock.Verify(r => r.AddSession(It.Is<SimulationSession>(s =>
            s.LearnerId == learnerId &&
            s.ScenarioVersionId == 10 &&
            s.AttemptNo == 2 &&
            s.SessionAgents.Count == 2
        )), Times.Once);

        _simulationRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
