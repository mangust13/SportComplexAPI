// Data/SportComplexContext.cs
using AutoMapper;
using SportComplexAPI.Models;
using SportComplexAPI.DTOs;

namespace SportComplexAPI.Profiles
{
    public class ClientProfile : Profile
    {
        public ClientProfile() 
        {
            CreateMap<Client, ClientDTO>()
                .ForMember(dest => dest.ClientId, opt => opt.MapFrom(src => src.client_id))
                .ForMember(dest => dest.ClientFullName, opt => opt.MapFrom(src => src.client_full_name))
                .ForMember(dest => dest.ClientPhoneNumber, opt => opt.MapFrom(src => src.client_phone_number))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender.gender_name));
        }
    }
}