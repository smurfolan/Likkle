using System.Linq;
using AutoMapper;
using Likkle.BusinessEntities;
using Likkle.BusinessEntities.Requests;
using Likkle.BusinessEntities.Responses;
using Likkle.DataModel;

namespace Likkle.BusinessServices
{
    public class EntitiesMappingProfile : Profile
    {
        public EntitiesMappingProfile()
        {
            CreateMap<Area, AreaDto>();
            CreateMap<Group, GroupDto>();
            CreateMap<User, UserDto>().PreserveReferences(); // Useful for circular references
            CreateMap<UpdateUserInfoRequestDto, User>();
            CreateMap<NewAreaRequest, Area>();
            CreateMap<Language, LanguageDto>();
            CreateMap<NotificationSetting, NotificationSettingDto>();
            CreateMap<StandaloneGroupRequestDto, Group>();
            CreateMap<GroupAsNewAreaRequestDto, Group>();
            CreateMap<NewUserRequestDto, User>();
            CreateMap<Tag, TagDto>().PreserveReferences(); // Useful for circular references
            CreateMap<Group, GroupMetadataResponseDto>()
                .ForMember(dest => dest.NumberOfUsers, opts => opts.MapFrom(src => src.Users.Count))
                .ForMember(dest => dest.TagIds, opts => opts.MapFrom(src => src.Tags.Select(t => t.Id)));
            CreateMap<Area, AreaForLocationResponseDto>();
        }
    }
}
