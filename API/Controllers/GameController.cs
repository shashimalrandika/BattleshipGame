using BattleshipAPI.Models;
using BattleshipAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BattleshipAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GameController : ControllerBase
    {
        private readonly GameService _gameService;

        public GameController(GameService gameService)
        {
            _gameService = gameService;
        }

        [HttpPost("fire")]
        public IActionResult FireShot([FromBody] FireRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Position))
                return BadRequest(new { message = "Position is required!" });

            var result = _gameService.FireShot(request.Position);
            return Ok(result); 
        }

        [HttpGet("ships")]
        public IActionResult GetShips()
        {
            return Ok(_gameService.GetShips());
        }

        [HttpPost("reset")]
        public IActionResult ResetGame()
        {
            Console.WriteLine("Resetting Game...");
            _gameService.SetupGame(); 

            return Ok(new { message = "Game has been reset!" });
        }
    }
}
