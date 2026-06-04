using AutoMapper;
using pos_service.Models;
using pos_service.Models.DTO.Roles;
using pos_service.Models.DTO.Users;
using pos_service.Repositories.Roles;

namespace pos_service.Services.Roles
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _repo;
        private readonly IMapper _mapper;

        public RoleService(IRoleRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        /// <summary>
        /// Retrieves all roles.
        /// </summary>
        public async Task<IEnumerable<RoleResDto>> GetAllAsync()
        { 
            var roles = await _repo.GetAllAsync();
            return _mapper.Map<IEnumerable<RoleResDto>>(roles);
        }

        /// <summary>
        /// Retrieves only active roles.
        /// </summary>
        public async Task<IEnumerable<RoleResDto>> GetActiveAsync()
        {
            var roles = await _repo.GetActiveAsync();
            return _mapper.Map<IEnumerable<RoleResDto>>(roles);
        }

        /// <summary>
        /// Retrieves a role by id.
        /// </summary>
        public async Task<RoleResDto?> GetByIdAsync(int id) 
        {
            var role = await _repo.GetByIdAsync(id);
            return _mapper.Map<RoleResDto?>(role);
        }

        /// <summary>
        /// Creates a new role. Creation of the protected SystemAdmin role (id=1 or name SystemAdmin) is not allowed.
        /// </summary>
        public async Task<RoleResDto?> CreateAsync(RoleReqDto roleDto)
        {
            // prevent creation of SystemAdmin with id 1 and name
            if (string.Equals(roleDto.Name, "SystemAdmin", StringComparison.OrdinalIgnoreCase) || roleDto.Id == 1)
                return null;

            var exists = await _repo.GetByNameAsync(roleDto.Name);
            if (exists != null) return null;

            var role = _mapper.Map<Role>(roleDto);

            var result =  await _repo.AddAsync(role);
            return _mapper.Map<RoleResDto>(result);
        }

        /// <summary>
        /// Updates an existing role by id. SystemAdmin (id=1) cannot be updated.
        /// </summary>
        public async Task<RoleResDto?> UpdateAsync(int id, RoleReqDto roleDto)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return null;

            // protect SystemAdmin
            if (existing.Id == 1) return null;  // don't allow updating SystemAdmin

            existing.Name        = roleDto.Name;
            existing.Description = roleDto.Description;
            existing.IsActive    = roleDto.IsActive;

            var result = await _repo.UpdateAsync(existing);
            return _mapper.Map<RoleResDto>(result);
        }

        /// <summary>
        /// Deletes a role by id. SystemAdmin (id=1) cannot be deleted.
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return false;

            if (existing.Id == 1) return false; // don't allow deleting SystemAdmin

            await _repo.DeleteAsync(existing);
            return true;
        }

        /// <summary>
        /// Sets the active status of a role. SystemAdmin (id=1) cannot be modified.
        /// </summary>
        public async Task<RoleResDto?> SetActiveStatusAsync(int id, bool isActive)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return null;

            if (existing.Id == 1) return null; // don't allow modifying SystemAdmin

            existing.IsActive = isActive;

            var result = await _repo.UpdateAsync(existing);
            return _mapper.Map<RoleResDto>(result);
        }
    }
}