using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers
{
    [ApiController]
    [Route("api/auctions")]
    public class AuctionsController : ControllerBase
    {
        private readonly AuctionDbContext _dbContext;
        private readonly IMapper _mapper;
      
        public AuctionsController(AuctionDbContext dbContext,IMapper mapper)
        {
            _mapper = mapper;
            _dbContext = dbContext;
            
        }

        [HttpGet]
        public async Task<ActionResult<List<Auction>>> GetAuctions(){
            var auctions = await _dbContext.Auctions
                                            .Include(a=>a.Item)
                                            .OrderBy(x => x.Item.Make)
                                            .ToListAsync();
            return Ok(_mapper.Map<List<AuctionDto>>(auctions));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Auction>> GetAuction(Guid id){
            var auction = await _dbContext.Auctions
                                            .Include(a=>a.Item)
                                            .FirstOrDefaultAsync(x => x.Id == id);
            if(auction == null){
                return NotFound();
            }
            return Ok(_mapper.Map<AuctionDto>(auction));
        }

        [HttpPost]
        public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto auctionDto)
        {
            var auction = _mapper.Map<Auction>(auctionDto);
            // TODO: add current user as seller
            auction.Seller = "test";

            _dbContext.Auctions.Add(auction);

            var result = await _dbContext.SaveChangesAsync() > 0;

            if (!result) return BadRequest("Could not save changes to the DB");

            return CreatedAtAction(nameof(GetAuction),
                new { auction.Id }, _mapper.Map<AuctionDto>(auction));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDto updateAuctionDto)
        {
            var auction = await _dbContext.Auctions.Include(x => x.Item)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (auction == null) return NotFound();

            // TODO: check seller == username

            auction.Item.Make = updateAuctionDto.Make ?? auction.Item.Make;
            auction.Item.Model = updateAuctionDto.Model ?? auction.Item.Model;
            auction.Item.Color = updateAuctionDto.Color ?? auction.Item.Color;
            auction.Item.Mileage = updateAuctionDto.Mileage ?? auction.Item.Mileage;
            auction.Item.Year = updateAuctionDto.Year ?? auction.Item.Year;

            var result = await _dbContext.SaveChangesAsync() > 0;

            if (result) return Ok();

            return BadRequest("Problem saving changes");
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAuction(Guid id)
        {
            var auction = await _dbContext.Auctions.FindAsync(id);

            if (auction == null) return NotFound();

            // TODO: check seller == username

            _dbContext.Auctions.Remove(auction);

            var result = await _dbContext.SaveChangesAsync() > 0;

            if (!result) return BadRequest("Could not update DB");

            return Ok();
        }
    }
}