using AuthService.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthService.Services
{
    public class CosmosDbUserService
    {
        private readonly Container _container;

        public CosmosDbUserService(IConfiguration config)
        {
            var cosmosConfig = config.GetSection("CosmosDb");
            var client = new CosmosClient(
                cosmosConfig["Account"],
                cosmosConfig["Key"]
            );
            var database = client.GetDatabase(cosmosConfig["DatabaseName"]);
            _container = database.GetContainer(cosmosConfig["ContainerName"]);
        }

        public async Task<IEnumerable<AuthService.Models.User>> GetAllAsync()
        {
            var query = _container.GetItemQueryIterator<AuthService.Models.User>("SELECT * FROM c");
            var results = new List<AuthService.Models.User>();
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                results.AddRange(response);
            }
            return results.AsEnumerable();
        }

        public async Task<AuthService.Models.User> GetByIdAsync(string id)
        {
            try
            {
                var response = await _container.ReadItemAsync<AuthService.Models.User>(id, new PartitionKey(id));
                return response.Resource;
            }
            catch (CosmosException)
            {
                return null;
            }
        }

        public async Task<AuthService.Models.User> CreateAsync(AuthService.Models.User user)
        {
            user.Id = Guid.NewGuid().ToString();
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
            await _container.CreateItemAsync(user, new PartitionKey(user.Id));
            return user;
        }

        public async Task<bool> UpdateAsync(string id, AuthService.Models.User user)
        {
            var existing = await GetByIdAsync(id);
            if (existing == null) return false;
            user.Id = id;
            await _container.ReplaceItemAsync(user, id, new PartitionKey(id));
            return true;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            try
            {
                await _container.DeleteItemAsync<AuthService.Models.User>(id, new PartitionKey(id));
                return true;
            }
            catch (CosmosException)
            {
                return false;
            }
        }
    }
}