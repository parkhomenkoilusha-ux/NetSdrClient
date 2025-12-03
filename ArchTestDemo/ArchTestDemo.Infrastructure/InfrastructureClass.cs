using ArchTestDemo.Application;

namespace ArchTestDemo.Infrastructure;

public class InfrastructureClass : IDataService // Реалізуємо інтерфейс
{
    public string GetData() => "Дані з Інфраструктури (правильно)";
}