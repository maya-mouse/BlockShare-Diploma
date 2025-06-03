using BlockShare.Data;
using BlockShare.Models;
using BlockShare.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlockShare.Controllers
{
    public class BlocksController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IpfsService _ipfs;
        private readonly WalletService _walletService;
        //    private readonly BlockchainService _blockchainService;

        public BlocksController(AppDbContext db, IpfsService ipfs, WalletService walletService)
        {
            _db = db;
            _ipfs = ipfs;
            _walletService = walletService;
        }
        public async Task<IActionResult> Index(string search)
        {
            var blocks = await _db.BlockchainRecords.OrderByDescending(b=>b.BlockNumber).ToListAsync();
            //      .ToListAsync();
            if (!string.IsNullOrEmpty(search))
            {
                blocks = await _db.BlockchainRecords.Where(b => b.UploaderAddress.Contains(search) ||
                  b.BlockHash.Contains(search) ||
                   b.ParentHash.Contains(search) ||
                  b.TransactionHash.Contains(search)).OrderByDescending(b => b.BlockNumber).ToListAsync();
            }

    
            return View(blocks);
        }

        [HttpPost]
        public async Task<IActionResult> SaveBlock([FromBody] BlockchainRecord record)
        {
            if (record == null || string.IsNullOrEmpty(record.IpfsHash))
                return BadRequest();

            record.Timestamp = record.Timestamp.ToUniversalTime(); // на всяк випадок

            _db.BlockchainRecords.Add(record);
            await _db.SaveChangesAsync();

            return Ok();
        }

    }
}
