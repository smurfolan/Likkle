using System.Linq;
using AutoMapper;
using Likkle.BusinessEntities;
using Likkle.BusinessEntities.Requests;
using Likkle.BusinessEntities.Responses;
using Likkle.DataModel;
using Likkle.BusinessEntities.SignalrDtos;

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
            CreateMap<User, UserInfoResponseDto>();
            CreateMap<NewAreaRequest, Area>();
            CreateMap<Language, LanguageDto>();
            CreateMap<AutomaticSubscriptionSetting, AutomaticSubscriptionSettingsDto>();
            CreateMap<AutomaticSubscriptionSettingsDto, AutomaticSubscriptionSetting>();
            CreateMap<StandaloneGroupRequestDto, Group>();
            CreateMap<GroupAsNewAreaRequestDto, Group>();
            CreateMap<NewUserRequestDto, User>();
            CreateMap<Tag, TagDto>().PreserveReferences(); // Useful for circular references
            CreateMap<Group, GroupMetadataResponseDto>()
                .ForMember(dest => dest.NumberOfUsers, opts => opts.MapFrom(src => src.Users.Count))
                .ForMember(dest => dest.TagIds, opts => opts.MapFrom(src => src.Tags.Select(t => t.Id)));
            CreateMap<Area, AreaForLocationResponseDto>();
            CreateMap<SocialLinksResponseDto, SocialLinksDto>();

            // SignalR mappings
            CreateMap<Group, SRGroupDto>()
                .ForMember(dest => dest.TagIds, opts => opts.MapFrom(src => src.Tags.Select(t => t.Id)))
                .ForMember(dest => dest.UserIds, opts => opts.MapFrom(src => src.Users.Select(u => u.Id)));

            CreateMap<Area, SRAreaDto>()
                .ForMember(dest => dest.GroupIds, opts => opts.MapFrom(src => src.Groups.Select(gr => gr.Id)));
        }
    }
}
