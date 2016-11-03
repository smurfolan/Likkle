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

            //CreateMap<RadiusDto, Radius>();
            //CreateMap<Radius, RadiusDto>();

            //CreateMap<AreaDto, Area>();
            //CreateMap<Area, AreaDto>()
            //    .ForMember(m => m.Groups, opt => opt.Ignore());

            //CreateMap<User, UserDto>()
            //    .ForMember(m => m.Groups, opt => opt.Ignore())
            //    .ForMember(m => m.Languages, opt => opt.Ignore());

            //CreateMap<UserDto, User>();

            //CreateMap<Language, LanguageDto>();
            //CreateMap<LanguageDto, Language>();

            //CreateMap<Group, GroupDto>()
            //    .ForMember(m => m.Tags, opt => opt.Ignore())
            //    .ForMember(m => m.Areas, opt => opt.Ignore())
            //    .ForMember(m => m.Users, opt => opt.Ignore());
            //CreateMap<GroupDto, Group>();

            //CreateMap<Tag, TagDto>();
            //CreateMap<TagDto, Tag>();
        }
    }
}
