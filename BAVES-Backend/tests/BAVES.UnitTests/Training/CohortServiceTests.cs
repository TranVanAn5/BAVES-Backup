using BAVES.Api.Modules.Artifact.Repositories.Interfaces;
using BAVES.Api.Modules.Assessment.Repositories.Interfaces;
using BAVES.Api.Modules.Identity.Services.Interfaces;
using BAVES.Api.Modules.Simulation.Services.Interfaces;
using BAVES.Api.Modules.Training.Models.DTOs;
using BAVES.Api.Modules.Training.Models.Entities;
using BAVES.Api.Modules.Training.Models.Enums;
using BAVES.Api.Modules.Training.Models.Requests;
using BAVES.Api.Modules.Training.Repositories.Interfaces;
using BAVES.Api.Modules.Training.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace BAVES.UnitTests.Training;

public class CohortServiceTests
{
    private readonly Mock<ICohortRepository> _cohortRepoMock = new();
    private readonly Mock<IUserService> _userServiceMock = new();
    private readonly Mock<ISimulationSessionService> _simulationSessionServiceMock = new();
    private readonly Mock<IArtifactRepository> _artifactRepoMock = new();
    private readonly Mock<IAssessmentRepository> _assessmentRepoMock = new();

    private readonly CohortService _sut;

    public CohortServiceTests()
    {
        _sut = new CohortService(
            _cohortRepoMock.Object,
            _userServiceMock.Object,
            _simulationSessionServiceMock.Object,
            _artifactRepoMock.Object,
            _assessmentRepoMock.Object
        );
    }

    [Fact]
    public async Task CreateCohortAsync_StartDateAfterEndDate_ThrowsArgumentException()
    {
        // Arrange
        var request = new CreateCohortRequest
        {
            Code = "SE101",
            Name = "Software Engineering Class",
            StartDate = new DateOnly(2026, 12, 31),
            EndDate = new DateOnly(2026, 1, 1)
        };

        // Act
        var act = () => _sut.CreateCohortAsync(1, request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Ngày kết thúc (EndDate) phải lớn hơn hoặc bằng ngày bắt đầu (StartDate).*");
    }

    [Fact]
    public async Task CreateCohortAsync_DuplicateCohortCode_ThrowsInvalidOperationException()
    {
        // Arrange
        var request = new CreateCohortRequest
        {
            Code = "SE101",
            Name = "Software Engineering Class",
            StartDate = new DateOnly(2026, 1, 1),
            EndDate = new DateOnly(2026, 6, 1)
        };

        _cohortRepoMock
            .Setup(c => c.ExistsByCodeAsync("SE101", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var act = () => _sut.CreateCohortAsync(1, request);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Mã lớp học 'SE101' đã tồn tại trong hệ thống.*");
    }

    [Fact]
    public async Task CreateCohortAsync_ValidRequest_CreatesAndReturnsCohortDetailDto()
    {
        // Arrange
        var request = new CreateCohortRequest
        {
            Code = "se101",
            Name = "Software Engineering Class",
            Description = "Course 2026",
            StartDate = new DateOnly(2026, 1, 1),
            EndDate = new DateOnly(2026, 6, 1),
            Status = CohortStatus.Active
        };

        _cohortRepoMock
            .Setup(c => c.ExistsByCodeAsync("SE101", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.CreateCohortAsync(1, request);

        // Assert
        result.Should().NotBeNull();
        result.Code.Should().Be("SE101");
        result.Name.Should().Be("Software Engineering Class");
        result.Status.Should().Be(CohortStatus.Active);
        result.InstructorId.Should().Be(1);

        _cohortRepoMock.Verify(c => c.AddCohort(It.Is<Cohort>(e =>
            e.Code == "SE101" &&
            e.InstructorId == 1 &&
            e.Status == CohortStatus.Active
        )), Times.Once);
        _cohortRepoMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetCohortByIdAsync_CohortNotFound_ReturnsNull()
    {
        // Arrange
        _cohortRepoMock
            .Setup(c => c.GetDetailByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CohortDetailDto?)null);

        // Act
        var result = await _sut.GetCohortByIdAsync(1, "INSTRUCTOR", 999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetCohortByIdAsync_InstructorAccessesOtherInstructorCohort_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var detail = new CohortDetailDto(
            Id: 10,
            InstructorId: 99, // Owned by instructor 99
            Code: "SE101",
            Name: "SE Class",
            Description: null,
            StartDate: null,
            EndDate: null,
            Status: CohortStatus.Active,
            ActiveMemberCount: 10,
            ActiveAssignmentCount: 2,
            TotalAssignmentCount: 3,
            CreatedAt: DateTimeOffset.UtcNow,
            UpdatedAt: null
        );

        _cohortRepoMock
            .Setup(c => c.GetDetailByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(detail);

        // Act - Instructor 1 tries to access Instructor 99's cohort
        var act = () => _sut.GetCohortByIdAsync(1, "INSTRUCTOR", 10);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Bạn không có quyền truy cập lớp học của giảng viên khác.*");
    }

    [Fact]
    public async Task GetCohortByIdAsync_LearnerNotMemberOfCohort_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var detail = new CohortDetailDto(
            Id: 10,
            InstructorId: 2,
            Code: "SE101",
            Name: "SE Class",
            Description: null,
            StartDate: null,
            EndDate: null,
            Status: CohortStatus.Active,
            ActiveMemberCount: 10,
            ActiveAssignmentCount: 2,
            TotalAssignmentCount: 3,
            CreatedAt: DateTimeOffset.UtcNow,
            UpdatedAt: null
        );

        _cohortRepoMock
            .Setup(c => c.GetDetailByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(detail);

        _cohortRepoMock
            .Setup(c => c.IsLearnerInCohortAsync(10, 50, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act - Learner 50 is not enrolled
        var act = () => _sut.GetCohortByIdAsync(50, "LEARNER", 10);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Bạn không phải là học viên thuộc lớp học này.*");
    }

    [Fact]
    public async Task GetCohortByIdAsync_AdminAccessesAnyCohort_ReturnsDetail()
    {
        // Arrange
        var detail = new CohortDetailDto(
            Id: 10,
            InstructorId: 2,
            Code: "SE101",
            Name: "SE Class",
            Description: null,
            StartDate: null,
            EndDate: null,
            Status: CohortStatus.Active,
            ActiveMemberCount: 10,
            ActiveAssignmentCount: 2,
            TotalAssignmentCount: 3,
            CreatedAt: DateTimeOffset.UtcNow,
            UpdatedAt: null
        );

        _cohortRepoMock
            .Setup(c => c.GetDetailByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(detail);

        // Act - Admin can access
        var result = await _sut.GetCohortByIdAsync(1, "ADMIN", 10);

        // Assert
        result.Should().NotBeNull();
        result!.Code.Should().Be("SE101");
    }

    [Fact]
    public async Task DeleteCohortAsync_DraftWithNoMembersOrAssignments_RemovesAndReturnsTrue()
    {
        // Arrange
        var entity = new Cohort
        {
            Id = 5,
            InstructorId = 1,
            Status = CohortStatus.Draft
        };

        _cohortRepoMock
            .Setup(c => c.GetEntityWithCountsByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        // Act
        var result = await _sut.DeleteCohortAsync(1, "INSTRUCTOR", 5);

        // Assert
        result.Should().BeTrue();
        _cohortRepoMock.Verify(c => c.RemoveCohort(entity), Times.Once);
        _cohortRepoMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteCohortAsync_ActiveCohort_ArchivesInsteadOfHardDelete()
    {
        // Arrange
        var entity = new Cohort
        {
            Id = 5,
            InstructorId = 1,
            Status = CohortStatus.Active
        };

        _cohortRepoMock
            .Setup(c => c.GetEntityWithCountsByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        // Act
        var result = await _sut.DeleteCohortAsync(1, "INSTRUCTOR", 5);

        // Assert
        result.Should().BeTrue();
        entity.Status.Should().Be(CohortStatus.Archived);
        _cohortRepoMock.Verify(c => c.RemoveCohort(It.IsAny<Cohort>()), Times.Never);
        _cohortRepoMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
