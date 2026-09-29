using Maskinpark.Client.Models;

namespace Maskinpark.Client.Services;

public interface IMachineService
{
    Task<IReadOnlyList<Machine>> GetMachinesAsync();

    Task<Machine?> GetMachineAsync(Guid id);

    Task<Machine> AddMachineAsync(Machine machine);

    Task<Machine?> UpdateMachineDataAsync(Guid id, string data);

    Task<Machine?> RemoveMachineAsync(Guid id);

    Task<Machine?> StartMachineAsync(Guid id);

    Task<Machine?> StopMachineAsync(Guid id);
}
