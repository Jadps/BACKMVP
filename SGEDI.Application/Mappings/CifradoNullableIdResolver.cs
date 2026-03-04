using AutoMapper;
using SGEDI.Domain.Cifrado;

namespace SGEDI.Application.Mappings;

public class CifradoNullableIdResolver : IMemberValueResolver<object, object, int?, string>
{
    private readonly ICifradoService _cifradoService;

    public CifradoNullableIdResolver(ICifradoService cifradoService)
    {
        _cifradoService = cifradoService;
    }

    public string Resolve(object source, object destination, int? sourceMember, string destMember, ResolutionContext context)
    {
        if (sourceMember.HasValue)
            return _cifradoService.Encriptar(sourceMember.Value.ToString());
            
        return null;
    }
}
