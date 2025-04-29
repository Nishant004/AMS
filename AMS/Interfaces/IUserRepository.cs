using AMS.Models;

namespace AMS.Interfaces
{
    public interface IUserRepository
    {

        Task<User> GetByUsername(string username);

        Task<IEnumerable<User>> GetAllAsync();


        Task<User?> GetByUsernameAsync(string username);


        Task<int> AddAsync(User user);

        Task<User> GetByIdAsync(string idColumn, int id);

        Task<int> DeleteAsync(string idColumn ,int id);

        //Task<User?> GetByIdAsync(string idColumn, int id); // Get by ID

        Task<int> UpdateAsync(string idColumn, User entity);



    }
}
