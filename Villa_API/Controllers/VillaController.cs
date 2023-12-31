﻿using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using Villa_API.Data;
using Villa_API.Models;
using Villa_API.Models.Dto;
using Villa_API.Repositorio;
using Villa_API.Repositorio.IRepositorio;

namespace Villa_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VillaController : ControllerBase
    {
        private readonly ILogger<VillaController> _logger;
        private readonly IVillaRepositorio _villaRepositorio;
        private readonly IMapper _mapper;
        protected APIResponse _apiResponse;

        public VillaController(ILogger<VillaController> logger, IVillaRepositorio villaRepositorio, IMapper mapper) { 
            _logger = logger;
            _villaRepositorio = villaRepositorio;
            _mapper = mapper;
            _apiResponse = new();
        }

        [HttpGet]
        public async Task<ActionResult<APIResponse>> GetVillas()
        {
            try
            {
                _logger.LogInformation("Obtener las Villas");

                IEnumerable<Villa> villaList = await _villaRepositorio.ObtenerTodos();

                _apiResponse.Resultado = _mapper.Map<IEnumerable<VillaDto>>(villaList);
                _apiResponse.StatusCode = HttpStatusCode.OK;

                return Ok(_apiResponse);
            }
            catch (Exception ex)
            {
                _apiResponse.IsExitoso = false;
                _apiResponse.ErrorsMessages = new List<string>() { ex.ToString() };
            }
            return _apiResponse;
        }

        [HttpGet("id:int", Name = "GetVilla")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<APIResponse>> GetVilla(int id)
        {
            try
            {
                if (id == 0)
                {
                    _logger.LogError("Error al traer Villa con Id " + id);
                    _apiResponse.IsExitoso = false;
                    _apiResponse.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_apiResponse);
                }

                // var villa = VillaStore.villaList.FirstOrDefault(v => v.Id == id);
                var villa = await _villaRepositorio.Obtener(v => v.Id == id);

                if (villa == null)
                {
                    _apiResponse.IsExitoso = false;
                    _apiResponse.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(_apiResponse);
                }

                _apiResponse.Resultado = _mapper.Map<VillaDto>(villa);
                _apiResponse.StatusCode = HttpStatusCode.OK;
                return Ok(_apiResponse);
            }
            catch (Exception ex)
            {
                _apiResponse.IsExitoso = false;
                _apiResponse.ErrorsMessages = new List<string>() { ex.ToString() };
            }

            return _apiResponse;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<APIResponse>> CreateVilla([FromBody] VillaCreateDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (await _villaRepositorio.Obtener(v => v.Nombre.ToLower() == createDto.Nombre.ToLower()) != null)
                {
                    ModelState.AddModelError("NombreExiste", "La Villa con ese nombre ya existe");
                    return BadRequest(ModelState);
                }

                if (createDto == null)
                {
                    return BadRequest(createDto);
                }

                Villa modelo = _mapper.Map<Villa>(createDto);

                modelo.FechaCreacion = DateTime.Now;
                modelo.FechaActualizacion = DateTime.Now;
                await _villaRepositorio.Crear(modelo);
                _apiResponse.Resultado = modelo;
                _apiResponse.StatusCode = HttpStatusCode.Created;

                return CreatedAtRoute("GetVilla", new { id = modelo.Id }, _apiResponse);
            }
            catch (Exception ex)
            {
                _apiResponse.IsExitoso = false;
                _apiResponse.ErrorsMessages = new List<string>() { ex.ToString() };
            }

            return _apiResponse;
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteVilla(int id)
        {
            try
            {
                if (id == 0)
                {
                    _apiResponse.IsExitoso = false;
                    _apiResponse.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_apiResponse);
                }

                var villa = await _villaRepositorio.Obtener(x => x.Id == id);

                if (villa == null)
                {
                    _apiResponse.IsExitoso = false;
                    _apiResponse.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(_apiResponse);
                }

                await _villaRepositorio.Remover(villa);
                _apiResponse.StatusCode = HttpStatusCode.NoContent;

                return Ok(_apiResponse);
            }
            catch (Exception ex)
            {
                _apiResponse.IsExitoso = false;
                _apiResponse.ErrorsMessages = new List<string>() { ex.ToString() };
            }

            return BadRequest(_apiResponse);
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateVilla(int id, [FromBody] VillaUpdateDto updateDto)
        {
            try
            {
                if (updateDto == null || id != updateDto.Id)
                {
                    _apiResponse.IsExitoso = false;
                    _apiResponse.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_apiResponse);
                }

                Villa modelo = _mapper.Map<Villa>(updateDto);

                await _villaRepositorio.Actualizar(modelo);
                _apiResponse.StatusCode = HttpStatusCode.NoContent;

                return Ok(_apiResponse);
            }
            catch (Exception ex)
            {
                _apiResponse.IsExitoso = false;
                _apiResponse.ErrorsMessages = new List<string>() { ex.ToString() };
            }

            return BadRequest(_apiResponse);
        }

        [HttpPatch("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdatePartialVilla(int id, JsonPatchDocument<VillaUpdateDto>  patchDto)
        {
            if (patchDto == null || id == 0)
            {
                return BadRequest();
            }

            var villa = await _villaRepositorio.Obtener(v => v.Id == id, tracked:false);

            VillaUpdateDto villaUpdateDto = _mapper.Map<VillaUpdateDto>(villa);

            if(villa == null)
            {
                return BadRequest();
            }

            patchDto.ApplyTo(villaUpdateDto, ModelState);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Villa modelo = _mapper.Map<Villa>(villaUpdateDto);

            await _villaRepositorio.Actualizar(modelo);
            _apiResponse.StatusCode = HttpStatusCode.NoContent;

            return Ok(_apiResponse);
        }
    }
}
