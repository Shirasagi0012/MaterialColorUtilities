using Avalonia.Data;

namespace MaterialColorUtilities.Avalonia;

public class MaterialColorSchemeExtension : MaterialColorScheme
{
    public MaterialColorSchemeExtension()
    {
    }

    public MaterialColorSchemeExtension(ISchemeProvider scheme) : base(scheme)
    {
    }

    public MaterialColorSchemeExtension(IBinding scheme) : base(scheme)
    {
        
    }

    public MaterialColorScheme ProvideTypedValue(IServiceProvider serviceProvider)
    {
        return this;
    }
}
