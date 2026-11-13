using DriveNow.Domain.Entities;

namespace DriveNow.Domain.Abstractions;

public interface IVehicleRepository
{
    Vehicle? GetById(int id);

    List<Vehicle> GetAll();

    void Update(Vehicle vehicle);
}