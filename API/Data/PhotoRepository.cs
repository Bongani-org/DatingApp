using API.DTOs;
using API.Entities;
using API.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class PhotoRepository : IPhotoRepository
    {
        private readonly DataContext _context;

        public PhotoRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<Photo> GetPhotoById(int id)
        {
            return await _context.Photos.IgnoreQueryFilters().SingleOrDefaultAsync(photo => photo.Id == id);
        }

        public async Task<IEnumerable<PhotoForApprovalDto>> GetUnapprovedPhotos()
        {
            return await _context.Photos.IgnoreQueryFilters().Where(photo => photo.IsApproved == false)
                .Select(photo => new PhotoForApprovalDto
                {
                    id = photo.Id,
                    Username = photo.AppUser.UserName,
                    Url = photo.Url,
                    IsApproved = photo.IsApproved
                }).ToListAsync();
        }

        public void RemovePhoto(Photo photo)
        {
            _context.Photos.Remove(photo);
        }
    }
}