//using AutoMapper;
//using Base_BE.Application.Common.Interfaces;
//using Base_BE.Application.Dtos;
//using MediatR;

//namespace Base_BE.Application.UserVoter.Commands
//{
//    public class UpdateUserVoteCommand : IRequest<UserVoteDto>
//    {
//        public required Guid Id { get; set; }
//        public string? UserId { get; set; }

//        public Guid? VoteId { get; set; }

//        public DateTime? CreatedDate { get; set; }

//        public string? Role { get; set; }

//        public string? BallotTransaction { get; set; }

//        public string? BallotAddress { get; set; }

//        public bool? Status { get; set; }


//    }

//    public class UpdateUserVoteCommandHandler : IRequestHandler<UpdateUserVoteCommand, UserVoteDto>
//    {
//        private readonly IApplicationDbContext _context;
//        private readonly IUser _user;
//        private readonly IMapper _mapper;

//        public UpdateUserVoteCommandHandler(IApplicationDbContext context, IUser user, IMapper mapper)
//        {
//            _context = context;
//            _user = user;
//            _mapper = mapper;
//        }

//        public async Task<UserVoteDto> Handle(UpdateUserVoteCommand request, CancellationToken cancellationToken)
//        {
//            var entity = await _context.UserVotes.FindAsync(request.Id, cancellationToken);
//            if (entity == null) 
//            {
//                return null;
//            }
//            else
//            {
//                entity.UserId = request.UserId;
//                entity.VoteId = request.VoteId;
//                entity.CreatedDate = request.CreatedDate;
//                entity.Role = request.Role;
//                entity.BallotTransaction = request.BallotTransaction;
//                entity.BallotAddress = request.BallotAddress;
//                entity.Status = request.Status;

//                await _context.SaveChangesAsync(cancellationToken);

//                return _mapper.Map<UserVoteDto>(entity);
//            }
//        }
//    }
//}
