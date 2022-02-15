using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using FakeXiecheng.API.Dtos;

namespace FakeXiecheng.API.ValidationAttributes
{
    public class TouristRouteTitleMustBeDifferentFromDescriptionAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var touristRouteDto = (TouristRouteForManipulationDto)validationContext.ObjectInstance;
            if (touristRouteDto.Title == touristRouteDto.Description)
            {
               return new ValidationResult(
                    "Title must not be the same as Description",
                    new[] { "TouristRouteForCreationDto" }
                    );
            }
            return ValidationResult.Success;
        }
    }
}
