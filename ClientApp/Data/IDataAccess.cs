using Refit;
using SharedLibrary.Models;
namespace ClientApp.Data
{
    public interface IDataAccess
    {
        [Post("/UpdateUser")]
        Task<AccountModel> UpdateUser(AccountModel account);

        [Get("/GetUser/{username}")]
        Task<AccountModel> GetUser(string username);
    }
}
