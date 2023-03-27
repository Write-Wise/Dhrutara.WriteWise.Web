using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Cosmos;
using Microsoft.Graph;
using System;

namespace Dhrutara.WriteWise.Web.Api
{
    public class GetDeletedUserStatus
    {
        private readonly Container _container;
        private readonly GraphServiceClient _graphClient;
        private readonly ILogger<GetDeletedUserStatus> _logger;        
        public GetDeletedUserStatus(Container container, GraphServiceClient graphClient, ILogger<GetDeletedUserStatus> logger)
        {
            _container = container;
            _graphClient = graphClient;
            _logger = logger;
        }

        [FunctionName("GetDeletedUserStatus")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            string userId = req.Query["userId"];
            string provider = req.Query["provider"];
            string? responseMessage = null;
            try{
              responseMessage = await GetUserStatusAsync(userId, provider, cancellationToken);
            }catch(Exception ex){
              _logger.LogError(ex.Message, ex);
              responseMessage = "User doesn't exist in Write Wise!";
            }
            return new OkObjectResult(responseMessage);
        }

        private async Task<string> GetUserStatusAsync(string userId, string provider, CancellationToken cancellationToken){

          try{
            ItemResponse<DeletedUser> response = await _container.ReadItemAsync<DeletedUser>(userId, new PartitionKey(provider)).ConfigureAwait(false);
            if(response?.Resource != null){
              return "User details deleted from Write Wise.";
            }
          }catch{
            try{
              Microsoft.Graph.Models.User? user = await _graphClient.Users[userId].GetAsync();
              if(user != null){
                return "Please use Write Wise mobile app to submit user data delation request. Please refer https://writewise.dhrutara.net/user-data-deletion.";
              }
            }catch{
              throw;
            }
          }
          return "User doesn't exist in Write Wise!";
        }
    }
public class DeletedUser
    {
        public DeletedUser() { }
        public DeletedUser(string id, string givenName, string familyName, string[] emails, string identityProvider)
        {
            this.id = id;
            this.givenName = givenName;
            this.familyName = familyName;
            this.emails = emails;
            this.identityProvider = identityProvider;
        }
        public string? id { get; set; }
        public string? givenName { get; set; }
        public string? familyName { get; set; }
        public string[]? emails { get; set; }
        public string? identityProvider { get; set; }
    }
}
