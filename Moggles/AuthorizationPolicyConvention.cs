using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Authorization;

namespace Moggles
{
    public class AuthorizationPolicyConvention : IApplicationModelConvention
    {
        private readonly string _defaultPolicy;
        private readonly string _jwtPolicy;
        private readonly bool _useJwt;

        public AuthorizationPolicyConvention(string defaultPolicy, bool useJwt, string jwtPolicy)
        {
            _defaultPolicy = defaultPolicy;
            _useJwt = useJwt;
            _jwtPolicy = jwtPolicy;
        }

        public void Apply(ApplicationModel application)
        {
            foreach (var controller in application.Controllers)
            {
                var isApiController = controller.Selectors.Any(x => x.AttributeRouteModel != null &&
                                                                    x.AttributeRouteModel.Template.StartsWith(
                                                                        "api/public"));

                if (isApiController)
                {
                    if (_useJwt)
                    {
                        controller.Filters.Add(new AuthorizeFilter(
                            new IAuthorizeData[]
                            {
                                new AuthorizeAttribute {AuthenticationSchemes = _jwtPolicy}
                            }));
                    }
                }
                else
                {
                    controller.Filters.Add(new AuthorizeFilter(_defaultPolicy));
                }
            }
        }
    }
}
