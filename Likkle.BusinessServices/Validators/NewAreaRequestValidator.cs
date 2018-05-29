using System.Device.Location;
using System.Linq;
using FluentValidation;
using Likkle.BusinessEntities.Requests;

namespace Likkle.BusinessServices.Validators
{
    public class NewAreaRequestValidator : AbstractValidator<NewAreaRequest>
    {
        private readonly IAreaService _areaService;
        private int _minimalDistanceBetweenTwoAreaCentersWithSameRadius;

        public NewAreaRequestValidator(
            IAreaService areaService,
            IConfigurationWrapper configurationWrapper)
        {
            _areaService = areaService;
            _minimalDistanceBetweenTwoAreaCentersWithSameRadius = configurationWrapper
                .MinimalDistanceBetweenTwoAreaCentersWithSameRadius;

            RuleFor(request => request.Latitude)
                .Must((request, latitude) => BeNonExistingAreaRequest(request))
                .WithMessage("There's a previous request close coordinates and radius");
        }

        private bool BeNonExistingAreaRequest(NewAreaRequest request)
        {
            var allAreas = _areaService.GetAllAreas();
            
            var requestCenter = new GeoCoordinate(request.Latitude, request.Longitude);

            return !allAreas.Any(area =>
                requestCenter.GetDistanceTo(new GeoCoordinate(area.Latitude, area.Longitude)) <= _minimalDistanceBetweenTwoAreaCentersWithSameRadius 
                &&
                area.Radius == request.Radius);
        }
    }
}
