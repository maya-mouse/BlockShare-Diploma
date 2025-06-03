using BlockShare.Data;
using BlockShare.Models;
using BlockShare.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NBitcoin.Secp256k1;
using System.Security.Claims;
using System.Text.Json;

namespace BlockShare.Controllers
{
    [Authorize]
    public class FileController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IpfsService _ipfs;
        private readonly WalletService _walletService;
        private readonly EncryptionService _encryptionService;
		private readonly IConfiguration _configuration;

		public FileController(AppDbContext db, IpfsService ipfs, WalletService walletService, EncryptionService encryptionService, IConfiguration configuration)
		{
			_db = db;
			_ipfs = ipfs;
			_walletService = walletService;
			_encryptionService = encryptionService;
			_configuration =
			_configuration = configuration;
		}

		/*       public async Task<IActionResult> Index()
			   {
				   var userId = User.Identity.Name;
				   var wallet = _walletService.GetWalletAddress();
				   var userFiles = await _db.Files
					   .Where(f => f.OwnerWalletAddress == wallet)
					   .OrderByDescending(f => f.UploadDate)
					   .Take(5)
					   .ToListAsync();
			 //      var wallet = _db.Users.Where(a => a.Username == userId).FirstOrDefault().WalletAddress;
				   int myFiles = await _db.Files.CountAsync(f => f.OwnerWalletAddress == wallet);
				   int publicFiles = await _db.Files.CountAsync(f => f.IsPublic);
				   // var wallet = await _walletService.GetConnectedWalletAsync(User);
				   int accessible = await _db.AccessEntries.CountAsync(e => e.WalletAddress == wallet);

				   var model = new DashboardViewModel
				   {
					   RecentFiles = userFiles,
					   TotalFiles = myFiles+publicFiles+accessible,
					   PublicFiles = publicFiles,
					   AccessibleFiles = accessible
				   };


				   return View(model);
			   }*/

		public IActionResult Index()
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			var myFiles = _db.Files.Where(f => f.OwnerWalletAddress == _walletService.GetWalletAddress()).ToList();

			var fileIds = myFiles.Select(f => f.Id).ToList();

			var walletsWithAccessToMine = _db.AccessEntries
				.Where(a => fileIds.Contains(a.FileId))
				.Select(a => a.WalletAddress)
				.Distinct()
				.ToList();

            var usernamesWithAccessToMine  = _db.Users.Where(u=>
			walletsWithAccessToMine.Contains(u.WalletAddress)).Select(a=>a.Username).
			ToList();


            var myWallet = _walletService.GetWalletAddress();

			var accessibleFileOwners = _db.AccessEntries
				.Where(a => a.WalletAddress == myWallet)
				.Join(_db.Files,
					  access => access.FileId,
					  file => file.Id,
					  (access, file) => file.OwnerWalletAddress)
				.Distinct()
				.ToList();

            var usernamesAccessibleFileOwners = _db.Users.Where(u =>
            accessibleFileOwners.Contains(u.WalletAddress)).Select(a => a.Username).
            ToList();

            var model = new DashboardViewModel
			{
				RecentFiles = myFiles.OrderByDescending(f => f.UploadDate).Take(5).ToList(),
				TotalFiles = myFiles.Count() + _db.AccessEntries.Count(a => a.WalletAddress == myWallet),
				PublicFiles = myFiles.Count(f => f.IsPublic),
				AccessibleFiles = _db.AccessEntries.Count(a => a.WalletAddress == myWallet),
				WalletsWithAccessToMine = walletsWithAccessToMine,
				WalletsIAccessed = accessibleFileOwners,
				UsernamesWithAccessToMine = usernamesWithAccessToMine,
				UsernamesIAccessed = usernamesAccessibleFileOwners
            };

			return View(model);
		}

		public IActionResult Upload()
		{
			return View();
		}


		/*  [HttpPost]
		  public async Task<IActionResult> UploadEncrypted(IFormFile file, string aesKey, string allowedAddresses, bool isPublic)
		  {
			  if (file == null || file.Length == 0)
				  return BadRequest(new { error = "Файл не вибрано" });

			  // 🔼 Завантаження зашифрованого файлу в IPFS
			  var ipfsHash = await _ipfs.UploadFileAsync(file);
			  var userId = User.Identity?.Name;
			  var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == userId);
			  var encryptedKey = _encryptionService.Encrypt(aesKey);

			  var fileMetadata = new FileMetadata
			  {
				  FileName = file.FileName,
				  IpfsHash = ipfsHash,
				  OwnerId = User.Identity?.Name,
				  UploadDate = DateTime.Now,
				  IsPublic = isPublic,
				  OwnerWalletAddress = _walletService.GetWalletAddress(),
				  BlockchainFileIndex = -1,
				  EncryptionKey = encryptedKey // ⚠ тимчасово plaintext
			  };

			  _db.Files.Add(fileMetadata);
			  await _db.SaveChangesAsync();

			  // 🔓 Обробка whitelist
			  List<string> addressList = new();
			  if (!string.IsNullOrWhiteSpace(allowedAddresses))
			  {
				  try
				  {
					  addressList = JsonSerializer.Deserialize<List<string>>(allowedAddresses);
				  }
				  catch (Exception ex)
				  {
					  Console.WriteLine("Помилка десеріалізації allowedAddresses: " + ex.Message);
					  return BadRequest(new { error = "Невірний формат allowedAddresses" });
				  }

				  foreach (var addr in addressList)
				  {
					  _db.AccessEntries.Add(new FilesAccess
					  {
						  FileId = fileMetadata.Id,
						  WalletAddress = addr
					  });
				  }

				  await _db.SaveChangesAsync();
			  }

			  return Json(new { ipfsHash, allowedAddresses = addressList });
		  }*/

		[HttpPost]
		public async Task<IActionResult> UploadEncrypted(IFormFile file, string aesKey, string allowedAddresses, bool isPublic, int fileId)
		{
			if (file == null || file.Length == 0)
				return BadRequest(new { error = "Файл не вибрано" });

			// 🔼 Завантаження зашифрованого файлу в IPFS
			var ipfsHash = await _ipfs.UploadFileAsync(file);
			var userId = User.Identity?.Name;
		
			Console.WriteLine(aesKey);
			var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == userId);
			var encryptedKey = _encryptionService.Encrypt(aesKey);
			var fileMetadata = new FileMetadata
			{
				FileName = file.FileName,
				IpfsHash = ipfsHash,
				OwnerId = userId,
				UploadDate = DateTime.Now,
				IsPublic = isPublic,
				OwnerWalletAddress = _walletService.GetWalletAddress(),
				BlockchainFileIndex = fileId,
				EncryptionKey = encryptedKey
			};

			_db.Files.Add(fileMetadata);
			await _db.SaveChangesAsync();
			Console.WriteLine("FUCK1");
			// 🔓 Обробка whitelist
			List<string> addressList = new();
			if (!string.IsNullOrWhiteSpace(allowedAddresses))
			{
				try
				{
					addressList = JsonSerializer.Deserialize<List<string>>(allowedAddresses);
				}
				catch (Exception ex)
				{
					Console.WriteLine("Помилка десеріалізації allowedAddresses: " + ex.Message);
					return BadRequest(new { error = "Невірний формат allowedAddresses" });
				}

				foreach (var addr in addressList)
				{
					_db.AccessEntries.Add(new FilesAccess
					{
						FileId = fileMetadata.Id,
						WalletAddress = addr
					});
				}
				Console.WriteLine("FUCK2");
				await _db.SaveChangesAsync();
			}

			return Json(new { ipfsHash, allowedAddresses = addressList });
		}

		[HttpPost]
        public async Task<IActionResult> SetBlockchainFileId([FromBody] JsonElement data)
        {
            string ipfsHash = data.GetProperty("ipfsHash").GetString();
            int blockchainFileId = data.GetProperty("blockchainFileId").GetInt32();

            var file = await _db.Files.FirstOrDefaultAsync(f => f.IpfsHash == ipfsHash);
            if (file == null)
                return NotFound();

            file.BlockchainFileIndex = blockchainFileId;
            await _db.SaveChangesAsync();

            return Ok();
        }


        public async Task<IActionResult> MyFiles(string search, string sort)
        {
            var userId = User.Identity.Name;

            var filesQuery = _db.Files.Where(f => f.OwnerWalletAddress == _walletService.GetWalletAddress());

            if (!string.IsNullOrEmpty(search))
            {
                filesQuery = filesQuery.Where(f => f.FileName.Contains(search));
            }

            filesQuery = sort switch
            {
                "name" => filesQuery.OrderBy(f => f.FileName),
                "date" => filesQuery.OrderByDescending(f => f.UploadDate),
                _ => filesQuery.OrderByDescending(f => f.UploadDate)
            };

            var files = await filesQuery.ToListAsync();

            // Статистика
            ViewBag.Total = await _db.Files.CountAsync(f => f.OwnerWalletAddress == _walletService.GetWalletAddress());
            ViewBag.Public = await _db.Files.CountAsync(f => f.OwnerWalletAddress == _walletService.GetWalletAddress() && f.IsPublic);

            return View(files);
        }

		[HttpPost]
		public IActionResult PrepareUpload([FromBody] FileMetadatShortView file)
		{
			var tempIpfsHash = Guid.NewGuid().ToString(); // або щось унікальне
			return Json(new { tempIpfsHash });
		}


		public async Task<IActionResult> AllPublicFiles(string search, string sort)
        {
            var files = _db.Files
                .Where(f => f.IsPublic);
             //   .OrderByDescending(f => f.UploadDate);

            if (!string.IsNullOrEmpty(search))
            {
                files =  files.Where(f => f.FileName.Contains(search) || f.OwnerWalletAddress.Contains(search));
              
            }
            files = sort switch
            {
                "name" =>files.OrderBy(f => f.FileName),
                "date" => files.OrderByDescending(f => f.UploadDate),
                _ => files.OrderByDescending(f => f.UploadDate)
            };
            foreach (var file in files)
            {
                file.EncryptionKey = _encryptionService.Decrypt(file.EncryptionKey);
            }
            var filesView = await files.ToListAsync();

            return View(filesView);
        }

		[HttpGet]
		public async Task<IActionResult> DownloadFromIpfs(string hash)
		{
			if (string.IsNullOrWhiteSpace(hash))
				return BadRequest("CID is missing");

			using var http = new HttpClient();
			var ipfsUrl = $"http://127.0.0.1:8080/ipfs/{hash}";

			var response = await http.GetAsync(ipfsUrl);
			if (!response.IsSuccessStatusCode)
				return StatusCode((int)response.StatusCode, "Помилка при доступі до IPFS");

			var content = await response.Content.ReadAsStreamAsync();
            var file = _db.Files.Where(a=>a.IpfsHash == hash).FirstOrDefault();

			var log = new FileAccessLog
            {
                FileId = file.Id,
                Filename = file.FileName,
                IpfsHash = hash,
				WalletAddress = _walletService.GetWalletAddress(),
                AccessedAt = DateTime.UtcNow
            };
            _db.FileAccessLogs.Add(log);
            await _db.SaveChangesAsync();
            return File(content, "application/octet-stream", hash);
		}

     
        [HttpGet]
        public async Task<IActionResult> AccessibleFiles(string search, string sort)
        {
            var wallet = _db.Users
                .Where(a => a.WalletAddress == _walletService.GetWalletAddress()).FirstOrDefault().WalletAddress;

			if (string.IsNullOrWhiteSpace(wallet))
                return Unauthorized("Не вдалося визначити адресу гаманця");

            var accessibleFileIds = await _db.AccessEntries
                .Where(a => a.WalletAddress == wallet)
                .Select(a => a.FileId)
                .ToListAsync();

            var files =  _db.Files
                .Where(f =>
                //f.IsPublic || 
                accessibleFileIds.Contains(f.Id));
            //.ToListAsync();
            if (!string.IsNullOrEmpty(search))
            {
                files = files.Where(f => f.FileName.Contains(search) || f.OwnerWalletAddress.Contains(search));

            }
            files = sort switch
            {
                "name" => files.OrderBy(f => f.FileName),
                "date" => files.OrderByDescending(f => f.UploadDate),
                _ => files.OrderByDescending(f => f.UploadDate)
            };
            foreach (var file in files)
            {
                file.EncryptionKey = _encryptionService.Decrypt(file.EncryptionKey);
            }
            var filesView = await files.ToListAsync();

            return View(filesView);

        }

  
        [HttpGet]
        public async Task<IActionResult> UpdateAccess(int id)
        {
            var file = await _db.Files.FindAsync(id);
            if (file == null) return NotFound();

            var wallets = await _db.AccessEntries
                .Where(a => a.FileId == id)
                .Select(a => a.WalletAddress)
                .ToListAsync();

            var model = new AccessUpdateViewModel
            {
                FileId = file.Id,
                IsPublic = file.IsPublic,
                AllowedWallets = wallets
            };

            return View(model);
        }

		/*    [HttpPost]
			public async Task<IActionResult> UpdateAccess([FromBody] AccessUpdateViewModel model)
			{
				var file = await _db.Files.FindAsync(model.FileId);
				if (file == null)
					return Json(new { success = false, error = "Файл не знайдено" });

				file.IsPublic = model.IsPublic;

				// Стара AccessEntries
				var existingEntries = _db.AccessEntries.Where(a => a.FileId == model.FileId);
				_db.AccessEntries.RemoveRange(existingEntries);

				// Нові записи
				if (!model.IsPublic && model.AllowedWallets != null)
				{
					var newEntries = model.AllowedWallets
						.Distinct()
						.Select(addr => new FilesAccess { FileId = model.FileId, WalletAddress = addr });
					await _db.AccessEntries.AddRangeAsync(newEntries);
				}

				await _db.SaveChangesAsync();
				return Json(new { success = true });
			}*/

		[HttpPost]
		[HttpPost]
		public async Task<IActionResult> UpdateAccess([FromBody] AccessUpdateViewModel input)
		{
			var file = await _db.Files.FirstOrDefaultAsync(f => f.Id == input.FileId);
			if (file == null) return NotFound();

			// Отримати старі гаманці, яким надано доступ
			var oldWallets = await _db.AccessEntries
				.Where(a => a.FileId == input.FileId)
				.Select(a => a.WalletAddress)
				.ToListAsync();

			var newWallets = input.AllowedWallets ?? new List<string>();

			// Визначити гаманці для надання/відміни доступу
			var toGrant = newWallets.Except(oldWallets, StringComparer.OrdinalIgnoreCase).ToList();
			var toRevoke = oldWallets.Except(newWallets, StringComparer.OrdinalIgnoreCase).ToList();

			// Оновити флаг публічності
			file.IsPublic = input.IsPublic;
			_db.Files.Update(file);

			// Видалити записи з таблиці доступу
			var entriesToRemove = await _db.AccessEntries
				.Where(a => a.FileId == input.FileId && toRevoke.Contains(a.WalletAddress))
				.ToListAsync();
			_db.AccessEntries.RemoveRange(entriesToRemove);

			// Додати нові записи
			var entriesToAdd = toGrant.Select(wallet => new FilesAccess
			{
				FileId = input.FileId,
				WalletAddress = wallet
			});
			await _db.AccessEntries.AddRangeAsync(entriesToAdd);

			await _db.SaveChangesAsync();

			return Json(new
			{
				success = true,
				granted = toGrant,
				revoked = toRevoke,
				fileIndex = file.BlockchainFileIndex // використаємо в JS для виклику контракту
			});
		}


		public IActionResult AccessHistory()
        {
			var files = _db.Files.Where(f => f.OwnerWalletAddress == _walletService.GetWalletAddress()).Select(a=>a.IpfsHash);
            var history = _db.FileAccessLogs
                .Where(log => files.Contains(log.IpfsHash))
                .OrderByDescending(log => log.AccessedAt)
                .ToList();

            return View(history);
        }

		public IActionResult GetContractAddress()
		{
			var address = _configuration["Blockchain:ContractAddress"];
			return Ok(new { contractAddress = address });
		}

		public IActionResult GetAbi()
		{
			var abiPath = _configuration["Blockchain:AbiPath"];
			var abi = System.IO.File.ReadAllText(abiPath);
			return Content(abi, "application/json");
		}

	}
}