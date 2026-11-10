using DriveNow.Domain.Abstractions;
using DriveNow.Domain.Entities;
using DriveNow.Infrastructure.Persistence;

namespace DriveNow.Infrastructure.Repositories;

public class VehicleRepository : IVehicleRepository
{
    readonly InMemoryDatabase _database;

    public VehicleRepository(InMemoryDatabase database) => _database = database;

    public Vehicle? GetById(int id) =>
        _database.Vehicles.FirstOrDefault(vehicle => vehicle.Id == id);

    public List<Vehicle> GetAll() => _database.Vehicles.ToList();

    public void Update(Vehicle vehicle)
    {
        var index = _database.Vehicles.FindIndex(item => item.Id == vehicle.Id);
        if (index >= 0)
            _database.Vehicles[index] = vehicle;
    }
}