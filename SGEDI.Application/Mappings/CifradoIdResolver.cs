using AutoMapper;
using SGEDI.Domain.Cifrado;

namespace SGEDI.Application.Mappings;

public class CifradoIdResolver : IMemberValueResolver<object, object, int, string>
{
    private readonly ICifradoService _cifradoService;

    public CifradoIdResolver(ICifradoService cifradoService)
    {
        _cifradoService = cifradoService;
    }

    public string Resolve(object source, object destination, int sourceMember, string destMember, ResolutionContext context)
    {
        return _cifradoService.Encriptar(sourceMember.ToString());
    }
}
