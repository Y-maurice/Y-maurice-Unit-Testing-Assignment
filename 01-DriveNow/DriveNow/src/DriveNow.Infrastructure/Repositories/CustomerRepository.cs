using DriveNow.Domain.Abstractions;
using DriveNow.Domain.Entities;
using DriveNow.Infrastructure.Persistence;

namespace DriveNow.Infrastructure.Repositories;

public class CustomerRepository : ICustomerRepository
{
    readonly InMemoryDatabase _database;

    public CustomerRepository(InMemoryDatabase database) => _database = database;

    public Customer? GetById(int id) =>
        _database.Customers.FirstOrDefault(customer => customer.Id == id);

    public List<Customer> GetAll() => _database.Customers.ToList();

    public void Update(Customer customer)
    {
        var index = _database.Customers.FindIndex(item => item.Id == customer.Id);
        if (index >= 0)
            _database.Customers[index] = customer;
    }
}