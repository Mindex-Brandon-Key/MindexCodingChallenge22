using AutoMapper;
using CodeChallenge.Models;

namespace CodeChallenge.Config;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<CompensationCreateDto, Compensation>();
        CreateMap<Compensation, CompensationCreateDto>();
        CreateMap<CompensationListItemDto, Compensation>();
        CreateMap<Compensation, CompensationListItemDto>();
    }
}
