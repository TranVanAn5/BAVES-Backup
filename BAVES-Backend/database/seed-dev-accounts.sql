-- Seed 4 demo accounts (password: antran596) — ASP.NET Identity V3 hash
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET NOCOUNT ON;

DECLARE @PasswordHash nvarchar(max) = N'AQAAAAIAAYagAAAAEO2h0JZVkwbiif/jPOH5ZUoBHUnFfmml7Z4V5jeADdqmDOjasOVpe353/TUu29hy/g==';
DECLARE @Now datetimeoffset(3) = SYSDATETIMEOFFSET();

-- Roles
IF NOT EXISTS (SELECT 1 FROM [identity].[Roles] WHERE [NormalizedName] = N'ADMIN')
  INSERT INTO [identity].[Roles] ([Description], [IsActive], [CreatedAt], [Name], [NormalizedName], [ConcurrencyStamp])
  VALUES (N'System administrator', 1, @Now, N'ADMIN', N'ADMIN', CONVERT(nvarchar(36), NEWID()));
IF NOT EXISTS (SELECT 1 FROM [identity].[Roles] WHERE [NormalizedName] = N'INSTRUCTOR')
  INSERT INTO [identity].[Roles] ([Description], [IsActive], [CreatedAt], [Name], [NormalizedName], [ConcurrencyStamp])
  VALUES (N'Instructor', 1, @Now, N'INSTRUCTOR', N'INSTRUCTOR', CONVERT(nvarchar(36), NEWID()));
IF NOT EXISTS (SELECT 1 FROM [identity].[Roles] WHERE [NormalizedName] = N'EXPERT')
  INSERT INTO [identity].[Roles] ([Description], [IsActive], [CreatedAt], [Name], [NormalizedName], [ConcurrencyStamp])
  VALUES (N'Domain expert', 1, @Now, N'EXPERT', N'EXPERT', CONVERT(nvarchar(36), NEWID()));
IF NOT EXISTS (SELECT 1 FROM [identity].[Roles] WHERE [NormalizedName] = N'LEARNER')
  INSERT INTO [identity].[Roles] ([Description], [IsActive], [CreatedAt], [Name], [NormalizedName], [ConcurrencyStamp])
  VALUES (N'BA-VES learner', 1, @Now, N'LEARNER', N'LEARNER', CONVERT(nvarchar(36), NEWID()));

DECLARE @AdminRoleId int = (SELECT [Id] FROM [identity].[Roles] WHERE [NormalizedName] = N'ADMIN');
DECLARE @InstructorRoleId int = (SELECT [Id] FROM [identity].[Roles] WHERE [NormalizedName] = N'INSTRUCTOR');
DECLARE @ExpertRoleId int = (SELECT [Id] FROM [identity].[Roles] WHERE [NormalizedName] = N'EXPERT');
DECLARE @LearnerRoleId int = (SELECT [Id] FROM [identity].[Roles] WHERE [NormalizedName] = N'LEARNER');

-- Helper: upsert user + role
DECLARE @Email nvarchar(320);
DECLARE @FullName nvarchar(200);
DECLARE @RoleId int;
DECLARE @UserId int;

DECLARE @Accounts TABLE (
  Email nvarchar(320),
  FullName nvarchar(200),
  RoleId int
);
INSERT INTO @Accounts (Email, FullName, RoleId) VALUES
  (N'admin@gmail.com', N'Demo Admin', @AdminRoleId),
  (N'instructor@gmail.com', N'Demo Instructor', @InstructorRoleId),
  (N'expert@gmail.com', N'Demo Expert', @ExpertRoleId),
  (N'learner@gmail.com', N'Demo Learner', @LearnerRoleId);

DECLARE acc CURSOR LOCAL FAST_FORWARD FOR
  SELECT Email, FullName, RoleId FROM @Accounts;
OPEN acc;
FETCH NEXT FROM acc INTO @Email, @FullName, @RoleId;
WHILE @@FETCH_STATUS = 0
BEGIN
  SELECT @UserId = [Id] FROM [identity].[Users] WHERE [NormalizedEmail] = UPPER(@Email);
  IF @UserId IS NULL
  BEGIN
    INSERT INTO [identity].[Users] (
      [FullName], [Status], [CreatedAt],
      [UserName], [NormalizedUserName], [Email], [NormalizedEmail],
      [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp],
      [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnabled], [AccessFailedCount]
    ) VALUES (
      @FullName, N'Active', @Now,
      @Email, UPPER(@Email), @Email, UPPER(@Email),
      1, @PasswordHash, CONVERT(nvarchar(36), NEWID()), CONVERT(nvarchar(36), NEWID()),
      0, 0, 1, 0
    );
    SET @UserId = SCOPE_IDENTITY();
  END
  ELSE
  BEGIN
    UPDATE [identity].[Users]
    SET [PasswordHash] = @PasswordHash,
        [Status] = N'Active',
        [EmailConfirmed] = 1,
        [FullName] = @FullName,
        [UpdatedAt] = @Now
    WHERE [Id] = @UserId;
  END

  IF NOT EXISTS (SELECT 1 FROM [identity].[UserRoles] WHERE [UserId] = @UserId AND [RoleId] = @RoleId)
  BEGIN
    INSERT INTO [identity].[UserRoles] ([UserId], [RoleId], [AssignedAt], [AssignedByUserId])
    VALUES (@UserId, @RoleId, @Now, NULL);
  END

  SET @UserId = NULL;
  FETCH NEXT FROM acc INTO @Email, @FullName, @RoleId;
END
CLOSE acc;
DEALLOCATE acc;

SELECT u.Id, u.Email, u.FullName, r.Name AS RoleName
FROM [identity].[Users] u
JOIN [identity].[UserRoles] ur ON ur.UserId = u.Id
JOIN [identity].[Roles] r ON r.Id = ur.RoleId
WHERE u.Email IN (N'admin@gmail.com', N'instructor@gmail.com', N'expert@gmail.com', N'learner@gmail.com')
ORDER BY u.Email;

PRINT N'>> Dev accounts seeded (password: antran596).';
