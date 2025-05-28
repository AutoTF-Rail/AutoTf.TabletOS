using Autofac.Builder;
using AutoTf.TabletOS.Avalonia.ViewModels.Base;

namespace AutoTf.TabletOS.Avalonia.Extensions;

public static class AutofacExtensions
{
    public static IRegistrationBuilder<T, ConcreteReflectionActivatorData, SingleRegistrationStyle>
        AsyncInit<T>(this IRegistrationBuilder<T, ConcreteReflectionActivatorData, SingleRegistrationStyle> registration)
        where T : ViewModelBase
    {
        return registration.OnActivated(e => e.Instance.InitializeAsync());
    }
}