namespace Infrastructure
{
    public class ShipmentNotFoundException(string message) : Exception(message)
    {
    }
    public class InvalidShipmentOperationException(string message, Exception? innerException = null) : Exception(message, innerException)
    {
    }
    public class InvalidShipmentDataException(string message) : Exception(message)
    {
    }

    public class ShipmentDataAccessException(string message, Exception innerException) : Exception(message, innerException)
    {
    }
}
