using Refit;
using SharedLibrary.Models;
namespace ClientApp.Data
{
    public interface IDataAccess
    {
        [Post("/UpdateUser")]
        Task<AccountModel> UpdateUser(AccountModel account);
        
        [Post("/RegisterUser")]
        Task<AccountModel> SaveUser(AccountModel account);

        [Get("/GetUser/{email}")]
        Task<AccountModel> GetUser(string email);

        [Post("/LoginUser")]
        Task<string> LoginUser(AccountModel account);
    }
}
