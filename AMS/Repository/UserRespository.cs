using AMS.Data;
using AMS.Interfaces;
using AMS.Models;
using Dapper;
using static Dapper.SqlMapper;

namespace AMS.Repository
{
    public class UserRepository : IUserRepository
    {

        private readonly IGenericRepository<User> _userGenRepository;

        public UserRepository(IGenericRepository<User> userGenRepository)
        {
            _userGenRepository = userGenRepository;
        }

        public async Task<User> GetByUsername(string username)
        {
            var filters = new Dictionary<string, object>() { { "username", username } };
            return await _userGenRepository.GetFirstAsync("id", filters);
        }




        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _userGenRepository.GetAllAsync();
        }




        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _userGenRepository.GetByUsernameAsync(username);
        }

        public Task<int> AddAsync(User user)
        {
            return _userGenRepository.InsertAsync(user);
        }


        public async Task<User> GetByIdAsync(string idColumn, int id)
        {

            return await _userGenRepository.GetByIdAsync(idColumn, id);
        }

        public async Task<int> DeleteAsync(string idColumn,int id)
        {
            return await _userGenRepository.DeleteAsyncPermanent(idColumn, id);

        }


        //public async Task<User?> GetByIdAsync(string idColumn, int id)
        //{
        //      return await _userGenRepository.GetByIdAsync(idColumn, id);
        //}

        public async Task<int> UpdateAsync(string idColumn, User entity)
        {
            return await _userGenRepository.UpdateAsync(idColumn, entity);
        }


    }
}







