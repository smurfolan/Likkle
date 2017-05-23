using System;
using System.Linq;
using FluentValidation;
using Likkle.BusinessEntities.Requests;

namespace Likkle.BusinessServices.Validators
{
    public class NewAreaRequestValidator : AbstractValidator<NewAreaRequest>
    {
        private readonly IAreaService _areaService;

        public NewAreaRequestValidator(IAreaService areaService)
        {
            _areaService = areaService;

            RuleFor(request => request.Latitude)
                .Must((request, latitude) => BeNonExistingAreaRequest(request))
                .WithMessage("There's a previous request with these coordinates and radius");
        }

        private bool BeNonExistingAreaRequest(NewAreaRequest request)
        {
            var allAreas = _areaService.GetAllAreas();

            return !allAreas.Any(area =>
                Math.Round(area.Latitude, 6) == Math.Round(request.Latitude, 6) &&
                Math.Round(area.Longitude, 6) == Math.Round(request.Longitude, 6) &&
                area.Radius == request.Radius);
        }
    }
}
