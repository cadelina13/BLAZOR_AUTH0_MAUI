using Refit;
using SharedLibrary.Models;
namespace ClientApp.Data
{
    public interface IDataAccess
    {
        [Post("/UpdateUser")]
        Task<AccountModel> UpdateUser(AccountModel account);
        
        [Post("/SaveUser")]
        Task<AccountModel> SaveUser(AccountModel account);

        [Get("/GetUser/{userId}")]
        Task<AccountModel> GetUser(string userId);
    }
}
