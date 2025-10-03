namespace KaivnitBoilerplate.Infrastructure.Installers;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class TargetServiceAttribute(params string[] serviceNames) : Attribute
{
    public string[] ServiceNames { get; } = serviceNames;
}
