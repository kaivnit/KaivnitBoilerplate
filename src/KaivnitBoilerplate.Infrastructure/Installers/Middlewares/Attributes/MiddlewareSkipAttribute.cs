namespace KaivnitBoilerplate.Infrastructure.Installers;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class MiddlewareSkipAttribute : Attribute
{
}
