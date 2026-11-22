namespace BAVES.Api.Shared.Exceptions;

public sealed class ConflictException(string message) : Exception(message);
