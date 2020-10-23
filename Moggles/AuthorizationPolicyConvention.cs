using System.Linq;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Authorization;

namespace Moggles
{
    public class AuthorizationPolicyConvention : IApplicationModelConvention
    {
        private readonly string _defaultPolicy;
        private readonly string _apiPolicy;
        private readonly bool _useJwt;

        public AuthorizationPolicyConvention(string defaultPolicy, string apiPolicy, bool useJwt)
        {
            _defaultPolicy = defaultPolicy;
            _apiPolicy = apiPolicy;
            _useJwt = useJwt;
        }

        public void Apply(ApplicationModel application)
        {
            foreach(var controller in application.Controllers)
            {
                var isApiController = controller.Selectors.Any(x => x.AttributeRouteModel != null &&
                                                                    x.AttributeRouteModel.Template.StartsWith("api/public"));

                if (isApiController)
                {
                    if (_useJwt)
                        controller.Filters.Add(new AuthorizeFilter());
                }
                else
                {
                    controller.Filters.Add(new AuthorizeFilter(_defaultPolicy));
                }
            }
        }
    }
}
