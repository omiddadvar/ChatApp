namespace ChatApp.API.Modules.Shared.Kernel.Abstractions;

public interface ISeeder
{
    Task SeedAsync(CancellationToken ct = default);
}
