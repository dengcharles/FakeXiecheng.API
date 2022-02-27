using FakeXiecheng.API.Dtos;
using FakeXiecheng.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using System.Text.RegularExpressions;
using FakeXiecheng.API.ResourceParameters;
using FakeXiecheng.API.Models;
using Microsoft.AspNetCore.JsonPatch;
using FakeXiecheng.API.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace FakeXiecheng.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TouristRoutesController : ControllerBase
    {
        private ITouristRouteRepository _touristRouteRepository;
        private readonly IMapper _mapper;
        private readonly IUrlHelper _urlHelper;
        private readonly IPropertyMappingService _propertyMappingService;
        public TouristRoutesController(
            ITouristRouteRepository touristRouteRepository,
            IMapper mapper, IUrlHelperFactory urlHelperFactory,
            IActionContextAccessor actionContextAccessor,
            IPropertyMappingService propertyMappingService
            )
        {
            _touristRouteRepository = touristRouteRepository;
            _mapper = mapper;
            _urlHelper = urlHelperFactory.GetUrlHelper(actionContextAccessor.ActionContext);
            _propertyMappingService = propertyMappingService;
        }

        private string GenerateTouristRouteResourceURL(TouristRouteResourceParameters parameters,
            PaginationResourceParameters parameters2,
            ResourceUriType type
            )
        {
            return type switch
            {
                ResourceUriType.PreviousPage => _urlHelper.Link("GetTouristRoutes",
                    new
                    {
                        fields = parameters.Fields,
                        orderBy = parameters.OrderBy,
                        keyword = parameters.Keyword,
                        rating = parameters.Rating,
                        pageNumber = parameters2.PageNumber - 1,
                        pageSize = parameters2.PageSize
                    }),
                ResourceUriType.NextPage => _urlHelper.Link("GetTouristRoutes",
                    new
                    {
                        fields = parameters.Fields,
                        orderBy = parameters.OrderBy,
                        keyword = parameters.Keyword,
                        rating = parameters.Rating,
                        pageNumber = parameters2.PageNumber + 1,
                        pageSize = parameters2.PageSize
                    }),
                _ => _urlHelper.Link("GetTouristRoutes",
                    new
                    {
                        fields = parameters.Fields,
                        orderBy = parameters.OrderBy,
                        keyword = parameters.Keyword,
                        rating = parameters.Rating,
                        pageNumber = parameters2.PageNumber,
                        pageSize = parameters2.PageSize
                    })
            };
        }

        //api/touristRoutes?keyword=value
        [HttpGet(Name = "GetTouristRoutes")]
        [HttpHead]
        public async Task<IActionResult> GetTouristRoutes(
            [FromQuery] TouristRouteResourceParameters parameters,
            [FromQuery] PaginationResourceParameters parameters2
            )
        {
            if(!_propertyMappingService.IsMappingExists<TouristRouteDto, TouristRoute>(parameters.OrderBy))
            {
                return BadRequest("Please enter correct order by value");
            }

            if (!_propertyMappingService.IsPropertiesExists<TouristRouteDto>(parameters.Fields))
            {
                return BadRequest("Please enter correct shape data field value");
            }

            var touristRoutesFromRepo = await _touristRouteRepository
                .GetTouristRoutesAsync(
                parameters.Keyword,
                parameters.RatingOperator,
                parameters.RatingValue,
                parameters2.PageSize,
                parameters2.PageNumber,
                parameters.OrderBy);
            if (touristRoutesFromRepo == null || touristRoutesFromRepo.Count() <= 0)
            {
                return NotFound("No tourist route available");
            }
            var touristRouteDtos = _mapper.Map<IEnumerable<TouristRouteDto>>(touristRoutesFromRepo);

            var previousPageLink = touristRoutesFromRepo.HasPrevious
                ? GenerateTouristRouteResourceURL(parameters, parameters2, ResourceUriType.PreviousPage)
                : null;

            var nextPageLink = touristRoutesFromRepo.HasNext
                ? GenerateTouristRouteResourceURL(parameters, parameters2, ResourceUriType.NextPage)
                : null;

            // x-pagination
            var paginationMetadata = new
            {
                previousPageLink,
                nextPageLink,
                totalCount = touristRoutesFromRepo.TotalCount,
                pageSize = touristRoutesFromRepo.PageSize,
                currentPage = touristRoutesFromRepo.CurrentPage,
                totalPages = touristRoutesFromRepo.TotalPages
            };

            Response.Headers.Add("x-pagination", Newtonsoft.Json.JsonConvert.SerializeObject(paginationMetadata));

            return Ok(touristRouteDtos.ShapeData(parameters.Fields));
        }


        //api/touristroutes/{touristRouteId}
        [HttpGet("{touristRouteId:Guid}", Name = "GetTouristRouteById")]
        [HttpHead("{touristRouteId:Guid}")]
        public async Task<IActionResult> GetTouristRouteById(Guid touristRouteId, string fields)
        {
            var touristRouteFromRepo = await _touristRouteRepository.GetTouristRouteAsync(touristRouteId);
            if (touristRouteFromRepo == null)
            {
                return NotFound($"Tourist route {touristRouteId} does not exist.");
            }

            //var touristRouteDto = new TouristRouteDto()
            //{
            //    Id = touristRouteFromRepo.Id,
            //    Title = touristRouteFromRepo.Title,
            //    Description = touristRouteFromRepo.Description,
            //    Price = touristRouteFromRepo.OriginalPrice * (decimal)(touristRouteFromRepo.DiscountPresent ?? 1),
            //    CreateTime = touristRouteFromRepo.CreateTime,
            //    UpdateTime = touristRouteFromRepo.UpdateTime,
            //    Features = touristRouteFromRepo.Features,
            //    Fees = touristRouteFromRepo.Fees,
            //    Notes = touristRouteFromRepo.Notes,
            //    Rating = touristRouteFromRepo.Rating,
            //    TravelDays = touristRouteFromRepo.TravelDays.ToString(),
            //    TripType = touristRouteFromRepo.TripType.ToString(),
            //    DepartureCity = touristRouteFromRepo.DepartureCity.ToString()
            //};

            var touristRouteDto = _mapper.Map<TouristRouteDto>(touristRouteFromRepo);

            return Ok(touristRouteDto.ShapeData(fields));
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = "Bearer")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateTouristRoute([FromBody] TouristRouteForCreationDto touristRouteForCreationDto)
        {
            var touristRouteModel = _mapper.Map<TouristRoute>(touristRouteForCreationDto);

            _touristRouteRepository.AddTouristRouteAsync(touristRouteModel);

            await _touristRouteRepository.SaveAsync();

            var touristRouteToReturn = _mapper.Map<TouristRouteDto>(touristRouteModel);

            return CreatedAtRoute("GetTouristRouteById",
                new { touristRouteId = touristRouteToReturn.Id },
                touristRouteToReturn
                );
        }

        [HttpPut("{touristRouteId}")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateTouristRoute([FromRoute] Guid touristRouteId, [FromBody] TouristRouteForUpdateDto touristRouteForUpdateDto)
        {
            if (!(await _touristRouteRepository.TouristRouteExistsAsync(touristRouteId)))
            {
                return NotFound("Tourist Route Not Exists.");
            }

            var touristRouteFomRepo = await _touristRouteRepository.GetTouristRouteAsync(touristRouteId);

            //1. Reflect to DTO
            //2. Update DTO
            //3. Reflect to Model
            _mapper.Map(touristRouteForUpdateDto, touristRouteFomRepo);

            await _touristRouteRepository.SaveAsync();

            return NoContent();

        }

        [HttpPatch("{touristRouteId}")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PartiallyUpdateTouristRoute([FromRoute] Guid touristRouteId,
            [FromBody] JsonPatchDocument<TouristRouteForUpdateDto> patchDocument
            )
        {
            if (!(await _touristRouteRepository.TouristRouteExistsAsync(touristRouteId)))
            {
                return NotFound("Tourist Route Not Exists.");
            }

            var touristRouteFromRepo = await _touristRouteRepository.GetTouristRouteAsync(touristRouteId);

            var touristRouteToPatch = _mapper.Map<TouristRouteForUpdateDto>(touristRouteFromRepo);

            patchDocument.ApplyTo(touristRouteToPatch, ModelState);

            if (!TryValidateModel(touristRouteToPatch))
            {
                return ValidationProblem(ModelState);
            }
            _mapper.Map(touristRouteToPatch, touristRouteFromRepo);

            await _touristRouteRepository.SaveAsync();

            return NoContent();
        }

        [HttpDelete("{touristRouteId}")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteTouristRoute([FromRoute] Guid touristRouteId)
        {
            if (!(await _touristRouteRepository.TouristRouteExistsAsync(touristRouteId)))
            {
                return NotFound("Tourist Route Not Exists.");
            }


            var touristRoute = await _touristRouteRepository.GetTouristRouteAsync(touristRouteId);

            _touristRouteRepository.DeleteTouristRoute(touristRoute);

            await _touristRouteRepository.SaveAsync();

            return NoContent();
        }

        [HttpDelete("({touristIDs})")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteByIDs([ModelBinder(BinderType = typeof(ArrayModelBinder))][FromRoute] IEnumerable<Guid> touristIDs)
        {
            if (touristIDs == null)
            {
                return BadRequest();
            }

            var touristRouteFromRepo = await _touristRouteRepository.GetTouristRoutesByIDListAsync(touristIDs);
            _touristRouteRepository.DeleteTouristRoutes(touristRouteFromRepo);
            await _touristRouteRepository.SaveAsync();

            return NoContent();

        }

    }
}
