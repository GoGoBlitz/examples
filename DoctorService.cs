using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Medgreat.Mobile.Proto;
using Medgreat.Pacient.Constants;
using Medgreat.Pacient.Page.DoctorDetails.Review.Add;
using Medgreat.Services;
using Medgreat.Services.Authentication;
using Shared.Constants;
using Shared.Services.Web;

namespace Medgreat.Pacient.Services.DoctorService
{
    public class DoctorService : IDoctorService
    {
        private readonly ICache _cache;

        private readonly IWebService _webService;
        private readonly IAuthenticationService _authenticationService;

        public DoctorService(ICache cache, IWebService webService, IAuthenticationService authenticationService)
        {
            _cache = cache;
            _webService = webService;
            _authenticationService = authenticationService;
        }

        public async Task<IEnumerable<DoctorDto>> SearchDoctors(int page, int size, string searchPhrase = null, int? specialtyId = null,
            int? hospitalId = null, int? locationId = null, bool? isConsulting = null, ConsultationType? type = null)
        {
            var doctors = await _cache.Get<IEnumerable<DoctorDto>>(string.Format(CacheConstants.DoctorSearch, page, size, searchPhrase, specialtyId, hospitalId, locationId, isConsulting, type));

            if (doctors == null || !doctors.Any())
            {
                doctors = await GetDoctorsFromServer(page, size, searchPhrase, specialtyId, hospitalId, locationId, isConsulting, type);

                if (doctors != null)
                {
                    await _cache.Add<IEnumerable<DoctorDto>>(string.Format(CacheConstants.DoctorSearch, page,
                            size, searchPhrase, specialtyId, hospitalId, locationId, isConsulting, type),
                        doctors, TimeSpan.FromMinutes(10));
                }
            }

            return doctors;
        }



        private async Task<IEnumerable<DoctorDto>> GetDoctorsFromServer(int page, int size, string searchPhrase = null, int? specialtyId = null,
            int? hospitalId = null, int? locationId = null, bool? isConsulting = null, ConsultationType? type = null)
        {
            string token = await _authenticationService.GetCachedToken();

            StringBuilder urlbuilder = new StringBuilder();
            urlbuilder.Append($"doctors?page={page}&size={size}");

            if (!string.IsNullOrEmpty(searchPhrase))
                urlbuilder.Append($"&searchPhrase={searchPhrase}");

            if (specialtyId.HasValue)
                urlbuilder.Append($"&specialtyId={specialtyId}");

            if (hospitalId.HasValue)
                urlbuilder.Append($"&hospitalId={hospitalId}");

            if (locationId.HasValue)
                urlbuilder.Append($"&locationId={locationId}");

            if (isConsulting.HasValue)
                urlbuilder.Append($"&isConsulting={isConsulting}");

            //if (type.HasValue)
            //    urlbuilder.Append($"&consultationType={(int)type}");

            var doctorList = await _webService.GetJsonAsync<DoctorsListDto>(urlbuilder.ToString(), new Dictionary<string, string> { { "X-Auth-Token", token } });

            return doctorList?.items;
        }

        public async Task<DoctorDtoJSONWrapper> GetDoctorInfoCachedOrFresh(ulong id)
        {
            var details = await _cache.Get<DoctorDtoJSON>(string.Format(CacheConstants.DoctorInfo, id));

            if (details != null) return new DoctorDtoJSONWrapper { isSuccess = true, doctor = details };

            return await GetDoctorInfo(id);
        }

        public async Task<DoctorDtoJSONWrapper> GetDoctorInfo(ulong id)
        {
            var url = $"doctor/{id}";

            var dto = await _webService.GetJsonAsync<DoctorDtoJSONWrapper>(url);

            if (dto != null)
                await _cache.Add<DoctorDtoJSON>(string.Format(CacheConstants.DoctorInfo, id), dto.doctor, TimeSpan.FromMinutes(60));

            return dto;
        }

        public async Task<DoctorDetailDto> GetDoctorInfoCachedOrFresh(ulong consultationId, ConsultationType type)
        {
            var details = await _cache.Get<DoctorDetailDto>(string.Format(CacheConstants.DoctorInfoByConsultation, consultationId, type));

            return details ?? new DoctorDetailDto();
        }

        public async Task<DoctorReviewDtoList> GetReviews(ulong id, int page, int size)
        {
            DoctorReviewDtoList dto = null;

            var response = await _webService.GetBytesAsync($"search/doctor-review/by-doctor?size={size}&page={page}&id={id}");

            dto = DoctorReviewDtoList.Deserialize(response);

            return dto;
        }

        public async Task<bool> SendReview(ulong doctorId, DoctorReviewAddDto dto)
        {
            bool result = false;

            string token = await _cache.Get<string>(SharedConstants.Token);

            await _webService.PostJsonAsync($"doctor/{doctorId}/review", dto, new Dictionary<string, string> { { "X-Auth-Token", token } });

            result = true;

            return result;
        }

        public async Task<DoctorPriceDetailDto> GetDoctorConsultationPrice(ulong doctorId)
        {
            string token = await _cache.Get<string>(SharedConstants.Token);

            var response = await _webService.GetJsonAsync<DoctorPriceDetailDto>($"doctor/{doctorId}/price", new Dictionary<string, string> { { "X-Auth-Token", token } });

            return response;
        }
    }
}