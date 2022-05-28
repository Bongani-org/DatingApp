using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace API.Controllers
{
    [Authorize]
    public class VisitsController : BaseApiController
    {
        private readonly IUnitOfWork _unitOfWork;

        public VisitsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        
        [HttpPost("{username}")]
        public async Task<ActionResult> AddVisit(string username)
        {
            var sourceUserId = User.GetUserId();
            var visitedUser = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username);
            var sourceUser = await _unitOfWork.VisitsRepository.GetUserWithVisits(sourceUserId);

            if (visitedUser == null) return NotFound();

            var userVisit = await _unitOfWork.VisitsRepository.GetUserVisit(sourceUserId, visitedUser.Id);

            userVisit = new Entities.UserVisit
            {
                SourceUserId = sourceUserId,
                VisitedUserId = visitedUser.Id
            };

            sourceUser.VisitedUser.Add(userVisit);

            if (await _unitOfWork.Complete()) return Ok();

            return BadRequest("Failed to visit user");
        }

        [Authorize(Policy = "ViewVisits")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<VisitDto>>> GetUserVisits([FromQuery] VisitsParams visitsParams)
        {
            visitsParams.UserId = User.GetUserId();
            var users =  await _unitOfWork.VisitsRepository.GetUserVisits(visitsParams);

            Response.AddPaginationHeader(users.CurrentPage, users.PageSize, 
                users.TotalCount, users.TotalPages);

            return Ok(users);
        }
    }
}