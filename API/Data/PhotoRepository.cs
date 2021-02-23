using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class PhotoRepository : IPhotoRepository
    {
        private readonly ApplicationDbContext _context;

        public PhotoRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        
        public async Task<Photo> GetPhotoById(int id)
        {
            return await _context.Photos
                .IgnoreQueryFilters()
                .SingleOrDefaultAsync(photo => photo.Id == id);
        }

        public void RemovePhoto(Photo photo)
        {
            _context.Photos.Remove(photo);
        }

        public async Task<IEnumerable<PhotoForApprovalDto>> GetUnapprovedPhotos()
        {
            return await _context.Photos
                .IgnoreQueryFilters()
                .Where(photo => photo.IsApproved == false)
                .Select(x => new PhotoForApprovalDto
                {
                    Id = x.Id,
                    UserName = x.AppUser.UserName,
                    Url = x.Url,
                    IsApproved = x.IsApproved
                }).ToListAsync();
        }
    }
}