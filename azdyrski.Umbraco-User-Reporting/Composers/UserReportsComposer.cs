using Umbraco.Core;
using Umbraco.Core.Composing;
using azdyrski.Umbraco.UserReports.Services;
using azdyrski.Umbraco.UserReports.Interfaces;

namespace azdyrski.Umbraco.UserReports.Composers
{
    //register plugin services
    public class DepInjectionComposer : IUserComposer
    {
        public void Compose(Composition composition)
        {
            composition.Register<IUmbracoUserService, UmbracoUserService>();
            composition.Register<IUmbracoGroupService, UmbracoGroupService>();
        }
    }
}
