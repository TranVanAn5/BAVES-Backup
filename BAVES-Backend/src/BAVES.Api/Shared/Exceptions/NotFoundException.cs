namespace BAVES.Api.Shared.Exceptions;

public sealed class NotFoundException(string message) : Exception(message);
