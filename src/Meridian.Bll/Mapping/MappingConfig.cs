using Mapster;
using Meridian.Bll.Dtos;
using Meridian.Domain.Entities;

namespace Meridian.Bll.Mapping;

// Central Mapster configuration (entity <-> DTO mappings).
public class MappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<User, UserDto>()
            .Map(dest => dest.Role, src => src.Role.ToString());

        config.NewConfig<TemplateCard, TemplateCardDto>()
            .Map(dest => dest.Type, src => src.Type.ToString());

        config.NewConfig<BoardCard, BoardCardDto>()
            .Map(dest => dest.Type, src => src.Type.ToString())
            .Map(dest => dest.Status, src => src.Status.ToString());
    }
}
