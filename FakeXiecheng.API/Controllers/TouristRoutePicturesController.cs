using AutoMapper;
using FakeXiecheng.API.Dtos;
using FakeXiecheng.API.Models;
using FakeXiecheng.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FakeXiecheng.API.Controllers
{
    [Route("api/touristRoutes/{touristRouteId}/pictures")]
    [ApiController]
    public class TouristRoutePicturesController : ControllerBase
    {
        private ITouristRouteRepository _touristRouteRepository;
        private IMapper _mapper;
        public TouristRoutePicturesController(ITouristRouteRepository touristRouteRepository, IMapper mapper)
        {
            _touristRouteRepository = touristRouteRepository ??
                throw new ArgumentNullException(nameof(touristRouteRepository));

            _mapper = mapper ??
                throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet]
        public async Task<IActionResult> GetPictureListForTouristRoute(Guid touristRouteId)
        {
            if (!(await _touristRouteRepository.TouristRouteExistsAsync(touristRouteId)))
            {
                return NotFound("Tourist route not exists");
            }

            var picturesFromRepo = await _touristRouteRepository.GetPicturesByTouristRouteIdAsync(touristRouteId);

            if (picturesFromRepo == null || picturesFromRepo.Count() <= 0)
            {
                return NotFound("Pictures not exists");
            }

            return Ok(_mapper.Map<IEnumerable<TouristRoutePictureDto>>(picturesFromRepo));
        }

        [HttpGet("{pictureId}", Name = "GetPicture")]
        public async Task<IActionResult> GetPicture(Guid touristRouteId, int pictureId)
        {
            if (!(await _touristRouteRepository.TouristRouteExistsAsync(touristRouteId)))
            {
                return NotFound("Tourist route not exists");
            }

            var pictureFromRepo = await _touristRouteRepository.GetPictureAsync(pictureId);

            if (pictureFromRepo == null)
            {
                return NotFound("Picture not found");
            }

            return Ok(_mapper.Map<TouristRoutePictureDto>(pictureFromRepo));
        }

        [HttpPost]
        public async Task<IActionResult> CreateTouristRoutePicture([FromRoute] Guid touristRouteId, [FromBody] TouristRoutePictureForCreationDto touristRoutePictureForCreationDto)
        {
            if (!(await _touristRouteRepository.TouristRouteExistsAsync(touristRouteId)))
            {
                return NotFound("Tourist route not exists");
            }

            var pictureModel = _mapper.Map<TouristRoutePicture>(touristRoutePictureForCreationDto);

            _touristRouteRepository.AddTouristRoutePictureAsync(touristRouteId, pictureModel);
            await _touristRouteRepository.SaveAsync();
            var pictureToReturn = _mapper.Map<TouristRoutePictureDto>(pictureModel);
            return CreatedAtRoute("GetPicture",
                new
                {
                    touristRouteId = pictureModel.TouristRouteId,
                    pictureId = pictureModel.Id
                },
                pictureToReturn
                );
        }

        [HttpDelete("{pictureId}")]
        public async Task<IActionResult> DeletePicture([FromRoute]Guid touristRouteId, [FromRoute]int pictureId)
        {
            if (!(await _touristRouteRepository.TouristRouteExistsAsync(touristRouteId)))
            {
                return NotFound("Tourist route not exists");
            }

            var picture = await _touristRouteRepository.GetPictureAsync(pictureId);

            _touristRouteRepository.DeleteTouristRoutePicture(picture);
            await _touristRouteRepository.SaveAsync();

            return NoContent();
        }


    }

}
