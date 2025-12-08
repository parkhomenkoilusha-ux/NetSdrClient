using ArchTestDemo.Application; // UI знає тільки про Application

namespace ArchTestDemo.UI;

public class UIClass
{
    private readonly IDataService _service;

    // Ми отримуємо сервіс через конструктор (Dependency Injection)
    // Ніякого 'new InfrastructureClass()' тут більше немає!
    public UIClass(IDataService service)
    {
        _service = service;
    }

    public void DoWork()
    {
        var data = _service.GetData();
    }
}