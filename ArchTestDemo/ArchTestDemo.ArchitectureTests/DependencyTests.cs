using NetArchTest.Rules;
using Xunit;
using System.Reflection;
using ArchTestDemo.UI;             // Щоб бачити UIClass
using ArchTestDemo.Infrastructure; // Щоб бачити InfrastructureClass

namespace ArchTestDemo.ArchitectureTests
{
    public class DependencyTests
    {
        // 1. Визначаємо назву "забороненого" простору імен
        private const string InfrastructureNamespace = "ArchTestDemo.Infrastructure";
        
        // 2. Отримуємо збірку (Assembly), яку будемо перевіряти (UI)
        private static readonly Assembly UIAssembly = typeof(UIClass).Assembly;
        
        [Fact]
        public void UI_Should_Not_Depend_On_Infrastructure()
        {
            // Act
            var result = Types
                .InAssembly(UIAssembly) // Беремо всі класи з UI
                .ShouldNot()            // Кажемо "НЕ ПОВИННІ"
                .HaveDependencyOn(InfrastructureNamespace) // Залежати від Infrastructure
                .GetResult();

            // Assert
            Assert.True(result.IsSuccessful, 
                $"АРХІТЕКТУРНЕ ПОРУШЕННЯ: UI не має залежати від Infrastructure! Знайдено у: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? new string[0])}");
        }
    }
}