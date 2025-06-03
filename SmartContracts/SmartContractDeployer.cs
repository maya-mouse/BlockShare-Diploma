using BlockShare.Data;
using BlockShare.Models;
using Microsoft.EntityFrameworkCore;
using Nethereum.BlockchainProcessing.BlockStorage.Entities.Mapping;
using Nethereum.Hex.HexTypes;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Newtonsoft.Json.Linq;

namespace BlockShare.SmartContracts
{
	public class SmartContractDeployer
	{
		private readonly string ganacheUrl = "http://127.0.0.1:7545"; // або порт твого Ganache
		private readonly string privateKey = "0xe9bd572c1b16b5feda90af9ecfbbd47a6152b68c84efd13244e3ff2bd1327917"; // приватний ключ з Ganache
		private readonly string abi = @"[ 
	{
		""anonymous"": false,
		""inputs"": [
			{
				""indexed"": false,
				""internalType"": ""uint256"",
				""name"": ""fileId"",
				""type"": ""uint256""
			},
			{
				""indexed"": false,
				""internalType"": ""address"",
				""name"": ""grantedTo"",
				""type"": ""address""
			}
		],
		""name"": ""AccessGranted"",
		""type"": ""event""
	},
	{
		""anonymous"": false,
		""inputs"": [
			{
				""indexed"": false,
				""internalType"": ""uint256"",
				""name"": ""fileId"",
				""type"": ""uint256""
			},
			{
				""indexed"": false,
				""internalType"": ""address"",
				""name"": ""revokedFrom"",
				""type"": ""address""
			}
		],
		""name"": ""AccessRevoked"",
		""type"": ""event""
	},
	{
		""inputs"": [
			{
				""internalType"": ""string"",
				""name"": ""_ipfsHash"",
				""type"": ""string""
			},
			{
				""internalType"": ""address[]"",
				""name"": ""allowedUsers"",
				""type"": ""address[]""
			}
		],
		""name"": ""addFile"",
		""outputs"": [],
		""stateMutability"": ""nonpayable"",
		""type"": ""function""
	},
	{
		""anonymous"": false,
		""inputs"": [
			{
				""indexed"": false,
				""internalType"": ""string"",
				""name"": ""ipfsHash"",
				""type"": ""string""
			},
			{
				""indexed"": false,
				""internalType"": ""address"",
				""name"": ""uploader"",
				""type"": ""address""
			},
			{
				""indexed"": false,
				""internalType"": ""uint256"",
				""name"": ""timestamp"",
				""type"": ""uint256""
			}
		],
		""name"": ""FileAdded"",
		""type"": ""event""
	},
	{
		""inputs"": [
			{
				""internalType"": ""uint256"",
				""name"": ""_index"",
				""type"": ""uint256""
			},
			{
				""internalType"": ""address"",
				""name"": ""user"",
				""type"": ""address""
			}
		],
		""name"": ""grantAccess"",
		""outputs"": [],
		""stateMutability"": ""nonpayable"",
		""type"": ""function""
	},
	{
		""inputs"": [
			{
				""internalType"": ""uint256"",
				""name"": ""_index"",
				""type"": ""uint256""
			},
			{
				""internalType"": ""address"",
				""name"": ""user"",
				""type"": ""address""
			}
		],
		""name"": ""revokeAccess"",
		""outputs"": [],
		""stateMutability"": ""nonpayable"",
		""type"": ""function""
	},
	{
		""inputs"": [],
		""name"": ""fileCount"",
		""outputs"": [
			{
				""internalType"": ""uint256"",
				""name"": """",
				""type"": ""uint256""
			}
		],
		""stateMutability"": ""view"",
		""type"": ""function""
	},
	{
		""inputs"": [
			{
				""internalType"": ""uint256"",
				""name"": ""_index"",
				""type"": ""uint256""
			}
		],
		""name"": ""getFile"",
		""outputs"": [
			{
				""internalType"": ""string"",
				""name"": """",
				""type"": ""string""
			}
		],
		""stateMutability"": ""view"",
		""type"": ""function""
	},
	{
		""inputs"": [
			{
				""internalType"": ""uint256"",
				""name"": ""_index"",
				""type"": ""uint256""
			},
			{
				""internalType"": ""address"",
				""name"": ""user"",
				""type"": ""address""
			}
		],
		""name"": ""hasAccess"",
		""outputs"": [
			{
				""internalType"": ""bool"",
				""name"": """",
				""type"": ""bool""
			}
		],
		""stateMutability"": ""view"",
		""type"": ""function""
	}
 ]"; // ABI у вигляді JSON-рядка
		private readonly string bytecode = File.ReadAllText(Directory.GetCurrentDirectory()+ "/SmartContracts/FIleRegistry.byte.txt"); // Bytecode контракту
		private readonly AppDbContext _db;

		public SmartContractDeployer(AppDbContext db)
		{
			_db = db;
		}

		public async Task<string> DeployContractAsync()
		{
			var account = new Account(privateKey);
			var web3 = new Web3(account, ganacheUrl);

			var receipt = await web3.Eth.DeployContract
				.SendRequestAndWaitForReceiptAsync(abi, bytecode, account.Address, new HexBigInteger(3000000));

			// Збереження у appsettings (наприклад, через IConfiguration)
			var configPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
			var json = JObject.Parse(File.ReadAllText(configPath));
			var blockchainSection = (JObject)json["Blockchain"];
			blockchainSection["ContractAddress"] = receipt.ContractAddress;
			File.WriteAllText(configPath, json.ToString());

			string parentHash = "";
			if (receipt.BlockNumber.ToLong() > 0)
			{
				parentHash = await _db.BlockchainRecords.Where(b => b.BlockNumber ==
				receipt.BlockNumber.ToLong()-1).Select(a=> a.BlockHash).FirstOrDefaultAsync();
			}
	
	
			var block = new BlockchainRecord
			{
				BlockHash = receipt.BlockHash,
				BlockNumber = receipt.BlockNumber.ToLong(),
				UploaderAddress = account.Address,
				IpfsHash = "Contract call.." + receipt.BlockNumber.ToLong()+"block",
				TransactionHash = receipt.TransactionHash,
				Timestamp = DateTime.UtcNow,
				ParentHash = parentHash
			};

			_db.BlockchainRecords.Add(block);
			 await _db.SaveChangesAsync();
			return receipt.ContractAddress;
		}

	}
}
