using API.Interfaces;
using AutoMapper;
using System.Threading.Tasks;

namespace API.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DataContext _dataContext;
        private readonly IMapper _mapper;

        public UnitOfWork(DataContext dataContext, IMapper mapper)
        {
            _dataContext = dataContext;
            _mapper = mapper;
        }

        private IUserRepository _userRepository;
        public IUserRepository UserRepository => _userRepository ?? (_userRepository = new UserRepository(_dataContext));
        private IMessageRepository _messageRepository;
        public IMessageRepository MessageRepository => _messageRepository ?? (_messageRepository = new MessageRepository(_dataContext, _mapper));
        private ILikesRepository _likesRepository;
        public ILikesRepository LikesRepository => _likesRepository ?? (_likesRepository = new LikesRepository(_dataContext, _mapper));

        private IPhotoRepository _photoRepository;
        public IPhotoRepository PhotoRepository => _photoRepository ?? (_photoRepository = new PhotoRepository(_dataContext));

        public async Task<bool> Complete()
        {
            return await _dataContext.SaveChangesAsync() > 0;
        }

        public bool HasChanges()
        {
            return _dataContext.ChangeTracker.HasChanges();
        }
    }
}
