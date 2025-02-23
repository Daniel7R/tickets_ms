using AutoMapper;
using TicketsMS.Application.Interfaces;
using TicketsMS.Domain.Entities;
using TicketsMS.Infrastructure.Repository;

namespace TicketsMS.Application.Services
{
    public class TicketService: ITicketService
    {
        private readonly IRepository<Tickets> _categoryRepository;
        private readonly IMapper _mapper;

        public TicketService(IRepository<Tickets> categoryRepository, IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

    }
}
