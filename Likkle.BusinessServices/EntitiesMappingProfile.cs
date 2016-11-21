using AutoMapper;
using Likkle.BusinessEntities;
using Likkle.BusinessEntities.Requests;
using Likkle.DataModel;

namespace Likkle.BusinessServices
{
    public class EntitiesMappingProfile : Profile
    {
        public EntitiesMappingProfile()
        {
            CreateMap<Area, AreaDto>();
            CreateMap<Group, GroupDto>();
            CreateMap<User, UserDto>();
            CreateMap<NewAreaRequest, Area>();
            CreateMap<Language, LanguageDto>();
            CreateMap<NotificationSetting, NotificationSettingDto>();
            CreateMap<StandaloneGroupRequestDto, Group>();
            CreateMap<GroupAsNewAreaRequestDto, Group>();
            CreateMap<NewUserRequestDto, User>();
            CreateMap<Tag, TagDto>();
        }
    }
}
