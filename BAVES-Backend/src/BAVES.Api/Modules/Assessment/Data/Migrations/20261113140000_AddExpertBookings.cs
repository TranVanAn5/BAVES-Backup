using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BAVES.Api.Modules.Assessment.Data.Migrations;

/// <summary>
/// Tables ExpertBookings / ConsultationMessages already existed in some environments.
/// This migration is idempotent so EF history can catch up safely.
/// </summary>
public partial class AddExpertBookings : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            IF OBJECT_ID(N'assessment.ExpertBookings', N'U') IS NULL
            BEGIN
                CREATE TABLE assessment.ExpertBookings (
                    Id int NOT NULL IDENTITY(1, 1),
                    ReviewRequestId int NOT NULL,
                    SubmissionId int NOT NULL,
                    LearnerId int NOT NULL,
                    ExpertId int NOT NULL,
                    Status nvarchar(20) NOT NULL,
                    ScheduledStartTime datetimeoffset NULL,
                    ScheduledEndTime datetimeoffset NULL,
                    ActualStartTime datetimeoffset NULL,
                    ActualEndTime datetimeoffset NULL,
                    LearnerNote nvarchar(1000) NULL,
                    ExpertSummaryNote nvarchar(2000) NULL,
                    CreatedAt datetimeoffset NOT NULL,
                    CONSTRAINT PK_ExpertBookings PRIMARY KEY (Id),
                    CONSTRAINT FK_ExpertBookings_ReviewRequests_ReviewRequestId
                        FOREIGN KEY (ReviewRequestId) REFERENCES assessment.ReviewRequests (Id) ON DELETE NO ACTION
                );
                CREATE INDEX IX_ExpertBookings_LearnerId ON assessment.ExpertBookings (LearnerId);
                CREATE INDEX IX_ExpertBookings_ExpertId ON assessment.ExpertBookings (ExpertId);
                CREATE INDEX IX_ExpertBookings_ReviewRequestId ON assessment.ExpertBookings (ReviewRequestId);
            END

            IF OBJECT_ID(N'assessment.ConsultationMessages', N'U') IS NULL
            BEGIN
                CREATE TABLE assessment.ConsultationMessages (
                    Id bigint NOT NULL IDENTITY(1, 1),
                    BookingId int NOT NULL,
                    SenderId int NOT NULL,
                    SenderRole nvarchar(20) NOT NULL,
                    Content nvarchar(max) NOT NULL,
                    SentAt datetimeoffset NOT NULL,
                    IsRead bit NOT NULL,
                    CONSTRAINT PK_ConsultationMessages PRIMARY KEY (Id),
                    CONSTRAINT FK_ConsultationMessages_ExpertBookings_BookingId
                        FOREIGN KEY (BookingId) REFERENCES assessment.ExpertBookings (Id) ON DELETE CASCADE
                );
                CREATE INDEX IX_ConsultationMessages_BookingId_SentAt
                    ON assessment.ConsultationMessages (BookingId, SentAt);
            END
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            IF OBJECT_ID(N'assessment.ConsultationMessages', N'U') IS NOT NULL
                DROP TABLE assessment.ConsultationMessages;
            IF OBJECT_ID(N'assessment.ExpertBookings', N'U') IS NOT NULL
                DROP TABLE assessment.ExpertBookings;
            """);
    }
}
