namespace BlazorShop.Web.Shared.Services
{
    using System.Net.Http.Json;
    using BlazorShop.Web.Shared.Helper.Contracts;
    using BlazorShop.Web.Shared.Models;
    using BlazorShop.Web.Shared.Models.SupportTicket;
    using BlazorShop.Web.Shared.Services.Contracts;

    public class SupportTicketService : ISupportTicketService
    {
        private readonly IHttpClientHelper _httpClientHelper;
        private readonly IApiCallHelper _apiCallHelper;

        public SupportTicketService(IHttpClientHelper httpClientHelper, IApiCallHelper apiCallHelper)
        {
            _httpClientHelper = httpClientHelper;
            _apiCallHelper = apiCallHelper;
        }

        public async Task<ServiceResponse> SubmitTicketAsync(SubmitTicketRequest request)
        {
            var client = _httpClientHelper.GetPublicClient();
            var currentApiCall = new ApiCall
            {
                Route = Constant.SupportTicket.Submit,
                Type = Constant.ApiCallType.Post,
                Client = client,
                Id = null!,
                Model = request,
            };

            var result = await _apiCallHelper.ApiCallTypeCall<SubmitTicketRequest>(currentApiCall);

            if (result == null || !result.IsSuccessStatusCode)
            {
                var errorResponse = result?.Content != null
                    ? await result.Content.ReadFromJsonAsync<ServiceResponse>()
                    : null;

                return errorResponse ?? new ServiceResponse { Success = false, Message = "Connection error" };
            }

            return await _apiCallHelper.GetServiceResponse<ServiceResponse>(result);
        }
    }
}
