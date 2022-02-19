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

namespace FakeXiecheng.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TouristRoutesController : ControllerBase
    {
        private ITouristRouteRepository _touristRouteRepository;
        private readonly IMapper _mapper;
        public TouristRoutesController(ITouristRouteRepository touristRouteRepository, IMapper mapper)
        {
            _touristRouteRepository = touristRouteRepository;
            _mapper = mapper;
        }

        //api/touristRoutes?keyword=value
        [HttpGet]
        [HttpHead]
        public async Task<IActionResult> GetTouristRoutes(
            [FromQuery] TouristRouteResourceParameters pamameters
            )
        {

            var touristRoutesFromRepo = await _touristRouteRepository.GetTouristRoutesAsync(pamameters.Keyword, pamameters.RatingOperator, pamameters.RatingValue);
            if (touristRoutesFromRepo == null || touristRoutesFromRepo.Count() <= 0)
            {
                return NotFound("No tourist route available");
            }
            var touristRouteDtos = _mapper.Map<IEnumerable<TouristRouteDto>>(touristRoutesFromRepo);

            return Ok(touristRouteDtos);
        }


        //api/touristroutes/{touristRouteId}
        [HttpGet("{touristRouteId:Guid}", Name = "GetTouristRouteById")]
        [HttpHead("{touristRouteId:Guid}")]
        public async Task<IActionResult> GetTouristRouteById(Guid touristRouteId)
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

            return Ok(touristRouteDto);
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
        public async Task<IActionResult> DeleteTouristRoute([FromRoute]Guid touristRouteId)
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
        public async Task<IActionResult> DeleteByIDs([ModelBinder(BinderType = typeof(ArrayModelBinder))][FromRoute]IEnumerable<Guid> touristIDs)
        {
            if(touristIDs == null)
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
